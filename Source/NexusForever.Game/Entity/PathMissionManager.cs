using System.Collections;
using System.Numerics;
using NexusForever.Database.Character;
using NexusForever.Database.Character.Model;
using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Map;
using NexusForever.Game.Prerequisite;
using NexusForever.Game.Static.Reputation;
using NexusForever.Game.Static.PlayerPath;
using NexusForever.GameTable;
using NexusForever.GameTable.Model;
using NexusForever.Network.World.Message.Model.PlayerPath;
using NLog;

namespace NexusForever.Game.Entity
{
    public class PathMissionManager : IPathMissionManager
    {
        // missions further away than this from their location only unlock when the player enters the location's zone
        private const float UnlockDistance = 100f;
        private const float RelocateCheckDistance = 5f;

        private static readonly ILogger log = LogManager.GetCurrentClassLogger();

        private static ILookup<uint, PathMissionEntry> missionsByEpisode;

        /// <summary>
        /// Mission types that are implemented, other mission types are never unlocked.
        /// </summary>
        private static readonly HashSet<PathMissionType> supportedTypes =
        [
            PathMissionType.ExplorerVista,
            PathMissionType.ExplorerArea,
            PathMissionType.ExplorerExploreZone,
            PathMissionType.ScientistScan,
            PathMissionType.ScientistScanChecklist,
            PathMissionType.SoldierHoldout
        ];

        public PathEpisodeEntry CurrentEpisode { get; private set; }

        private readonly IPlayer player;
        private readonly Dictionary<ushort, IPathMission> missions = new();
        private readonly Dictionary<ushort, CharacterPathEpisodeModel> episodes = new();
        private readonly HashSet<ushort> newEpisodes = new();
        private readonly HashSet<uint> sentEpisodes = new();

        private Vector3 lastCheckPosition;
        private bool initialised;

        /// <summary>
        /// Create a new <see cref="IPathMissionManager"/> from <see cref="IPlayer"/> database model.
        /// </summary>
        public PathMissionManager(IPlayer owner, CharacterModel model)
        {
            player = owner;

            missionsByEpisode ??= GameTableManager.Instance.PathMission.Entries
                .Where(e => e != null && e.PathEpisodeId != 0)
                .ToLookup(e => e.PathEpisodeId);

            foreach (CharacterPathMissionModel missionModel in model.PathMission)
            {
                PathMissionEntry entry = GameTableManager.Instance.PathMission.GetEntry(missionModel.MissionId);
                if (entry == null)
                    continue;

                missions.Add(missionModel.MissionId, new PathMission(player, entry, missionModel));
            }

            foreach (CharacterPathEpisodeModel episodeModel in model.PathEpisode)
                episodes.Add(episodeModel.EpisodeId, episodeModel);
        }

        public void Save(CharacterContext context)
        {
            foreach (IPathMission mission in missions.Values)
                mission.Save(context);

            foreach (ushort episodeId in newEpisodes)
                context.Add(episodes[episodeId]);
            newEpisodes.Clear();
        }

        public IPathMission GetMission(uint missionId)
        {
            return missions.TryGetValue((ushort)missionId, out IPathMission mission) ? mission : null;
        }

        public bool IsMissionComplete(uint missionId)
        {
            return GetMission(missionId)?.Completed ?? false;
        }

        public void SendInitialPackets()
        {
            initialised = true;
            sentEpisodes.Clear();
            CurrentEpisode = null;

            // send every episode the player has progress in, this populates the path log
            foreach (IGrouping<uint, IPathMission> group in missions.Values
                .Where(m => m.Entry.PathTypeEnum == (uint)player.Path)
                .GroupBy(m => m.Entry.PathEpisodeId))
                SendEpisodeProgress(group.Key);

            OnZoneUpdate();
        }

        public void OnPathChanged()
        {
            SendInitialPackets();
        }

