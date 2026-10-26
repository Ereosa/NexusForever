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
}
