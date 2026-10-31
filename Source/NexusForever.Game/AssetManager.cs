using System.Collections.Immutable;
using System.Reflection;
using NexusForever.Database;
using NexusForever.Database.Character;
using NexusForever.Database.World;
using NexusForever.Database.World.Model;
using NexusForever.Game.Abstract;
using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Entity;
using NexusForever.Game.Static;
using NexusForever.Game.Static.Entity;
using NexusForever.Game.Static.Quest;
using NexusForever.Game.Static.Reward;
using NexusForever.GameTable;
using NexusForever.GameTable.Model;
using NexusForever.Script.Template.Filter.Dynamic;
using NexusForever.Shared;
using NLog;

namespace NexusForever.Game
{
    public sealed class AssetManager : Singleton<AssetManager>, IAssetManager
    {
        private static readonly ILogger log = LogManager.GetCurrentClassLogger();

        public static ImmutableDictionary<InventoryLocation, uint> InventoryLocationCapacities { get; private set; }

        /// <summary>
        /// Id to be assigned to the next created mail.
        /// </summary>
        public ulong NextMailId => nextMailId++;

        private ulong nextMailId;

        private ImmutableDictionary<uint, ImmutableList<ItemDisplaySourceEntryEntry>> itemDisplaySourcesEntry;

        private ImmutableDictionary</*zoneId*/uint, /*tutorialId*/uint> zoneTutorials;
        private ImmutableDictionary</*creatureId*/uint, /*targetGroupIds*/ImmutableList<uint>> creatureAssociatedTargetGroups;
        private ImmutableDictionary</*creatureId*/uint, /*virtualItemIds*/ImmutableList<uint>> creatureVirtualCollectItems;
        private ImmutableHashSet<(uint CreatureId, uint VirtualItemId)> guaranteedVirtualCollectItems;
        private ImmutableDictionary</*targetGroupId*/uint, /*targetGroupIds*/ImmutableList<uint>> questObjectiveTargets;
        private ImmutableDictionary</*creatureId*/uint, /*pathScientistCreatureInfoIds*/ImmutableList<uint>> scientistCreatureInfos;
        private ImmutableDictionary</*pathSoldierEventId*/uint, ImmutableList<PathSoldierHoldoutSpawnModel>> holdoutSpawns;
        private ImmutableDictionary</*creatureId*/uint, /*builtCreatureId*/uint> settlerStructures;
        private ImmutableDictionary</*creatureId*/uint, PathExplorerSwitchModel> explorerSwitches;
        private ImmutableDictionary</*creatureId*/uint, IVendorInfo> creatureVendors;

        private ImmutableDictionary<AccountTier, ImmutableList<RewardPropertyPremiumModifierEntry>> rewardPropertiesByTier;

        public void Initialise()
        {
            nextMailId = DatabaseManager.Instance.GetDatabase<CharacterDatabase>().GetNextMailId() + 1ul;

            CacheInventoryBagCapacities();
            CacheItemDisplaySourceEntries();
            CacheTutorials();
            CacheCreatureTargetGroups();
            CacheCreatureVirtualCollectItems();
            CacheChecklistTargetGroups();
            CacheScientistCreatureInfos();
            CacheHoldoutSpawns();
            CacheSettlerStructures();
            CacheExplorerSwitches();
            CacheCreatureVendors();
            CacheRewardPropertiesByTier();
        }

        private void CacheInventoryBagCapacities()
        {
            var entries = new Dictionary<InventoryLocation, uint>();
            foreach (FieldInfo field in typeof(InventoryLocation).GetFields())
            {
                foreach (InventoryLocationAttribute attribute in field.GetCustomAttributes<InventoryLocationAttribute>())
                {
                    InventoryLocation location = (InventoryLocation)field.GetValue(null);
                    entries.Add(location, attribute.DefaultCapacity);
                }
            }

            InventoryLocationCapacities = entries.ToImmutableDictionary();
        }