        public void OnZoneUpdate()
        {
            if (!initialised || player.Zone == null || player.Map == null)
                return;

            PathEpisodeEntry episode = FindEpisode();
            if (episode != CurrentEpisode)
            {
                CurrentEpisode = episode;
                if (episode != null)
                {
                    if (!sentEpisodes.Contains(episode.Id))
                        SendEpisodeProgress(episode.Id);

                    player.Session.EnqueueMessageEncrypted(new ServerPathSetCurrentEpisode
                    {
                        PathEpisodeId = (ushort)episode.Id
                    });
                }
                else
                    player.Session.EnqueueMessageEncrypted(new ServerPathSetCurrentEpisode());
            }

            CheckUnlocks(player.Position);
        }

        public void OnRelocate(Vector3 position)
        {
            if (!initialised || CurrentEpisode == null)
                return;

            if (Vector3.DistanceSquared(lastCheckPosition, position) < RelocateCheckDistance * RelocateCheckDistance)
                return;

            CheckUnlocks(position);
        }

        private PathEpisodeEntry FindEpisode()
        {
            HashSet<uint> zones = GetZoneChain();
            return GameTableManager.Instance.PathEpisode.Entries
                .FirstOrDefault(e => e != null
                    && e.PathTypeEnum == (uint)player.Path
                    && e.WorldId == player.Map.Entry.Id
                    && zones.Contains(e.WorldZoneId));
        }

        private HashSet<uint> GetZoneChain()
        {
            var zones = new HashSet<uint>();
            for (WorldZoneEntry zone = player.Zone; zone != null && zones.Add(zone.Id); zone = zone.ParentZoneId != 0u ? GameTableManager.Instance.WorldZone.GetEntry(zone.ParentZoneId) : null)
                ;
            return zones;
        }

        private void SendEpisodeProgress(uint episodeId)
        {
            sentEpisodes.Add(episodeId);
            player.Session.EnqueueMessageEncrypted(new ServerPathEpisodeProgress
            {
                EpisodeId = (ushort)episodeId,
                Missions  = missions.Values
                    .Where(m => m.Entry.PathEpisodeId == episodeId)
                    .Select(m => m.Build())
                    .ToList()
            });
        }

        /// <summary>
        /// Unlock any missions in the current episode the player now meets the requirements for.
        /// </summary>
        private void CheckUnlocks(Vector3 position)
        {
            lastCheckPosition = position;
            if (CurrentEpisode == null)
                return;

            HashSet<uint> zones = null;
            foreach (PathMissionEntry entry in missionsByEpisode[CurrentEpisode.Id])
            {
                if (missions.ContainsKey((ushort)entry.Id))
                    continue;

                if (!CanUnlock(entry))
                    continue;

                if (entry.WorldLocation2Id00 != 0u)
                {
                    WorldLocation2Entry location = GameTableManager.Instance.WorldLocation2.GetEntry(entry.WorldLocation2Id00);
                    if (location != null)
                    {
                        zones ??= GetZoneChain();
                        bool inZone   = location.WorldZoneId != 0u && zones.Contains(location.WorldZoneId);
                        float range   = MathF.Max(UnlockDistance, location.Radius);
                        bool inRange  = location.WorldId == player.Map.Entry.Id
                            && Vector3.DistanceSquared(position, new Vector3(location.Position0, location.Position1, location.Position2)) <= range * range;
                        if (!inZone && !inRange)
                            continue;
                    }
                }

                UnlockMission(entry);
            }
        }

        private bool CanUnlock(PathMissionEntry entry)
        {
            if (entry.PathTypeEnum != (uint)player.Path)
                return false;

            if (!supportedTypes.Contains((PathMissionType)entry.PathMissionTypeEnum))
                return false;

            // missions with an unlock creature are given by talking to it
            if (entry.Creature2IdUnlock != 0u)
                return false;

            switch (entry.PathMissionFactionEnum)
            {
                case 1u when player.Faction1 != Faction.Exile:
                case 2u when player.Faction1 != Faction.Dominion:
                    return false;
            }

            if (entry.PrerequisiteId != 0u && !PrerequisiteManager.Instance.Meets(player, entry.PrerequisiteId))
                return false;

            return true;
        }

