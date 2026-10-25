using System.Collections;
using System.Numerics;
using NexusForever.Database.Character;
using NexusForever.Database.Character.Model;
using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Map;
using NexusForever.Game.Prerequisite;
using NexusForever.Game.Static;
using NexusForever.Game.Static.Entity;
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
            PathMissionType.SoldierHoldout,
            PathMissionType.SettlerHub,
            PathMissionType.SettlerScout,
            PathMissionType.SettlerSheriff,
            PathMissionType.SoldierAssassinate,
            PathMissionType.SoldierSWAT,
            PathMissionType.SoldierDemolition,
            PathMissionType.SoldierRescue,
            PathMissionType.ExplorerActivateChecklist,
            PathMissionType.ScientistDatacubeDiscovery,
            PathMissionType.ExplorerScavengerHunt,
            PathMissionType.ExplorerDoor,
            PathMissionType.SettlerMayor,
            PathMissionType.ScientistSpecimenSurvey,
            PathMissionType.ExplorerPowerMap
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
            CheckDoors();
        }

        public void OnRelocate(Vector3 position)
        {
            if (!initialised || CurrentEpisode == null)
                return;

            if (Vector3.DistanceSquared(lastCheckPosition, position) < RelocateCheckDistance * RelocateCheckDistance)
                return;

            CheckUnlocks(position);
            CheckScavengerProximity(position);
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

                // missions with an unlock creature are given by talking to it
                if (entry.Creature2IdUnlock != 0u)
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
            if (hidden || mission.Type == PathMissionType.ScientistSpecimenSurvey)
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
                case PathMissionType.ScientistDatacubeDiscovery:
                    UpdateDatacubeDiscovery(mission);
                    break;
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
        private bool IsObjectiveComplete(IPathMission mission)
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
                case PathMissionType.ScientistSpecimenSurvey:
                {
                    PathScientistSpecimenSurveyEntry survey = GameTableManager.Instance.PathScientistSpecimenSurvey.GetEntry(mission.Entry.ObjectId);
                    if (survey == null)
                        return false;

                    // client GetNumNeeded counts the objectives with text
                    TextTable text = GameTableManager.Instance.GetTextTable(Language.English);
                    uint needed = (uint)new[]
                    {
                        survey.LocalizedTextIdObjective00, survey.LocalizedTextIdObjective01, survey.LocalizedTextIdObjective02,
                        survey.LocalizedTextIdObjective03, survey.LocalizedTextIdObjective04, survey.LocalizedTextIdObjective05,
                        survey.LocalizedTextIdObjective06, survey.LocalizedTextIdObjective07, survey.LocalizedTextIdObjective08,
                        survey.LocalizedTextIdObjective09
                    }.Count(id => id != 0u && !string.IsNullOrEmpty(text?.GetEntry(id)));
                    return needed > 0u && BitOperations.PopCount(mission.Progress) >= needed;
                }
                case PathMissionType.SettlerMayor:
                {
                    uint needed = (uint)GetMayorTasks(mission.Entry.ObjectId).Count(t => t.TextId != 0u);
                    return needed > 0u && BitOperations.PopCount(mission.Progress) >= needed;
                }
                case PathMissionType.ExplorerScavengerHunt:
                {
                    uint needed = (uint)GetScavengerClues(mission.Entry.ObjectId).Count(c => c != 0u);
                    return needed > 0u && BitOperations.PopCount(mission.Progress) >= needed;
                }
                case PathMissionType.ExplorerDoor:
                {
                    PathExplorerDoorEntry door = GameTableManager.Instance.PathExplorerDoor.GetEntry(mission.Entry.ObjectId);
                    if (door == null)
                        return false;

                    uint needed = DoorStateEntered;
                    if (door.TargetGroupIdActivate != 0u)
                        needed |= DoorStateActivated;
                    if (door.TargetGroupIdKill != 0u)
                        needed |= DoorStateKilled;
                    return (mission.State & needed) == needed;
                }
                case PathMissionType.ScientistDatacubeDiscovery:
                {
                    uint needed = (uint)GetZoneDatacubes(mission.Entry.ObjectId).Count();
                    return needed > 0u && mission.Progress >= needed;
                }
                case PathMissionType.SoldierAssassinate:
                    return mission.Progress >= (GameTableManager.Instance.PathSoldierAssassinate.GetEntry(mission.Entry.ObjectId)?.Count ?? uint.MaxValue);
                case PathMissionType.SoldierSWAT:
                    return mission.Progress >= (GameTableManager.Instance.PathSoldierSWAT.GetEntry(mission.Entry.ObjectId)?.Count ?? uint.MaxValue);
                case PathMissionType.SoldierDemolition:
                case PathMissionType.SoldierRescue:
                {
                    PathSoldierActivateEntry entry = GameTableManager.Instance.PathSoldierActivate.GetEntry(mission.Entry.ObjectId);
                    if (entry == null)
                        return false;

                    // client GetNumCompleted, checklist activates count each objective bit
                    uint completed = entry.SoldierActivateModeEnum != 0u ? (uint)BitOperations.PopCount(mission.Progress) : mission.Progress;
                    return completed >= entry.Count;
                }
                case PathMissionType.ExplorerActivateChecklist:
                {
                    PathExplorerActivateEntry entry = GameTableManager.Instance.PathExplorerActivate.GetEntry(mission.Entry.ObjectId);
                    return entry != null && BitOperations.PopCount(mission.Progress) >= entry.Count;
                }
                case PathMissionType.SettlerSheriff:
                {
                    uint needed = (uint)GetSheriffQuests(mission.Entry.ObjectId).Count(q => q != 0u);
                    return needed > 0u && BitOperations.PopCount(mission.Progress) >= needed;
                }
                case PathMissionType.SettlerHub:
                {
                    PathSettlerHubEntry hub = GameTableManager.Instance.PathSettlerHub.GetEntry(mission.Entry.ObjectId);
                    return hub != null && hub.MissionCount > 0u && mission.Progress >= hub.MissionCount;
                }
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

            // civil service and public safety missions reward a title
            uint titleId = mission.Type switch
            {
                PathMissionType.SettlerMayor   => GameTableManager.Instance.PathSettlerMayor.GetEntry(mission.Entry.ObjectId)?.CharacterTitleIdReward ?? 0u,
                PathMissionType.SettlerSheriff => GameTableManager.Instance.PathSettlerSheriff.GetEntry(mission.Entry.ObjectId)?.CharacterTitleIdReward ?? 0u,
                _                              => 0u
            };
            if (titleId != 0u)
                player.TitleManager.AddTitle((ushort)titleId);

            CheckEpisodeComplete(mission.Entry.PathEpisodeId);
            UpdateMayorTasks(mission.Entry.WorldLocation2Id00, 0u);

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

        private static uint[] GetSheriffQuests(uint pathSettlerSheriffId)
        {
            PathSettlerSheriffEntry entry = GameTableManager.Instance.PathSettlerSheriff.GetEntry(pathSettlerSheriffId);
            if (entry == null)
                return [];

            return
            [
                entry.Quest2IdSheriff00, entry.Quest2IdSheriff01, entry.Quest2IdSheriff02, entry.Quest2IdSheriff03,
                entry.Quest2IdSheriff04, entry.Quest2IdSheriff05, entry.Quest2IdSheriff06, entry.Quest2IdSheriff07
            ];
        }

        private IEnumerable<IPathMission> GetActiveMissions(PathMissionType type)
        {
            return missions.Values
                .Where(m => !m.Completed && m.Type == type && m.Entry.PathTypeEnum == (uint)player.Path)
                .ToList();
        }

        private static bool MatchesTarget(uint creatureId, uint targetGroupId, IWorldEntity entity)
        {
            if (creatureId != 0u && creatureId == entity.CreatureId)
                return true;

            return targetGroupId != 0u && AssetManager.Instance.GetTargetGroupsForCreatureId(entity.CreatureId).Contains(targetGroupId);
        }

        public void OnCreatureKilled(IWorldEntity entity)
        {
            foreach (IPathMission mission in GetActiveMissions(PathMissionType.ExplorerDoor))
            {
                PathExplorerDoorEntry door = GameTableManager.Instance.PathExplorerDoor.GetEntry(mission.Entry.ObjectId);
                if (door != null && MatchesTarget(0u, door.TargetGroupIdKill, entity))
                    UpdateMission(mission, mission.Progress, mission.State | DoorStateKilled);
            }

            foreach (IPathMission mission in GetActiveMissions(PathMissionType.SoldierAssassinate))
            {
                PathSoldierAssassinateEntry entry = GameTableManager.Instance.PathSoldierAssassinate.GetEntry(mission.Entry.ObjectId);
                if (entry != null && MatchesTarget(entry.Creature2Id, entry.TargetGroupId, entity))
                    UpdateMission(mission, Math.Min(entry.Count, mission.Progress + 1u), mission.State | 1u);
            }

            foreach (IPathMission mission in GetActiveMissions(PathMissionType.SoldierSWAT))
            {
                PathSoldierSWATEntry entry = GameTableManager.Instance.PathSoldierSWAT.GetEntry(mission.Entry.ObjectId);
                if (entry != null && MatchesTarget(entry.Creature2Id, entry.TargetGroupId, entity))
                    UpdateMission(mission, Math.Min(entry.Count, mission.Progress + 1u), mission.State | 1u);
            }
        }

        public void OnEntityActivated(IWorldEntity entity)
        {
            foreach (IPathMission mission in GetActiveMissions(PathMissionType.ExplorerDoor))
            {
                PathExplorerDoorEntry door = GameTableManager.Instance.PathExplorerDoor.GetEntry(mission.Entry.ObjectId);
                if (door != null && MatchesTarget(0u, door.TargetGroupIdActivate, entity))
                    UpdateMission(mission, mission.Progress, mission.State | DoorStateActivated);
            }

            foreach (IPathMission mission in GetActiveMissions(PathMissionType.ExplorerScavengerHunt))
            {
                if ((mission.State & ScavengerHuntStarted) == 0u)
                    continue;

                uint[] clues = GetScavengerClues(mission.Entry.ObjectId);
                for (int i = 0; i < clues.Length; i++)
                {
                    PathExplorerScavengerClueEntry clue = GameTableManager.Instance.PathExplorerScavengerClue.GetEntry(clues[i]);
                    if (clue == null || clue.ExplorerScavengerClueTypeEnum == ScavengerClueTypeDig)
                        continue;

                    if (MatchesTarget(clue.Creature2Id, clue.TargetGroupId, entity))
                        CompleteScavengerClue(mission, i);
                }
            }

            foreach (PathMissionType type in new[] { PathMissionType.SoldierDemolition, PathMissionType.SoldierRescue })
            {
                foreach (IPathMission mission in GetActiveMissions(type))
                {
                    PathSoldierActivateEntry entry = GameTableManager.Instance.PathSoldierActivate.GetEntry(mission.Entry.ObjectId);
                    if (entry == null || !MatchesTarget(entry.Creature2Id, entry.TargetGroupId, entity))
                        continue;

                    uint progress = entry.SoldierActivateModeEnum != 0u
                        ? mission.Progress | entity.QuestChecklistMask
                        : Math.Min(entry.Count, mission.Progress + 1u);
                    UpdateMission(mission, progress, mission.State | 1u);
                }
            }

            foreach (IPathMission mission in GetActiveMissions(PathMissionType.ExplorerActivateChecklist))
            {
                PathExplorerActivateEntry entry = GameTableManager.Instance.PathExplorerActivate.GetEntry(mission.Entry.ObjectId);
                if (entry != null && MatchesTarget(entry.Creature2Id, entry.TargetGroupId, entity))
                    UpdateMission(mission, mission.Progress | entity.QuestChecklistMask, mission.State);
            }
        }

        /// <summary>
        /// Datacubes of the zone a datacube discovery mission is for, this mirrors the client zone datacube count.
        /// </summary>
        private IEnumerable<DatacubeEntry> GetZoneDatacubes(uint pathScientistDatacubeDiscoveryId)
        {
            PathScientistDatacubeDiscoveryEntry entry = GameTableManager.Instance.PathScientistDatacubeDiscovery.GetEntry(pathScientistDatacubeDiscoveryId);
            if (entry == null)
                return [];

            uint faction = player.Faction1 == Faction.Exile ? 1u : 2u;
            return GameTableManager.Instance.Datacube.Entries
                .Where(d => d != null
                    && d.WorldZoneId == entry.WorldZoneId
                    && d.DatacubeTypeEnum == (uint)DatacubeType.Datacube
                    && (d.DatacubeFactionEnum == 0u || d.DatacubeFactionEnum == faction));
        }

        private void UpdateDatacubeDiscovery(IPathMission mission)
        {
            uint discovered = (uint)GetZoneDatacubes(mission.Entry.ObjectId)
                .Count(d => player.DatacubeManager.GetDatacube((ushort)d.Id, DatacubeType.Datacube) != null);
            UpdateMission(mission, discovered, mission.State);
        }

        public void OnDatacubeDiscovered(uint datacubeId)
        {
            foreach (IPathMission mission in GetActiveMissions(PathMissionType.ScientistDatacubeDiscovery))
                UpdateDatacubeDiscovery(mission);
        }

        private const uint DoorStateEntered   = 0x01;
        private const uint DoorStateActivated = 0x02;
        private const uint DoorStateKilled    = 0x04;

        private const uint ScavengerHuntStarted = 0x40000000;
        private const uint ScavengerClueTypeDig = 2u;

        /// <summary>
        /// Explorer door missions are explored by entering the micro zone behind the door.
        /// </summary>
        private void CheckDoors()
        {
            HashSet<uint> zones = null;
            foreach (IPathMission mission in GetActiveMissions(PathMissionType.ExplorerDoor))
            {
                PathExplorerDoorEntry door = GameTableManager.Instance.PathExplorerDoor.GetEntry(mission.Entry.ObjectId);
                if (door == null || (mission.State & DoorStateEntered) != 0u)
                    continue;

                zones ??= GetZoneChain();
                if (zones.Contains(door.WorldZoneIdInsideMicro))
                    UpdateMission(mission, mission.Progress, mission.State | DoorStateEntered);
            }
        }

        private static uint[] GetScavengerClues(uint pathExplorerScavengerHuntId)
        {
            PathExplorerScavengerHuntEntry entry = GameTableManager.Instance.PathExplorerScavengerHunt.GetEntry(pathExplorerScavengerHuntId);
            if (entry == null)
                return [];

            return
            [
                entry.PathExplorerScavengerClueId00, entry.PathExplorerScavengerClueId01, entry.PathExplorerScavengerClueId02,
                entry.PathExplorerScavengerClueId03, entry.PathExplorerScavengerClueId04, entry.PathExplorerScavengerClueId05,
                entry.PathExplorerScavengerClueId06
            ];
        }

        public void OnEntityInteract(IWorldEntity entity)
        {
            // missions given by talking to a creature
            if (CurrentEpisode != null)
            {
                foreach (PathMissionEntry entry in missionsByEpisode[CurrentEpisode.Id])
                {
                    if (entry.Creature2IdUnlock == 0u || entry.Creature2IdUnlock != entity.CreatureId)
                        continue;

                    if (!missions.ContainsKey((ushort)entry.Id) && CanUnlock(entry))
                        UnlockMission(entry);
                }
            }

            // scavenger hunts are started by talking to their start creature
            foreach (IPathMission mission in GetActiveMissions(PathMissionType.ExplorerScavengerHunt))
            {
                PathExplorerScavengerHuntEntry hunt = GameTableManager.Instance.PathExplorerScavengerHunt.GetEntry(mission.Entry.ObjectId);
                if (hunt == null || hunt.Creature2IdStart != entity.CreatureId || (mission.State & ScavengerHuntStarted) != 0u)
                    continue;

                UpdateMission(mission, mission.Progress, mission.State | ScavengerHuntStarted);
                SendScavengerClues(mission);
            }
        }

        private void SendScavengerClues(IPathMission mission)
        {
            foreach (uint clueId in GetScavengerClues(mission.Entry.ObjectId))
            {
                PathExplorerScavengerClueEntry clue = GameTableManager.Instance.PathExplorerScavengerClue.GetEntry(clueId);
                if (clue == null)
                    continue;

                if (clue.ExplorerScavengerClueTypeEnum == ScavengerClueTypeDig)
                {
                    if (clue.WorldLocation2IdMiniMap == 0u)
                        continue;

                    player.Session.EnqueueMessageEncrypted(new ServerPathExplorerScavengerHuntClueAddWorldLocation
                    {
                        WorldLocation2Id            = clue.WorldLocation2IdMiniMap,
                        PathExplorerScavengerClueId = (ushort)clue.Id
                    });
                }
                else
                {
                    uint[] units = player.GetVisible()
                        .OfType<IWorldEntity>()
                        .Where(e => MatchesTarget(clue.Creature2Id, clue.TargetGroupId, e))
                        .Select(e => e.Guid)
                        .ToArray();
                    if (units.Length == 0)
                        continue;

                    player.Session.EnqueueMessageEncrypted(new ServerPathExplorerScavengerHuntClueAddUnitList
                    {
                        PathExplorerScavengerClueId = (ushort)clue.Id,
                        UnitIdList                  = units
                    });
                }
            }
        }

        private void CompleteScavengerClue(IPathMission mission, int index)
        {
            if ((mission.Progress & (1u << index)) != 0u)
                return;

            uint clueId = GetScavengerClues(mission.Entry.ObjectId)[index];
            player.Session.EnqueueMessageEncrypted(new ServerPathExplorerScavengerHuntClueRemoveByClueId
            {
                PathExplorerScavengerClueId = (ushort)clueId
            });

            UpdateMission(mission, mission.Progress | (1u << index), mission.State);
        }

        /// <summary>
        /// Creature clues with an active radius are found by getting close to the creature.
        /// </summary>
        private void CheckScavengerProximity(Vector3 position)
        {
            foreach (IPathMission mission in GetActiveMissions(PathMissionType.ExplorerScavengerHunt))
            {
                if ((mission.State & ScavengerHuntStarted) == 0u)
                    continue;

                uint[] clues = GetScavengerClues(mission.Entry.ObjectId);
                for (int i = 0; i < clues.Length; i++)
                {
                    PathExplorerScavengerClueEntry clue = GameTableManager.Instance.PathExplorerScavengerClue.GetEntry(clues[i]);
                    if (clue == null || clue.Creature2Id == 0u || clue.ActiveRadius <= 0f || (mission.Progress & (1u << i)) != 0u)
                        continue;

                    if (player.GetVisibleCreature<IWorldEntity>(clue.Creature2Id).Any(e => Vector3.Distance(position, e.Position) <= clue.ActiveRadius))
                        CompleteScavengerClue(mission, i);
                }
            }
        }

        public void OnExplorerSearch(uint clueId, byte searchRadiusBand)
        {
            // the client picks the band from how close the player is to the dig location, the last band is a successful dig
            if (searchRadiusBand < 2)
                return;

            foreach (IPathMission mission in GetActiveMissions(PathMissionType.ExplorerScavengerHunt))
            {
                if ((mission.State & ScavengerHuntStarted) == 0u)
                    continue;

                int index = Array.IndexOf(GetScavengerClues(mission.Entry.ObjectId), clueId);
                if (index < 0)
                    continue;

                PathExplorerScavengerClueEntry clue = GameTableManager.Instance.PathExplorerScavengerClue.GetEntry(clueId);
                WorldLocation2Entry location = clue != null ? GameTableManager.Instance.WorldLocation2.GetEntry(clue.WorldLocation2IdMiniMap) : null;
                if (location == null)
                    continue;

                float range = MathF.Max(location.Radius, 10f);
                if (Vector3.Distance(player.Position, new Vector3(location.Position0, location.Position1, location.Position2)) > range)
                    continue;

                CompleteScavengerClue(mission, index);
            }
        }

        private static (uint TextId, uint WorldLocation2Id)[] GetMayorTasks(uint pathSettlerMayorId)
        {
            PathSettlerMayorEntry entry = GameTableManager.Instance.PathSettlerMayor.GetEntry(pathSettlerMayorId);
            if (entry == null)
                return [];

            return
            [
                (entry.LocalizedTextId00, entry.WorldLocation2Id00), (entry.LocalizedTextId01, entry.WorldLocation2Id01),
                (entry.LocalizedTextId02, entry.WorldLocation2Id02), (entry.LocalizedTextId03, entry.WorldLocation2Id03),
                (entry.LocalizedTextId04, entry.WorldLocation2Id04), (entry.LocalizedTextId05, entry.WorldLocation2Id05),
                (entry.LocalizedTextId06, entry.WorldLocation2Id06), (entry.LocalizedTextId07, entry.WorldLocation2Id07)
            ];
        }

        /// <summary>
        /// Civil service tasks are completed by helping out where they point to: completing the settler mission at the task location
        /// or completing quests in the task location's zone.
        /// </summary>
        private void UpdateMayorTasks(uint completedMissionLocationId, uint questZoneId)
        {
            foreach (IPathMission mission in GetActiveMissions(PathMissionType.SettlerMayor))
            {
                (uint TextId, uint WorldLocation2Id)[] tasks = GetMayorTasks(mission.Entry.ObjectId);
                uint progress = mission.Progress;
                for (int i = 0; i < tasks.Length; i++)
                {
                    if (tasks[i].TextId == 0u || tasks[i].WorldLocation2Id == 0u)
                        continue;

                    if (completedMissionLocationId != 0u && tasks[i].WorldLocation2Id == completedMissionLocationId)
                        progress |= 1u << i;
                    else if (questZoneId != 0u && GameTableManager.Instance.WorldLocation2.GetEntry(tasks[i].WorldLocation2Id)?.WorldZoneId == questZoneId)
                        progress |= 1u << i;
                }

                if (progress != mission.Progress)
                    UpdateMission(mission, progress, mission.State);
            }
        }

        public void OnQuestCompleted(uint questId)
        {
            if (player.Zone != null)
                foreach (uint zoneId in GetZoneChain())
                    UpdateMayorTasks(0u, zoneId);

            foreach (IPathMission mission in GetActiveMissions(PathMissionType.SettlerSheriff))
            {
                int index = Array.IndexOf(GetSheriffQuests(mission.Entry.ObjectId), questId);
                if (index >= 0)
                    UpdateMission(mission, mission.Progress | (1u << index), mission.State);
            }
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
