using NexusForever.Game.Abstract.Entity.Creature;
using NexusForever.GameTable;
using NexusForever.Script.Template;
using NexusForever.Script.Template.Filter;

namespace NexusForever.Script.Alizar.Quest
{
    // Server-only wave events of Alizar quests (objective type 25). The attackers are the event-only creatures of the client data:
    // variants named as in the objective text, of the quest's level, that are never spawned in the world. Wave sizes and the split
    // between creatures aren't in any data or guide that survived and are guesses, kept small for a solo player.

    /// <summary>
    /// Gallow's Departed (Algoroc): "Defend the memorial service behind the Chronicle House in Gallow" from Cartel thugs.
    /// </summary>
    /// <remarks>
    /// Cartel Thug, Butcher and Striker 17796-17798 (level 9, never spawned), the memorial's Cartel variants. Three waves, guessed.
    /// </remarks>
    [ScriptFilterOwnerId(4023)]
    public class GallowsDepartedQuestScript : QuestWaveEventScript
    {
        protected override IReadOnlyList<(uint ObjectiveId, uint[][] Waves)> Events { get; } =
        [
            (5281, [[17796, 17796], [17797, 17798], [17796, 17797, 17798]])
        ];

        public GallowsDepartedQuestScript(IGameTableManager gameTableManager, ICreatureInfoManager creatureInfoManager)
            : base(gameTableManager, creatureInfoManager)
        {
        }
    }

    /// <summary>
    /// Defend the Gate (Galeras): "Repel the Crimson Legion soldiers assaulting Kriton's Command Post".
    /// </summary>
    /// <remarks>
    /// Crimson Legionnaire 18925 (level 14, never spawned; 19442 is the spawned one). Three waves, guessed.
    /// </remarks>
    [ScriptFilterOwnerId(4954)]
    public class DefendTheGateQuestScript : QuestWaveEventScript
    {
        protected override IReadOnlyList<(uint ObjectiveId, uint[][] Waves)> Events { get; } =
        [
            (7260, [[18925, 18925], [18925, 18925, 18925], [18925, 18925, 18925]])
        ];

        public DefendTheGateQuestScript(IGameTableManager gameTableManager, ICreatureInfoManager creatureInfoManager)
            : base(gameTableManager, creatureInfoManager)
        {
        }
    }

    /// <summary>
    /// Hold the Line (Galeras): "Kill Stormwing Falkrin near Corporal Darby's troopers in Crosswind Fields", after speaking with Trooper Vog.
    /// </summary>
    /// <remarks>
    /// Stormwing Striker 23953, Butcher 17205, Razorbeak 17092 and Aerial Scout 17360 (levels 17-21, never spawned). Three waves, guessed.
    /// </remarks>
    [ScriptFilterOwnerId(4694)]
    public class HoldTheLineQuestScript : QuestWaveEventScript
    {
        protected override IReadOnlyList<(uint ObjectiveId, uint[][] Waves)> Events { get; } =
        [
            (16116, [[23953, 23953], [17205, 17092], [17360, 17205, 23953]])
        ];

        public HoldTheLineQuestScript(IGameTableManager gameTableManager, ICreatureInfoManager creatureInfoManager)
            : base(gameTableManager, creatureInfoManager)
        {
        }
    }

    /// <summary>
    /// Strange Idol (Galeras): "Survive an ambush by the Stormwing Falkrin at the Windy Hollow".
    /// </summary>
    /// <remarks>
    /// Stormwing Protector 17211/17212, Sentry 17131 and Aerial Guardian 17412 (levels 19-20, never spawned). Two waves, guessed.
    /// </remarks>
    [ScriptFilterOwnerId(5323)]
    public class StrangeIdolQuestScript : QuestWaveEventScript
    {
        protected override IReadOnlyList<(uint ObjectiveId, uint[][] Waves)> Events { get; } =
        [
            (8098, [[17211, 17212], [17131, 17412, 17211]])
        ];

        public StrangeIdolQuestScript(IGameTableManager gameTableManager, ICreatureInfoManager creatureInfoManager)
            : base(gameTableManager, creatureInfoManager)
        {
        }
    }

    /// <summary>
    /// Striking Morale (Galeras): "Hold out against incoming waves of Crimson forces at the top of Focus of Air Control Site".
    /// </summary>
    /// <remarks>
    /// Crimson Ensign 21810, Conscript 21811, Attacker 22232 and Cavalier 22233 (level 18, never spawned, created with the
    /// quest's banners 21759-21829). Three waves, guessed.
    /// </remarks>
    [ScriptFilterOwnerId(4805)]
    public class StrikingMoraleQuestScript : QuestWaveEventScript
    {
        protected override IReadOnlyList<(uint ObjectiveId, uint[][] Waves)> Events { get; } =
        [
            (7733, [[21810, 21811], [21811, 22232], [22233, 21810, 22232]])
        ];

