using System.Reflection;
using System.Text;
using NexusForever.Game.Prerequisite;
using NexusForever.Game.Quest;
using NexusForever.Game.Static.Prerequisite;
using NexusForever.Game.Static.Quest;
using NexusForever.Game.Static.Spell;
using NexusForever.GameTable;
using NexusForever.GameTable.Model;

namespace NexusForever.QuestAudit
{
    /// <summary>
    /// Audits every quest in the client tables against what the server supports: objective types, prerequisite types,
    /// how the quest is started and handed in, and the communicator triggers that deliver it.
    /// </summary>
    /// <remarks>
    /// Usage: NexusForever.QuestAudit --tbl &lt;tbl dir&gt; [--spawned &lt;file&gt;] [--out &lt;report.md&gt;] [--quests &lt;quests.tsv&gt;]
    /// The spawned file lists one "creatureId worldId" pair per line (select creature, world from entity), without it spawns aren't checked.
    /// </remarks>
    public static class Program
    {
        private static readonly uint[] creatureDataTypes = [2, 3, 5, 9, 12, 21];
        private static readonly uint[] targetGroupDataTypes = [8, 14, 16, 18, 23];
        // objective types whose data is legitimately 0 (counted events rather than targets)
        private static readonly uint[] noDataTypes = [19, 37, 39, 41, 47, 48];
        // objective types credited through their reward pane target group when they have no data (QuestManager.ObjectiveUpdateRewardPane)
        private static readonly uint[] rewardPaneTypes = [2, 3, 5, 9, 10, 12];

        private static GameTable<Quest2Entry> quests;
        private static GameTable<QuestObjectiveEntry> objectives;
        private static GameTable<PrerequisiteEntry> prerequisites;
        private static GameTable<TargetGroupEntry> targetGroups;
        private static TextTable text;
        private static readonly Dictionary<string, int> textHandled = [];

        // target-less activate objectives the server handles by their short text (QuestObjectiveInfo: area verbs, Listen to, Follow, Use/Open/Select)
        private static readonly string[] areaVerbs = ["Go to", "Travel to", "Head to", "Head toward", "Return to", "Investigate", "Locate", "Find", "Enter", "Explore", "Search", "Reach", "Visit", "Escape", "Follow"];

        private static bool TextHandled(QuestObjectiveEntry objective, out string how)
        {
            how = null;
            uint textId = objective.LocalizedTextIdShort != 0u ? objective.LocalizedTextIdShort : objective.LocalizedTextIdFull;
            string t = textId != 0u ? text.GetEntry(textId) ?? "" : "";
            bool creature = System.Text.RegularExpressions.Regex.IsMatch(t, @"\$\w*\(creature=\d+\)");
            bool marker = objective.WorldLocationsIdIndicator00 != 0u || objective.QuestDirectionId != 0u;
            if (t.StartsWith("Listen to", StringComparison.OrdinalIgnoreCase) && creature) how = "Listen to";
            else if (t.StartsWith("Follow", StringComparison.OrdinalIgnoreCase) && creature && marker) how = "Follow";
            else if ((t.StartsWith("Use the", StringComparison.OrdinalIgnoreCase) || t.StartsWith("Open the", StringComparison.OrdinalIgnoreCase) || t.StartsWith("Select the", StringComparison.OrdinalIgnoreCase)) && creature) how = "Use/Open/Select";
            else if (marker && areaVerbs.Any(v => t.StartsWith(v, StringComparison.OrdinalIgnoreCase))) how = "Area verb";
            return how != null;
        }

        private static HashSet<PrerequisiteType> handledPrerequisites;
        private static Dictionary<uint, HashSet<uint>> spawned;

