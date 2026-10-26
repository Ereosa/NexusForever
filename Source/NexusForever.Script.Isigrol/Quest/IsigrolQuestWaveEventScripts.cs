using NexusForever.Game.Abstract.Entity.Creature;
using NexusForever.GameTable;
using NexusForever.Script.Template;
using NexusForever.Script.Template.Filter;

namespace NexusForever.Script.Isigrol.Quest
{
    // Server-only wave events of quest steps (objective type 25) whose reward pane target group lists the attackers.

    /// <summary>
    /// Web of Lies: "Free your (creature=54934) in the Northern Brood Lair".
    /// </summary>
    /// <remarks>
    /// The objective's reward pane lists the attackers: Chittering Direweb 54937, Direweb Weaver 54938, Direweb Recluse 54939.
    /// Wave sizes are guessed.
    /// </remarks>
    [ScriptFilterOwnerId(9190)]
    public class WebOfLiesQuestScript : QuestWaveEventScript
    {
        protected override IReadOnlyList<(uint ObjectiveId, uint[][] Waves)> Events { get; } =
        [
            (16605, [[54938, 54938], [54938, 54937], [54937, 54939]])
        ];

        public WebOfLiesQuestScript(IGameTableManager gameTableManager, ICreatureInfoManager creatureInfoManager)
            : base(gameTableManager, creatureInfoManager)
        {
        }
    }

    /// <summary>
    /// Rescuing the Chief Mineralogist: "Save the Chief Mineralogist from waves of attacking Sandthorne eco-terrorists".
    /// </summary>
    /// <remarks>
    /// Sandthorne Malverine 35947, Sandshifter 36006 and Leafblade 35943 (levels 43-44). Three waves, guessed.
    /// </remarks>
    [ScriptFilterOwnerId(7426)]
    public class RescuingTheChiefMineralogistQuestScript : QuestWaveEventScript
    {
        protected override IReadOnlyList<(uint ObjectiveId, uint[][] Waves)> Events { get; } =
        [
            (13676, [[35947, 35947], [36006, 35947], [35943, 36006, 35947]])
        ];

        public RescuingTheChiefMineralogistQuestScript(IGameTableManager gameTableManager, ICreatureInfoManager creatureInfoManager)
            : base(gameTableManager, creatureInfoManager)
        {
        }
    }

    /// <summary>
    /// The Underground Evil: "Vanquish the underground evil in Farshade Vale".
    /// </summary>
    /// <remarks>
    /// The objective's reward pane names the Vile Mineral 34802, it rises once the player is there.
    /// </remarks>
    [ScriptFilterOwnerId(7145)]
    public class TheUndergroundEvilQuestScript : QuestWaveEventScript
    {
        protected override IReadOnlyList<(uint ObjectiveId, uint[][] Waves)> Events { get; } =
        [
            (13187, [[34802]])
        ];

        public TheUndergroundEvilQuestScript(IGameTableManager gameTableManager, ICreatureInfoManager creatureInfoManager)
            : base(gameTableManager, creatureInfoManager)
        {
        }
    }

    /// <summary>
    /// Come Out and Play: "Survive the Strain ambush near the Widowmaker's exit".
    /// </summary>
    /// <remarks>
    /// The Starblight Pest 46443, Skulker 46454 and Triturator 46470 of the other version's reward pane (7681). Guessed.
    /// </remarks>
    [ScriptFilterOwnerId(8095)]
    public class ComeOutAndPlay8095QuestScript : QuestWaveEventScript
    {
        protected override IReadOnlyList<(uint ObjectiveId, uint[][] Waves)> Events { get; } =
        [
            (14619, [[46443, 46443], [46443, 46454], [46454, 46470]])
        ];

        public ComeOutAndPlay8095QuestScript(IGameTableManager gameTableManager, ICreatureInfoManager creatureInfoManager)
            : base(gameTableManager, creatureInfoManager)
        {
        }
    }

    /// <summary>
    /// Science Without Bounds: "Defend the Golden Jabbit by the Auric Gardens".
    /// </summary>
    /// <remarks>
    /// As Clean Room (8993). Guessed.
    /// </remarks>
    [ScriptFilterOwnerId(9003)]
    public class ScienceWithoutBoundsQuestScript : QuestWaveEventScript
    {
        protected override IReadOnlyList<(uint ObjectiveId, uint[][] Waves)> Events { get; } =
        [
            (16516, [[52467, 52467], [52467, 52466], [52466, 52467, 52467]])
        ];

