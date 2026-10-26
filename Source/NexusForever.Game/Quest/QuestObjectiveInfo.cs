using System.Collections.Immutable;
using System.Text.RegularExpressions;
using NexusForever.Game.Abstract.Quest;
using NexusForever.Game.Static;
using NexusForever.Game.Static.Quest;
using NexusForever.GameTable;
using NexusForever.GameTable.Model;

namespace NexusForever.Game.Quest
{
    public class QuestObjectiveInfo : IQuestObjectiveInfo
    {
        public uint Id => Entry.Id;
        public QuestObjectiveType Type => (QuestObjectiveType)Entry.Type;

        public QuestObjectiveEntry Entry { get; }

        /// <summary>
        /// Create a new <see cref="QuestObjectiveInfo"/> with supplied <see cref="QuestObjectiveEntry"/>.
        /// </summary>
        public QuestObjectiveInfo(QuestObjectiveEntry entry)
        {
            Entry = entry;
        }

        /// <summary>
        /// Quest objective is sequential, previous quest objective must be complete.
        /// </summary>
        public bool IsSequential()
        {
            return ((QuestObjectiveFlags)Entry.Flags & QuestObjectiveFlags.Sequential) != 0;
        }

        /// <summary>
        /// Quest objective is hidden until previous objective is complete.
        /// </summary>
        public bool IsHidden()
        {
            return ((QuestObjectiveFlags)Entry.Flags & QuestObjectiveFlags.Hidden) != 0;
        }

        /// <summary>
        /// Quest objective is optional, doesn't need to be complete for quest to be achieved.
        /// </summary>
        public bool IsOptional()
        {
            return ((QuestObjectiveFlags)Entry.Flags & QuestObjectiveFlags.Optional) != 0;
        }

        public bool HasUnknown0200()
        {
            return ((QuestObjectiveFlags)Entry.Flags & QuestObjectiveFlags.Unknown0200) != 0;
        }

        /// <summary>
        /// Short text openings of target-less activate objectives that ask to reach a place, retail completed these with server trigger volumes.
        /// </summary>
        private static readonly string[] areaVerbs =
        [
            "Go to", "Travel to", "Head to", "Head toward", "Return to", "Investigate", "Locate", "Find", "Enter", "Explore", "Search", "Reach", "Visit", "Escape", "Follow", "Attend", "Discover", "Learn the fate", "Learn more about the", "See all", "Base jump"
        ];

        private bool? isAreaObjective;
        private uint? listenCreatureId;

        /// <summary>
        /// Returns if the objective is completed by reaching its area, enter area objectives and target-less objectives asking to go somewhere.
        /// </summary>
        public bool IsAreaObjective()
        {
            if (Type == QuestObjectiveType.EnterArea)
                return true;

            isAreaObjective ??= IsTargetless()
                && areaVerbs.Any(v => GetShortText().StartsWith(v, StringComparison.OrdinalIgnoreCase))
                && GetAreaLocations().Any();
            return isAreaObjective.Value;
        }

        /// <summary>
        /// Return the creature a target-less "Listen to" objective names, 0 if it isn't one.
        /// </summary>
        /// <remarks>
        /// Retail completed these once the creature finished speaking to the player, here they complete once the player is close to the creature.
        /// </remarks>
        public uint GetListenCreatureId()
        {
            if (listenCreatureId == null)
            {
                listenCreatureId = 0u;
                string text = GetShortText();
                // "Meet $(creature=20306) on the hill": being with them is the meeting
                if (IsTargetless() && (text.StartsWith("Listen to", StringComparison.OrdinalIgnoreCase) || text.StartsWith("Meet ", StringComparison.OrdinalIgnoreCase)))
                {
                    Match match = Regex.Match(text, @"\$\w*\(creature=(\d+)\)");
                    if (match.Success)
                        listenCreatureId = uint.Parse(match.Groups[1].Value);
                }
            }

            return listenCreatureId.Value;
        }

        private uint? useCreatureId;
        private static readonly string[] useVerbs = { "Use the", "Open the", "Select the", "Use a", "Open a", "Squash" };