        public static int Main(string[] args)
        {
            string tbl     = GetArg(args, "--tbl") ?? @"E:\Wildstar Server\data\tbl";
            string out_    = GetArg(args, "--out");
            string questsOut = GetArg(args, "--quests");
            string spawnedFile = GetArg(args, "--spawned");

            quests        = new GameTable<Quest2Entry>(Path.Combine(tbl, "Quest2.tbl"));
            objectives    = new GameTable<QuestObjectiveEntry>(Path.Combine(tbl, "QuestObjective.tbl"));
            prerequisites = new GameTable<PrerequisiteEntry>(Path.Combine(tbl, "Prerequisite.tbl"));
            targetGroups  = new GameTable<TargetGroupEntry>(Path.Combine(tbl, "TargetGroup.tbl"));
            var creatures     = new GameTable<Creature2Entry>(Path.Combine(tbl, "Creature2.tbl"));
            var communicators = new GameTable<CommunicatorMessagesEntry>(Path.Combine(tbl, "CommunicatorMessages.tbl"));
            var items         = new GameTable<Item2Entry>(Path.Combine(tbl, "Item2.tbl"));
            var pathRewards   = new GameTable<PathRewardEntry>(Path.Combine(tbl, "PathReward.tbl"));
            var spellEffects  = new GameTable<Spell4EffectsEntry>(Path.Combine(tbl, "Spell4Effects.tbl"));
            text = new TextTable(Path.Combine(tbl, "en-US.bin"));

            handledPrerequisites = typeof(PrerequisiteManager).Assembly.GetTypes()
                .SelectMany(t => t.GetCustomAttributes<PrerequisiteCheckAttribute>())
                .Select(a => a.Type)
                .ToHashSet();

            if (spawnedFile != null)
            {
                spawned = new Dictionary<uint, HashSet<uint>>();
                foreach (string line in File.ReadLines(spawnedFile))
                {
                    string[] parts = line.Split(['\t', ' ', ','], StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length < 2 || !uint.TryParse(parts[0], out uint creatureId) || !uint.TryParse(parts[1], out uint worldId))
                        continue;
                    if (!spawned.TryGetValue(creatureId, out HashSet<uint> worlds))
                        spawned.Add(creatureId, worlds = []);
                    worlds.Add(worldId);
                }
            }

            // relations
            var givers = new Dictionary<uint, List<Creature2Entry>>();
            var receivers = new Dictionary<uint, List<Creature2Entry>>();
            var questAnimObjects = new Dictionary<(uint, uint), List<Creature2Entry>>();
            var creatureById = new Dictionary<uint, Creature2Entry>();
            foreach (Creature2Entry creature in creatures.Entries.Where(c => c != null))
            {
                creatureById[creature.Id] = creature;
                foreach (uint questId in creature.QuestIdGiven.Where(q => q != 0u))
                    Add(givers, questId, creature);
                foreach (uint questId in creature.QuestIdReceive.Where(q => q != 0u))
                    Add(receivers, questId, creature);
                if (creature.QuestAnimStateId != 0u)
                    Add(questAnimObjects, (creature.QuestAnimStateId, creature.QuestAnimObjectiveIndex), creature);
            }

            var deliveringMessages = new Dictionary<uint, List<CommunicatorMessagesEntry>>();
            foreach (CommunicatorMessagesEntry message in communicators.Entries.Where(c => c != null && c.QuestIdDelivered != 0u))
                Add(deliveringMessages, message.QuestIdDelivered, message);

            var activationItems = items.Entries.Where(i => i != null && i.Quest2IdActivation != 0u)
                .GroupBy(i => i.Quest2IdActivation).ToDictionary(g => g.Key, g => g.ToList());
            // objectives advanced directly by a spell effect (quest objects, quest items), DataBits00 quest, DataBits01 objective index
            var spellAdvancedObjectives = spellEffects.Entries
                .Where(e => e != null && e.EffectType == SpellEffectType.QuestAdvanceObjective)
                .Select(e => (e.DataBits00, e.DataBits01))
                .ToHashSet();

            var pathRewardQuests = pathRewards.Entries.Where(p => p != null && p.Quest2Id != 0u).Select(p => p.Quest2Id).ToHashSet();

            // per objective type and per prerequisite type statistics
            var objectiveStats = new Dictionary<uint, ObjectiveStat>();
            var prerequisiteStats = new Dictionary<PrerequisiteType, PrerequisiteStat>();
            var triggerStats = new Dictionary<string, int>();
            var targetGroupTypeStats = new Dictionary<uint, (int Objectives, HashSet<uint> Quests)>();
            var results = new List<QuestResult>();

            foreach (Quest2Entry quest in quests.Entries.Where(q => q != null))
            {
                var result = new QuestResult
                {
                    Quest = quest,
                    Title = text.GetEntry(quest.LocalizedTextIdTitle) ?? ""
                };

                // start paths
                List<Creature2Entry> questGivers = givers.GetValueOrDefault(quest.Id) ?? [];
                List<CommunicatorMessagesEntry> messages = deliveringMessages.GetValueOrDefault(quest.Id) ?? [];
                bool item = activationItems.ContainsKey(quest.Id);
                bool pathReward = pathRewardQuests.Contains(quest.Id);

                if (questGivers.Count > 0)
                    result.StartPaths.Add(IsSpawned(questGivers) ? "giver" : "giver(unspawned)");
                if (item)
                    result.StartPaths.Add("item");
                if (pathReward)
                    result.StartPaths.Add("path-reward");
                foreach (CommunicatorMessagesEntry message in messages)
                {
                    string trigger = GetTrigger(message, quest.Id);
                    triggerStats[trigger] = triggerStats.GetValueOrDefault(trigger) + 1;
                    result.StartPaths.Add($"communicator({trigger})");
                    CollectPrerequisites(message.PrerequisiteId, result.PrerequisiteTypes, []);
                }

                bool startable = (questGivers.Count > 0 && (spawned == null || IsSpawned(questGivers)))
                    || item
                    || pathReward
                    || messages.Any(m => IsTriggerSupported(GetTrigger(m, quest.Id)));
                result.Live = result.StartPaths.Count > 0 && IsLiveTitle(result.Title);
                if (!startable)
                {
                    // a giver exists but isn't in the world database
                    string blocker = result.StartPaths.Count == 0 ? "no start path" : "start: " + string.Join(",", result.StartPaths);
                    result.Blockers.Add(questGivers.Count > 0 ? "spawn: " + blocker : blocker);
                }

                // hand in paths
                List<Creature2Entry> questReceivers = receivers.GetValueOrDefault(quest.Id) ?? [];
                bool autoComplete = (quest.Flags & 0x01) != 0u;
                bool communicatorReceived = (quest.Flags & 0x08) != 0u;
                bool handIn = autoComplete || communicatorReceived || (questReceivers.Count > 0 && (spawned == null || IsSpawned(questReceivers)));
                if (!handIn)
                    result.Blockers.Add(questReceivers.Count == 0 ? "no hand in" : "spawn: receiver unspawned");

                // prerequisites
                CollectPrerequisites(quest.PrerequisiteId, result.PrerequisiteTypes, []);

                // objectives
                for (byte index = 0; index < quest.Objectives.Length; index++)
                {
                    uint objectiveId = quest.Objectives[index];
                    if (objectiveId == 0u)
                        continue;

                    QuestObjectiveEntry objective = objectives.GetEntry(objectiveId);
                    if (objective == null)
                        continue;

                    if (!objectiveStats.TryGetValue(objective.Type, out ObjectiveStat stat))
                        objectiveStats.Add(objective.Type, stat = new ObjectiveStat());
                    stat.Objectives++;
                    stat.Quests.Add(quest.Id);

                    QuestSupportLevel level = QuestObjectiveSupport.GetLevel((QuestObjectiveType)objective.Type);
                    bool hidden = (objective.Flags & 0x08) != 0u;
                    bool optional = (objective.Flags & 0x20) != 0u;

                    List<Creature2Entry> animObjects = questAnimObjects.GetValueOrDefault((quest.Id, index)) ?? [];
                    bool spellAdvanced = spellAdvancedObjectives.Contains((quest.Id, index));
                    if (spellAdvanced)
                        stat.SpellAdvanced++;

                    if (objective.Data == 0u && !noDataTypes.Contains(objective.Type))
                    {
                        stat.DataZero++;
                        if (animObjects.Count > 0)
                            stat.DataZeroAnimObject++;
                        else if (spellAdvanced)
                            stat.DataZeroSpell++;
                        else if (objective.TargetGroupIdRewardPane != 0u && rewardPaneTypes.Contains(objective.Type))
                            stat.DataZeroPane++;
                        else if (objective.Type == 5 && TextHandled(objective, out string how))
                        {
                            stat.DataZeroText++;
                            textHandled[how] = textHandled.GetValueOrDefault(how) + 1;
                        }
                        else if (hidden && objective.LocalizedTextIdFull == 0u)
                            stat.DataZeroServer++;
                        else
                        {
                            stat.DataZeroNoTarget++;
                            if (!optional)
                                result.Blockers.Add($"objective {index} type {objective.Type} has no target");
                        }
                    }
                    else if (level != QuestSupportLevel.Supported && !optional && !spellAdvanced)
                        result.Blockers.Add($"objective {index} type {(QuestObjectiveType)objective.Type} {level}");

                    if (level == QuestSupportLevel.Partial)
                        result.Partial = true;

                    // target creatures, their visibility and activation prerequisites gate the objective
                    var targets = new List<uint>();
                    if (creatureDataTypes.Contains(objective.Type))
                        targets.Add(objective.Data);
                    uint targetGroupId = targetGroupDataTypes.Contains(objective.Type) ? objective.Data : objective.TargetGroupIdRewardPane;
                    if (targetGroupId != 0u)
                        CollectTargetGroup(targetGroupId, targets, targetGroupTypeStats, quest.Id, []);
                    targets.AddRange(animObjects.Select(c => c.Id));

                    foreach (uint creatureId in targets.Distinct())
                    {
                        if (!creatureById.TryGetValue(creatureId, out Creature2Entry creature))
                            continue;
                        CollectPrerequisites(creature.PrerequisiteIdVisibility, result.PrerequisiteTypes, []);
                        foreach (uint prerequisiteId in creature.PrerequisiteIdActivateSpells)
                            CollectPrerequisites(prerequisiteId, result.PrerequisiteTypes, []);
                    }

                    if (spawned != null && targets.Count > 0 && !targets.Any(spawned.ContainsKey) && !optional
                        && objective.Type is not (4 or 32 or 33 or 38))
                        result.Blockers.Add($"spawn: objective {index} targets unspawned");
                }

                foreach (PrerequisiteType type in result.PrerequisiteTypes)
                {
                    if (!prerequisiteStats.TryGetValue(type, out PrerequisiteStat stat))
                        prerequisiteStats.Add(type, stat = new PrerequisiteStat());
                    stat.Quests.Add(quest.Id);
                    if (!handledPrerequisites.Contains(type))
                        result.Blockers.Add($"prerequisite {type} unhandled");
                }

                results.Add(result);
            }

            string report = BuildReport(results, objectiveStats, prerequisiteStats, triggerStats, targetGroupTypeStats);
            if (out_ != null)
                File.WriteAllText(out_, report);
            else
                Console.Write(report);

            if (questsOut != null)
            {
                var sb = new StringBuilder("id\ttitle\tlive\tstatus\tstart\tblockers\n");
                foreach (QuestResult result in results)
                    sb.Append($"{result.Quest.Id}\t{result.Title}\t{result.Live}\t{result.Status}\t{string.Join(",", result.StartPaths)}\t{string.Join("; ", result.Blockers.Distinct())}\n");
                File.WriteAllText(questsOut, sb.ToString());
            }

            return 0;
        }

