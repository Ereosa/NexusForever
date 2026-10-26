using System.Numerics;
using NexusForever.Game.Abstract.Cinematic;
using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Entity.Creature;
using NexusForever.Game.Abstract.PublicEvent;
using NexusForever.Game.Abstract.Quest;
using NexusForever.Game.Abstract.Spell;
using NexusForever.Game.Static.Cinematic;
using NexusForever.Game.Static.Reputation;
using NexusForever.GameTable;
using NexusForever.Script.Template.Filter;
using NexusForever.Shared;

namespace NexusForever.Script.Instance.Expedition.Gautlet
{
    /// <summary>
    /// The Gauntlet (public event 446, world 2183): Pilot Taboro's shuttle is snatched by "Brick" Braggor's Darkspur and the crew wakes up
    /// as contestants of his televised arena show.
    /// </summary>
    /// <remarks>
    /// No packet captures exist. The order follows the objective data and where its world locations are: speak to Pilot Taboro, get
    /// everyone into the shuttle airlock, wake up aboard the Gauntlet (the show's introduction), throw the three switches out of the
    /// electrified room, survive the Swarm Pit's three waves, the Chamber of Choices (sponsored products to endorse), clear the Faction
    /// Friction and Charnel Chamber arenas, survive three acts of the Main Event, (veteran) defeat "Brick" Braggor himself, then talk to Judge
    /// Kain and Agent Lex before leaving.
    /// Each arena has several line-ups and a run gets one of each: Faction Friction the Bash Brothers, a handler with his pets or the opposing
    /// faction's team, the Charnel Chamber the Rockstar Yeti, the Championator or the Voodoo Kin, the Main Event three of the Goon Squad, the
    /// Handler, Slice and Dice, Pyro and Maniac, Showtime and the Shock King.
    /// Positions come from the archived Jabbithole sightings of each combatant, the cutscenes from the client (the show's introduction,
    /// GC193 before the Main Event, the departure).
    /// Guessed: the line-up picks, Swarm Pit wave sizes and creatures, the kidnapping teleport, a contestant being saved from the Darkspur
    /// once none is left near them, golden skulls picked up by touching them, which force fields and gates each step lowers.
    /// </remarks>
    [ScriptFilterOwnerId(446)]
    public class GauntletEventScript : ExpeditionEventScript
    {
        private enum Phase : uint
        {
            TalkToTaboro,
            Airlock,
            WakeUp,
            ElectrifiedRoom,
            SwarmPit,
            SwarmPitWaves,
            ChamberOfChoices,
            SideArenas,
            MainEvent,
            Braggor,
            Leave,
            Done
        }

        private static class Objective
        {
            public const uint Airlock           = 1818;
            public const uint WakeUp            = 1819;
            public const uint TalkToTaboro      = 1821;
            public const uint BashBrothers      = 1822;
            public const uint HandlerAndPets    = 1834;
            public const uint FactionTeam       = 1835;
            public const uint Switches          = 1837;
            public const uint GatherSwarmPit    = 1842;
            public const uint SwarmPitWaves     = 1854;
            public const uint EnterFaction      = 1857;
            public const uint EnterMainEvent    = 1858;
            public const uint EnterCharnel      = 1859;
            public const uint RockstarYeti      = 1864;
            public const uint Championator      = 1865;
            public const uint VoodooKin         = 1866;
            public const uint GoonSquad         = 1869;
            public const uint TheHandler        = 1870;
            public const uint SliceAndDice      = 1871;
            public const uint PyroManiac        = 1872;
            public const uint Showtime          = 1873;
            public const uint ShockKing         = 1874;
            public const uint SurviveMainEvent  = 1875;
            public const uint ClearSideArenas   = 1896;
            public const uint TalkToKain        = 1914;
            public const uint TalkToLex         = 1915;
            public const uint SaveContestants   = 1951;
            public const uint GoldenSkulls      = 1957;
            public const uint ChamberOfChoices  = 1977;
            public const uint Endorse           = 2001;
            public const uint Braggor           = 4671;
            public const uint GoldMedalTimer    = 4707;
        }

