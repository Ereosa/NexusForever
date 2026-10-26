using System.Numerics;
using NexusForever.Game.Abstract.Cinematic;
using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Entity.Creature;
using NexusForever.Game.Abstract.Quest;
using NexusForever.Game.Abstract.Spell;
using NexusForever.Game.Static.PublicEvent;
using NexusForever.Game.Static.Reputation;
using NexusForever.GameTable;
using NexusForever.Script.Template.Filter;
using NexusForever.Shared;

namespace NexusForever.Script.Instance.Adventure.WarOfTheWilds
{
    /// <summary>
    /// War of the Wilds (public event 158, world 1393): the players lead the Coldburrow skeech against the Samu-Samu moodies, minions
    /// march down three lanes between the giant totems until one falls.
    /// </summary>
    /// <remarks>
    /// Built from the client data alone (nothing of the battle was archived): The Short Straw (170 for Exiles with Professor Darven and
    /// Commander Breza, 171 for the Dominion with Director Gigasta and Colonel Rakaar), then the battle: the Giant Skeech Totem in the
    /// skeech's camp (zone 4361), the Giant Moodie Totem at the battle's objective location (19299), skeech and moodie minions spawning in
    /// waves down the north, middle and south lanes (world locations 37997, 37995, 37996), the moodies' champions (the NWADV player-class
    /// team) joining the moodies after a few waves, the Samu-Samu Chieftain guarding the moodie totem.
    /// Guessed: the lanes, wave timing and make-up, the champions' timing, defeat when the skeech totem falls; the huts' minion upgrades,
    /// energy nodes, shrines and their bosses, and the allies' strategies (175, 176) are not built yet.
    /// </remarks>
    [ScriptFilterOwnerId(158)]
    public class WarOfTheWildsEventScript : AdventureEventScript
    {
        private const uint SkeechTotem = 25956;
        private const uint MoodieTotem = 25952;
        private const uint MoodieChieftain = 25958;

        private static readonly uint[] SkeechWave = [25930, 26107, 26108];
        private static readonly uint[] MoodieWave = [26046, 26157, 26116];
        private static readonly uint[] Champions = [25710, 25703, 25711, 25709, 25705];

        private static readonly uint SkeechCamp = 38046;   // zone 4361
        private static readonly uint MoodieCamp = 19299;   // objective 408's location, zone 4355
        private static readonly uint[] Lanes = [37997, 37995, 37996];

        protected override uint WorldId => 1393;
        protected override uint IntroEvent => Players.FirstOrDefault()?.Faction1 == Faction.Dominion ? 171u : 170u;
        protected override uint[][] Storylines => [];
        protected override uint[] FinalObjectives => [408];

        private bool battle;
        private bool over;
        private int waves;
        private double waveTimer;
        private IWorldEntity skeechTotem;
        private IWorldEntity moodieTotem;

        public WarOfTheWildsEventScript(
            IGlobalQuestManager globalQuestManager,
            ICreatureInfoManager creatureInfoManager,
            IEntityFactory entityFactory,
            IFactory<ISpellParameters> spellParametersFactory,
            IGameTableManager gameTableManager,
            ICinematicFactory cinematicFactory)
            : base(globalQuestManager, creatureInfoManager, entityFactory, spellParametersFactory, gameTableManager, cinematicFactory)
        {
        }

        protected override (uint VoteId, int[] Options)? GetVote(int stage, IReadOnlyList<int> chosen) => null;

        protected override double StallTime(uint subEventId) => 240d;

        protected override void OnSubEventStarted(uint id)
        {
            if (id is not (170 or 171))
                return;

            // The Short Straw: the scientist in the skeech cave, the commander at its entrance, the rootbrutes at the energy source,
            // the moodies holding the control point and the enemy's warrior
            bool exile = id == 170;
            SpawnCreature(exile ? 26867u : 27056u, GetLocation(20288), 0f);
            SpawnCreature(exile ? 26868u : 27057u, GetLocation(20344), MathF.PI);
            SpawnCreature(26916, GetLocation(20296), 0f);
            foreach (uint rootbrute in new uint[] { 25961, 25962, 25963 })
                SpawnCreature(rootbrute, GetLocation(20297) + new Vector3(Random.Shared.Next(-4, 5), 0f, Random.Shared.Next(-4, 5)), 0f);
            SpawnCreature(26889, GetLocation(20336), 0f);
            SpawnCreature(26046, GetLocation(20336) + new Vector3(3f, 0f, 0f), 0f);
            SpawnCreature(26046, GetLocation(20336) + new Vector3(-3f, 0f, 0f), 0f);
            SpawnCreature(exile ? 25710u : 25711u, GetLocation(20341), 0f);
        }

        protected override void NextSubEvent(uint finishedId)
        {
            if (battle)
                return;

            // the battle for the wilds
            battle = true;
            Activate(408);
            Activate(1658);
            Activate(1659);
            skeechTotem = SpawnCreature(SkeechTotem, GetLocation(SkeechCamp), 0f);
            moodieTotem = SpawnCreature(MoodieTotem, GetLocation(MoodieCamp), 0f);
            SpawnCreature(MoodieChieftain, GetLocation(MoodieCamp) + new Vector3(6f, 0f, 6f), 0f);
            waveTimer = 5d;
        }

        protected override void OnTick(double lastTick)
        {
            base.OnTick(lastTick);

            if (!battle || over)
                return;

            waveTimer -= lastTick;
            if (waveTimer > 0d)
                return;

            waveTimer = 40d;
            waves++;
            foreach (uint lane in Lanes)
            {
                SpawnMarch(SkeechWave, GetLocation(SkeechCamp), GetLocation(lane), GetLocation(MoodieCamp));
                SpawnMarch(MoodieWave, GetLocation(MoodieCamp), GetLocation(lane), GetLocation(SkeechCamp));
            }

            // the moodies' champions take the field every fourth wave
            if (waves % 4 == 0)
            {
                uint lane = Lanes[Random.Shared.Next(Lanes.Length)];
                SpawnMarch([Champions[Random.Shared.Next(Champions.Length)]], GetLocation(MoodieCamp), GetLocation(lane), GetLocation(SkeechCamp));
            }
        }

        private void SpawnMarch(uint[] creatures, Vector3 from, Vector3 lane, Vector3 to)
        {
            for (int i = 0; i < creatures.Length; i++)
            {
                Vector3 position = from + new Vector3(i * 2f - 2f, 0f, 3f);
                SpawnCreature(creatures[i], position, 0f, (map, guid, _) =>
                {
                    if (mapInstance.GetEntity<IUnitEntity>(guid) is IUnitEntity unit)
                        Walk(unit, [lane, to], 4f);
                });
            }
        }

        public override void OnEntityDeath(IUnitEntity entity)
        {
            if (battle && !over && entity == moodieTotem)
            {
                over = true;
                Update(1659, 100);
                FinishAdventure();
                return;
            }

            if (battle && !over && entity == skeechTotem)
            {
                // the skeech's totem fell: the battle is lost
                over = true;
                publicEvent.Finish(PublicEventTeam.PublicTeam);
                return;
            }

            base.OnEntityDeath(entity);
        }
    }
}
