using System.Numerics;
using System.Text.RegularExpressions;
using Microsoft.Extensions.DependencyInjection;
using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Entity.Creature;
using NexusForever.Game.Static.Entity;
using NexusForever.Game.Static.Entity.Movement.Spline;
using NexusForever.Game.Static.PlayerPath;
using NexusForever.GameTable;
using NexusForever.GameTable.Model;
using NexusForever.Network.World.Message.Model.PlayerPath;
using NexusForever.Shared;
using NexusForever.Shared.Game;
using NLog;
using Path = NexusForever.Game.Static.PlayerPath.Path;

namespace NexusForever.Game.Entity
{
    /// <summary>
    /// Explorer power maps (tracking missions): a traveler moves away from the mission start and the explorer has to stay close to it,
    /// using the path action to collect its trail until the collect quantity is reached.
    /// </summary>
    public class PathExplorerManager : IPathExplorerManager
    {
        private const float StartDistance          = 15f;
        private const float TravelerSpeed          = 3.5f;
        private const float WanderRadius           = 40f;
        private const double LostTime              = 15d;
        private const double CollectCooldown       = 2d;

        private static readonly ILogger log = LogManager.GetCurrentClassLogger();

        private static Dictionary<uint, uint> travelers;
        private static readonly object travelerLock = new();

        private sealed class PowerMap
        {
            public IPathMission Mission;
            public PathExplorerPowerMapEntry Entry;
            public uint TravelerGuid;
            public bool Entered;
            public double OutsideTime;
            public double CollectTimer;
            public double VictoryTimer = -1d;
        }

        private readonly IPlayer player;
        private PowerMap powerMap;
        private readonly UpdateTimer checkTimer = new(0.5d);

        public PathExplorerManager(IPlayer owner)
        {
            player = owner;
        }

        /// <summary>
        /// The traveler of each power map mission is the creature the client data describes as the mission's power map traveler.
        /// </summary>
        private static uint GetTravelerCreatureId(uint missionId)
        {
            lock (travelerLock)
            {
                if (travelers == null)
                {
                    travelers = new Dictionary<uint, uint>();
                    var tag = new Regex(@"\bm ?(\d+)\b", RegexOptions.IgnoreCase);
                    foreach (Creature2Entry entry in GameTableManager.Instance.Creature2.Entries)
                    {
                        if (entry?.Description == null || !entry.Description.Contains("Power Map", StringComparison.OrdinalIgnoreCase))
                            continue;

                        bool traveler = Regex.IsMatch(entry.Description, "travel+er|power map creature", RegexOptions.IgnoreCase)
                            && !entry.Description.Contains("Inactive", StringComparison.OrdinalIgnoreCase)
                            && !entry.Description.Contains("Flavor", StringComparison.OrdinalIgnoreCase);
                        if (!traveler)
                            continue;

                        foreach (Match match in tag.Matches(entry.Description))
                            travelers.TryAdd(uint.Parse(match.Groups[1].Value), entry.Id);
                    }
                }

                return travelers.TryGetValue(missionId, out uint creatureId) ? creatureId : 0u;
            }
        }

        public void Update(double lastTick)
        {
            checkTimer.Update(lastTick);
            bool check = checkTimer.HasElapsed;
            if (check)
                checkTimer.Reset();

            if (powerMap == null)
            {
                if (check)
                    CheckStart();
                return;
            }

            powerMap.CollectTimer -= lastTick;

            if (powerMap.VictoryTimer >= 0d)
            {
                powerMap.VictoryTimer -= lastTick;
                if (powerMap.VictoryTimer < 0d)
                    Finish(true);
                return;
            }

            IWorldEntity traveler = player.Map?.GetEntity<IWorldEntity>(powerMap.TravelerGuid);
            if (traveler == null)
            {
                Finish(false);
                return;
            }

            bool inside = Vector3.Distance(player.Position, traveler.Position) <= powerMap.Entry.DistanceThreshold;
            if (inside != powerMap.Entered)
            {
                powerMap.Entered = inside;
                if (inside)
                    player.Session.EnqueueMessageEncrypted(new ServerPathExplorerPowerMapEntered { PathMissionId = powerMap.Mission.Id });
                else
                    player.Session.EnqueueMessageEncrypted(new ServerPathExplorerPowerMapExited { PathMissionId = powerMap.Mission.Id });
            }

            powerMap.OutsideTime = inside ? 0d : powerMap.OutsideTime + lastTick;
            if (powerMap.OutsideTime >= LostTime)
                Finish(false);
        }

        private void CheckStart()
        {
            if (player.Path != Path.Explorer || player.Map == null || !player.IsAlive)
                return;

            foreach (IPathMission mission in player.PathMissionManager
                .Where(m => !m.Completed && m.Type == PathMissionType.ExplorerPowerMap)
                .ToList())
            {
                WorldLocation2Entry start = GameTableManager.Instance.WorldLocation2.GetEntry(mission.Entry.WorldLocation2Id00);
                if (start == null || start.WorldId != player.Map.Entry.Id)
                    continue;

                var startPosition = new Vector3(start.Position0, start.Position1, start.Position2);
                if (Vector3.Distance(player.Position, startPosition) > MathF.Max(StartDistance, start.Radius))
                    continue;

                Start(mission, startPosition);
                return;
            }
        }