        private static readonly Vector3 ShuttleAirlock = new(523.081f, 0.199f, -507.944f);   // WorldLocation2 38909, objective 1818
        private static readonly Vector3 WakeUpRoom     = new(-1006.810f, 2.617f, 1184.265f); // WorldLocation2 38925, objective 1819
        private static readonly Vector3 SwarmPit       = new(-1007.078f, 0f, 1063.784f);     // WorldLocation2 39001, objectives 1842 and 1854
        private static readonly Vector3 Chamber        = new(-1007.155f, 0.152f, 967.062f);  // WorldLocation2 39559, objective 1977
        private static readonly Vector3 FactionArena   = new(-1048.819f, -0.024f, 967.042f); // WorldLocation2 38893, objective 1857
        private static readonly Vector3 CharnelArena   = new(-935.691f, -2.681f, 967.210f);  // WorldLocation2 38847, objective 1859
        private static readonly Vector3 MainArena      = new(-1007.349f, -1.561f, 881.145f); // WorldLocation2 39109, objective 1858
        private static readonly Vector3 Exit           = new(-1004.069f, 0f, 811.810f);      // WorldLocation2 39166, objectives 1914 and 1915

        // gates and force fields (Jabbithole sightings) in the order the steps lower them
        private static readonly Vector3 ElectrifiedGate = new(-1009f, 3.058f, 1120f);
        private static readonly Vector3 SwarmPitGate    = new(-1006f, 0.158f, 1046f);
        private static readonly Vector3 ChamberField    = new(-1000f, 0.058f, 1009f);
        private static readonly Vector3 FactionField    = new(-1030f, 0.025f, 974f);
        private static readonly Vector3[] CharnelFields = [new(-983f, 0.042f, 973f), new(-953f, 0.995f, 973f)];
        private static readonly Vector3 MainEventField  = new(-1001f, 2.506f, 925f);

        private static readonly Dictionary<uint, uint> Veteran = new()
        {
            { 48461, 69237 }, { 48765, 69239 }, { 48793, 69240 }, { 48833, 69241 }, { 48835, 69242 }, { 48842, 69243 }, { 48843, 69244 },
            { 48844, 69246 }, { 48863, 69247 }, { 48865, 69248 }, { 48867, 69251 }, { 48491, 69254 }, { 48529, 69255 }, { 48499, 69257 },
            { 48511, 69258 }, { 49213, 69273 }, { 49209, 69274 }, { 49212, 69275 }, { 49214, 69276 }, { 49399, 69277 }, { 49401, 69278 },
            { 48454, 69280 }, { 48458, 69281 }, { 48666, 69282 }, { 48667, 69283 }, { 48669, 69284 }, { 48670, 69285 }, { 48689, 69286 },
            { 48695, 69287 }, { 48697, 69288 }, { 48721, 69289 }, { 48723, 69290 }, { 48728, 69291 }, { 48493, 69300 }, { 48510, 69301 },
            { 48516, 69302 }, { 48518, 69303 }, { 48519, 69304 }, { 48554, 69305 }, { 48557, 69306 }, { 48558, 69307 }, { 48579, 69308 },
            { 48588, 69309 }, { 48589, 69310 }, { 48591, 69311 }, { 49419, 69235 }, { 49824, 69236 }
        };

        private static readonly uint[] VeteranOnly = [69688, 69728];

        // Swarm Pit creatures, "x5 horde fodder waves"
        private static readonly uint[] Fodder = [48461, 48765, 48793, 48833, 48835, 48842, 48843, 48844, 48863, 48865, 48866, 48867];

