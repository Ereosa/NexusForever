using System.Numerics;
using NexusForever.Game.Abstract.Cinematic;
using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Entity.Creature;
using NexusForever.Game.Abstract.Quest;
using NexusForever.Game.Abstract.Spell;
using NexusForever.Game.Static.PublicEvent;
using NexusForever.Game.Static.Reputation;
using NexusForever.GameTable;
using NexusForever.GameTable.Model;
using NexusForever.Script.Template.Filter;
using NexusForever.Shared;

namespace NexusForever.Script.Instance.Adventure.WarOfTheWilds
{
    /// <summary>
    /// War of the Wilds (public event 158, world 1393): the players lead the Coldburrow skeech against the Samu-Samu moodies in a tug of
    /// war over the thirteen energy nodes between the two giant totems.
    /// </summary>
    /// <remarks>
    /// Built from the client data, the archived Jabbithole sightings (totems, energy nodes, bosses, huts) and the archived guides (Ten Ton
    /// Hammer 2014): The Short Straw by faction (170 Exile with Professor Darven and Commander Breza, 171 Dominion with Director Gigasta and
    /// Colonel Rakaar), then the battle. Minions march from both giant totems down three lanes; neutral (grey) energy nodes are captured by
    /// standing at them, each node held gives the skeech another soldier per wave and a promotion point, the moodies and their heroes take
    /// nodes back; the skeech base's warrior, hunter and shaman huts spend promotion points to train their stronger minions; three neutral
    /// bosses guard the shrines (the Frozen Corrupter in the north, Lord Hoarfrost in the middle, Glaciax in the south), a shrine taken after
    /// its boss upgrades the skeech waves; the moodies' heroes (the NWADV champions) roam the lanes; the allies' strategies (175 Exile, 176
    /// Dominion) come up as optional missions: capture nodes, kill heroes, kill 20 moodies, take a shrine. The moodie totem falling wins, the
    /// skeech totem falling loses.
    /// Guessed: wave timing and size, capture times and radii, the costs, which strategy objective a run asks for.
    /// </remarks>
    [ScriptFilterOwnerId(158)]
    public class WarOfTheWildsEventScript : AdventureEventScript
    {
        private const uint SkeechTotem = 25956;
        private const uint MoodieTotem = 25952;
        private const uint MoodieChieftain = 25958;
        private const uint NeutralNode = 25429;
        private const uint SkeechNode = 26291;
        private const uint MoodieNode = 26292;

        private static readonly Vector3 SkeechCamp = new(4742f, -730.5f, -5686f);
        private static readonly Vector3 MoodieCamp = new(3930f, -696f, -5318f);

        // the thirteen energy nodes (densest Jabbithole sightings of the nodes in all their states)
        private static readonly Vector3[] Nodes =
        [
            new(4197.3f, -703.95f, -5303.1f), new(4393.0f, -720.00f, -5288.5f), new(4065.5f, -688.88f, -5848.0f), new(4307.5f, -752.38f, -5699.0f),
            new(4259.5f, -743.25f, -5666.5f), new(3992.0f, -719.38f, -5570.5f), new(4387.0f, -743.00f, -5509.0f), new(4543.4f, -711.15f, -5817.4f),
            new(3997.0f, -726.00f, -5481.0f), new(4228.0f, -720.00f, -5405.0f), new(4135.8f, -745.50f, -5564.5f), new(4510.0f, -738.00f, -5727.0f),
            new(4565.0f, -731.25f, -5629.0f)
        ];

        // the lanes' waypoints, north, middle and south
        private static readonly Vector3[][] Lanes =
        [
            [new(4565f, -731.25f, -5629f), new(4393f, -720f, -5288.5f), new(4197.3f, -703.95f, -5303.1f)],
            [new(4387f, -743f, -5509f), new(4135.8f, -745.5f, -5564.5f), new(3997f, -726f, -5481f)],
            [new(4510f, -738f, -5727f), new(4307.5f, -752.38f, -5699f), new(4065.5f, -688.88f, -5848f)]
        ];

        // the neutral bosses and their shrines
        private static readonly (uint Boss, uint Shrine, Vector3 Position)[] Shrines =
        [
            (25970, 26318, new(4560f, -725f, -5316f)), // the Frozen Corrupter
            (25967, 26323, new(4352f, -749f, -5608f)), // Lord Hoarfrost
            (25968, 26324, new(4121f, -702f, -5852f))  // Glaciax
        ];