        public ScienceWithoutBoundsQuestScript(IGameTableManager gameTableManager, ICreatureInfoManager creatureInfoManager)
            : base(gameTableManager, creatureInfoManager)
        {
        }
    }

    /// <summary>
    /// Brawn and Brains: "Activate the Portable Generator and defend the Alpha and Beta Team scientists".
    /// </summary>
    /// <remarks>
    /// Repair Construct 56108 and Enforcer Construct 56109 (level 50, never spawned, created with the generators 56099-56102). Guessed.
    /// </remarks>
    [ScriptFilterOwnerId(9177)]
    public class BrawnAndBrains9177QuestScript : QuestWaveEventScript
    {
        protected override IReadOnlyList<(uint ObjectiveId, uint[][] Waves)> Events { get; } =
        [
            (16615, [[56108, 56108], [56108, 56109]]),
            (16616, [[56108, 56109], [56109, 56109]])
        ];

        public BrawnAndBrains9177QuestScript(IGameTableManager gameTableManager, ICreatureInfoManager creatureInfoManager)
            : base(gameTableManager, creatureInfoManager)
        {
        }
    }

    /// <summary>
    /// Brawn and Brains: "Activate the Portable Generator and defend the Epsilon and Gamma Squad scientists".
    /// </summary>
    /// <remarks>
    /// As 9177. Guessed.
    /// </remarks>
    [ScriptFilterOwnerId(9183)]
    public class BrawnAndBrains9183QuestScript : QuestWaveEventScript
    {
        protected override IReadOnlyList<(uint ObjectiveId, uint[][] Waves)> Events { get; } =
        [
            (17859, [[56108, 56108], [56108, 56109]]),
            (17860, [[56108, 56109], [56109, 56109]])
        ];

        public BrawnAndBrains9183QuestScript(IGameTableManager gameTableManager, ICreatureInfoManager creatureInfoManager)
            : base(gameTableManager, creatureInfoManager)
        {
        }
    }

    /// <summary>
    /// Adventures in Archaeology: escort the archaeologist (54689) through the Sonic Plaza, attacked on the way.
    /// </summary>
    /// <remarks>
    /// The reward panes name the attackers: Executioner Construct 58289 on the way to the first stop and Mysterious Assailant 56309 on the way to the third, the last walk to the makeshift camp is unopposed. The second stop is a console the player uses. Wave sizes guessed.
    /// </remarks>
    [ScriptFilterOwnerId(9176)]
    public class AdventuresInArchaeology9176QuestScript : QuestWaveEventScript
    {
        protected override IReadOnlyList<(uint ObjectiveId, uint[][] Waves)> Events { get; } =
        [
            (16595, [[58289], [58289, 58289]]),
            (18186, [[56309], [56309, 56309]]),
            (18187, [[]])
        ];

        protected override uint GetEscortCreatureId(uint objectiveId) => 54689u;

        public AdventuresInArchaeology9176QuestScript(IGameTableManager gameTableManager, ICreatureInfoManager creatureInfoManager)
            : base(gameTableManager, creatureInfoManager)
        {
        }
    }

    /// <summary>
    /// Adventures in Archaeology: escort the archaeologist (54686) through the Sonic Plaza, attacked on the way.
    /// </summary>
    /// <remarks>
    /// The reward panes name the attackers: Executioner Construct 58289 on the way to the first stop and Mysterious Assailant 57049 on the way to the third, the last walk to the research area is unopposed. The second stop is a console the player uses. Wave sizes guessed.
    /// </remarks>
    [ScriptFilterOwnerId(9182)]
    public class AdventuresInArchaeology9182QuestScript : QuestWaveEventScript
    {
        protected override IReadOnlyList<(uint ObjectiveId, uint[][] Waves)> Events { get; } =
        [
            (16622, [[58289], [58289, 58289]]),
            (18275, [[57049], [57049, 57049]]),
            (18277, [[]])
        ];

        protected override uint GetEscortCreatureId(uint objectiveId) => 54686u;

        public AdventuresInArchaeology9182QuestScript(IGameTableManager gameTableManager, ICreatureInfoManager creatureInfoManager)
            : base(gameTableManager, creatureInfoManager)
        {
        }
    }
}
