using NexusForever.Game.Abstract.Entity.Creature;
using NexusForever.GameTable;
using NexusForever.Script.Template;
using NexusForever.Script.Template.Filter;

namespace NexusForever.Script.Isigrol.Quest
{
    // Server-only wave events of quest steps (objective type 25) whose reward pane target group lists the attackers.

    /// <summary>
    /// Darkspur Karaoke: "Survive the bar brawl caused by your "singing" in the Speakeasy".
    /// </summary>
    /// <remarks>
    /// The objective's reward pane lists the attackers: Darkspur Loader 57072, Darkspur Hitman 57073, Darkspur Butcher 57074, Darkspur Enforcer 57076.
    /// Wave sizes are guessed.
    /// </remarks>
    [ScriptFilterOwnerId(7154)]
    public class DarkspurKaraokeQuestScript : QuestWaveEventScript
    {
        protected override IReadOnlyList<(uint ObjectiveId, uint[][] Waves)> Events { get; } =
        [
            (13274, [[57072, 57072], [57072, 57073], [57073, 57074, 57076]])
        ];

        public DarkspurKaraokeQuestScript(IGameTableManager gameTableManager, ICreatureInfoManager creatureInfoManager)
            : base(gameTableManager, creatureInfoManager)
        {
        }
    }

    /// <summary>
    /// Hazard Pay: "Protect the (creature=34967) from attacking canimid in Witherdust Plains".
    /// </summary>
    /// <remarks>
    /// The objective's reward pane lists the attackers: Witherdust Runt 57035, Witherdust Yearling 57036, Witherdust Burrower 57038.
    /// Wave sizes are guessed.
    /// </remarks>
    [ScriptFilterOwnerId(7166)]
    public class HazardPayQuestScript : QuestWaveEventScript
    {
        protected override IReadOnlyList<(uint ObjectiveId, uint[][] Waves)> Events { get; } =
        [
            (13251, [[57035, 57035], [57035, 57036], [57036, 57038]])
        ];

        public HazardPayQuestScript(IGameTableManager gameTableManager, ICreatureInfoManager creatureInfoManager)
            : base(gameTableManager, creatureInfoManager)
        {
        }
    }

    /// <summary>
    /// Heresy Cubed: "Survive the attacking Probes on the edge of Flamedancer Commune".
    /// </summary>
    /// <remarks>
    /// The objective's reward pane lists the attackers: Eldan Neutralization Probe 36140, Eldan Termination Probe 36141, Eldan Execution Probe 36144, Flamedancer Probe 37490.
    /// Wave sizes are guessed.
    /// </remarks>
    [ScriptFilterOwnerId(7372)]
    public class HeresyCubed7372QuestScript : QuestWaveEventScript
    {
        protected override IReadOnlyList<(uint ObjectiveId, uint[][] Waves)> Events { get; } =
        [
            (13601, [[37490, 37490], [37490, 36144], [36144, 36141, 36140]])
        ];

        public HeresyCubed7372QuestScript(IGameTableManager gameTableManager, ICreatureInfoManager creatureInfoManager)
            : base(gameTableManager, creatureInfoManager)
        {
        }
    }

    /// <summary>
    /// Yatish's Ultimatum: "Plant and defend the Lopp Banner in the upper levels of Scourwind Perch".
    /// </summary>
    /// <remarks>
    /// The objective's reward pane lists the attackers: Scourwind Mudrunner 36112 (never spawned), Scourwind Skullthumper 36113 (never spawned), Scourwind Raider 36114 (never spawned), Scourwind Perchmaster 36115 (never spawned).
    /// Wave sizes are guessed.
    /// </remarks>
    [ScriptFilterOwnerId(7404)]
    public class YatishsUltimatumQuestScript : QuestWaveEventScript
    {
        protected override IReadOnlyList<(uint ObjectiveId, uint[][] Waves)> Events { get; } =
        [
            (13592, [[36112, 36112], [36112, 36114], [36114, 36113], [36115]])
        ];

        public YatishsUltimatumQuestScript(IGameTableManager gameTableManager, ICreatureInfoManager creatureInfoManager)
            : base(gameTableManager, creatureInfoManager)
        {
        }
    }

