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
            "Go to", "Travel to", "Head to", "Head toward", "Return to", "Investigate", "Locate", "Find", "Enter", "Explore", "Search", "Reach", "Visit", "Escape", "Follow"
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
                if (IsTargetless() && text.StartsWith("Listen to", StringComparison.OrdinalIgnoreCase))
                {
                    Match match = Regex.Match(text, @"\$\w*\(creature=(\d+)\)");
                    if (match.Success)
                        listenCreatureId = uint.Parse(match.Groups[1].Value);
                }
            }

            return listenCreatureId.Value;
        }

        private uint? useCreatureId;
        private static readonly string[] useVerbs = { "Use the", "Open the", "Select the", "Use a", "Open a" };

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
    }
}