        /// <summary>
        /// Return the creature a target-less "Use/Open/Select the" objective names, 0 if it isn't one.
        /// </summary>
        /// <remarks>
        /// Tutorial objectives such as Touring Tremor Ridge ("Use the Taxi Kiosk", "Open the Mailbox", "Select the Rented Mount Vendor")
        /// have no target, retail scripts completed them when the player interacted with the creature the text names.
        /// </remarks>
        public uint GetUseCreatureId()
        {
            if (useCreatureId == null)
            {
                useCreatureId = 0u;
                string text = GetShortText();
                if (IsTargetless() && useVerbs.Any(v => text.StartsWith(v, StringComparison.OrdinalIgnoreCase)))
                {
                    Match match = Regex.Match(text, @"\$\w*\(creature=(\d+)\)");
                    if (match.Success)
                        useCreatureId = uint.Parse(match.Groups[1].Value);
                }
            }

            return useCreatureId.Value;
        }

        private uint? followCreatureId;

        /// <summary>
        /// Return the creature a target-less "Follow" objective names, 0 if it isn't one.
        /// </summary>
        /// <remarks>
        /// Retail scripts summoned the creature and moved it to the objective's marker, the objective completes on arriving there (<see cref="IsAreaObjective"/>).
        /// </remarks>
        public uint GetFollowCreatureId()
        {
            if (followCreatureId == null)
            {
                followCreatureId = 0u;
                string text = GetShortText();
                if (IsAreaObjective() && text.StartsWith("Follow", StringComparison.OrdinalIgnoreCase))
                {
                    Match match = Regex.Match(text, @"\$\w*\(creature=(\d+)\)");
                    if (match.Success)
                        followCreatureId = uint.Parse(match.Groups[1].Value);
                }
            }

            return followCreatureId.Value;
        }

        /// <remarks>
        /// Target-less activate objectives that aren't advanced by a quest object or reward pane target group.
        /// </remarks>
        private bool IsTargetless()
        {
            return Type == QuestObjectiveType.ActivateEntity
                && Entry.Data == 0u
                && Entry.TargetGroupIdRewardPane == 0u;
        }

        /// <summary>
        /// Return the faction whose name the text contains, the longest name for nested factions ("Algoroc Accord" over "Algoroc").
        /// </summary>
        private static uint? FindReputationFaction(string text)
        {
            TextTable textTable = GameTableManager.Instance.GetTextTable(Language.English);
            return GameTableManager.Instance.Faction2.Entries
                .Where(f => f != null)
                .Select(f => (f.Id, Name: Regex.Replace(textTable?.GetEntry(f.LocalizedTextIdName) ?? "", "^The ", "")))
                .Where(f => !string.IsNullOrWhiteSpace(f.Name) && f.Name.Length > 3 && text.Contains(f.Name, StringComparison.OrdinalIgnoreCase))
                .OrderByDescending(f => f.Name.Length)
                .Select(f => (uint?)f.Id)
                .FirstOrDefault();
        }

        private (uint CreatureId, string Command)? emoteTrigger;

        /// <summary>
        /// Return the creature and emote command of an objective asking for an emote before a creature
        /// ("Kneel before the $(creature=65411)", full text "Perform a /kneel before ..."), (0, null) if it isn't one.
        /// </summary>
        public (uint CreatureId, string Command) GetEmoteTrigger()
        {
            if (emoteTrigger == null)
            {
                emoteTrigger = (0u, null);
                if (Type == QuestObjectiveType.ActivateEntity && Entry.Data == 0u)
                {
                    TextTable textTable = GameTableManager.Instance.GetTextTable(Language.English);
                    string full = (Entry.LocalizedTextIdFull != 0u ? textTable?.GetEntry(Entry.LocalizedTextIdFull) : null) ?? "";
                    Match command = Regex.Match(full, @"\bPerform an? /(?<command>\w+)", RegexOptions.IgnoreCase);
                    Match creature = Regex.Match(GetShortText() + " " + full, @"\$\w*\(creature=(\d+)\)");
                    if (command.Success && creature.Success)
                        emoteTrigger = (uint.Parse(creature.Groups[1].Value), command.Groups["command"].Value.ToLowerInvariant());
                }
            }

            return emoteTrigger.Value;
        }

