using System.Numerics;
using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Static.PlayerPath;
using NexusForever.GameTable;
using NexusForever.GameTable.Model;
using NexusForever.Network.World.Message.Model.PlayerPath;
using NexusForever.Network.World.Message.Static;
using NLog;
using Path = NexusForever.Game.Static.PlayerPath.Path;

namespace NexusForever.Game.Entity
{
    public class PathSettlerManager : IPathSettlerManager
    {
        // the client passes the result to the SettlerBuildResult event without using it
        private enum SettlerBuildResult
        {
            Ok     = 0,
            Failed = 1
        }

        /// <summary>
        /// State of a settler improvement, improvements are shared by everyone on the same world.
        /// </summary>
        private sealed class ImprovementState
        {
            public int Tier = -1;
            public DateTime Expires;
            public uint BundleCount;
            public readonly List<string> Owners = new();

            public bool IsActive => Tier >= 0 && DateTime.UtcNow < Expires;
        }

        private const float DepotInteractDistance = 15f;

        private static readonly ILogger log = LogManager.GetCurrentClassLogger();
        private static readonly Dictionary<(uint WorldId, uint GroupId), ImprovementState> improvements = new();
        private static readonly object improvementLock = new();

        private readonly IPlayer player;
        private uint depotGuid;

        public PathSettlerManager(IPlayer owner)
        {
            player = owner;
        }

        private static IEnumerable<PathSettlerImprovementGroupEntry> GetGroups(uint hubId)
        {
            return GameTableManager.Instance.PathSettlerImprovementGroup.Entries
                .Where(g => g != null && g.PathSettlerHubId == hubId);
        }

        private static IEnumerable<PathSettlerImprovementGroupEntry> GetDepotGroups(uint creatureId)
        {
            return GameTableManager.Instance.PathSettlerImprovementGroup.Entries
                .Where(g => g != null && g.Creature2IdDepot == creatureId);
        }

        private static uint GetTierImprovementId(PathSettlerImprovementGroupEntry group, uint tier)
        {
            return tier switch
            {
                0u => group.PathSettlerImprovementIdTier00,
                1u => group.PathSettlerImprovementIdTier01,
                2u => group.PathSettlerImprovementIdTier02,
                3u => group.PathSettlerImprovementIdTier03,
                _  => 0u
            };
        }

        private ImprovementState GetState(uint groupId)
        {
            lock (improvementLock)
            {
                var key = (player.Map.Entry.Id, groupId);
                if (!improvements.TryGetValue(key, out ImprovementState state))
                {
                    state = new ImprovementState();
                    improvements.Add(key, state);
                }

                if (state.Tier >= 0 && !state.IsActive)
                {
                    state.Tier        = -1;
                    state.BundleCount = 0u;
                    state.Owners.Clear();
                }

                return state;
            }
        }

        private SettlerImprovementGroupStatus BuildStatus(PathSettlerImprovementGroupEntry group)
        {
            ImprovementState state = GetState(group.Id);
            return new SettlerImprovementGroupStatus
            {
                PathSettlerImprovementGroupId = (ushort)group.Id,
                Tier                          = state.Tier,
                RemainingTimeMS               = state.IsActive ? (uint)(state.Expires - DateTime.UtcNow).TotalMilliseconds : 0u,
                BundleCount                   = state.BundleCount
            };
        }

        public void OpenDepot(IWorldEntity depot)
        {
            if (player.Path != Path.Settler)
                return;

            List<PathSettlerImprovementGroupEntry> groups = GetDepotGroups(depot.CreatureId).ToList();
            if (groups.Count == 0)
                return;

            depotGuid = depot.Guid;

            foreach (IGrouping<uint, PathSettlerImprovementGroupEntry> hub in groups.GroupBy(g => g.PathSettlerHubId))
                SendHubStatus(hub.Key);

            player.Session.EnqueueMessageEncrypted(new ServerPathSettlerBuildInvoke
            {
                SettlerHubUnitId               = depot.Guid,
                PathSettlerImprovementGroupIds = groups.Select(g => g.Id).ToList()
            });
        }