        public StrikingMoraleQuestScript(IGameTableManager gameTableManager, ICreatureInfoManager creatureInfoManager)
            : base(gameTableManager, creatureInfoManager)
        {
        }
    }

    /// <summary>
    /// Balancing Power (Galeras): "Defend Belle from the attackers on Skywind Beach".
    /// </summary>
    /// <remarks>
    /// The attackers are the creature the objective text names (19147). Two waves, guessed.
    /// </remarks>
    [ScriptFilterOwnerId(4909)]
    public class BalancingPowerQuestScript : QuestWaveEventScript
    {
        protected override IReadOnlyList<(uint ObjectiveId, uint[][] Waves)> Events { get; } =
        [
            (6930, [[19147, 19147], [19147, 19147, 19147]])
        ];

        public BalancingPowerQuestScript(IGameTableManager gameTableManager, ICreatureInfoManager creatureInfoManager)
            : base(gameTableManager, creatureInfoManager)
        {
        }
    }

    /// <summary>
    /// Ravenous Dusk (Celestion): "Defend against attacking Ravenous as the detonator is rigged at Grimhold's Last Stand".
    /// </summary>
    /// <remarks>
    /// Ravenous Ghoul, Ripper, Freak and Brute 33571-33574 (level 13, never spawned, one set of increasing difficulty). Three waves, guessed.
    /// </remarks>
    [ScriptFilterOwnerId(6900)]
    public class RavenousDuskQuestScript : QuestWaveEventScript
    {
        protected override IReadOnlyList<(uint ObjectiveId, uint[][] Waves)> Events { get; } =
        [
            (12928, [[33571, 33571, 33572], [33572, 33573], [33574]])
        ];

        public RavenousDuskQuestScript(IGameTableManager gameTableManager, ICreatureInfoManager creatureInfoManager)
            : base(gameTableManager, creatureInfoManager)
        {
        }
    }

    /// <summary>
    /// The Lifeblood Pool (Celestion): "Defend against Liferoot Guardians at the Lifeblood Pool".
    /// </summary>
    /// <remarks>
    /// Liferoot Guardian 33230 (level 11, never spawned, created with the quest's NPC 33231). Two waves, guessed.
    /// </remarks>
    [ScriptFilterOwnerId(6956)]
    public class LifebloodPoolQuestScript : QuestWaveEventScript
    {
        protected override IReadOnlyList<(uint ObjectiveId, uint[][] Waves)> Events { get; } =
        [
            (12725, [[33230, 33230], [33230, 33230, 33230]])
        ];

        public LifebloodPoolQuestScript(IGameTableManager gameTableManager, ICreatureInfoManager creatureInfoManager)
            : base(gameTableManager, creatureInfoManager)
        {
        }
    }

    /// <summary>
    /// Rowsdower at Stake (Celestion): "Kill the Monster when it comes for the bait you've set in Tanglewood Grove".
    /// </summary>
    /// <remarks>
    /// The objective's reward pane lists the Tanglemore Munchkin 33069 and Chomper 33070 that come for the bait. Two waves, guessed.
    /// </remarks>
    [ScriptFilterOwnerId(7000)]
    public class RowsdowerAtStakeQuestScript : QuestWaveEventScript
    {
        protected override IReadOnlyList<(uint ObjectiveId, uint[][] Waves)> Events { get; } =
        [
            (12763, [[33069, 33069], [33070]])
        ];

        public RowsdowerAtStakeQuestScript(IGameTableManager gameTableManager, ICreatureInfoManager creatureInfoManager)
            : base(gameTableManager, creatureInfoManager)
        {
        }
    }

    /// <summary>
    /// Here They Come (Whitevale, Raxen's Holdout): "Ignite the Signal Flare and repel the Squirg Zombie attack", three flares.
    /// </summary>
    /// <remarks>
    /// Squirg Zombie 24420-24423 (level 25, never spawned). Two waves per flare, guessed. The final flare is a kill objective of its own.
    /// </remarks>
    [ScriptFilterOwnerId(5655)]
    public class HereTheyComeRaxensHoldoutQuestScript : QuestWaveEventScript
    {
        protected override IReadOnlyList<(uint ObjectiveId, uint[][] Waves)> Events { get; } =
        [
            (8412, [[24420, 24421], [24422, 24423, 24420]]),
            (8449, [[24421, 24422], [24423, 24420, 24421]]),
            (8450, [[24422, 24423], [24420, 24421, 24422]])
        ];