        private void CacheItemDisplaySourceEntries()
        {
            var entries = new Dictionary<uint, List<ItemDisplaySourceEntryEntry>>();
            foreach (ItemDisplaySourceEntryEntry entry in GameTableManager.Instance.ItemDisplaySourceEntry.Entries)
            {
                if (!entries.ContainsKey(entry.ItemSourceId))
                    entries.Add(entry.ItemSourceId, new List<ItemDisplaySourceEntryEntry>());

                entries[entry.ItemSourceId].Add(entry);
            }

            itemDisplaySourcesEntry = entries.ToImmutableDictionary(e => e.Key, e => e.Value.ToImmutableList());
        }

        private void CacheTutorials()
        {
            var zoneEntries =  ImmutableDictionary.CreateBuilder<uint, uint>();
            foreach (TutorialModel tutorial in DatabaseManager.Instance.GetDatabase<WorldDatabase>().GetTutorialTriggers())
            {
                if (tutorial.TriggerId == 0) // Don't add Tutorials with no trigger ID
                    continue;

                if (tutorial.Type == 29 && !zoneEntries.ContainsKey(tutorial.TriggerId))
                    zoneEntries.Add(tutorial.TriggerId, tutorial.Id);
            }

            zoneTutorials = zoneEntries.ToImmutable();
        }

        private void CacheScientistCreatureInfos()
        {
            scientistCreatureInfos = DatabaseManager.Instance.GetDatabase<WorldDatabase>().GetPathScientistCreatures()
                .GroupBy(m => m.CreatureId)
                .ToImmutableDictionary(g => g.Key, g => g.Select(m => (uint)m.PathScientistCreatureInfoId).ToImmutableList());
        }

        private void CacheHoldoutSpawns()
        {
            holdoutSpawns = DatabaseManager.Instance.GetDatabase<WorldDatabase>().GetPathSoldierHoldoutSpawns()
                .GroupBy(m => (uint)m.PathSoldierEventId)
                .ToImmutableDictionary(g => g.Key, g => g.ToImmutableList());

            // the waves (not the units to defend, flag 0x02) fight the player, most of their creatures are NoAI in the game tables
            ScriptFilterDynamicCombatAI.AddCreatures(holdoutSpawns.Values
                .SelectMany(s => s)
                .Where(s => (s.Flags & 0x02) == 0)
                .Select(s => GameTableManager.Instance.Creature2.GetEntry(s.CreatureId))
                .Where(e => e != null && e.FactionId != 0u && !ScriptFilterDynamicCombatAI.IsEnvironmentalHazard(e)
                    && e.Creature2ArcheTypeId is not (25u or 27u or 29u))
                .Select(e => e.Id));
        }

        private void CacheSettlerStructures()
        {
            settlerStructures = DatabaseManager.Instance.GetDatabase<WorldDatabase>().GetPathSettlerStructures()
                .ToImmutableDictionary(m => m.CreatureId, m => m.BuiltCreatureId);
        }

        private void CacheCreatureVendors()
        {
            (ImmutableList<CreatureVendorModel> vendors, ImmutableList<CreatureVendorCategoryModel> categories, ImmutableList<CreatureVendorItemModel> items)
                = DatabaseManager.Instance.GetDatabase<WorldDatabase>().GetCreatureVendors();

            ILookup<uint, CreatureVendorCategoryModel> categoriesByCreature = categories.ToLookup(c => c.CreatureId);
            ILookup<uint, CreatureVendorItemModel> itemsByCreature = items.ToLookup(i => i.CreatureId);

            creatureVendors = vendors.ToImmutableDictionary(v => v.CreatureId,
                v => (IVendorInfo)new VendorInfo(v, categoriesByCreature[v.CreatureId], itemsByCreature[v.CreatureId]));
        }

        public IVendorInfo GetCreatureVendor(uint creatureId)
        {
            return creatureVendors.TryGetValue(creatureId, out IVendorInfo vendorInfo) ? vendorInfo : null;
        }

        private void CacheExplorerSwitches()
        {
            explorerSwitches = DatabaseManager.Instance.GetDatabase<WorldDatabase>().GetPathExplorerSwitches()
                .ToImmutableDictionary(m => m.CreatureId);
        }

        public PathExplorerSwitchModel GetExplorerSwitch(uint creatureId)
        {
            return explorerSwitches.TryGetValue(creatureId, out PathExplorerSwitchModel model) ? model : null;
        }

