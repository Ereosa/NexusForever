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
}