        public void BuildImprovement(uint improvementGroupId, uint tier)
        {
            PathSettlerImprovementGroupEntry group = GameTableManager.Instance.PathSettlerImprovementGroup.GetEntry(improvementGroupId);
            SettlerBuildResult result = CanBuild(group, tier, out PathSettlerImprovementEntry improvement, out bool contribution);
            if (result != SettlerBuildResult.Ok)
            {
                SendBuildResult(result, group);
                return;
            }

            PathSettlerHubEntry hub = GameTableManager.Instance.PathSettlerHub.GetEntry(group.PathSettlerHubId);
            uint[] resources = [hub.Item2IdResource00, hub.Item2IdResource01, hub.Item2IdResource02];
            uint[] counts = contribution
                ? [improvement.CountRecontributionResource00, improvement.CountRecontributionResource01, improvement.CountRecontributionResource02]
                : [improvement.CountResource00, improvement.CountResource01, improvement.CountResource02];
            for (int i = 0; i < resources.Length; i++)
                if (resources[i] != 0u && counts[i] != 0u)
                    player.Inventory.ItemDelete(resources[i], counts[i], ItemUpdateReason.SettlerImprovementConsumeResource);

            ImprovementState state = GetState(group.Id);
            lock (improvementLock)
            {
                DateTime start = state.IsActive ? state.Expires : DateTime.UtcNow;
                state.Tier        = (int)tier;
                state.BundleCount = Math.Min(group.MaxBundleCount, state.BundleCount + 1u);
                state.Expires     = start.AddMilliseconds(group.DurationPerBundleMs);

                // the improvement can't last longer than the maximum number of bundles
                DateTime max = DateTime.UtcNow.AddMilliseconds((double)group.DurationPerBundleMs * Math.Max(1u, group.MaxBundleCount));
                if (state.Expires > max)
                    state.Expires = max;

                if (!state.Owners.Contains(player.Name))
                    state.Owners.Add(player.Name);
            }

            log.Trace($"Player {player.CharacterId} built settler improvement group {group.Id} tier {tier}.");

            SendBuildResult(SettlerBuildResult.Ok, group);
            BroadcastStatus(group);
            UpdateHubMission(group);
        }

        private SettlerBuildResult CanBuild(PathSettlerImprovementGroupEntry group, uint tier, out PathSettlerImprovementEntry improvement, out bool contribution)
        {
            improvement  = null;
            contribution = false;

            if (player.Path != Path.Settler || group == null)
                return SettlerBuildResult.Failed;

            IWorldEntity depot = player.GetVisible<IWorldEntity>(depotGuid);
            if (depot == null || depot.CreatureId != group.Creature2IdDepot
                || Vector3.Distance(player.Position, depot.Position) > DepotInteractDistance)
                return SettlerBuildResult.Failed;

            uint improvementId = GetTierImprovementId(group, tier);
            improvement = GameTableManager.Instance.PathSettlerImprovement.GetEntry(improvementId);
            if (improvement == null)
                return SettlerBuildResult.Failed;

            // an outpost improvement has to be built before the improvements that depend on it
            if (group.PathSettlerImprovementGroupIdOutpostRequired != 0u && !GetState(group.PathSettlerImprovementGroupIdOutpostRequired).IsActive)
                return SettlerBuildResult.Failed;

            ImprovementState state = GetState(group.Id);
            contribution = state.IsActive && state.Tier == (int)tier;
            if (state.IsActive && state.Tier > (int)tier)
                return SettlerBuildResult.Failed;

            PathSettlerHubEntry hub = GameTableManager.Instance.PathSettlerHub.GetEntry(group.PathSettlerHubId);
            if (hub == null)
                return SettlerBuildResult.Failed;

            uint[] resources = [hub.Item2IdResource00, hub.Item2IdResource01, hub.Item2IdResource02];
            uint[] counts = contribution
                ? [improvement.CountRecontributionResource00, improvement.CountRecontributionResource01, improvement.CountRecontributionResource02]
                : [improvement.CountResource00, improvement.CountResource01, improvement.CountResource02];
            for (int i = 0; i < resources.Length; i++)
                if (resources[i] != 0u && counts[i] != 0u && !player.Inventory.HasItemCount(resources[i], counts[i]))
                    return SettlerBuildResult.Failed;

            return SettlerBuildResult.Ok;
        }

        private void SendBuildResult(SettlerBuildResult result, PathSettlerImprovementGroupEntry group)
        {
            player.Session.EnqueueMessageEncrypted(new ServerPathSettlerBuildResult
            {
                Result                        = (uint)result,
                Unknown                       = result == SettlerBuildResult.Ok ? 1u : 0u,
                PathSettlerImprovementGroupId = group?.Id ?? 0u
            });
        }