        // Faction Friction line-ups
        private static readonly uint[] BashBrothers = [48454, 48458];
        private static readonly uint[] ExileTeam = [48721, 48723, 48728];
        private static readonly uint[] DominionTeam = [48689, 48695, 48697];
        private static readonly uint[] HoundmasterAndHounds = [48669, 48667, 48667];
        private static readonly uint[] GarrmasterAndGarr = [48670, 48666, 48666];

        // Main Event acts: objective, combatants
        private static readonly (uint Objective, uint[] Creatures)[] MainEventActs =
        [
            (Objective.GoonSquad,    [48588, 48589, 48591, 48590]),
            (Objective.TheHandler,   [48516, 48518, 48519]),
            (Objective.SliceAndDice, [48557, 48558]),
            (Objective.PyroManiac,   [48493, 48510]),
            (Objective.Showtime,     [48579]),
            (Objective.ShockKing,    [48554])
        ];

        private const uint Taboro = 48580;
        private const uint ShuttleDoor = 48585;
        private const uint FenceGate = 48512;
        private const uint ForceField = 49758;
        private const uint Contestant = 48496;
        private const uint GoldenSkull = 49375;
        private const uint BraggorFighter = 69688;
        private const uint BraggorSplorg = 69728;
        private static readonly uint[] Darkspur = [49209, 49212, 49213, 49214, 49399, 49401];

        protected override uint WorldId => 2183;
        protected override uint InitialPhase => (uint)Phase.TalkToTaboro;
        protected override IReadOnlyDictionary<uint, uint> VeteranCreatures => Veteran;
        protected override IReadOnlyCollection<uint> VeteranOnlyCreatures => VeteranOnly;

        private readonly int factionLineUp = Random.Shared.Next(3);
        private readonly int charnelLineUp = Random.Shared.Next(3);
        private readonly int[] acts = [.. Enumerable.Range(0, MainEventActs.Length).OrderBy(_ => Random.Shared.Next()).Take(3)];

        private readonly List<IWorldEntity> group = [];
        private readonly List<IWorldEntity> factionGroup = [];
        private readonly HashSet<uint> savedContestants = [];
        private readonly HashSet<uint> skulls = [];
        private int swarmWave;
        private int act;
        private bool factionStarted;
        private bool charnelStarted;
        private bool factionDone;
        private bool charnelDone;
        private IWorldEntity braggor;
        private int braggorSplorgs;

        public GauntletEventScript(
            IGlobalQuestManager globalQuestManager,
            ICreatureInfoManager creatureInfoManager,
            IEntityFactory entityFactory,
            IFactory<ISpellParameters> spellParametersFactory,
            IGameTableManager gameTableManager,
            ICinematicFactory cinematicFactory)
            : base(globalQuestManager, creatureInfoManager, entityFactory, spellParametersFactory, gameTableManager, cinematicFactory)
        {
        }