        private void Start(IPathMission mission, Vector3 startPosition)
        {
            PathExplorerPowerMapEntry entry = GameTableManager.Instance.PathExplorerPowerMap.GetEntry(mission.Entry.ObjectId);
            uint creatureId = GetTravelerCreatureId(mission.Id);
            if (entry == null || creatureId == 0u)
                return;

            ICreatureInfo creatureInfo = LegacyServiceProvider.Provider.GetService<ICreatureInfoManager>().GetCreatureInfo(creatureId);
            if (creatureInfo == null)
                return;

            // the client counts collections from the power map start
            mission.Progress = 0u;

            List<Vector3> nodes = GetTravelerPath(mission, startPosition);

            void OnAdd(Abstract.Map.IBaseMap map, uint guid, Vector3 vector)
            {
                IWorldEntity traveler = map.GetEntity<IWorldEntity>(guid);
                traveler?.MovementManager.LaunchSpline(nodes, SplineType.Linear, nodes.Count > 2 && nodes[0] == nodes[^1] ? SplineMode.Cyclic : SplineMode.OneShot, TravelerSpeed);

                player.Session.EnqueueMessageEncrypted(new ServerPathExplorerPowerMapStarted
                {
                    PathMissionId = mission.Id,
                    UnitId        = guid
                });
            }

            IWorldEntity spawned = creatureInfo.Entry.CreationTypeEnum == EntityType.NonPlayer
                ? player.SummonFactory.Summon<INonPlayerEntity>(creatureInfo, startPosition, Vector3.Zero, OnAdd)
                : player.SummonFactory.Summon<ISimpleEntity>(creatureInfo, startPosition, Vector3.Zero, OnAdd);

            powerMap = new PowerMap
            {
                Mission      = mission,
                Entry        = entry,
                TravelerGuid = 0u
            };

            // guid is only known once the entity is on the map
            powerMap.TravelerGuid = spawned.Guid;
            log.Trace($"Player {player.CharacterId} started power map for mission {mission.Id}.");
        }

        /// <summary>
        /// The traveler moves through the mission locations, a mission with a single location makes it circle around it.
        /// </summary>
        private List<Vector3> GetTravelerPath(IPathMission mission, Vector3 start)
        {
            var nodes = new List<Vector3> { start };
            foreach (uint locationId in new[] { mission.Entry.WorldLocation2Id01, mission.Entry.WorldLocation2Id02, mission.Entry.WorldLocation2Id03 })
            {
                WorldLocation2Entry location = GameTableManager.Instance.WorldLocation2.GetEntry(locationId);
                if (location != null && location.WorldId == player.Map.Entry.Id)
                    nodes.Add(new Vector3(location.Position0, location.Position1, location.Position2));
            }

            if (nodes.Count > 1)
                return nodes;

            for (int i = 1; i <= 6; i++)
            {
                float angle = MathF.PI * 2f * i / 6f;
                float x = start.X + MathF.Cos(angle) * WanderRadius;
                float z = start.Z + MathF.Sin(angle) * WanderRadius;
                nodes.Add(new Vector3(x, player.Map.GetTerrainHeight(x, z) ?? start.Y, z));
            }
            nodes.Add(start);
            return nodes;
        }

        public void OnPowerMapProgress(uint missionId)
        {
            if (powerMap == null || powerMap.Mission.Id != missionId || !powerMap.Entered || powerMap.VictoryTimer >= 0d)
                return;

            if (powerMap.CollectTimer > 0d)
                return;

            powerMap.CollectTimer = CollectCooldown;

            IPathMission mission = powerMap.Mission;
            uint collected = mission.Progress + 1u;
            mission.Progress = collected;

            player.Session.EnqueueMessageEncrypted(new ServerPathMissionAdvanced
            {
                PathMissionId = mission.Id
            });

            if (collected < powerMap.Entry.CollectQuantity)
                return;

            // pause before the mission completes, the client shows the victory waiting state
            powerMap.VictoryTimer = powerMap.Entry.VictoryPauseMS / 1000d;
            player.Session.EnqueueMessageEncrypted(new ServerPathExplorerPowerMapWaiting
            {
                PathMissionId = mission.Id
            });
        }

        private void Finish(bool success)
        {
            PowerMap ended = powerMap;
            powerMap = null;

            player.Map?.GetEntity<IWorldEntity>(ended.TravelerGuid)?.RemoveFromMap();

            if (success)
            {
                player.PathMissionManager.CompleteMission(ended.Mission);
                return;
            }

            // a failed power map starts over
            ended.Mission.Progress = 0u;
            player.Session.EnqueueMessageEncrypted(new ServerPathExplorerPowerMapFailed
            {
                PathMissionId = ended.Mission.Id
            });
        }

        public void Cancel()
        {
            if (powerMap != null)
                Finish(false);
        }
    }
}