        public uint GetBuiltSettlerStructure(uint creatureId)
        {
            return settlerStructures.TryGetValue(creatureId, out uint builtCreatureId) ? builtCreatureId : 0u;
        }

        public ImmutableList<PathSoldierHoldoutSpawnModel> GetHoldoutSpawns(uint pathSoldierEventId)
        {
            return holdoutSpawns.TryGetValue(pathSoldierEventId, out ImmutableList<PathSoldierHoldoutSpawnModel> spawns) ? spawns : ImmutableList<PathSoldierHoldoutSpawnModel>.Empty;
        }

        public ImmutableList<uint> GetScientistCreatureInfoIds(uint creatureId)
        {
            return scientistCreatureInfos.TryGetValue(creatureId, out ImmutableList<uint> infoIds) ? infoIds : ImmutableList<uint>.Empty;
        }

        private void CacheCreatureTargetGroups()
        {
            var entries = ImmutableDictionary.CreateBuilder<uint, List<uint>>();
            var factionGroups = new Dictionary<uint, List<uint>>();
            var raceGroups = new Dictionary<uint, List<uint>>();
            foreach (TargetGroupEntry entry in GameTableManager.Instance.TargetGroup.Entries)
            {
                switch ((TargetGroupType)entry.Type)
                {
                    // type 9 lists creature ids the same way (inferred from the data, eg: kill objective groups of creature variants)
                    case TargetGroupType.CreatureIdGroup:
                    case TargetGroupType.Unknown9:
                    {
                        foreach (uint creatureId in entry.DataEntries.Where(d => d != 0u))
                        {
                            if (!entries.ContainsKey(creatureId))
                                entries.Add(creatureId, new List<uint>());

                            entries[creatureId].Add(entry.Id);
                        }
                        break;
                    }
                    case TargetGroupType.FactionIdGroup:
                    {
                        foreach (uint factionId in entry.DataEntries.Where(d => d != 0u))
                        {
                            if (!factionGroups.ContainsKey(factionId))
                                factionGroups.Add(factionId, new List<uint>());

                            factionGroups[factionId].Add(entry.Id);
                        }
                        break;
                    }
                    case TargetGroupType.CreatureRaceIdGroup:
                    {
                        foreach (uint raceId in entry.DataEntries.Where(d => d != 0u))
                        {
                            if (!raceGroups.ContainsKey(raceId))
                                raceGroups.Add(raceId, new List<uint>());

                            raceGroups[raceId].Add(entry.Id);
                        }
                        break;
                    }
                }
            }

            // groups by faction or creature race (eg: "kill any Dominion", "kill any Skeech") apply to every creature of that faction or race
            if (factionGroups.Count > 0 || raceGroups.Count > 0)
            {
                foreach (Creature2Entry creature in GameTableManager.Instance.Creature2.Entries)
                {
                    IEnumerable<uint> groups = Enumerable.Empty<uint>();
                    if (factionGroups.TryGetValue((uint)creature.FactionId, out List<uint> factionGroupIds))
                        groups = groups.Concat(factionGroupIds);
                    if (raceGroups.TryGetValue(creature.UnitRaceId, out List<uint> raceGroupIds))
                        groups = groups.Concat(raceGroupIds);

                    foreach (uint targetGroupId in groups)
                    {
                        if (!entries.ContainsKey(creature.Id))
                            entries.Add(creature.Id, new List<uint>());

                        entries[creature.Id].Add(targetGroupId);
                    }
                }
            }

            // target groups can contain other target groups (eg: kill objectives covering several creature families),
            // a creature also belongs to every group above its own
            var parents = new Dictionary<uint, List<uint>>();
            foreach (TargetGroupEntry entry in GameTableManager.Instance.TargetGroup.Entries)
            {
                if ((TargetGroupType)entry.Type is not (TargetGroupType.OtherTargetGroup or TargetGroupType.OtherTargetGroupCreatures))
                    continue;

                foreach (uint childId in entry.DataEntries.Where(d => d != 0u))
                {
                    if (!parents.ContainsKey(childId))
                        parents.Add(childId, new List<uint>());

                    parents[childId].Add(entry.Id);
                }
            }

            foreach (List<uint> targetGroupIds in entries.Values)
            {
                var pending = new Queue<uint>(targetGroupIds);
                var seen    = new HashSet<uint>(targetGroupIds);
                while (pending.TryDequeue(out uint targetGroupId))
                {
                    if (!parents.TryGetValue(targetGroupId, out List<uint> parentIds))
                        continue;

                    foreach (uint parentId in parentIds)
                    {
                        if (!seen.Add(parentId))
                            continue;

                        targetGroupIds.Add(parentId);
                        pending.Enqueue(parentId);
                    }
                }
            }

            creatureAssociatedTargetGroups = entries.ToImmutableDictionary(e => e.Key, e => e.Value.ToImmutableList());
        }