        protected override void OnPhase(uint phase)
        {
            switch ((Phase)phase)
            {
                case Phase.TalkToTaboro:
                    Activate(Objective.GoldenSkulls, 10u);
                    if (IsVeteran)
                        Activate(Objective.GoldMedalTimer);
                    break;
                case Phase.Airlock:
                    foreach (IDoorEntity door in GetEntities<IDoorEntity>(ShuttleDoor))
                        door.OpenDoor();
                    ActivateForPlayers(Objective.Airlock);
                    AddTrigger(5735, ShuttleAirlock, 4f);
                    break;
                case Phase.WakeUp:
                    // the airlock was a trap: the Darkspur gas the crew and carry them aboard the Gauntlet
                    Activate(Objective.WakeUp);
                    foreach (IPlayer player in Players.ToList())
                        player.TeleportToLocal(Surface(WakeUpRoom.X + (float)(Random.Shared.NextDouble() * 4d - 2d), WakeUpRoom.Z + (float)(Random.Shared.NextDouble() * 4d - 2d), WakeUpRoom.Y));
                    Delay(3d, () => PlayCinematic(CinematicSceneId.GauntletShowIntro));
                    Delay(35d, () => Update(Objective.WakeUp));
                    break;
                case Phase.ElectrifiedRoom:
                    Activate(Objective.Switches);
                    break;
                case Phase.SwarmPit:
                    OpenGate(ElectrifiedGate);
                    ActivateForPlayers(Objective.GatherSwarmPit);
                    AddTrigger(5753, SwarmPit, 9f);
                    break;
                case Phase.SwarmPitWaves:
                    Activate(Objective.SwarmPitWaves);
                    NextSwarmWave();
                    break;
                case Phase.ChamberOfChoices:
                    OpenGate(SwarmPitGate);
                    RemoveField(ChamberField);
                    ActivateForPlayers(Objective.ChamberOfChoices);
                    AddTrigger(5894, Chamber, 6f);
                    Activate(Objective.Endorse);
                    Activate(Objective.SaveContestants, 3u);
                    break;
                case Phase.SideArenas:
                    RemoveField(FactionField);
                    foreach (Vector3 field in CharnelFields)
                        RemoveField(field);
                    Activate(Objective.ClearSideArenas);
                    ActivateForPlayers(Objective.EnterFaction);
                    ActivateForPlayers(Objective.EnterCharnel);
                    AddTrigger(5755, FactionArena, 12f);
                    AddTrigger(5736, CharnelArena, 12f);
                    break;
                case Phase.MainEvent:
                    RemoveField(MainEventField);
                    ActivateForPlayers(Objective.EnterMainEvent);
                    AddTrigger(5774, MainArena, 12f);
                    break;
                case Phase.Braggor:
                    Activate(Objective.Braggor);
                    braggor = SpawnCreature(BraggorFighter, Surface(MainArena.X, MainArena.Z, MainArena.Y), MathF.PI, AggroNearestPlayer);
                    break;
                case Phase.Leave:
                    Activate(Objective.TalkToKain);
                    Activate(Objective.TalkToLex);
                    break;
            }
        }

        private void OpenGate(Vector3 position)
        {
            IDoorEntity gate = GetEntities<IDoorEntity>(FenceGate).OrderBy(g => Vector3.DistanceSquared(g.Position, position)).FirstOrDefault();
            if (gate != null && Vector3.Distance(gate.Position, position) < 4f)
                gate.OpenDoor();
        }

        private void RemoveField(Vector3 position)
        {
            IWorldEntity field = GetEntities<IWorldEntity>(ForceField).OrderBy(f => Vector3.DistanceSquared(f.Position, position)).FirstOrDefault();
            if (field != null && Vector3.Distance(field.Position, position) < 4f)
                field.RemoveFromMap();
        }

        private List<IWorldEntity> SpawnGroup(IEnumerable<uint> creatures, Vector3 centre, float radius)
        {
            var spawned = new List<IWorldEntity>();
            uint[] list = [.. creatures];
            for (int i = 0; i < list.Length; i++)
            {
                float angle = MathF.Tau / list.Length * i;
                IWorldEntity unit = SpawnCreature(list[i], Surface(centre.X + MathF.Cos(angle) * radius, centre.Z + MathF.Sin(angle) * radius, centre.Y), angle + MathF.PI, AggroNearestPlayer);
                if (unit != null)
                    spawned.Add(unit);
            }
            return spawned;
        }

        private static bool AllDead(List<IWorldEntity> units, IUnitEntity dying)
        {
            return units.Count > 0 && units.All(u => u == dying || u is not IUnitEntity unit || !unit.IsAlive);
        }

        /// <summary>
        /// Swarm Pit: each wave two kinds of arena-frenzied creatures, five of each.
        /// </summary>
        private void NextSwarmWave()
        {
            swarmWave++;
            uint[] kinds = [.. Fodder.OrderBy(_ => Random.Shared.Next()).Take(2)];
            group.Clear();
            group.AddRange(SpawnGroup(Enumerable.Repeat(kinds[0], 5).Concat(Enumerable.Repeat(kinds[1], 5)), SwarmPit, 12f));
        }

