using NexusForever.Game.Abstract.Entity.Creature;
using NexusForever.GameTable;
using NexusForever.Script.Template;
using NexusForever.Script.Template.Filter;

namespace NexusForever.Script.Main.Quest
{
    // Server-only wave events of quest steps (objective type 25) whose reward pane target group lists the attackers.

    /// <summary>
    /// Bubble Blockade: "Locate and defend the (creature=49306) while they retrieve data from the Temporal Quarantine 01.".
    /// </summary>
    /// <remarks>
    /// The objective's reward pane lists the attackers: Corpseweb Spinner 49598 (never spawned). The Field Researcher 49306 of the pane is the one defended.
    /// Wave sizes are guessed.
    /// </remarks>
    [ScriptFilterOwnerId(8889)]
    public class BubbleBlockadeQuestScript : QuestWaveEventScript
    {
        protected override IReadOnlyList<(uint ObjectiveId, uint[][] Waves)> Events { get; } =
        [
            (15964, [[49598, 49598], [49598, 49598], [49598, 49598]])
        ];

        public BubbleBlockadeQuestScript(IGameTableManager gameTableManager, ICreatureInfoManager creatureInfoManager)
            : base(gameTableManager, creatureInfoManager)
        {
        }
    }

    /// <summary>
    /// Establishing Base Perimeters (Northern Wastes): "Defend yourself against intruders" at the landing site once its defenses are up.
    /// </summary>
    /// <remarks>
    /// The soldier events (343 Exile, 344 Dominion) have no client data and no creature is tagged with the quest. The Augmented
    /// survivors that roam the Northern Wastes pocket cap (Augmented Scout 33998, Brawler 33999, Trooper 34070, Soldier 34064) stand in.
    /// Waves guessed.
    /// </remarks>
    public abstract class EstablishingBasePerimetersQuestScript : QuestWaveEventScript
    {
        protected static uint[][] IntruderWaves => [[33998, 33999], [33998, 34070], [33999, 34070, 34064]];

        protected EstablishingBasePerimetersQuestScript(IGameTableManager gameTableManager, ICreatureInfoManager creatureInfoManager)
            : base(gameTableManager, creatureInfoManager)
        {
        }
    }

    [ScriptFilterOwnerId(7085)]
    public class EstablishingBasePerimetersExileQuestScript : EstablishingBasePerimetersQuestScript
    {
        protected override IReadOnlyList<(uint ObjectiveId, uint[][] Waves)> Events { get; } = [(13058, IntruderWaves)];

        public EstablishingBasePerimetersExileQuestScript(IGameTableManager gameTableManager, ICreatureInfoManager creatureInfoManager)
            : base(gameTableManager, creatureInfoManager)
        {
        }
    }

    [ScriptFilterOwnerId(7107)]
    public class EstablishingBasePerimetersDominionQuestScript : EstablishingBasePerimetersQuestScript
    {
        protected override IReadOnlyList<(uint ObjectiveId, uint[][] Waves)> Events { get; } = [(13067, IntruderWaves)];

        public EstablishingBasePerimetersDominionQuestScript(IGameTableManager gameTableManager, ICreatureInfoManager creatureInfoManager)
            : base(gameTableManager, creatureInfoManager)
        {
        }
    }
}