        private void BroadcastStatus(PathSettlerImprovementGroupEntry group)
        {
            var message = new ServerPathSettlerBuildStatus
            {
                PathSettlerHubId = (ushort)group.PathSettlerHubId,
                Status           = BuildStatus(group)
            };
            ServerPathSettlerHubUpdate hubUpdate = BuildHubUpdate(group.PathSettlerHubId);

            player.Session.EnqueueMessageEncrypted(message);
            player.Session.EnqueueMessageEncrypted(hubUpdate);

            foreach (IPlayer other in player.GetVisible().OfType<IPlayer>())
            {
                if (other == player || other.Path != Path.Settler)
                    continue;

                other.Session.EnqueueMessageEncrypted(message);
                other.Session.EnqueueMessageEncrypted(hubUpdate);
            }
        }

        private ServerPathSettlerHubUpdate BuildHubUpdate(uint hubId)
        {
            PathSettlerHubEntry hub = GameTableManager.Instance.PathSettlerHub.GetEntry(hubId);
            uint[] avenues = new uint[3];
            foreach (PathSettlerImprovementGroupEntry group in GetGroups(hubId))
            {
                ImprovementState state = GetState(group.Id);
                if (!state.IsActive || group.SettlerAvenueTypeEnum > 2u)
                    continue;

                avenues[group.SettlerAvenueTypeEnum] += group.ContributionValue + group.PerTierBonusContributionValue * (uint)state.Tier;
            }

            return new ServerPathSettlerHubUpdate
            {
                PathSettlerHubId         = hubId,
                AvenueEconomyValue       = Math.Min(avenues[0], hub?.MaxAvenueEconomy ?? avenues[0]),
                AvenueSecurityValue      = Math.Min(avenues[1], hub?.MaxAvenueSecurity ?? avenues[1]),
                AvenueQualityOfLifeValue = Math.Min(avenues[2], hub?.MaxAvenueQualityOfLife ?? avenues[2])
            };
        }

        private void SendHubStatus(uint hubId)
        {
            player.Session.EnqueueMessageEncrypted(new ServerPathSettlerBuildStatusList
            {
                PathSettlerHubId         = (ushort)hubId,
                ImprovementGroupStatuses = GetGroups(hubId).Select(BuildStatus).ToList()
            });
            player.Session.EnqueueMessageEncrypted(BuildHubUpdate(hubId));
        }

        /// <summary>
        /// Hub missions need an improvement built in each improvement group of the hub, state holds a bit for each group built.
        /// </summary>
        private void UpdateHubMission(PathSettlerImprovementGroupEntry group)
        {
            List<PathSettlerImprovementGroupEntry> groups = GetGroups(group.PathSettlerHubId).OrderBy(g => g.Id).ToList();
            int index = groups.FindIndex(g => g.Id == group.Id);
            if (index < 0 || index >= 32)
                return;

            foreach (IPathMission mission in player.PathMissionManager
                .Where(m => !m.Completed && m.Type == PathMissionType.SettlerHub && m.Entry.ObjectId == group.PathSettlerHubId)
                .ToList())
            {
                uint state = mission.State | (1u << index);
                player.PathMissionManager.UpdateMission(mission, (uint)BitOperations.PopCount(state), state);

                if (mission.Completed)
                {
                    player.Session.EnqueueMessageEncrypted(new ServerPathSettlerHubReward
                    {
                        PathSettlerHubId = group.PathSettlerHubId
                    });
                }
            }
        }

        public void OnZoneUpdate()
        {
            if (player.Path != Path.Settler || player.Zone == null || player.Map == null)
                return;

            var zones = new HashSet<uint>();
            for (WorldZoneEntry zone = player.Zone; zone != null && zones.Add(zone.Id); zone = zone.ParentZoneId != 0u ? GameTableManager.Instance.WorldZone.GetEntry(zone.ParentZoneId) : null)
                ;

            foreach (PathSettlerHubEntry hub in GameTableManager.Instance.PathSettlerHub.Entries.Where(h => h != null && zones.Contains(h.WorldZoneId)))
                SendHubStatus(hub.Id);
        }
    }
}
