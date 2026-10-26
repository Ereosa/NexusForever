using NexusForever.Game.Abstract.Entity.Creature;
using NexusForever.GameTable;
using NexusForever.Script.Template;
using NexusForever.Script.Template.Filter;

namespace NexusForever.Script.Olyssia.Quest
{
    // Server-only wave events of quest steps (objective type 25) whose reward pane target group lists the attackers.

    /// <summary>
    /// Dark Moodie Magic: "Eliminate Ancient Skeletons in caverns near Deadwind Hollow".
    /// </summary>
    /// <remarks>
    /// The objective's reward pane lists the attackers: Ancient Skeleton 23775, Ancient Skeleton 23790.
    /// Wave sizes are guessed.
    /// </remarks>
    [ScriptFilterOwnerId(5501)]
    public class DarkMoodieMagicQuestScript : QuestWaveEventScript
    {
        protected override IReadOnlyList<(uint ObjectiveId, uint[][] Waves)> Events { get; } =
        [
            (16341, [[23775, 23775], [23775, 23790], [23790, 23790]])
        ];

        public DarkMoodieMagicQuestScript(IGameTableManager gameTableManager, ICreatureInfoManager creatureInfoManager)
            : base(gameTableManager, creatureInfoManager)
        {
        }
    }

    /// <summary>
    /// Hostile Negotiations: "Defeat the waves of Falkrin near Bloodtalon Perch".
    /// </summary>
    /// <remarks>
    /// The objective's reward pane lists the attackers: Bloodtalon Fighter 50877 (never spawned), Bloodtalon Warrior 50878 (never spawned), Bloodtalon Guardian 50880 (never spawned), Bloodtalon Apostle 50881 (never spawned), Bloodtalon Stormbringer 50882 (never spawned).
    /// Wave sizes are guessed.
    /// </remarks>
    [ScriptFilterOwnerId(5511)]
    public class HostileNegotiationsQuestScript : QuestWaveEventScript
    {
        protected override IReadOnlyList<(uint ObjectiveId, uint[][] Waves)> Events { get; } =
        [
            (8175, [[50877, 50877], [50877, 50878], [50878, 50880, 50881]])
        ];

        public HostileNegotiationsQuestScript(IGameTableManager gameTableManager, ICreatureInfoManager creatureInfoManager)
            : base(gameTableManager, creatureInfoManager)
        {
        }
    }

    /// <summary>
    /// The Ground Beneath Your Feet: "Poke the (creature=33893) and draw out the Terminites inside the Malignant Burrow".
    /// </summary>
    /// <remarks>
    /// The objective's reward pane lists the attackers: Noxious Tyrant 27412 (never spawned), Slimy Gorger 28485 (never spawned).
    /// Wave sizes are guessed.
    /// </remarks>
    [ScriptFilterOwnerId(6331)]
    public class TheGroundBeneathYourFeetQuestScript : QuestWaveEventScript
    {
        protected override IReadOnlyList<(uint ObjectiveId, uint[][] Waves)> Events { get; } =
        [
            (9695, [[28485, 28485], [28485, 28485], [28485, 28485], [27412]])
        ];

        public TheGroundBeneathYourFeetQuestScript(IGameTableManager gameTableManager, ICreatureInfoManager creatureInfoManager)
            : base(gameTableManager, creatureInfoManager)
        {
        }
    }

    /// <summary>
    /// Field Study: "Activate the (creature=39208) in Twilight Grove".
    /// </summary>
    /// <remarks>
    /// The objective's reward pane lists the attackers: Corrupted Screecher 39204, Corrupted Devourer 39205, Twisted Monstrosity 39206, Corrupted Chaser 48385.
    /// Wave sizes are guessed.
    /// </remarks>
    [ScriptFilterOwnerId(7753)]
    public class FieldStudy7753QuestScript : QuestWaveEventScript
    {
        protected override IReadOnlyList<(uint ObjectiveId, uint[][] Waves)> Events { get; } =
        [
            (14398, [[39204, 39204], [39204, 48385], [48385, 39205], [39206]])
        ];

        public FieldStudy7753QuestScript(IGameTableManager gameTableManager, ICreatureInfoManager creatureInfoManager)
            : base(gameTableManager, creatureInfoManager)
        {
        }
    }

    /// <summary>
    /// Field Study: "Activate the (creature=39208) in Twilight Grove".
    /// </summary>
    /// <remarks>
    /// The objective's reward pane lists the attackers: Corrupted Screecher 39204, Corrupted Devourer 39205, Twisted Monstrosity 39206, Corrupted Chaser 48385.
    /// Wave sizes are guessed.
    /// </remarks>
    [ScriptFilterOwnerId(8075)]
    public class FieldStudy8075QuestScript : QuestWaveEventScript
    {
        protected override IReadOnlyList<(uint ObjectiveId, uint[][] Waves)> Events { get; } =
        [
            (14588, [[39204, 39204], [39204, 48385], [48385, 39205], [39206]])
        ];

        public FieldStudy8075QuestScript(IGameTableManager gameTableManager, ICreatureInfoManager creatureInfoManager)
            : base(gameTableManager, creatureInfoManager)
        {
        }
    }

    /// <summary>
    /// Experiment Extermination: "Kill (creature=49385) escaped experiments outside Fool's Hope".
    /// </summary>
    /// <remarks>
    /// The objective's reward pane lists the attackers: Lab Slank Specimen 49610 (never spawned), Chemically Infused Embryo 49611 (never spawned), Failed Experiment 49612 (never spawned).
    /// Wave sizes are guessed.
    /// </remarks>
    [ScriptFilterOwnerId(8914)]
    public class ExperimentExterminationQuestScript : QuestWaveEventScript
    {
        protected override IReadOnlyList<(uint ObjectiveId, uint[][] Waves)> Events { get; } =
        [
            (16011, [[49610, 49610], [49610, 49611], [49611, 49611], [49612]])
        ];

        public ExperimentExterminationQuestScript(IGameTableManager gameTableManager, ICreatureInfoManager creatureInfoManager)
            : base(gameTableManager, creatureInfoManager)
        {
        }
    }
}