        public IPathMission UnlockMission(PathMissionEntry entry, bool notify = true)
        {
            if (missions.TryGetValue((ushort)entry.Id, out IPathMission existing))
                return existing;

            var mission = new PathMission(player, entry);
            missions.Add(mission.Id, mission);

            if (!sentEpisodes.Contains(entry.PathEpisodeId))
                SendEpisodeProgress(entry.PathEpisodeId);

            // scan missions stay hidden until the first scan, the client plays the communicator itself when they are discovered
            bool hidden = mission.Type is PathMissionType.ScientistScan or PathMissionType.ScientistScanChecklist;
            player.Session.EnqueueMessageEncrypted(new ServerPathMissionActivate
            {
                Missions =
                [
                    mission.Build(notify && !hidden ? PathMissionState.CommunicatorUnlock : PathMissionState.NoMission)
                ]
            });

            log.Trace($"Player {player.CharacterId} unlocked path mission {entry.Id}.");

            OnMissionUnlocked(mission);
            if (hidden)
                player.PathScientistManager.RefreshScannableEntities();
            return mission;
        }

        /// <summary>
        /// Check objectives that might already be met when the mission unlocks.
        /// </summary>
        private void OnMissionUnlocked(IPathMission mission)
        {
            switch (mission.Type)
            {
                case PathMissionType.ExplorerExploreZone:
                {
                    IZoneMap zoneMap = player.ZoneMapManager.GetZoneMap(mission.Entry.ObjectId);
                    if (zoneMap != null && zoneMap.IsComplete)
                        CompleteMission(mission);
                    break;
                }
            }
        }

        public void UpdateMission(IPathMission mission, uint progress, uint state)
        {
            if (mission.Completed)
                return;

            if (mission.Progress == progress && mission.State == state)
                return;

            mission.Progress = progress;
            mission.State    = state;

            if (IsObjectiveComplete(mission))
            {
                CompleteMission(mission);
                return;
            }

            SendMissionUpdate(mission);
        }

        private void SendMissionUpdate(IPathMission mission)
        {
            player.Session.EnqueueMessageEncrypted(new ServerPathMissionUpdate
            {
                Mission = mission.Build()
            });
        }

        /// <summary>
        /// Returns if the objectives of the mission are met, this mirrors the client GetNumCompleted and GetNumNeeded.
        /// </summary>
        private static bool IsObjectiveComplete(IPathMission mission)
        {
            switch (mission.Type)
            {
                case PathMissionType.ExplorerArea:
                case PathMissionType.ExplorerVista:
                case PathMissionType.SettlerScout:
                {
                    uint needed = GetExplorerNodeCount(mission.Entry.ObjectId);
                    return needed > 0u && BitOperations.PopCount(mission.Progress) >= needed;
                }
                case PathMissionType.ScientistScan:
                    return mission.Progress >= 100u;
                case PathMissionType.ScientistScanChecklist:
                {
                    PathScientistCreatureInfoEntry info = GameTableManager.Instance.PathScientistCreatureInfo.GetEntry(mission.Entry.ObjectId);
                    return info != null && info.ChecklistCount > 0u && BitOperations.PopCount(mission.Progress) >= info.ChecklistCount;
                }
                default:
                    return false;
            }
        }

        private static uint GetExplorerNodeCount(uint pathExplorerAreaId)
        {
            return (uint)GameTableManager.Instance.PathExplorerNode.Entries
                .Count(n => n != null && n.PathExplorerAreaId == pathExplorerAreaId);
        }