        private void CacheCreatureVirtualCollectItems()
        {
            // the reward pane target group of a virtual collect objective lists the creatures the item is gathered from
            var entries = new Dictionary<uint, HashSet<uint>>();
            foreach (QuestObjectiveEntry objective in GameTableManager.Instance.QuestObjective.Entries)
            {
                if ((QuestObjectiveType)objective.Type != QuestObjectiveType.VirtualCollect || objective.TargetGroupIdRewardPane == 0u)
                    continue;

                TargetGroupEntry targetGroup = GameTableManager.Instance.TargetGroup.GetEntry(objective.TargetGroupIdRewardPane);
                if (targetGroup == null)
                    continue;

                // the pane can list other target groups (eg: A Plagued Forest, Barkblight Feeders and Biters)
                var creatureIds = new List<uint>();
                var unhandled = new List<TargetGroupType>();
                AddToTargets(targetGroup, ref creatureIds, ref unhandled);

                foreach (uint creatureId in creatureIds.Distinct())
                {
                    if (!entries.ContainsKey(creatureId))
                        entries.Add(creatureId, new HashSet<uint>());

                    entries[creatureId].Add(objective.Data);
                }
            }

            // objectives without a reward pane take the item from the quest's own kill target, eg: Queen of the Blight collects the
            // Barkblight Queen's Viscera from the Barkblight Queen it asks to kill (report 67), a single named kill always drops it
            var guaranteed = new HashSet<(uint, uint)>();
            foreach (Quest2Entry quest in GameTableManager.Instance.Quest2.Entries)
            {
                if (quest == null)
                    continue;

                List<QuestObjectiveEntry> objectives = quest.Objectives
                    .Where(o => o != 0u)
                    .Select(o => GameTableManager.Instance.QuestObjective.GetEntry(o))
                    .Where(o => o != null)
                    .ToList();

                foreach (QuestObjectiveEntry objective in objectives.Where(o => (QuestObjectiveType)o.Type == QuestObjectiveType.VirtualCollect
                    && o.TargetGroupIdRewardPane == 0u))
                {
                    foreach (QuestObjectiveEntry kill in objectives.Where(o => (QuestObjectiveType)o.Type == QuestObjectiveType.KillCreature && o.Data != 0u))
                    {
                        if (!entries.ContainsKey(kill.Data))
                            entries.Add(kill.Data, new HashSet<uint>());

                        entries[kill.Data].Add(objective.Data);
                        if (kill.Count == 1u)
                            guaranteed.Add((kill.Data, objective.Data));
                    }
                }
            }

            creatureVirtualCollectItems   = entries.ToImmutableDictionary(e => e.Key, e => e.Value.ToImmutableList());
            guaranteedVirtualCollectItems = guaranteed.ToImmutableHashSet();
        }

        private void CacheRewardPropertiesByTier()
        {
            // VIP was intended to be used in China from what I can see, you can force the VIP premium system in the client with the China game mode parameter
            // not supported as the system was unfinished
            IEnumerable<RewardPropertyPremiumModifierEntry> hybridEntries = GameTableManager.Instance
                .RewardPropertyPremiumModifier.Entries
                .Where(e => (PremiumSystem)e.PremiumSystemEnum == PremiumSystem.Hybrid)
                .ToList();

            // base reward properties are determined by current account tier and lower if fall through flag is set
            rewardPropertiesByTier = hybridEntries
                .Select(e => e.Tier)
                .Distinct()
                .ToImmutableDictionary(k => (AccountTier)k, k => hybridEntries
                    .Where(r => r.Tier == k)
                    .Concat(hybridEntries
                        .Where(r => r.Tier < k && ((RewardPropertyPremiumModiferFlags)r.Flags & RewardPropertyPremiumModiferFlags.FallThrough) != 0))
                    .ToImmutableList());
        }