        private static string BuildReport(List<QuestResult> results, Dictionary<uint, ObjectiveStat> objectiveStats,
            Dictionary<PrerequisiteType, PrerequisiteStat> prerequisiteStats, Dictionary<string, int> triggerStats,
            Dictionary<uint, (int Objectives, HashSet<uint> Quests)> targetGroupTypeStats)
        {
            var live = results.Where(r => r.Live).ToList();
            var sb = new StringBuilder();
            sb.AppendLine("# Quest coverage audit");
            sb.AppendLine();
            sb.AppendLine($"Generated {DateTime.UtcNow:yyyy-MM-dd HH:mm} UTC. Spawn check: {(spawned != null ? "on" : "off")}.");
            sb.AppendLine();
            sb.AppendLine($"Live quests (a title and a start path): {live.Count} of {results.Count} in Quest2.");
            sb.AppendLine();
            sb.AppendLine("Server support, ignoring missing world database spawns:");
            sb.AppendLine();
            foreach (string status in new[] { "Supported", "Partial", "Blocked" })
            {
                int count = live.Count(r => r.CodeStatus == status);
                sb.AppendLine($"- {status}: {count} ({100d * count / live.Count:0.0}%)");
            }
            sb.AppendLine();
            sb.AppendLine($"Playable with the current world database spawns{(spawned == null ? " (spawn check off)" : "")}:");
            sb.AppendLine();
            foreach (string status in new[] { "Supported", "Partial", "Blocked" })
            {
                int count = live.Count(r => r.Status == status);
                sb.AppendLine($"- {status}: {count} ({100d * count / live.Count:0.0}%)");
            }
            sb.AppendLine();

            sb.AppendLine("## Blockers by quest count (live quests, \"spawn:\" = missing from the world database)");
            sb.AppendLine();
            sb.AppendLine("| Blocker | Quests | Only blocker |");
            sb.AppendLine("|---|---|---|");
            foreach (var group in live
                .SelectMany(r => r.Blockers.Select(Generalise).Distinct().Select(b => (Blocker: b, Result: r)))
                .GroupBy(p => p.Blocker)
                .OrderByDescending(g => g.Count()))
            {
                int only = group.Count(p => p.Result.Blockers.Select(Generalise).Distinct().Count() == 1);
                sb.AppendLine($"| {group.Key} | {group.Count()} | {only} |");
            }
            sb.AppendLine();

            sb.AppendLine("## Objective types");
            sb.AppendLine();
            sb.AppendLine("| Type | Name | Support | Hook | Objectives | Quests | Spell advanced | Data 0 | Data 0 via quest object | Data 0 via spell | Data 0 via reward pane | Data 0 via text | Data 0 server-only | Data 0 no target |");
            sb.AppendLine("|---|---|---|---|---|---|---|---|---|---|---|---|---|---|");
            foreach ((uint type, ObjectiveStat stat) in objectiveStats.OrderByDescending(s => s.Value.Quests.Count))
            {
                QuestObjectiveSupport.Types.TryGetValue((QuestObjectiveType)type, out var support);
                sb.AppendLine($"| {type} | {(QuestObjectiveType)type} | {QuestObjectiveSupport.GetLevel((QuestObjectiveType)type)} | {support.Hook} | {stat.Objectives} | {stat.Quests.Count} | {stat.SpellAdvanced} | {stat.DataZero} | {stat.DataZeroAnimObject} | {stat.DataZeroSpell} | {stat.DataZeroPane} | {stat.DataZeroText} | {stat.DataZeroServer} | {stat.DataZeroNoTarget} |");
            }
            sb.AppendLine();

            sb.AppendLine("Target-less activate objectives handled by their text: " + string.Join(", ", textHandled.OrderByDescending(t => t.Value).Select(t => $"{t.Key} {t.Value}")));
            sb.AppendLine();

            sb.AppendLine("## Prerequisite types reachable from quests, communicator messages and objective targets");
            sb.AppendLine();
            sb.AppendLine("| Type | Handled | Quests |");
            sb.AppendLine("|---|---|---|");
            foreach ((PrerequisiteType type, PrerequisiteStat stat) in prerequisiteStats.OrderByDescending(s => s.Value.Quests.Count))
                sb.AppendLine($"| {(uint)type} {type} | {(handledPrerequisites.Contains(type) ? "yes" : "**no**")} | {stat.Quests.Count} |");
            sb.AppendLine();

            sb.AppendLine("## Communicator messages that deliver a quest, by trigger");
            sb.AppendLine();
            sb.AppendLine("| Trigger | Messages | Supported |");
            sb.AppendLine("|---|---|---|");
            foreach ((string trigger, int count) in triggerStats.OrderByDescending(s => s.Value))
                sb.AppendLine($"| {trigger} | {count} | {(IsTriggerSupported(trigger) ? "yes" : "**no**")} |");
            sb.AppendLine();

            sb.AppendLine("## Unhandled target group types used by objectives");
            sb.AppendLine();
            sb.AppendLine("| TargetGroup type | Objectives | Quests |");
            sb.AppendLine("|---|---|---|");
            foreach ((uint type, var stat) in targetGroupTypeStats.OrderByDescending(s => s.Value.Quests.Count))
                sb.AppendLine($"| {type} {(TargetGroupType)type} | {stat.Objectives} | {stat.Quests.Count} |");

            return sb.ToString();
        }