        public void CompleteMission(IPathMission mission)
        {
            if (mission.Completed)
                return;

            mission.Completed = true;
            SendMissionUpdate(mission);

            log.Trace($"Player {player.CharacterId} completed path mission {mission.Id}.");

            if ((uint)player.Path == mission.Entry.PathTypeEnum)
                player.PathManager.AddXp(GetRewardXp(mission.Entry));

            foreach (PathRewardEntry reward in GameTableManager.Instance.PathReward.Entries
                .Where(r => r != null && r.PathRewardTypeEnum == 1u && r.ObjectId == mission.Id))
                player.PathManager.GrantPathReward(reward);

            CheckEpisodeComplete(mission.Entry.PathEpisodeId);

            if (mission.Type is PathMissionType.ScientistScan or PathMissionType.ScientistScanChecklist)
                player.PathScientistManager.RefreshScannableEntities();
        }

        /// <summary>
        /// Path experience granted for completing a mission.
        /// </summary>
        private static uint GetRewardXp(PathMissionEntry entry)
        {
            // client PathMission:GetRewardXp, the same for every mission
            return GameTableManager.Instance.GameFormula.GetEntry(378)?.Dataint0 ?? 50u;
        }

        private void CheckEpisodeComplete(uint episodeId)
        {
            if (episodes.TryGetValue((ushort)episodeId, out CharacterPathEpisodeModel episodeModel) && episodeModel.RewardReceived != 0)
                return;

            foreach (PathMissionEntry entry in missionsByEpisode[episodeId])
            {
                if (((PathMissionFlags)entry.PathMissionFlags & PathMissionFlags.Optional) != 0)
                    continue;

                // missions for the other faction never count
                if ((entry.PathMissionFactionEnum == 1u && player.Faction1 != Faction.Exile)
                    || (entry.PathMissionFactionEnum == 2u && player.Faction1 != Faction.Dominion))
                    continue;

                if (!IsMissionComplete(entry.Id))
                    return;
            }

            if (episodeModel == null)
            {
                episodeModel = new CharacterPathEpisodeModel
                {
                    Id        = player.CharacterId,
                    EpisodeId = (ushort)episodeId
                };
                episodes.Add((ushort)episodeId, episodeModel);
                newEpisodes.Add((ushort)episodeId);
            }
            episodeModel.RewardReceived = 1;

            log.Trace($"Player {player.CharacterId} completed path episode {episodeId}.");

            foreach (PathRewardEntry reward in GameTableManager.Instance.PathReward.Entries
                .Where(r => r != null && r.PathRewardTypeEnum == 2u && r.ObjectId == episodeId))
                player.PathManager.GrantPathReward(reward);
        }

        public void OnExplorerProgressReport(uint missionId, uint nodeIndex)
        {
            IPathMission mission = GetMission(missionId);
            if (mission == null || mission.Completed)
                return;

            if (mission.Type is not (PathMissionType.ExplorerArea or PathMissionType.ExplorerVista or PathMissionType.SettlerScout))
                return;

            PathExplorerNodeEntry node = GameTableManager.Instance.PathExplorerNode.Entries
                .Where(n => n != null && n.PathExplorerAreaId == mission.Entry.ObjectId)
                .ElementAtOrDefault((int)nodeIndex);
            if (node == null || nodeIndex >= 32u)
                return;

            // the client checks the distance before reporting, verify it as well
            WorldLocation2Entry location = GameTableManager.Instance.WorldLocation2.GetEntry(node.WorldLocation2Id);
            if (location != null)
            {
                float range = MathF.Max(location.Radius, 5f) * 2f;
                if (Vector3.DistanceSquared(player.Position, new Vector3(location.Position0, location.Position1, location.Position2)) > range * range)
                {
                    log.Warn($"Player {player.CharacterId} reported explorer node {nodeIndex} for mission {missionId} out of range.");
                    return;
                }
            }

            UpdateMission(mission, mission.Progress | (1u << (int)nodeIndex), mission.State);
        }

        public void OnZoneMapUpdate(uint mapZoneId, bool complete)
        {
            if (!complete)
                return;

            foreach (IPathMission mission in missions.Values
                .Where(m => !m.Completed && m.Type == PathMissionType.ExplorerExploreZone && m.Entry.ObjectId == mapZoneId)
                .ToList())
                CompleteMission(mission);
        }

        public IEnumerator<IPathMission> GetEnumerator()
        {
            return missions.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