        private void AddToTargets(TargetGroupEntry entry, ref List<uint> targetIds, ref List<TargetGroupType> unhandledTargetGroups)
        {
            switch ((TargetGroupType)entry.Type)
            {
                case TargetGroupType.CreatureIdGroup:
                case TargetGroupType.Unknown9:
                    targetIds.AddRange(entry.DataEntries.Where(d => d != 0u));
                    break;
                case TargetGroupType.OtherTargetGroup:
                case TargetGroupType.OtherTargetGroupCreatures:
                    foreach (uint targetGroupId in entry.DataEntries.Where(d => d != 0u))
                    {
                        TargetGroupEntry targetGroup = GameTableManager.Instance.TargetGroup.GetEntry(targetGroupId);
                        if (targetGroup == null)
                            continue;

                        AddToTargets(targetGroup, ref targetIds, ref unhandledTargetGroups);
                    }
                    break;
                default:
                    if (!(unhandledTargetGroups.Contains((TargetGroupType)entry.Type)))
                        unhandledTargetGroups.Add((TargetGroupType)entry.Type);
                    break;
            }
        }

        private void CacheQuestObjectiveTargetGroups()
        {
            List<TargetGroupType> unhandledTargetGroups = new List<TargetGroupType>();

            var entries = ImmutableDictionary.CreateBuilder<uint, List<uint>>();
            foreach (QuestObjectiveEntry questObjectiveEntry in GameTableManager.Instance.QuestObjective.Entries
                .Where(o => o.TargetGroupIdRewardPane > 0u ||
                    (QuestObjectiveType)o.Type == QuestObjectiveType.ActivateTargetGroup ||
                    (QuestObjectiveType)o.Type == QuestObjectiveType.ActivateTargetGroupChecklist ||
                    (QuestObjectiveType)o.Type == QuestObjectiveType.KillTargetGroup ||
                    (QuestObjectiveType)o.Type == QuestObjectiveType.KillTargetGroups ||
                    (QuestObjectiveType)o.Type == QuestObjectiveType.TalkToTargetGroup))
            {
                uint targetGroupId = questObjectiveEntry.Data > 0 ? questObjectiveEntry.Data : questObjectiveEntry.TargetGroupIdRewardPane;
                if (targetGroupId == 0u)
                    continue;

                TargetGroupEntry targetGroup = GameTableManager.Instance.TargetGroup.GetEntry(targetGroupId);
                if (targetGroup == null)
                    continue;

                List<uint> targetIds = new List<uint>();
                AddToTargets(targetGroup, ref targetIds, ref unhandledTargetGroups);
                entries.Add(questObjectiveEntry.Id, targetIds);
            }

            questObjectiveTargets = entries.ToImmutableDictionary(e => e.Key, e => e.Value.ToImmutableList());

            string targetGroupTypes = "";
            foreach (TargetGroupType targetGroupType in unhandledTargetGroups)
                targetGroupTypes += targetGroupType.ToString() + " ";

            log.Warn($"Unhandled TargetGroup Types for Quest Objectives: {targetGroupTypes}");
        }

        /// <summary>
        /// Returns an <see cref="ImmutableList{T}"/> containing all <see cref="ItemDisplaySourceEntryEntry"/>'s for the supplied itemSource.
        /// </summary>
        public ImmutableList<ItemDisplaySourceEntryEntry> GetItemDisplaySource(uint itemSource)
        {
            return itemDisplaySourcesEntry.TryGetValue(itemSource, out ImmutableList<ItemDisplaySourceEntryEntry> entries) ? entries : null;
        }