        private static string Generalise(string blocker)
        {
            // "objective 2 type 22 ..." -> "objective type 22 ..."
            string[] parts = blocker.Split(' ');
            if (parts.Length > 2 && parts[0] == "objective" && uint.TryParse(parts[1], out _))
                return "objective " + string.Join(' ', parts.Skip(2));
            if (blocker.StartsWith("start: "))
                return "start: " + string.Join(",", blocker[7..].Split(',').Distinct().Order());
            return blocker;
        }

        private static string GetTrigger(CommunicatorMessagesEntry message, uint questId)
        {
            if (message.Quests.Any(q => q != 0u && q != questId))
                return "QuestState";
            if (message.WorldZoneId != 0u)
                return "Zone";
            if (message.WorldId != 0u)
                return "World";
            if (message.MinLevel != 0u || message.MaxLevel != 0u)
                return "Level";
            return message.PrerequisiteId != 0u ? "Prerequisite" : "None";
        }

        private static bool IsTriggerSupported(string trigger)
        {
            return QuestObjectiveSupport.CommunicatorTriggers.TryGetValue(trigger, out QuestSupportLevel level)
                && level != QuestSupportLevel.Unsupported;
        }

        private static void CollectPrerequisites(uint prerequisiteId, HashSet<PrerequisiteType> types, HashSet<uint> visited)
        {
            if (prerequisiteId == 0u || !visited.Add(prerequisiteId))
                return;

            PrerequisiteEntry entry = prerequisites.GetEntry(prerequisiteId);
            if (entry == null)
                return;

            for (int i = 0; i < entry.PrerequisiteTypeId.Length; i++)
            {
                PrerequisiteType type = entry.PrerequisiteTypeId[i];
                if (type == PrerequisiteType.None)
                    continue;

                types.Add(type);
                if (type == PrerequisiteType.OtherPrerequisite)
                    CollectPrerequisites(entry.ObjectId[i], types, visited);
            }
        }