        public HereTheyComeRaxensHoldoutQuestScript(IGameTableManager gameTableManager, ICreatureInfoManager creatureInfoManager)
            : base(gameTableManager, creatureInfoManager)
        {
        }
    }

    /// <summary>
    /// Here They Come (Whitevale, Inception's Stand): the same zombie attack at the other camp's flares.
    /// </summary>
    [ScriptFilterOwnerId(5724)]
    public class HereTheyComeInceptionsStandQuestScript : QuestWaveEventScript
    {
        protected override IReadOnlyList<(uint ObjectiveId, uint[][] Waves)> Events { get; } =
        [
            (8540, [[24420, 24421], [24422, 24423, 24420]]),
            (8541, [[24421, 24422], [24423, 24420, 24421]]),
            (8542, [[24422, 24423], [24420, 24421, 24422]])
        ];

        public HereTheyComeInceptionsStandQuestScript(IGameTableManager gameTableManager, ICreatureInfoManager creatureInfoManager)
            : base(gameTableManager, creatureInfoManager)
        {
        }
    }

    /// <summary>
    /// Raxen's Riders (Whitevale): "Defend against the Freebots at the Gas 'n' Guzzle".
    /// </summary>
    /// <remarks>
    /// Unhinged Mindless Freebot 39606-39608 (level 23, never spawned, increasing difficulty). Three waves, guessed.
    /// </remarks>
    [ScriptFilterOwnerId(5029)]
    public class RaxensRidersQuestScript : QuestWaveEventScript
    {
        protected override IReadOnlyList<(uint ObjectiveId, uint[][] Waves)> Events { get; } =
        [
            (7354, [[39606, 39606], [39606, 39607], [39608]])
        ];

        public RaxensRidersQuestScript(IGameTableManager gameTableManager, ICreatureInfoManager creatureInfoManager)
            : base(gameTableManager, creatureInfoManager)
        {
        }
    }

    /// <summary>
    /// Underground (Whitevale): "Fight Deadrock Inmates in Deadrock Prison" at the hideout.
    /// </summary>
    /// <remarks>
    /// Deadrock Rioter 23549 and Riot Leader 23617 (levels 23-24, never spawned). Two waves, guessed.
    /// </remarks>
    [ScriptFilterOwnerId(5387)]
    public class UndergroundQuestScript : QuestWaveEventScript
    {
        protected override IReadOnlyList<(uint ObjectiveId, uint[][] Waves)> Events { get; } =
        [
            (7886, [[23549, 23549], [23549, 23617]])
        ];

        public UndergroundQuestScript(IGameTableManager gameTableManager, ICreatureInfoManager creatureInfoManager)
            : base(gameTableManager, creatureInfoManager)
        {
        }
    }

    /// <summary>
    /// Last One In, Shut the Door (Algoroc): "Fend off Exo-Lab security measures while Belle repairs the fractured Caretaker node in Exo-Lab 14", twice.
    /// </summary>
    /// <remarks>
    /// Eldan Replicator 18349/18337 and Eradicator 18348/18350 (levels 11-13, never spawned, created with the quest's security door
    /// consoles 18341/18342). Two waves per repair, guessed.
    /// </remarks>
    [ScriptFilterOwnerId(4853)]
    public class LastOneInShutTheDoorQuestScript : QuestWaveEventScript
    {
        protected override IReadOnlyList<(uint ObjectiveId, uint[][] Waves)> Events { get; } =
        [
            (6751, [[18349, 18349], [18348, 18349]]),
            (6852, [[18337, 18350], [18350, 18348, 18337]])
        ];

        public LastOneInShutTheDoorQuestScript(IGameTableManager gameTableManager, ICreatureInfoManager creatureInfoManager)
            : base(gameTableManager, creatureInfoManager)
        {
        }
    }

    /// <summary>
    /// Coming in Hot! (Algoroc): "Activate the beacon and defend the landing zone in Annihilator Silo 14".
    /// </summary>
    /// <remarks>
    /// The silo is Eldan, Eldan Protector 18026 and Replicator 18337 (level 12, never spawned, created with Trooper Gorok 18010
    /// who gives the quest) stand in for its defenders. Three waves, guessed.
    /// </remarks>
    [ScriptFilterOwnerId(4978)]
    public class ComingInHotQuestScript : QuestWaveEventScript
    {
        protected override IReadOnlyList<(uint ObjectiveId, uint[][] Waves)> Events { get; } =
        [
            (7084, [[18026, 18026], [18026, 18337], [18026, 18337, 18337]])
        ];