        private void StartFactionArena()
        {
            factionStarted = true;
            bool dominion = Players.FirstOrDefault()?.Faction1 == Faction.Dominion;
            switch (factionLineUp)
            {
                case 0:
                    Activate(Objective.BashBrothers, 2u);
                    factionGroup.AddRange(SpawnGroup(BashBrothers, FactionArena, 6f));
                    break;
                case 1:
                    Activate(Objective.HandlerAndPets, 3u);
                    factionGroup.AddRange(SpawnGroup(dominion ? HoundmasterAndHounds : GarrmasterAndGarr, FactionArena, 6f));
                    break;
                default:
                    Activate(Objective.FactionTeam, 3u);
                    factionGroup.AddRange(SpawnGroup(dominion ? ExileTeam : DominionTeam, FactionArena, 6f));
                    break;
            }
        }

        private void StartCharnelArena()
        {
            charnelStarted = true;
            switch (charnelLineUp)
            {
                case 0:
                    Activate(Objective.RockstarYeti);
                    SpawnGroup([48491], CharnelArena, 0f);
                    break;
                case 1:
                    Activate(Objective.Championator);
                    SpawnGroup([48529], CharnelArena, 0f);
                    break;
                default:
                    Activate(Objective.VoodooKin);
                    SpawnGroup([48511, 48499], CharnelArena, 4f);
                    break;
            }
        }

        private void NextAct()
        {
            (uint objective, uint[] creatures) = MainEventActs[acts[act]];
            act++;
            // cluster objectives count each member of the act's target group
            Activate(objective, objective is Objective.Showtime or Objective.ShockKing ? 0u : (uint)creatures.Count(c => c != 48590));
            group.Clear();
            group.AddRange(SpawnGroup(creatures, MainArena, 6f));
        }

        protected override void OnObjectiveSucceeded(uint objectiveId, IPublicEventTeam team)
        {
            switch (objectiveId)
            {
                case Objective.TalkToTaboro:
                    publicEvent.SetPhase(Phase.Airlock);
                    break;
                case Objective.Airlock:
                    publicEvent.SetPhase(Phase.WakeUp);
                    break;
                case Objective.WakeUp:
                    publicEvent.SetPhase(Phase.ElectrifiedRoom);
                    break;
                case Objective.Switches:
                    publicEvent.SetPhase(Phase.SwarmPit);
                    break;
                case Objective.GatherSwarmPit:
                    publicEvent.SetPhase(Phase.SwarmPitWaves);
                    break;
                case Objective.SwarmPitWaves:
                    publicEvent.SetPhase(Phase.ChamberOfChoices);
                    break;
                case Objective.ChamberOfChoices:
                    publicEvent.SetPhase(Phase.SideArenas);
                    break;
                case Objective.EnterFaction when !factionStarted:
                    StartFactionArena();
                    break;
                case Objective.EnterCharnel when !charnelStarted:
                    StartCharnelArena();
                    break;
                case Objective.BashBrothers:
                case Objective.HandlerAndPets:
                case Objective.FactionTeam:
                    if (!factionDone)
                    {
                        factionDone = true;
                        Update(Objective.ClearSideArenas);
                    }
                    break;
                case Objective.RockstarYeti:
                case Objective.Championator:
                case Objective.VoodooKin:
                    if (!charnelDone)
                    {
                        charnelDone = true;
                        Update(Objective.ClearSideArenas);
                    }
                    break;
                case Objective.ClearSideArenas:
                    publicEvent.SetPhase(Phase.MainEvent);
                    break;
                case Objective.EnterMainEvent:
                    Activate(Objective.SurviveMainEvent);
                    PlayCinematic(CinematicSceneId.GauntletFightIntro);
                    Delay(29d, NextAct);
                    break;
                case Objective.GoonSquad:
                case Objective.TheHandler:
                case Objective.SliceAndDice:
                case Objective.PyroManiac:
                case Objective.Showtime:
                case Objective.ShockKing:
                    Update(Objective.SurviveMainEvent);
                    if (act < acts.Length)
                        Delay(8d, NextAct);
                    break;
                case Objective.SurviveMainEvent:
                    publicEvent.SetPhase(IsVeteran ? Phase.Braggor : Phase.Leave);
                    break;
                case Objective.Braggor:
                    publicEvent.SetPhase(Phase.Leave);
                    break;
                case Objective.TalkToKain:
                case Objective.TalkToLex:
                    if (IsSucceeded(team, Objective.TalkToKain, Objective.TalkToLex))
                    {
                        publicEvent.SetPhase(Phase.Done);
                        PlayCinematic(CinematicSceneId.GauntletDeparture);
                        Complete(team);
                    }
                    break;
            }
        }