        private bool? activationAtMarker;

        /// <summary>
        /// Returns if the objective has no target of its own and is advanced by using something at its marker
        /// (eg: "Place protective wards on Elder Tree saplings", one objective per sapling).
        /// </summary>
        public bool IsActivationAtMarker()
        {
            activationAtMarker ??= Type is QuestObjectiveType.ActivateEntity or QuestObjectiveType.SucceedCSI
                && Entry.Data == 0u
                && Entry.TargetGroupIdRewardPane == 0u
                && GetTextTrigger().Kind == QuestTextTrigger.None
                && GetEmoteTrigger().CreatureId == 0u
                && !IsAreaObjective()
                && GetUseCreatureId() == 0u
                && GetListenCreatureId() == 0u
                && GetAreaLocations().Any(l => l.Radius <= 20f);
            return activationAtMarker.Value;
        }

        private string GetShortText()
        {
            TextTable textTable = GameTableManager.Instance.GetTextTable(Language.English);
            uint textId = Entry.LocalizedTextIdShort != 0u ? Entry.LocalizedTextIdShort : Entry.LocalizedTextIdFull;
            return (textId != 0u ? textTable?.GetEntry(textId) : null) ?? "";
        }

        private ImmutableList<WorldLocation2Entry> areaLocations;

        /// <summary>
        /// Return the world locations an enter area objective is completed at.
        /// </summary>
        /// <remarks>
        /// The objective data is a server trigger volume which isn't in the client data,
        /// the objective's map indicators (or quest direction locations) mark the area instead.
        /// </remarks>
        public IEnumerable<WorldLocation2Entry> GetAreaLocations()
        {
            if (areaLocations != null)
                return areaLocations;

            var builder = ImmutableList.CreateBuilder<WorldLocation2Entry>();
            foreach (uint locationId in new[] { Entry.WorldLocationsIdIndicator00, Entry.WorldLocationsIdIndicator01, Entry.WorldLocationsIdIndicator02, Entry.WorldLocationsIdIndicator03 })
            {
                WorldLocation2Entry location = GameTableManager.Instance.WorldLocation2.GetEntry(locationId);
                if (location != null)
                    builder.Add(location);
            }

            if (builder.Count == 0 && Entry.QuestDirectionId != 0u)
            {
                QuestDirectionEntry direction = GameTableManager.Instance.QuestDirection.GetEntry(Entry.QuestDirectionId);
                if (direction != null)
                {
                    foreach (uint directionEntryId in new[]
                    {
                        direction.QuestDirectionEntryId00, direction.QuestDirectionEntryId01, direction.QuestDirectionEntryId02, direction.QuestDirectionEntryId03,
                        direction.QuestDirectionEntryId04, direction.QuestDirectionEntryId05, direction.QuestDirectionEntryId06, direction.QuestDirectionEntryId07,
                        direction.QuestDirectionEntryId08, direction.QuestDirectionEntryId09, direction.QuestDirectionEntryId10, direction.QuestDirectionEntryId11,
                        direction.QuestDirectionEntryId12, direction.QuestDirectionEntryId13, direction.QuestDirectionEntryId14, direction.QuestDirectionEntryId15
                    })
                    {
                        QuestDirectionEntryEntry directionEntry = GameTableManager.Instance.QuestDirectionEntry.GetEntry(directionEntryId);
                        WorldLocation2Entry location = directionEntry != null ? GameTableManager.Instance.WorldLocation2.GetEntry(directionEntry.WorldLocation2Id) : null;
                        if (location != null)
                            builder.Add(location);
                    }
                }
            }

            areaLocations = builder.ToImmutable();
            return areaLocations;
        }
    
        private (QuestTextTrigger Kind, uint Id, string Name)? textTrigger;