    /// <summary>
    /// Come Out and Play: "Survive the Strain ambush at the Fist of Azrion's exit".
    /// </summary>
    /// <remarks>
    /// The objective's reward pane lists the attackers: Starblight Pest 46443 (never spawned), Starblight Skulker 46454 (never spawned), Starblight Triturator 46470 (never spawned). The Starblight Desolator 46473 of the pane, a difficulty 9 elite, is left out.
    /// Wave sizes are guessed.
    /// </remarks>
    [ScriptFilterOwnerId(7681)]
    public class ComeOutAndPlayQuestScript : QuestWaveEventScript
    {
        protected override IReadOnlyList<(uint ObjectiveId, uint[][] Waves)> Events { get; } =
        [
            (13973, [[46443, 46443], [46443, 46454], [46454, 46470]])
        ];

        public ComeOutAndPlayQuestScript(IGameTableManager gameTableManager, ICreatureInfoManager creatureInfoManager)
            : base(gameTableManager, creatureInfoManager)
        {
        }
    }

    /// <summary>
    /// Heresy Cubed: "Survive the attacking Probes on the edge of Flamedancer Commune".
    /// </summary>
    /// <remarks>
    /// The objective's reward pane lists the attackers: Eldan Neutralization Probe 36140, Eldan Termination Probe 36141, Eldan Execution Probe 36144, Flamedancer Probe 37490.
    /// Wave sizes are guessed.
    /// </remarks>
    [ScriptFilterOwnerId(8568)]
    public class HeresyCubed8568QuestScript : QuestWaveEventScript
    {
        protected override IReadOnlyList<(uint ObjectiveId, uint[][] Waves)> Events { get; } =
        [
            (15258, [[37490, 37490], [37490, 36144], [36144, 36141, 36140]])
        ];

        public HeresyCubed8568QuestScript(IGameTableManager gameTableManager, ICreatureInfoManager creatureInfoManager)
            : base(gameTableManager, creatureInfoManager)
        {
        }
    }

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
    /// Prose in Peril: "Collect the (creature=64810) in the Sonic Plaza".
    /// </summary>
    /// <remarks>
    /// The objective's reward pane lists the attackers: Vulgar Darkspur Underboss 64813 (never spawned), Uncouth Darkspur Thug 64814 (never spawned), Boorish Darkspur Bandit 64815 (never spawned), Uncultured Darkspur Ruffian 64816 (never spawned).
    /// Wave sizes are guessed.
    /// </remarks>
    [ScriptFilterOwnerId(9631)]
    public class ProseInPerilQuestScript : QuestWaveEventScript
    {
        protected override IReadOnlyList<(uint ObjectiveId, uint[][] Waves)> Events { get; } =
        [
            (18458, [[64814, 64814], [64814, 64815], [64815, 64816, 64813]])
        ];

        public ProseInPerilQuestScript(IGameTableManager gameTableManager, ICreatureInfoManager creatureInfoManager)
            : base(gameTableManager, creatureInfoManager)
        {
        }
    }

    /// <summary>
    /// Constructive Criticism: "Edit the (creature=64810) in the Sonic Plaza".
    /// </summary>
    /// <remarks>
    /// The objective's reward pane lists the attackers: Vulgar Darkspur Underboss 64813 (never spawned), Uncouth Darkspur Thug 64814 (never spawned), Boorish Darkspur Bandit 64815 (never spawned), Uncultured Darkspur Ruffian 64816 (never spawned).
    /// Wave sizes are guessed.
    /// </remarks>
    [ScriptFilterOwnerId(9632)]
    public class ConstructiveCriticismQuestScript : QuestWaveEventScript
    {
        protected override IReadOnlyList<(uint ObjectiveId, uint[][] Waves)> Events { get; } =
        [
            (18472, [[64814, 64814], [64814, 64815], [64815, 64816, 64813]])
        ];

        public ConstructiveCriticismQuestScript(IGameTableManager gameTableManager, ICreatureInfoManager creatureInfoManager)
            : base(gameTableManager, creatureInfoManager)
        {
        }
    }
}
