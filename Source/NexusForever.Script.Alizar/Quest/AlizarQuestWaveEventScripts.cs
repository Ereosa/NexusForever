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
    /// Here They Come (Whitevale, Raxen's Holdout): "Ignite the Signal Flare and repel the Squirg Zombie attack", three flares.
    /// </summary>
    /// <remarks>
    /// Squirg Zombie 24420-24423 (level 25, never spawned). Two waves per flare, guessed. At the final flare the Squirg Giant comes,
    /// the objective's target (34203 here, 34237 at Inception's Stand, tagged "Squirg Giant - Q5655" in its description).
    /// </remarks>
    [ScriptFilterOwnerId(5655)]
    public class HereTheyComeRaxensHoldoutQuestScript : QuestWaveEventScript
    {
        protected override IReadOnlyList<(uint ObjectiveId, uint[][] Waves)> Events { get; } =
        [
            (8412, [[24420, 24421], [24422, 24423, 24420]]),
            (8449, [[24421, 24422], [24423, 24420, 24421]]),
            (8450, [[24422, 24423], [24420, 24421, 24422]]),
            (8553, [[24420, 24421], [34203]])
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
            (8542, [[24422, 24423], [24420, 24421, 24422]]),
            (8555, [[24420, 24421], [34237]])
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

}