        private static readonly uint[] Champions = [25710, 25703, 25711, 25709, 25705, 25704, 25706, 25707, 25708, 25714];
        private static readonly Dictionary<uint, uint> Huts = new() { { 26704, 26107 }, { 26705, 26109 }, { 26706, 26108 } };

        private enum NodeState { Neutral, Skeech, Moodie }

        protected override uint WorldId => 1393;
        protected override uint IntroEvent => Players.FirstOrDefault()?.Faction1 == Faction.Dominion ? 171u : 170u;
        protected override uint[][] Storylines => [];
        protected override uint[] FinalObjectives => [408];

        private readonly NodeState[] nodeStates = new NodeState[13];
        private readonly IWorldEntity[] nodeEntities = new IWorldEntity[13];
        private readonly double[] capture = new double[13];
        private readonly bool[] shrineTaken = new bool[3];
        private readonly bool[] bossDead = new bool[3];
        private readonly IWorldEntity[] bosses = new IWorldEntity[3];
        private readonly HashSet<uint> trained = [];
        private readonly HashSet<IWorldEntity> champions = [];
        private int promotion;
        private bool battle;
        private bool over;
        private int waves;
        private double waveTimer;
        private double strategyTimer;
        private uint strategy;
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

        protected override double StallTime(uint subEventId) => subEventId is 175 or 176 ? 300d : 240d;

        // the strategies' objectives are counted by the battle
        protected override bool IsScriptDriven(uint objectiveId) => objectiveId is >= 566 and <= 577 or 2219 or 2220;

        /// <summary>
        /// A strategy asks for one thing at a time.
        /// </summary>
        protected override List<PublicEventObjectiveEntry> SelectObjectives(uint subEventId, List<PublicEventObjectiveEntry> mainObjectives)
        {
            if (subEventId is not (175 or 176))
                return mainObjectives;

            // capture nodes, kill champions, kill 20 moodies, capture a shrine
            uint[] options = subEventId == 175 ? [566, 567, 568, 569] : [572, 573, 574, 575];
            strategy = options[Random.Shared.Next(options.Length)];
            return mainObjectives.Where(o => o.Id == strategy).ToList();
        }

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
            skeechTotem = SpawnCreature(SkeechTotem, SkeechCamp, 0f);
            moodieTotem = SpawnCreature(MoodieTotem, MoodieCamp, 0f);
            SpawnCreature(MoodieChieftain, MoodieCamp + new Vector3(6f, 0f, 6f), 0f);
            for (int i = 0; i < Nodes.Length; i++)
                SetNode(i, NodeState.Neutral);
            for (int i = 0; i < Shrines.Length; i++)
            {
                bosses[i] = SpawnCreature(Shrines[i].Boss, Shrines[i].Position, 0f);
                SpawnCreature(Shrines[i].Shrine, Shrines[i].Position + new Vector3(8f, 0f, 0f), 0f);
            }
            // the skeech base's huts (Jabbithole sightings)
            SpawnCreature(26704, Surface(4807f, -5654f, -730.5f), 0f);
            SpawnCreature(26705, Surface(4781f, -5645f, -730.5f), 0f);
            SpawnCreature(26706, Surface(4797f, -5645f, -730.5f), 0f);
            waveTimer = 5d;
            strategyTimer = 120d;
        }

        private void SetNode(int index, NodeState state)
        {
            RemoveEntity(ref nodeEntities[index]);
            nodeStates[index] = state;
            nodeEntities[index] = SpawnCreature(state switch { NodeState.Skeech => SkeechNode, NodeState.Moodie => MoodieNode, _ => NeutralNode },
                Nodes[index], 0f);
        }

        protected override void OnTick(double lastTick)
        {
            base.OnTick(lastTick);

            if (!battle || over)
                return;

            CaptureNodes(lastTick);

            waveTimer -= lastTick;
            if (waveTimer <= 0d)
            {
                waveTimer = 40d;
                SpawnWaves();
            }

            // the allies ask for a strategy every few minutes
            strategyTimer -= lastTick;
            if (strategyTimer <= 0d && CurrentSubEvent == null)
            {
                strategyTimer = 240d;
                StartSubEvent(IntroEvent == 171u ? 176u : 175u);
            }
        }