        /// <summary>
        /// Returns a Tutorial ID if it's found in the Zone Tutorials cache
        /// </summary>
        public uint GetTutorialIdForZone(uint zoneId)
        {
            return zoneTutorials.TryGetValue(zoneId, out uint tutorialId) ? tutorialId : 0;
        }

        private ImmutableHashSet<uint> checklistTargetGroups;

        /// <summary>
        /// Target groups of checklist objectives (types 10, 14, 18 and 23), each entity of them sets its own QuestChecklistIdx bit.
        /// </summary>
        private void CacheChecklistTargetGroups()
        {
            checklistTargetGroups = GameTableManager.Instance.QuestObjective.Entries
                .Where(o => o != null && (QuestObjectiveType)o.Type is QuestObjectiveType.Unknown10
                    or QuestObjectiveType.ActivateTargetGroupChecklist
                    or QuestObjectiveType.TalkToTargetGroup
                    or QuestObjectiveType.ActivateTargetGroup)
                .SelectMany(o => new[] { o.Data, o.TargetGroupIdRewardPane })
                .Where(g => g != 0u)
                .ToImmutableHashSet();

            // counted objectives (type 10) whose data is a creature rather than a target group (eg: Firestorm Flingatrons)
            checklistCreatures = GameTableManager.Instance.QuestObjective.Entries
                .Where(o => o != null && (QuestObjectiveType)o.Type == QuestObjectiveType.Unknown10 && o.Data != 0u
                    && GameTableManager.Instance.TargetGroup.GetEntry(o.Data) == null && GameTableManager.Instance.Creature2.GetEntry(o.Data) != null)
                .Select(o => o.Data)
                .ToImmutableHashSet();
        }

        private ImmutableHashSet<uint> checklistCreatures;

        /// <summary>
        /// Returns if entities of the creature are targets of a checklist objective.
        /// </summary>
        public bool IsChecklistTarget(uint creatureId)
        {
            return GetTargetGroupsForCreatureId(creatureId).Any(checklistTargetGroups.Contains) || checklistCreatures.Contains(creatureId);
        }

        /// <summary>
        /// Returns an <see cref="ImmutableList{T}"/> containing all TargetGroup ID's associated with the creatureId.
        /// </summary>
        public ImmutableList<uint> GetTargetGroupsForCreatureId(uint creatureId)
        {
            return creatureAssociatedTargetGroups.TryGetValue(creatureId, out ImmutableList<uint> entries) ? entries : [];
        }

        /// <summary>
        /// Returns the virtual item ids that quest collect objectives say are gathered from the supplied creature id.
        /// </summary>
        public ImmutableList<uint> GetVirtualCollectItemsForCreatureId(uint creatureId)
        {
            return creatureVirtualCollectItems.TryGetValue(creatureId, out ImmutableList<uint> entries) ? entries : [];
        }

        /// <summary>
        /// Returns if the supplied virtual item always drops from the creature: the quest asks to kill that one creature and collect it.
        /// </summary>
        public bool IsGuaranteedVirtualCollectItem(uint creatureId, uint virtualItemId)
        {
            return guaranteedVirtualCollectItems.Contains((creatureId, virtualItemId));
        }

        /// <summary>
        /// Returns an <see cref="ImmutableList{T}"/> containing all target ID's associated with the questObjectiveId.
        /// </summary>
        public ImmutableList<uint> GetQuestObjectiveTargetIds(uint questObjectiveId)
        {
            return questObjectiveTargets.TryGetValue(questObjectiveId, out ImmutableList<uint> entries) ? entries : Enumerable.Empty<uint>().ToImmutableList();
        }

        /// <summary>
        /// Returns an <see cref="ImmutableList{T}"/> containing all <see cref="RewardPropertyPremiumModifierEntry"/> for the given <see cref="AccountTier"/>.
        /// </summary>
        public ImmutableList<RewardPropertyPremiumModifierEntry> GetRewardPropertiesForTier(AccountTier tier)
        {
            return rewardPropertiesByTier.TryGetValue(tier, out ImmutableList<RewardPropertyPremiumModifierEntry> entries) ? entries : ImmutableList<RewardPropertyPremiumModifierEntry>.Empty;
        }
    }
}