        private static void CollectTargetGroup(uint targetGroupId, List<uint> targets, Dictionary<uint, (int, HashSet<uint>)> unhandled, uint questId, HashSet<uint> visited)
        {
            if (!visited.Add(targetGroupId))
                return;

            TargetGroupEntry entry = targetGroups.GetEntry(targetGroupId);
            if (entry == null)
                return;

            switch ((TargetGroupType)entry.Type)
            {
                case TargetGroupType.CreatureIdGroup:
                case TargetGroupType.Unknown9:
                    targets.AddRange(entry.DataEntries.Where(d => d != 0u));
                    break;
                case TargetGroupType.OtherTargetGroup:
                case TargetGroupType.OtherTargetGroupCreatures:
                    foreach (uint id in entry.DataEntries.Where(d => d != 0u))
                        CollectTargetGroup(id, targets, unhandled, questId, visited);
                    break;
                // matched against the creature's faction or race on kill (AssetManager)
                case TargetGroupType.FactionIdGroup:
                case TargetGroupType.CreatureRaceIdGroup:
                    break;
                default:
                    (int count, HashSet<uint> questIds) = unhandled.GetValueOrDefault(entry.Type, (0, []));
                    questIds.Add(questId);
                    unhandled[entry.Type] = (count + 1, questIds);
                    break;
            }
        }