        private void CaptureNodes(double lastTick)
        {
            List<IUnitEntity> moodies = champions.OfType<IUnitEntity>().Where(c => c.IsAlive).ToList();
            for (int i = 0; i < Nodes.Length; i++)
            {
                bool players = AnyPlayerNear(Nodes[i], 6f);
                bool heroes = moodies.Any(m => Vector3.Distance(m.Position, Nodes[i]) < 8f);

                if (players && nodeStates[i] != NodeState.Skeech && !heroes)
                {
                    capture[i] += lastTick;
                    if (capture[i] >= 5d)
                    {
                        capture[i] = 0d;
                        SetNode(i, NodeState.Skeech);
                        promotion++;
                        OnStrategyProgress(566, 572, 2219, 2220);
                    }
                }
                else if (heroes && !players && nodeStates[i] == NodeState.Skeech)
                {
                    capture[i] += lastTick;
                    if (capture[i] >= 10d)
                    {
                        capture[i] = 0d;
                        SetNode(i, NodeState.Moodie);
                    }
                }
                else
                    capture[i] = 0d;
            }

            // shrines are taken once their boss is dead
            for (int i = 0; i < Shrines.Length; i++)
            {
                if (shrineTaken[i] || !bossDead[i] || !AnyPlayerNear(Shrines[i].Position + new Vector3(8f, 0f, 0f), 6f))
                    continue;

                shrineTaken[i] = true;
                OnStrategyProgress(569, 575);
            }
        }

        private void OnStrategyProgress(params uint[] objectives)
        {
            foreach (uint objective in objectives.Where(o => o == strategy || ((o == 2219 || o == 2220) && (strategy == 566 || strategy == 572))))
                UpdateSubObjective(objective);
        }

        private void SpawnWaves()
        {
            waves++;
            int skeechNodes = nodeStates.Count(s => s == NodeState.Skeech);
            int moodieNodes = nodeStates.Count(s => s == NodeState.Moodie);
            bool upgraded = shrineTaken.Any(t => t);

            for (int lane = 0; lane < Lanes.Length; lane++)
            {
                // skeech: basic soldiers, one more for every three nodes held, the huts' trained lines, upgraded by the shrines
                var skeech = new List<uint> { 25930, 25930 };
                for (int i = 0; i < skeechNodes / 3; i++)
                    skeech.Add(25930);
                skeech.AddRange(trained);
                if (upgraded)
                    skeech.Add(26107);

                var moodies = new List<uint> { 26046, 26046, 26157 };
                for (int i = 0; i < moodieNodes / 3; i++)
                    moodies.Add(26116);

                Vector3[] path = Lanes[lane];
                SpawnMarch(skeech, SkeechCamp, path, MoodieCamp);
                SpawnMarch(moodies, MoodieCamp, [.. path.Reverse()], SkeechCamp);
            }

            // the moodies' heroes roam the lanes every fourth wave
            if (waves % 4 == 0)
            {
                Vector3[] path = Lanes[Random.Shared.Next(Lanes.Length)];
                IWorldEntity hero = SpawnCreature(Champions[Random.Shared.Next(Champions.Length)], MoodieCamp, 0f, (map, guid, _) =>
                {
                    if (mapInstance.GetEntity<IUnitEntity>(guid) is IUnitEntity unit)
                        Walk(unit, [.. path.Reverse(), SkeechCamp], 4f);
                });
                if (hero != null)
                    champions.Add(hero);
            }
        }

        private void SpawnMarch(List<uint> creatures, Vector3 from, Vector3[] path, Vector3 to)
        {
            for (int i = 0; i < creatures.Count; i++)
            {
                Vector3 position = from + new Vector3(i * 1.5f - creatures.Count * 0.75f, 0f, 3f);
                SpawnCreature(creatures[i], position, 0f, (map, guid, _) =>
                {
                    if (mapInstance.GetEntity<IUnitEntity>(guid) is IUnitEntity unit)
                        Walk(unit, [.. path, to], 4f);
                });
            }
        }

        /// <summary>
        /// A hut trains its line of stronger skeech for five promotion points.
        /// </summary>
        public void UseHut(IWorldEntity hut, IPlayer player)
        {
            if (!Huts.TryGetValue(hut.CreatureId, out uint minion) || trained.Contains(minion))
                return;

            if (promotion < 5)
            {
                player.SendSystemMessage($"Not enough promotion points ({promotion} of 5): capture energy nodes.");
                return;
            }

            promotion -= 5;
            trained.Add(minion);
            player.SendSystemMessage("The skeech will send stronger soldiers into the next waves.");
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

            int boss = Array.IndexOf(bosses, entity);
            if (boss >= 0)
                bossDead[boss] = true;

            if (champions.Remove(entity))
                OnStrategyProgress(567, 573);

            if (entity.CreatureId is 26046 or 26157 or 26116 or 26117)
                OnStrategyProgress(568, 574);

            base.OnEntityDeath(entity);
        }
    }
}