        public ComingInHotQuestScript(IGameTableManager gameTableManager, ICreatureInfoManager creatureInfoManager)
            : base(gameTableManager, creatureInfoManager)
        {
        }
    }

    /// <summary>
    /// Ghost Intel (Celestion): "Defend the Thayd Dispatches against ICI Operatives in Verdant Vale".
    /// </summary>
    /// <remarks>
    /// No creature carries the name, the Dominion Zeta Agents 33467 and 33133 (level 11, never spawned) stand in for the operatives
    /// coming for the Exile courier's dispatches. Guessed.
    /// </remarks>
    [ScriptFilterOwnerId(6981)]
    public class GhostIntelQuestScript : QuestWaveEventScript
    {
        protected override IReadOnlyList<(uint ObjectiveId, uint[][] Waves)> Events { get; } =
        [
            (12696, [[33467, 33467], [33133, 33467]])
        ];

        public GhostIntelQuestScript(IGameTableManager gameTableManager, ICreatureInfoManager creatureInfoManager)
            : base(gameTableManager, creatureInfoManager)
        {
        }
    }

    /// <summary>
    /// Primal Extraction (Celestion): "Defend the Primal Siphon against Torine in the Heart of the Godwood Tree", after signalling the
    /// extraction crew at each of three Godwood Nodes.
    /// </summary>
    /// <remarks>
    /// The defend objectives have no marker, they start where the player signalled the crew. Torine Swordsister 33924, Swordmaiden 34470
    /// and Battlemaiden 58421 (level 12, never spawned). Wave sizes guessed.
    /// </remarks>
    [ScriptFilterOwnerId(6995)]
    public class PrimalExtractionQuestScript : QuestWaveEventScript
    {
        protected override IReadOnlyList<(uint ObjectiveId, uint[][] Waves)> Events { get; } =
        [
            (12780, [[33924, 33924], [33924, 34470]]),
            (12961, [[33924, 34470], [34470, 34470]]),
            (12965, [[33924, 34470], [34470, 33924], [58421]])
        ];

        protected override bool StartWithoutMarker => true;

        public PrimalExtractionQuestScript(IGameTableManager gameTableManager, ICreatureInfoManager creatureInfoManager)
            : base(gameTableManager, creatureInfoManager)
        {
        }
    }

    /// <summary>
    /// Wolves Among the Flock (Whitevale): "Examine Acolyte Maros' body and avenge his death in Palerock Post".
    /// </summary>
    /// <remarks>
    /// The hooded preachers of the quest are the Truthbound (Preacher 23379, created with the acolyte), their Initiates 23381,
    /// Vindicators 23383 and Evocators 23384 come for the player. Two waves, guessed.
    /// </remarks>
    [ScriptFilterOwnerId(5452)]
    public class WolvesAmongTheFlockQuestScript : QuestWaveEventScript
    {
        protected override IReadOnlyList<(uint ObjectiveId, uint[][] Waves)> Events { get; } =
        [
            (14707, [[23381, 23381], [23383, 23384]])
        ];

        public WolvesAmongTheFlockQuestScript(IGameTableManager gameTableManager, ICreatureInfoManager creatureInfoManager)
            : base(gameTableManager, creatureInfoManager)
        {
        }
    }

    /// <summary>
    /// The Fall of the Ascendancy (Whitevale): "Kill Harbringer Sveltla in Augment Facility X426", after hacking the core terminal.
    /// </summary>
    /// <remarks>
    /// Harbringer Sveltla 26005 is never spawned, she arrives with two Soulcore guards (Disciple 26001, Evocator 25999) once the terminal is hacked. Guessed.
    /// </remarks>
    [ScriptFilterOwnerId(5826)]
    public class FallOfTheAscendancyQuestScript : QuestWaveEventScript
    {
        protected override IReadOnlyList<(uint ObjectiveId, uint[][] Waves)> Events { get; } =
        [
            (8755, [[26001, 25999], [26005]])
        ];

        public FallOfTheAscendancyQuestScript(IGameTableManager gameTableManager, ICreatureInfoManager creatureInfoManager)
            : base(gameTableManager, creatureInfoManager)
        {
        }
    }
}
