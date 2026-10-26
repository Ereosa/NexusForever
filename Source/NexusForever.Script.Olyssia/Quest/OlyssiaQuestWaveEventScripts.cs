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
    /// Artemis Pinned: "Recover Artemis Zin's Datachron from the attacking pumera in Wilderrun Chase".
    /// </summary>
    /// <remarks>
    /// Torine Pumera 40967 (levels 34-38). Two waves, guessed.
    /// </remarks>
    [ScriptFilterOwnerId(6271)]
    public class ArtemisPinnedQuestScript : QuestWaveEventScript
    {
        protected override IReadOnlyList<(uint ObjectiveId, uint[][] Waves)> Events { get; } =
        [
            (9574, [[40967, 40967], [40967, 40967, 40967]])
        ];

        public ArtemisPinnedQuestScript(IGameTableManager gameTableManager, ICreatureInfoManager creatureInfoManager)
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

    /// <summary>
    /// Fall of the Bladevault Keeper: "Use the Voreth War Drums to challenge the Bladevault Keeper".
    /// </summary>
    /// <remarks>
    /// Bladevault Warhound 30960 and Bladevault Keeper 30954 (levels 20-21, never spawned, created with the drums 30968). The warhounds coming first is a guess.
    /// </remarks>
    [ScriptFilterOwnerId(6706)]
    public class FallOfTheBladevaultKeeperQuestScript : QuestWaveEventScript
    {
        protected override IReadOnlyList<(uint ObjectiveId, uint[][] Waves)> Events { get; } =
        [
            (10335, [[30960, 30960], [30954]])
        ];

        public FallOfTheBladevaultKeeperQuestScript(IGameTableManager gameTableManager, ICreatureInfoManager creatureInfoManager)
            : base(gameTableManager, creatureInfoManager)
        {
        }
    }

    /// <summary>
    /// Trial of Strength: "Survive the Trial of Strength in Fort Vigilance".
    /// </summary>
    /// <remarks>
    /// The fort's hostile trial fighters, Vigilant Aspirant 28301 and Vigilant Petitioner 40266 (levels 34-35, faction 218). Guessed.
    /// </remarks>
    [ScriptFilterOwnerId(6340)]
    public class TrialOfStrengthQuestScript : QuestWaveEventScript
    {
        protected override IReadOnlyList<(uint ObjectiveId, uint[][] Waves)> Events { get; } =
        [
            (9747, [[28301, 28301], [28301, 28301], [40266]])
        ];

        public TrialOfStrengthQuestScript(IGameTableManager gameTableManager, ICreatureInfoManager creatureInfoManager)
            : base(gameTableManager, creatureInfoManager)
        {
        }
    }

    /// <summary>
    /// Blademother's Refuge: "Defeat all of Tresayne's best Torine warriors to pass the final trial".
    /// </summary>
    /// <remarks>
    /// One at a time: Torine Swordmaiden 40976, Deathbringer 40965 and Swordmaiden 47698 (levels 34-38, Sisterhood, never spawned). Guessed.
    /// </remarks>
    [ScriptFilterOwnerId(6371)]
    public class BlademothersRefugeQuestScript : QuestWaveEventScript
    {
        protected override IReadOnlyList<(uint ObjectiveId, uint[][] Waves)> Events { get; } =
        [
            (14704, [[40976], [40965], [47698]])
        ];

        public BlademothersRefugeQuestScript(IGameTableManager gameTableManager, ICreatureInfoManager creatureInfoManager)
            : base(gameTableManager, creatureInfoManager)
        {
        }
    }
}
