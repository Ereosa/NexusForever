using NexusForever.Game.Abstract.Entity.Creature;
using NexusForever.GameTable;
using NexusForever.Script.Template;
using NexusForever.Script.Template.Filter;

namespace NexusForever.Script.Farside.Quest
{
    // Server-only wave events of quest steps (objective type 25) whose reward pane target group lists the attackers.

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
    /// Showdown With Harlock: "Subdue Corporal Harlock at Arcanus Base".
    /// </summary>
    /// <remarks>
    /// Corporal Harlock 27395 is the hostile variant (faction 371, never spawned) that comes out to fight, the step completes once he is beaten.
    /// </remarks>
    [ScriptFilterOwnerId(6159)]
    public class ShowdownWithHarlockQuestScript : QuestWaveEventScript
    {
        protected override IReadOnlyList<(uint ObjectiveId, uint[][] Waves)> Events { get; } =
        [
            (9933, [[27395]])
        ];

        public ShowdownWithHarlockQuestScript(IGameTableManager gameTableManager, ICreatureInfoManager creatureInfoManager)
            : base(gameTableManager, creatureInfoManager)
        {
        }
    }

    /// <summary>
    /// Showdown With Paxton: "Subdue Private Paxton at Arcanus Base".
    /// </summary>
    /// <remarks>
    /// Private Paxton 29321 is the hostile variant (faction 371, never spawned) that comes out to fight.
    /// </remarks>
    [ScriptFilterOwnerId(6463)]
    public class ShowdownWithPaxtonQuestScript : QuestWaveEventScript
    {
        protected override IReadOnlyList<(uint ObjectiveId, uint[][] Waves)> Events { get; } =
        [
            (9972, [[29321]])
        ];

        public ShowdownWithPaxtonQuestScript(IGameTableManager gameTableManager, ICreatureInfoManager creatureInfoManager)
            : base(gameTableManager, creatureInfoManager)
        {
        }
    }
}