        private void Complete(IPublicEventTeam team)
        {
            bool gold = IsVeteran && !IsFailed(team, Objective.GoldMedalTimer) && IsSucceeded(team, Objective.GoldenSkulls, Objective.SaveContestants);

            // Assess the situation on the ship, Survive the trials of the Gauntlet, the prime dailies; Gauntlet complete, Prime, Gold (Prime)
            CompleteExpedition([19357, 19358], [18942, 18943], 3718, 6026, 6011, gold);
        }

        public override void OnEntityDeath(IUnitEntity entity)
        {
            uint id = entity.CreatureId;
            Phase phase = (Phase)publicEvent.Phase;

            if (phase == Phase.SwarmPitWaves && group.Contains(entity) && AllDead(group, entity))
            {
                Update(Objective.SwarmPitWaves);
                if (swarmWave < 3)
                    Delay(6d, NextSwarmWave);
            }

            if (factionGroup.Contains(entity))
            {
                uint objective = factionLineUp switch { 0 => Objective.BashBrothers, 1 => Objective.HandlerAndPets, _ => Objective.FactionTeam };
                Update(objective);
            }

            if (entity == braggor)
                Update(Objective.Braggor);

            if (Darkspur.Any(d => GetCreature(d) == id))
                CheckContestants();
        }

        /// <summary>
        /// A contestant is saved from the Darkspur once none is left near them, three must be saved.
        /// </summary>
        private void CheckContestants()
        {
            List<IUnitEntity> darkspur = Darkspur.SelectMany(GetEntities<IUnitEntity>).Where(d => d.IsAlive).ToList();
            foreach (IUnitEntity contestant in GetEntities<IUnitEntity>(Contestant))
            {
                if (savedContestants.Contains(contestant.Guid) || darkspur.Any(d => Vector3.Distance(d.Position, contestant.Position) < 12f))
                    continue;

                savedContestants.Add(contestant.Guid);
                Update(Objective.SaveContestants);
            }
        }

        protected override void OnTick(double lastTick)
        {
            // "Brick" Braggor sends his pet splorgs in at two thirds and one third of his health
            if (braggor is not IUnitEntity brick || !brick.IsAlive || !brick.InCombat || braggorSplorgs >= 2)
                return;

            if (brick.Health > brick.MaxHealth * (braggorSplorgs == 0 ? 0.66f : 0.33f))
                return;

            braggorSplorgs++;
            SpawnGroup(Enumerable.Repeat(BraggorSplorg, 4), MainArena, 10f);
        }

        protected override void OnCheck()
        {
            // golden skulls are picked up by touching them
            foreach (IWorldEntity skull in GetEntities<IWorldEntity>(GoldenSkull).ToList())
            {
                if (skulls.Contains(skull.Guid) || !AnyPlayerNear(skull.Position, 2f))
                    continue;

                skulls.Add(skull.Guid);
                skull.RemoveFromMap();
                Update(Objective.GoldenSkulls);
            }
        }
    }
}
