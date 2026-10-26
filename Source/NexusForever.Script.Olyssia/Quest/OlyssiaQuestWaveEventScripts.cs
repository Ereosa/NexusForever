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
    /// Walking into a Trap: "Recover Dorian Walker's Datachron from the attacking pumera in the Undergrove".
    /// </summary>
    /// <remarks>
    /// Torine Pumera 40967 (levels 34-38, the Sisterhood's pumera of Wilderrun). Two waves, guessed.
    /// </remarks>
    [ScriptFilterOwnerId(6227)]
    public class WalkingIntoATrapQuestScript : QuestWaveEventScript
    {
        protected override IReadOnlyList<(uint ObjectiveId, uint[][] Waves)> Events { get; } =
        [
            (9498, [[40967, 40967], [40967, 40967, 40967]])
        ];

        public WalkingIntoATrapQuestScript(IGameTableManager gameTableManager, ICreatureInfoManager creatureInfoManager)
            : base(gameTableManager, creatureInfoManager)
        {
        }
    }

    /// <summary>
    /// Wildbranch Shrine: "Defend against Wildbranch razortails in the Undergrove".
    /// </summary>
    /// <remarks>
    /// No razortail carries the Wildbranch name, Augmented Razortail Runt 43522 and Augmented Razortail 43483 (levels 34-35, never spawned) stand in. Guessed.
    /// </remarks>
    [ScriptFilterOwnerId(6441)]
    public class WildbranchShrine6441QuestScript : QuestWaveEventScript
    {
        protected override IReadOnlyList<(uint ObjectiveId, uint[][] Waves)> Events { get; } =
        [
            (9921, [[43522, 43522], [43522, 43483], [43483, 43483]])
        ];

        public WildbranchShrine6441QuestScript(IGameTableManager gameTableManager, ICreatureInfoManager creatureInfoManager)
            : base(gameTableManager, creatureInfoManager)
        {
        }
    }

    /// <summary>
    /// Wildbranch Shrine: "Defend against Wildbranch razortails in Discovery Heights".
    /// </summary>
    /// <remarks>
    /// As the Undergrove shrine (6441). Guessed.
    /// </remarks>
    [ScriptFilterOwnerId(8029)]
    public class WildbranchShrine8029QuestScript : QuestWaveEventScript
    {
        protected override IReadOnlyList<(uint ObjectiveId, uint[][] Waves)> Events { get; } =
        [
            (14521, [[43522, 43522], [43522, 43483], [43483, 43483]])
        ];

        public WildbranchShrine8029QuestScript(IGameTableManager gameTableManager, ICreatureInfoManager creatureInfoManager)
            : base(gameTableManager, creatureInfoManager)
        {
        }
    }

    /// <summary>
    /// Medical Defense: "Defend Collegium Researchers from Eldan Constructs at the Windfall Digsite".
    /// </summary>
    /// <remarks>
    /// XT-9 Repair Construct 30815 and 38907 (level 18, Eldan, never spawned) stand in for the constructs. Guessed.
    /// </remarks>
    [ScriptFilterOwnerId(6421)]
    public class MedicalDefenseQuestScript : QuestWaveEventScript
    {
        protected override IReadOnlyList<(uint ObjectiveId, uint[][] Waves)> Events { get; } =
        [
            (9867, [[30815, 30815], [30815, 38907], [38907, 38907]])
        ];

        public MedicalDefenseQuestScript(IGameTableManager gameTableManager, ICreatureInfoManager creatureInfoManager)
            : base(gameTableManager, creatureInfoManager)
        {
        }
    }

    /// <summary>
    /// Buzzbing Onslaught: "Defend the honey supplies from Buzzbings at the Protostar Honeyworks".
    /// </summary>
    /// <remarks>
    /// Swarming Buzzbing 8569 (level 19, never spawned). Three waves, guessed.
    /// </remarks>
    [ScriptFilterOwnerId(5980)]
    public class BuzzbingOnslaughtQuestScript : QuestWaveEventScript
    {
        protected override IReadOnlyList<(uint ObjectiveId, uint[][] Waves)> Events { get; } =
        [
            (9054, [[8569, 8569], [8569, 8569, 8569], [8569, 8569, 8569]])
        ];

        public BuzzbingOnslaughtQuestScript(IGameTableManager gameTableManager, ICreatureInfoManager creatureInfoManager)
            : base(gameTableManager, creatureInfoManager)
        {
        }
    }

}
