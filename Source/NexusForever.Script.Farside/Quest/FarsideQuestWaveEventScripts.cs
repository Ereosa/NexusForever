using NexusForever.Game.Abstract.Entity.Creature;
using NexusForever.GameTable;
using NexusForever.Script.Template;
using NexusForever.Script.Template.Filter;

namespace NexusForever.Script.Farside.Quest
{
    // Server-only wave events of quest steps (objective type 25) whose reward pane target group lists the attackers.

    /// <summary>
    /// To the Dishonor: "Fight waves of Darktide Ikthians while (creature=44134) duels (creature=28039) near the Plasmic Equalizer".
    /// </summary>
    /// <remarks>
    /// The objective's reward pane lists the attackers: Darktide Swarmer 28505 (never spawned), Darktide Bodyguard 28508 (never spawned).
    /// Wave sizes are guessed.
    /// </remarks>
    [ScriptFilterOwnerId(6164)]
    public class ToTheDishonor6164QuestScript : QuestWaveEventScript
    {
        protected override IReadOnlyList<(uint ObjectiveId, uint[][] Waves)> Events { get; } =
        [
            (9480, [[28505, 28505], [28505, 28508], [28508, 28508]])
        ];

        public ToTheDishonor6164QuestScript(IGameTableManager gameTableManager, ICreatureInfoManager creatureInfoManager)
            : base(gameTableManager, creatureInfoManager)
        {
        }
    }

    /// <summary>
    /// To the Dishonor: "Fight waves of Darktide Ikthians while (creature=27957) duels (creature=28039) near the Plasmic Equalizer".
    /// </summary>
    /// <remarks>
    /// The objective's reward pane lists the attackers: Darktide Swarmer 28505 (never spawned), Darktide Bodyguard 28508 (never spawned).
    /// Wave sizes are guessed.
    /// </remarks>
    [ScriptFilterOwnerId(6496)]
    public class ToTheDishonor6496QuestScript : QuestWaveEventScript
    {
        protected override IReadOnlyList<(uint ObjectiveId, uint[][] Waves)> Events { get; } =
        [
            (10063, [[28505, 28505], [28505, 28508], [28508, 28508]])
        ];

        public ToTheDishonor6496QuestScript(IGameTableManager gameTableManager, ICreatureInfoManager creatureInfoManager)
            : base(gameTableManager, creatureInfoManager)
        {
        }
    }

    /// <summary>
    /// Element of Surprise: "Defend against the Darkspur Cartel's ambush at the Dustrunner crash site".
    /// </summary>
    /// <remarks>
    /// Darkspur Mugger 28948, Swindler 28120, Doubledealer 28119 and Safecracker 28950 (levels 31-32, never spawned). Guessed.
    /// </remarks>
    [ScriptFilterOwnerId(6179)]
    public class ElementOfSurprise6179QuestScript : QuestWaveEventScript
    {
        protected override IReadOnlyList<(uint ObjectiveId, uint[][] Waves)> Events { get; } =
        [
            (9513, [[28948, 28120], [28119, 28950], [28948, 28119, 28950]])
        ];

        public ElementOfSurprise6179QuestScript(IGameTableManager gameTableManager, ICreatureInfoManager creatureInfoManager)
            : base(gameTableManager, creatureInfoManager)
        {
        }
    }

    /// <summary>
    /// Element of Surprise: the Dominion version of the Dustrunner ambush.
    /// </summary>
    /// <remarks>
    /// As 6179. Guessed.
    /// </remarks>
    [ScriptFilterOwnerId(6471)]
    public class ElementOfSurprise6471QuestScript : QuestWaveEventScript
    {
        protected override IReadOnlyList<(uint ObjectiveId, uint[][] Waves)> Events { get; } =
        [
            (9988, [[28948, 28120], [28119, 28950], [28948, 28119, 28950]])
        ];

        public ElementOfSurprise6471QuestScript(IGameTableManager gameTableManager, ICreatureInfoManager creatureInfoManager)
            : base(gameTableManager, creatureInfoManager)
        {
        }
    }

    /// <summary>
    /// Out of Orbit: "Defend Zangu and his camp from the attacking Aetherwings above the Gamma Fields".
    /// </summary>
    /// <remarks>
    /// Aetherwing Cutter 29570 and Skimmer 30074 (level 50, never spawned, created with Zangu 29565). Guessed.
    /// </remarks>
    [ScriptFilterOwnerId(6505)]
    public class OutOfOrbitQuestScript : QuestWaveEventScript
    {
        protected override IReadOnlyList<(uint ObjectiveId, uint[][] Waves)> Events { get; } =
        [
            (10082, [[29570, 29570], [29570, 30074], [30074, 30074]])
        ];

        public OutOfOrbitQuestScript(IGameTableManager gameTableManager, ICreatureInfoManager creatureInfoManager)
            : base(gameTableManager, creatureInfoManager)
        {
        }
    }
}
