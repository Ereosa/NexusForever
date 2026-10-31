using System.Numerics;
using Microsoft.Extensions.DependencyInjection;
using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Entity.Creature;
using NexusForever.Game.Abstract.Map;
using NexusForever.Shared;
using NexusForever.Game.Static;
using NexusForever.Game.Static.Entity;
using NexusForever.Game.Spell;
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

            // the improvement's unit in the world while it is active, e.g. a mailbox or a buff station
            public IBaseMap SpawnMap;
            public uint SpawnGuid;
            public uint SpawnCreatureId;
            public bool SpawnPending;

            public bool IsActive => Tier >= 0 && DateTime.UtcNow < Expires;
        }

        /// <summary>
        /// Improvements whose display spell isn't the activate spell of a creature, the client data doesn't link them to their unit.
        /// </summary>
        /// <remarks>
        /// Keyed by the display spell's base name, the units are the "Settler Template - Improvement" creatures.
        /// </remarks>
        private static readonly Dictionary<string, uint> improvementCreaturesByDisplayName = new()
        {
            { "Mailbox", 18829u } // Settler Template - Improvement - Settler Mailbox
        };

        private static readonly Dictionary<uint, uint> improvementCreatures = new();
        private static ILookup<uint, Creature2Entry> creaturesByActivateSpell;

        /// <summary>
        /// Distance from the display point that improvements sharing a display point are placed at.
        /// </summary>
        private const float ImprovementSpacing = 5f;

        private const float DepotInteractDistance = 30f;

        private static readonly ILogger log = LogManager.GetCurrentClassLogger();
        private static readonly Dictionary<(uint WorldId, uint GroupId), ImprovementState> improvements = new();
        private static readonly object improvementLock = new();

        /// <summary>
        /// Shared progress of a settler infrastructure project on a world.
        /// </summary>
        private sealed class InfrastructureState
        {
            public uint Count;
            public DateTime Started;
            public DateTime BuiltUntil;

            public bool IsBuilt => DateTime.UtcNow < BuiltUntil;
        }

        private const double InfrastructureContributeCooldown = 2d;

        private static readonly Dictionary<(uint WorldId, uint InfrastructureId), InfrastructureState> infrastructure = new();

        private readonly IPlayer player;
        private uint depotGuid;

        // infrastructure resources the player is carrying, by PathSettlerInfrastructure id and resource index
        private readonly Dictionary<(uint InfrastructureId, int Index), uint> carried = new();
        private DateTime lastContribution = DateTime.MinValue;

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
                    state.SpawnPending = false;
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
                    TakeResource(resources[i], counts[i]);

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

            SpawnImprovement(group, improvement, state);

            SendBuildResult(SettlerBuildResult.Ok, group);
            BroadcastStatus(group);
            UpdateHubMission(group);
        }

        /// <summary>
        /// Settler resources are tradeskill materials, they are kept in the supply satchel unless the satchel is full.
        /// </summary>
        private static ushort GetSupplySatchelMaterialId(uint itemId)
        {
            return (ushort)(GameTableManager.Instance.TradeskillMaterial.Entries
                .FirstOrDefault(m => m != null && m.Item2Id == itemId)?.Id ?? 0u);
        }

        private bool HasResource(uint itemId, uint count)
        {
            ushort materialId = GetSupplySatchelMaterialId(itemId);
            uint satchel = materialId != 0 ? player.SupplySatchelManager.GetAmount(materialId) : 0u;
            if (satchel >= count)
                return true;

            return player.Inventory.HasItemCount(itemId, count - satchel);
        }

        private void TakeResource(uint itemId, uint count)
        {
            ushort materialId = GetSupplySatchelMaterialId(itemId);
            if (materialId != 0)
            {
                uint satchel = Math.Min(count, player.SupplySatchelManager.GetAmount(materialId));
                if (satchel > 0u)
                {
                    player.SupplySatchelManager.RemoveAmount(materialId, satchel);
                    count -= satchel;
                }
            }

            if (count > 0u)
                player.Inventory.ItemDelete(itemId, count, ItemUpdateReason.SettlerImprovementConsumeResource);
        }

        private SettlerBuildResult CanBuild(PathSettlerImprovementGroupEntry group, uint tier, out PathSettlerImprovementEntry improvement, out bool contribution)
        {
            improvement  = null;
            contribution = false;

            SettlerBuildResult Fail(string reason)
            {
                log.Debug($"Player {player.CharacterId} can't build settler improvement group {group?.Id} tier {tier}: {reason}.");
                return SettlerBuildResult.Failed;
            }

            if (player.Path != Path.Settler || group == null)
                return Fail("player is not a settler or the improvement group doesn't exist");

            IWorldEntity depot = player.GetVisible<IWorldEntity>(depotGuid);
            if (depot == null || depot.CreatureId != group.Creature2IdDepot
                || Vector3.Distance(player.Position, depot.Position) > DepotInteractDistance)
                return Fail("the player isn't at the improvement group's depot");

            uint improvementId = GetTierImprovementId(group, tier);
            improvement = GameTableManager.Instance.PathSettlerImprovement.GetEntry(improvementId);
            if (improvement == null)
                return Fail("the improvement tier doesn't exist");

            // an outpost improvement has to be built before the improvements that depend on it
            if (group.PathSettlerImprovementGroupIdOutpostRequired != 0u && !GetState(group.PathSettlerImprovementGroupIdOutpostRequired).IsActive)
                return Fail("the required outpost isn't built");

            ImprovementState state = GetState(group.Id);
            contribution = state.IsActive && state.Tier == (int)tier;
            if (state.IsActive && state.Tier > (int)tier)
                return Fail("a higher tier is already active");

            PathSettlerHubEntry hub = GameTableManager.Instance.PathSettlerHub.GetEntry(group.PathSettlerHubId);
            if (hub == null)
                return Fail("the improvement group has no hub");

            uint[] resources = [hub.Item2IdResource00, hub.Item2IdResource01, hub.Item2IdResource02];
            uint[] counts = contribution
                ? [improvement.CountRecontributionResource00, improvement.CountRecontributionResource01, improvement.CountRecontributionResource02]
                : [improvement.CountResource00, improvement.CountResource01, improvement.CountResource02];
            for (int i = 0; i < resources.Length; i++)
                if (resources[i] != 0u && counts[i] != 0u && !HasResource(resources[i], counts[i]))
                    return Fail("the player doesn't have the resources");

            return SettlerBuildResult.Ok;
        }

        /// <summary>
        /// Return the creature that represents a settler improvement in the world, 0 if there is none.
        /// </summary>
        /// <remarks>
        /// Buff improvements display the spell their station casts when activated, e.g. "Shield Capacity Booster" is the "Max Shields" station.
        /// A station named after the improvement group is preferred, then the "Settler Template" stations.
        /// </remarks>
        private static uint GetImprovementCreatureId(PathSettlerImprovementGroupEntry group, PathSettlerImprovementEntry improvement)
        {
            lock (improvementLock)
            {
                if (improvementCreatures.TryGetValue(improvement.Id, out uint creatureId))
                    return creatureId;

                creaturesByActivateSpell ??= GameTableManager.Instance.Creature2.Entries
                    .Where(c => c?.Spell4IdActivate != null)
                    .SelectMany(c => c.Spell4IdActivate.Where(s => s != 0u).Distinct().Select(s => (Spell: s, Creature: c)))
                    .ToLookup(t => t.Spell, t => t.Creature);

                TextTable text = GameTableManager.Instance.GetTextTable(Language.English);
                string groupName = text?.GetEntry(group.LocalizedTextIdName);

                Creature2Entry creature = creaturesByActivateSpell[improvement.Spell4IdDisplay]
                    .OrderByDescending(c => groupName != null && StripPlural(text.GetEntry(c.LocalizedTextIdName) ?? "") == groupName)
                    .ThenByDescending(c => c.Description?.Contains("Settler Template") ?? false)
                    .ThenBy(c => c.Id)
                    .FirstOrDefault();
                creatureId = creature?.Id ?? 0u;

                if (creatureId == 0u)
                {
                    Spell4Entry spell = GameTableManager.Instance.Spell4.GetEntry(improvement.Spell4IdDisplay);
                    Spell4BaseEntry spellBase = spell != null ? GameTableManager.Instance.Spell4Base.GetEntry(spell.Spell4BaseIdBaseSpell) : null;
                    string name = spellBase != null ? text?.GetEntry(spellBase.LocalizedTextIdName) : null;
                    if (name != null)
                        improvementCreaturesByDisplayName.TryGetValue(name, out creatureId);
                }

                improvementCreatures.Add(improvement.Id, creatureId);
                return creatureId;
            }
        }

        /// <summary>
        /// Return the position of an improvement's unit, improvements of a depot often share one display point and are placed around it.
        /// </summary>
        private Vector3? GetImprovementPosition(PathSettlerImprovementGroupEntry group, out float yaw)
        {
            yaw = 0f;
            WorldLocation2Entry location = GameTableManager.Instance.WorldLocation2.GetEntry(group.WorldLocation2IdDisplayPoint);
            if (location == null)
                return null;

            var point = new Vector3(location.Position0, location.Position1, location.Position2);
            List<PathSettlerImprovementGroupEntry> shared = GameTableManager.Instance.PathSettlerImprovementGroup.Entries
                .Where(g => g != null && g.WorldLocation2IdDisplayPoint == group.WorldLocation2IdDisplayPoint)
                .OrderBy(g => g.Id)
                .ToList();
            if (shared.Count <= 1)
                return point;

            float angle = MathF.PI * 2f * shared.FindIndex(g => g.Id == group.Id) / shared.Count;
            float x = point.X + MathF.Cos(angle) * ImprovementSpacing;
            float z = point.Z + MathF.Sin(angle) * ImprovementSpacing;
            float y = player.Map.GetTerrainHeight(x, z) ?? point.Y;

            // face the display point
            yaw = MathF.Atan2(point.X - x, point.Z - z);
            return new Vector3(x, y, z);
        }

        /// <summary>
        /// Place the improvement's unit in the world until the improvement expires, contributions extend its time.
        /// </summary>
        private void SpawnImprovement(PathSettlerImprovementGroupEntry group, PathSettlerImprovementEntry improvement, ImprovementState state)
        {
            uint creatureId = GetImprovementCreatureId(group, improvement);
            if (creatureId == 0u)
            {
                log.Debug($"Settler improvement {improvement.Id} of group {group.Id} has no unit to place.");
                return;
            }

            IBaseMap map = player.Map;
            TimeSpan remaining = state.Expires - DateTime.UtcNow;

            lock (improvementLock)
            {
                if (state.SpawnPending)
                    return;

                // already in the world, only the time changes
                if (state.SpawnMap == map && state.SpawnCreatureId == creatureId && map.GetEntity<IWorldEntity>(state.SpawnGuid) != null)
                {
                    map.CancelDespawn(state.SpawnGuid);
                    map.ScheduleDespawn(state.SpawnGuid, remaining);
                    return;
                }

                // a different tier has a different unit
                if (state.SpawnMap == map)
                    map.GetEntity<IWorldEntity>(state.SpawnGuid)?.RemoveFromMap();
            }

            ICreatureInfo creatureInfo = LegacyServiceProvider.Provider.GetService<ICreatureInfoManager>().GetCreatureInfo(creatureId);
            Vector3? position = GetImprovementPosition(group, out float yaw);
            if (creatureInfo == null || position == null)
                return;

            EntityType type = creatureInfo.Entry.CreationTypeEnum == EntityType.Player ? EntityType.SimpleCollidable : creatureInfo.Entry.CreationTypeEnum;
            IWorldEntity unit = LegacyServiceProvider.Provider.GetService<IEntityFactory>().CreateWorldEntity(type);
            if (unit == null)
                return;

            unit.Initialise(creatureInfo);
            unit.Rotation = new Vector3(yaw, 0f, 0f);

            lock (improvementLock)
            {
                state.SpawnPending    = true;
                state.SpawnMap        = map;
                state.SpawnCreatureId = creatureId;
            }

            // the guid is only known once the map adds the unit
            unit.AddToMap(map, position.Value, (m, guid, vector) =>
            {
                lock (improvementLock)
                {
                    state.SpawnPending = false;
                    state.SpawnGuid    = guid;
                }

                TimeSpan left = state.Expires - DateTime.UtcNow;
                m.ScheduleDespawn(guid, left > TimeSpan.Zero ? left : TimeSpan.Zero);
            });

            log.Trace($"Placed settler improvement {improvement.Id} unit {creatureId} for group {group.Id} for {remaining}.");
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

        private static IEnumerable<(IPathMission Mission, PathSettlerInfrastructureEntry Entry)> GetInfrastructureMissions(IPlayer player)
        {
            foreach (IPathMission mission in player.PathMissionManager
                .Where(m => !m.Completed && m.Type == PathMissionType.SettlerInfrastructure)
                .ToList())
            {
                PathSettlerInfrastructureEntry entry = GameTableManager.Instance.PathSettlerInfrastructure.GetEntry(mission.Entry.ObjectId);
                if (entry != null)
                    yield return (mission, entry);
            }
        }

        // Creature2Affiliation of settler world objects
        private const uint AffiliationSettlerDepot     = 152u;
        private const uint AffiliationSettlerResource  = 159u;
        private const uint AffiliationSettlerStructure = 332u;

        /// <summary>
        /// Time before a collected settler resource or a built settler structure is available again.
        /// </summary>
        /// <remarks>
        /// Retail values are unknown, the client data has no respawn times.
        /// </remarks>
        private static readonly TimeSpan SettlerObjectRespawnDelay = TimeSpan.FromMinutes(3d);

        private static Dictionary<string, uint> resourceItems;

        /// <summary>
        /// Settler resource nodes are named after the hub resource item they give, e.g. "Salvaged Machine Parts".
        /// </summary>
        private static uint GetResourceItemId(uint creatureId)
        {
            lock (improvementLock)
            {
                TextTable text = GameTableManager.Instance.GetTextTable(Language.English);
                if (resourceItems == null)
                {
                    resourceItems = new Dictionary<string, uint>(StringComparer.OrdinalIgnoreCase);
                    foreach (PathSettlerHubEntry hub in GameTableManager.Instance.PathSettlerHub.Entries.Where(h => h != null))
                    {
                        foreach (uint itemId in new[] { hub.Item2IdResource00, hub.Item2IdResource01, hub.Item2IdResource02 })
                        {
                            Item2Entry item = itemId != 0u ? GameTableManager.Instance.Item.GetEntry(itemId) : null;
                            string name = item != null ? StripPlural(text?.GetEntry(item.LocalizedTextIdName)) : null;
                            if (!string.IsNullOrEmpty(name))
                                resourceItems.TryAdd(name, itemId);
                        }
                    }
                }

                Creature2Entry creature = GameTableManager.Instance.Creature2.GetEntry(creatureId);
                string creatureName = creature != null ? StripPlural(text?.GetEntry(creature.LocalizedTextIdName)) : null;
                return creatureName != null && resourceItems.TryGetValue(creatureName, out uint resourceItemId) ? resourceItemId : 0u;
            }
        }

        private static string StripPlural(string name)
        {
            if (name == null)
                return null;

            int plural = name.IndexOf('{');
            return (plural >= 0 ? name[..plural] : name).Trim();
        }

        public void OnEntityActivated(IWorldEntity entity)
        {
            if (player.Path != Path.Settler)
                return;

            switch (entity.CreatureInfo?.Entry?.Creature2AffiliationId ?? 0u)
            {
                // resource nodes give the hub resource used to build improvements
                case AffiliationSettlerResource:
                {
                    uint itemId = GetResourceItemId(entity.CreatureId);
                    if (itemId != 0u)
                    {
                        player.Inventory.ItemCreate(InventoryLocation.Inventory, itemId, 1u, ItemUpdateReason.Loot);
                        entity.DespawnWithRespawn(SettlerObjectRespawnDelay);
                    }
                    break;
                }
                // building a settler structure (minfrastructure) is worth a path mission reward
                case AffiliationSettlerStructure:
                {
                    if (BuildStructure(entity))
                        player.PathManager.AddXp(GameTableManager.Instance.GameFormula.GetEntry(378)?.Dataint0 ?? 50u);
                    break;
                }
            }

            foreach ((IPathMission mission, PathSettlerInfrastructureEntry entry) in GetInfrastructureMissions(player))
            {
                uint[] resources = [entry.Creature2IdResource00, entry.Creature2IdResource01, entry.Creature2IdResource02];
                uint[] spells    = [entry.Spell4IdResource00, entry.Spell4IdResource01, entry.Spell4IdResource02];
                uint[] maxStacks = [entry.MaxStackCountResource00, entry.MaxStackCountResource01, entry.MaxStackCountResource02];
                for (int i = 0; i < resources.Length; i++)
                {
                    if (resources[i] == 0u || resources[i] != entity.CreatureId)
                        continue;

                    carried.TryGetValue((entry.Id, i), out uint count);
                    if (count >= Math.Max(1u, maxStacks[i]))
                        continue;

                    carried[(entry.Id, i)] = count + 1u;

                    // the resource spell shows the carried stack
                    if (spells[i] != 0u)
                        player.CastSpell(spells[i], new SpellParameters());
                }
            }
        }

        /// <summary>
        /// Time a settler structure stays built before it falls back into disrepair.
        /// </summary>
        /// <remarks>
        /// Retail values are unknown, the client data has no durations for settler structures.
        /// </remarks>
        private static readonly TimeSpan SettlerStructureBuiltTime = TimeSpan.FromMinutes(15d);

        /// <summary>
        /// A broken or unbuilt settler structure is replaced by its built version for a while, e.g. a broken banner becomes an Exile banner.
        /// </summary>
        private bool BuildStructure(IWorldEntity structure)
        {
            uint builtCreatureId = AssetManager.Instance.GetBuiltSettlerStructure(structure.CreatureId);
            if (builtCreatureId == 0u)
                return false;

            ICreatureInfo creatureInfo = LegacyServiceProvider.Provider.GetService<ICreatureInfoManager>().GetCreatureInfo(builtCreatureId);
            if (creatureInfo == null)
                return false;

            // built banners use creation type 20 in the client data, their world spawns are simple collidable units
            EntityType type = creatureInfo.Entry.CreationTypeEnum == EntityType.Player ? EntityType.SimpleCollidable : creatureInfo.Entry.CreationTypeEnum;
            IWorldEntity built = LegacyServiceProvider.Provider.GetService<IEntityFactory>().CreateWorldEntity(type);
            if (built == null)
                return false;

            IBaseMap map = structure.Map;
            Vector3 position = structure.Position;
            built.Initialise(creatureInfo);
            built.Rotation = structure.Rotation;

            structure.DespawnWithRespawn(SettlerStructureBuiltTime);
            built.AddToMap(map, position, (m, guid, vector) => m.ScheduleDespawn(guid, SettlerStructureBuiltTime));

            log.Trace($"Player {player.CharacterId} built settler structure {structure.CreatureId} into {builtCreatureId}.");
            return true;
        }

        public void OnEntityInteract(IWorldEntity entity)
        {
            if (player.Path != Path.Settler)
                return;

            // depots open the build window whichever way the client interacts with them
            if (GetDepotGroups(entity.CreatureId).Any())
            {
                OpenDepot(entity);
                return;
            }

            foreach ((IPathMission mission, PathSettlerInfrastructureEntry entry) in GetInfrastructureMissions(player))
            {
                if (entry.Creature2IdDepot == 0u || entry.Creature2IdDepot != entity.CreatureId)
                    continue;

                if ((DateTime.UtcNow - lastContribution).TotalSeconds < InfrastructureContributeCooldown)
                    return;

                uint contribution = TakeInfrastructureResources(entry);
                if (contribution == 0u)
                    continue;

                lastContribution = DateTime.UtcNow;
                ContributeInfrastructure(mission, entry, contribution);
            }
        }

        /// <summary>
        /// Resources for a project are the carried resource creatures, projects without resource creatures take the hub resource item.
        /// </summary>
        private uint TakeInfrastructureResources(PathSettlerInfrastructureEntry entry)
        {
            uint[] resources = [entry.Creature2IdResource00, entry.Creature2IdResource01, entry.Creature2IdResource02];
            if (resources.Any(r => r != 0u))
            {
                uint total = 0u;
                for (int i = 0; i < resources.Length; i++)
                {
                    if (carried.Remove((entry.Id, i), out uint count))
                        total += count;
                }
                return total;
            }

            PathSettlerHubEntry hub = GameTableManager.Instance.PathSettlerHub.GetEntry(entry.PathSettlerHubId00);
            if (hub == null || hub.Item2IdResource00 == 0u)
                return 1u;

            if (!HasResource(hub.Item2IdResource00, 1u))
                return 0u;

            TakeResource(hub.Item2IdResource00, 1u);
            return 1u;
        }

        private void ContributeInfrastructure(IPathMission mission, PathSettlerInfrastructureEntry entry, uint contribution)
        {
            InfrastructureState state;
            bool started = false, built = false;
            lock (improvementLock)
            {
                var key = (player.Map.Entry.Id, entry.Id);
                if (!infrastructure.TryGetValue(key, out state))
                {
                    state = new InfrastructureState();
                    infrastructure.Add(key, state);
                }

                // an unfinished project resets after its maximum time
                if (!state.IsBuilt && state.Count > 0u && entry.MaxTime > 0u && DateTime.UtcNow > state.Started.AddMilliseconds(entry.MaxTime))
                    state.Count = 0u;

                if (!state.IsBuilt)
                {
                    if (state.Count == 0u)
                    {
                        state.Started = DateTime.UtcNow;
                        started = true;
                    }

                    state.Count += contribution;
                    if (entry.Count > 0u && state.Count >= entry.Count)
                    {
                        state.Count      = 0u;
                        state.BuiltUntil = DateTime.UtcNow.AddMilliseconds(Math.Max(entry.MaxTime, 3600000u));
                        built = true;
                    }
                }
            }

            if (started)
                player.Session.EnqueueMessageEncrypted(new ServerPathSettlerInfrastructureStarted());

            player.Session.EnqueueMessageEncrypted(new ServerPathSettlerInfrastructureUpdated
            {
                PathMissionId   = mission.Id,
                State           = state.IsBuilt ? SettlerInfrastructureState.Built : SettlerInfrastructureState.Building,
                PercentComplete = state.IsBuilt || entry.Count == 0u ? 1f : (float)state.Count / entry.Count
            });

            if (built)
                player.Session.EnqueueMessageEncrypted(new ServerPathSettlerInfrastructureComplete());

            // the mission needs the player's own contributions
            uint progress = Math.Min(Math.Max(1u, entry.MissionCount), mission.Progress + contribution);
            player.PathMissionManager.UpdateMission(mission, progress, mission.State);
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