        private static bool IsSpawned(IEnumerable<Creature2Entry> creatures)
        {
            return spawned == null || creatures.Any(c => spawned.ContainsKey(c.Id));
        }

        private static bool IsLiveTitle(string title)
        {
            if (string.IsNullOrWhiteSpace(title))
                return false;

            string lower = title.ToLowerInvariant();
            return !lower.Contains("test") && !lower.Contains("[ph]") && !lower.Contains("deprecated") && !lower.StartsWith("zz") && !lower.Contains("(old)");
        }

        private static void Add<TKey, TValue>(Dictionary<TKey, List<TValue>> dictionary, TKey key, TValue value)
        {
            if (!dictionary.TryGetValue(key, out List<TValue> list))
                dictionary.Add(key, list = []);
            list.Add(value);
        }

        private static string GetArg(string[] args, string name)
        {
            int index = Array.IndexOf(args, name);
            return index >= 0 && index + 1 < args.Length ? args[index + 1] : null;
        }

        private class ObjectiveStat
        {
            public int Objectives;
            public HashSet<uint> Quests = [];
            public int DataZero;
            public int DataZeroAnimObject;
            public int DataZeroSpell;
            public int DataZeroPane;
            public int DataZeroText;
            public int SpellAdvanced;
            public int DataZeroServer;
            public int DataZeroNoTarget;
        }

        private class PrerequisiteStat
        {
            public HashSet<uint> Quests = [];
        }

        private class QuestResult
        {
            public Quest2Entry Quest;
            public string Title;
            public bool Live;
            public bool Partial;
            public List<string> StartPaths = [];
            public List<string> Blockers = [];
            public HashSet<PrerequisiteType> PrerequisiteTypes = [];

            public string Status => Blockers.Count > 0 ? "Blocked" : Partial ? "Partial" : "Supported";

            /// <summary>
            /// Status ignoring blockers caused by missing world database spawns.
            /// </summary>
            public string CodeStatus => Blockers.Any(b => !b.StartsWith("spawn: ")) ? "Blocked" : Partial ? "Partial" : "Supported";
        }
    }
}