        /// <summary>
        /// Return what a target-less activate objective asks for when its text names it (equip or buy an item, learn about a creature,
        /// complete a public event, adventure, dungeon, expedition or quest).
        /// </summary>
        /// <remarks>
        /// Retail scripts completed these, the short text is the only data naming the thing to do (eg: "Equip the $(item=12631)").
        /// </remarks>
        public (QuestTextTrigger Kind, uint Id, string Name) GetTextTrigger()
        {
            if (textTrigger != null)
                return textTrigger.Value;

            textTrigger = (QuestTextTrigger.None, 0u, null);
            if (!IsTargetless())
                return textTrigger.Value;

            string text = GetShortText().Trim();
            Match item = Regex.Match(text, @"\$\w*\(item=(\d+)\)");
            Match creature = Regex.Match(text, @"\$\w*\(creature=(\d+)\)");

            if (item.Success && text.StartsWith("Equip", StringComparison.OrdinalIgnoreCase))
                textTrigger = (QuestTextTrigger.EquipItem, uint.Parse(item.Groups[1].Value), null);
            else if (item.Success && (text.StartsWith("Buy", StringComparison.OrdinalIgnoreCase) || text.StartsWith("Purchase", StringComparison.OrdinalIgnoreCase)))
                textTrigger = (QuestTextTrigger.BuyItem, uint.Parse(item.Groups[1].Value), null);
            else if (creature.Success && text.StartsWith("Learn more about", StringComparison.OrdinalIgnoreCase))
                textTrigger = (QuestTextTrigger.TalkTo, uint.Parse(creature.Groups[1].Value), null);
            else if (Regex.Match(text, @"^(Complete|Participate in) an? (?<zone>.*?)\s*(Public )?Event\b", RegexOptions.IgnoreCase) is { Success: true } publicEvent)
                // id 1: taking part is enough, otherwise the event must be won
                textTrigger = (QuestTextTrigger.PublicEvent, publicEvent.Groups[1].Value.StartsWith("Participate", StringComparison.OrdinalIgnoreCase) ? 1u : 0u, publicEvent.Groups["zone"].Value.Trim());
            // a text naming a place rather than a faction ("Earn Celestion Reputation") is the reputation the quest itself rewards (id 0)
            else if (Regex.IsMatch(text, @"^(Gain|Earn)\b.*\bReputation\b", RegexOptions.IgnoreCase))
                textTrigger = (QuestTextTrigger.Reputation, FindReputationFaction(text) ?? 0u, null);
            else if (Regex.Match(text, @"^(Hunt|Slay|Kill) an? (animal|beast|creature)\b.*?\bnear an? (?<name>.+?)\s*$", RegexOptions.IgnoreCase) is { Success: true } hunt)
                textTrigger = (QuestTextTrigger.HuntNear, 0u, hunt.Groups["name"].Value);
            // internal script steps (no text, or developer notes such as "Dorian Despawned", "Other Objectives completed; wait 2 seconds"),
            // retail scripts completed them right after the step before
            else if (text.Length == 0 || Regex.IsMatch(text, @"(Despawned$|^Other Objectives completed|^Vis prerequisites|^Virtual Item Remove|^Detonation Delay$)", RegexOptions.IgnoreCase))
                textTrigger = (QuestTextTrigger.Wait, 3u, null);
            // retail scripts played these out (an NPC drinking, a test running), they are waited out
            else if (Regex.IsMatch(text, @"^(Wait (for|as|until)|Observe|Watch|Remain in|Listen to the)\b", RegexOptions.IgnoreCase))
                textTrigger = (QuestTextTrigger.Wait, text.StartsWith("Remain", StringComparison.OrdinalIgnoreCase) ? 20u : 10u, null);
            else if (Regex.Match(text, @"^Complete (the )?(?<name>[^$]+?)\s*$", RegexOptions.IgnoreCase) is { Success: true } content)
                textTrigger = (QuestTextTrigger.CompleteContent, 0u, content.Groups["name"].Value);

            return textTrigger.Value;
        }
    }
}
