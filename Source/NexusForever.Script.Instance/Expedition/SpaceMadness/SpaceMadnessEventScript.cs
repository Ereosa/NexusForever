using System.Numerics;
using NexusForever.Game.Abstract.Cinematic;
using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Entity.Creature;
using NexusForever.Game.Abstract.PublicEvent;
using NexusForever.Game.Abstract.Quest;
using NexusForever.Game.Abstract.Spell;
using NexusForever.Game.Static.Cinematic;
using NexusForever.GameTable;
using NexusForever.Script.Template.Filter;
using NexusForever.Shared;

namespace NexusForever.Script.Instance.Expedition.SpaceMadness
{
    /// <summary>
    /// Space Madness (public event 390, world 2149): Captain Tero's shuttle docks at Space Station Venture, whose crew went mad from the
    /// air of the Venture Labs.
    /// </summary>
    /// <remarks>
    /// No packet captures exist. The order follows the objective data and where its world locations are: talk to Captain Tero, pass the
    /// airlock, talk to Major Lee Barmy, the observation deck computer, save the panicked workers from their nightmares (datapads on the way),
    /// enter the research laboratory and fight off its hallucinations as madness grows (four waves, Exact Change 3.0 last), open the hazmat
    /// closet and put on a hazmat suit, kill the hallucinating livestock (air helms for the hallucinating workers), activate the air
    /// scrubber and hold out six waves of a nightmare theme (fire, electric or shadow, wave six their boss), then report the lab safe from the
    /// engineering computer and the departure cinematic.
    /// Every run of the expedition is different: the Jabbithole sightings hold every hallucination ever seen at each spot, a run picks a few
    /// kinds of nightmares, crazed crew, haunted objects and one livestock herd and leaves the rest out, and one wave theme.
    /// Guessed: the picks and wave sizes, a panicked worker being saved when no nightmare is left near them, the escaped experiments (veteran)
    /// appearing one per lab wave, the doors each step opens.
    /// </remarks>
    [ScriptFilterOwnerId(390)]
    public class SpaceMadnessEventScript : ExpeditionEventScript
    {
        private enum Phase : uint
        {
            TalkToTero,
            Airlock,
            TalkToBarmy,
            ObservationComputer,
            PanickedWorkers,
            EnterLab,
            LabWaves,
            HazmatCloset,
            HazmatSuit,
            Livestock,
            AirScrubber,
            ScrubberWaves,
            EngineeringComputer,
            Done
        }

        private static class Objective
        {
            public const uint TalkToTero          = 1579;
            public const uint TalkToBarmy         = 1588;
            public const uint ObservationComputer = 1589;
            public const uint PanickedWorkers     = 1590;
            public const uint EnterLab            = 1591;
            public const uint HazmatClosetOld     = 1592;
            public const uint Livestock           = 1593;
            public const uint AirHelms            = 1594;
            public const uint AirScrubber         = 1595;
            public const uint Datapads            = 1596;
            public const uint LabWave1            = 1597;
            public const uint Airlock             = 1602;
            public const uint EngineeringComputer = 1603;
            public const uint LabBoss             = 1641;
            public const uint LabWave2            = 1642;
            public const uint LabWave3            = 1643;
            public const uint HazmatSuit          = 1656;
            public const uint LabWaves            = 1662;
            public const uint ScrubberWaves       = 2115;
            public const uint GoldMedalTimer      = 4705;
            public const uint VolatileRowsdowers  = 4708;
            public const uint Slank               = 4709;
            public const uint Rockmite            = 4710;
            public const uint Downgrazer          = 4711;
            public const uint Experiments         = 4712;
            public const uint HazmatCloset        = 4736;
        }

        private static readonly Vector3 Airlock     = new(-35.282f, 4.942f, 270.041f);   // WorldLocation2 37702, objective 1602
        private static readonly Vector3 LabEntrance = new(-55.090f, -0.052f, 137.582f);  // WorldLocation2 37621, objective 1591
        private static readonly Vector3 Lab         = new(0.542f, -2.837f, 119.998f);    // WorldLocation2 37993, objective 1662
        private static readonly Vector3 Scrubbers   = new(191.734f, -2.931f, 322.190f);  // WorldLocation2 40117, objective 2115

        // doors (client area props) in the order the steps open them
        private static readonly Vector3 AirlockOuterDoor = new(-35.343f, 5.153f, 278.544f);
        private static readonly Vector3 AirlockInnerDoor = new(-35.358f, 5.153f, 263.196f);
        private static readonly Vector3 StationDoor      = new(-35.316f, 5.153f, 215.173f);
        private static readonly Vector3 LabDoor          = new(-72.277f, 0.145f, 139.997f);
        private static readonly Vector3 HazmatDoor       = new(-0.551f, 0.296f, 79.033f);
        private static readonly Vector3[] PasturesDoors  = [new(20.971f, -2.958f, 150.972f), new(72.805f, -2.937f, 202.806f)];
        private static readonly Vector3 ScrubberDoor     = new(163.464f, -2.937f, 293.408f);

        private static readonly Dictionary<uint, uint> Veteran = new()
        {
            { 45724, 69137 }, { 45726, 69138 }, { 45727, 69171 }, { 45728, 69172 }, { 45729, 69139 }, { 45730, 69140 }, { 45785, 69180 },
            { 45844, 69181 }, { 45845, 69182 }, { 45846, 69183 }, { 45849, 69184 }, { 45866, 69185 }, { 45879, 69186 }, { 45903, 69141 },
            { 45904, 69142 }, { 45923, 69187 }, { 45936, 69188 }, { 46080, 69189 }, { 46091, 69190 }, { 46105, 69191 }, { 46108, 69192 },
            { 46120, 69193 }, { 46123, 69213 }, { 46124, 69214 }, { 46126, 69215 }, { 46127, 69216 }, { 46128, 69217 }, { 46130, 69218 },
            { 46132, 69219 }, { 46133, 69220 }, { 46135, 69221 }, { 46138, 69194 }, { 46186, 69195 }, { 46476, 69196 }, { 46621, 69197 },
            { 46660, 69198 }, { 46661, 69199 }, { 46671, 69143 }, { 46672, 69144 }, { 46714, 69205 }, { 46720, 69206 }, { 46721, 69207 },
            { 46722, 69208 }, { 46723, 69209 }, { 46724, 69210 }, { 46856, 69145 }, { 46858, 69146 }, { 46881, 69147 }, { 46882, 69148 },
            { 46883, 69149 }, { 46884, 69150 }, { 46885, 69151 }, { 46887, 69152 }, { 46889, 69153 }, { 48098, 69154 }, { 48099, 69155 },
            { 48101, 69156 }, { 48102, 69157 }, { 48103, 69158 }, { 48104, 69159 }, { 48105, 69160 }, { 48106, 69161 }, { 48107, 69162 },
            { 48345, 69163 }, { 48346, 69164 }, { 48347, 69165 }, { 48348, 69173 }, { 48349, 69166 }, { 48350, 69167 }, { 48351, 69168 },
            { 48352, 69169 }, { 48353, 69170 }, { 58783, 69223 }, { 58798, 69222 }, { 46483, 69204 }
        };

        private static readonly uint[] VeteranOnly = [69899, 69900, 69901];

        // hallucinations a run picks from (the nightmares attacking the workers, the crazed crew, the haunted objects, the livestock herds)
        private static readonly uint[] Nightmares = [46123, 46124, 46126, 46127, 46128, 46130, 46131, 46132, 46133, 46135, 46137, 58783, 58798, 46721];
        private static readonly uint[] CrazedCrew = [45845, 45847, 46671, 46672, 46080, 46091, 46105, 45936, 46661];
        private static readonly uint[] HauntedObjects = [45849, 45866, 45879, 45923, 46108, 46186, 46476, 46660, 46120, 46138, 46621, 46720];
        private static readonly uint[][] Herds = [[45724], [45726, 45727], [45728, 45729, 45730]];
        private static readonly uint[] Livestock = [45724, 45726, 45727, 45728, 45729, 45730];

        // the air scrubber waves by theme, waves two to five and the theme's boss
        private static readonly uint[][][] Themes =
        [
            [[46856, 46858], [46881, 46882], [46883, 46884], [46885, 46887], [46889]], // fire
            [[48098, 48099], [48101, 48102], [48103, 48104], [48105, 48106], [48107]], // electric
            [[48345, 48346], [48347, 48348], [48349, 48350], [48351, 48352], [48353]]  // shadow
        ];

        private const uint PanickedWorker = 46062;
        private const uint PanickedWorkerFemale = 46063;
        private const uint HallucinatingWorker = 45903;
        private const uint HallucinatingWorkerFemale = 45904;
        private const uint HazmatSuitObject = 45981;
        private const uint VentureDoor = 45811;
        private const uint BlazingCrewman = 46714;
        private const uint SmokeNightmare = 46722;
        private const uint EyeNightmare = 46723;
        private const uint EnragedLamp = 46724;
        private const uint MaskedArachnid = 46720;
        private const uint ExactChange = 46483;
        private const uint Rowsdower = 45723;
        private static readonly uint[] ExperimentCreatures = [69899, 69900, 69901];

        protected override uint WorldId => 2149;
        protected override uint InitialPhase => (uint)Phase.TalkToTero;
        protected override IReadOnlyDictionary<uint, uint> VeteranCreatures => Veteran;
        protected override IReadOnlyCollection<uint> VeteranOnlyCreatures => VeteranOnly;

        private readonly HashSet<uint> picked;
        private readonly HashSet<uint> excluded;
        private readonly uint[][] theme;

        private readonly HashSet<uint> savedWorkers = [];
        private readonly HashSet<uint> calmedWorkers = [];
        private readonly List<IWorldEntity> wave = [];
        private int labWave;
        private int scrubberWave;
        private bool suitTaken;

        public SpaceMadnessEventScript(
            IGlobalQuestManager globalQuestManager,
            ICreatureInfoManager creatureInfoManager,
            IEntityFactory entityFactory,
            IFactory<ISpellParameters> spellParametersFactory,
            IGameTableManager gameTableManager,
            ICinematicFactory cinematicFactory)
            : base(globalQuestManager, creatureInfoManager, entityFactory, spellParametersFactory, gameTableManager, cinematicFactory)
        {
            // this run's madness
            picked = [.. Nightmares.OrderBy(_ => Random.Shared.Next()).Take(3)];
            picked.UnionWith(CrazedCrew.Where(c => c != 46672).OrderBy(_ => Random.Shared.Next()).Take(2));
            if (picked.Contains(46671))
                picked.Add(46672);
            picked.UnionWith(HauntedObjects.OrderBy(_ => Random.Shared.Next()).Take(3));
            picked.UnionWith(Herds[Random.Shared.Next(Herds.Length)]);

            excluded = [.. Nightmares.Concat(CrazedCrew).Concat(HauntedObjects).Concat(Livestock).Where(c => !picked.Contains(c))];
            theme = Themes[Random.Shared.Next(Themes.Length)];
        }

        protected override bool IsExcluded(uint creatureId) => excluded.Contains(creatureId);

        protected override void OnPhase(uint phase)
        {
            switch ((Phase)phase)
            {
                case Phase.TalkToTero:
                    Activate(Objective.Datapads);
                    if (IsVeteran)
                    {
                        Activate(Objective.GoldMedalTimer);
                        Activate(Objective.Experiments);
                    }
                    break;
                case Phase.Airlock:
                    OpenDoor(AirlockOuterDoor);
                    OpenDoor(AirlockInnerDoor);
                    ActivateForPlayers(Objective.Airlock);
                    AddTrigger(5512, Airlock, 5.5f);
                    break;
                case Phase.TalkToBarmy:
                    Activate(Objective.TalkToBarmy);
                    break;
                case Phase.ObservationComputer:
                    OpenDoor(StationDoor);
                    Activate(Objective.ObservationComputer);
                    break;
                case Phase.PanickedWorkers:
                    Activate(Objective.PanickedWorkers, (uint)Math.Clamp(Workers().Count(), 1, 8));
                    CheckWorkers();
                    break;
                case Phase.EnterLab:
                    OpenDoor(LabDoor);
                    ActivateForPlayers(Objective.EnterLab);
                    AddTrigger(5531, LabEntrance, 17f);
                    break;
                case Phase.LabWaves:
                    Activate(Objective.LabWaves);
                    NextLabWave();
                    break;
                case Phase.HazmatCloset:
                    Activate(Objective.HazmatCloset);
                    break;
                case Phase.HazmatSuit:
                    OpenDoor(HazmatDoor);
                    Activate(Objective.HazmatSuit);
                    break;
                case Phase.Livestock:
                    foreach (Vector3 door in PasturesDoors)
                        OpenDoor(door);
                    Activate(Objective.Livestock, 10u);
                    Activate(Objective.AirHelms, 6u);
                    Activate(Objective.VolatileRowsdowers);
                    break;
                case Phase.AirScrubber:
                    OpenDoor(ScrubberDoor);
                    Activate(Objective.AirScrubber);
                    break;
                case Phase.ScrubberWaves:
                    Activate(Objective.ScrubberWaves);
                    NextScrubberWave();
                    break;
                case Phase.EngineeringComputer:
                    Activate(Objective.EngineeringComputer);
                    break;
            }
        }

        private void OpenDoor(Vector3 position)
        {
            IDoorEntity door = GetEntities<IDoorEntity>(VentureDoor)
                .OrderBy(d => Vector3.DistanceSquared(d.Position, position))
                .FirstOrDefault();
            if (door != null && Vector3.Distance(door.Position, position) < 3f)
                door.OpenDoor();
        }

        private IEnumerable<IUnitEntity> Workers()
        {
            return GetEntities<IUnitEntity>(PanickedWorker).Concat(GetEntities<IUnitEntity>(PanickedWorkerFemale));
        }

        /// <summary>
        /// A panicked worker is saved once no nightmare is left near them.
        /// </summary>
        private void CheckWorkers()
        {
            if ((Phase)publicEvent.Phase != Phase.PanickedWorkers)
                return;

            List<IUnitEntity> nightmares = Nightmares.SelectMany(GetEntities<IUnitEntity>).Where(n => n.IsAlive).ToList();
            foreach (IUnitEntity worker in Workers())
            {
                if (savedWorkers.Contains(worker.Guid))
                    continue;
                if (nightmares.Any(n => Vector3.Distance(n.Position, worker.Position) < 12f))
                    continue;

                savedWorkers.Add(worker.Guid);
                Update(Objective.PanickedWorkers);
            }
        }

        /// <summary>
        /// The laboratory's hallucinations, madness grows with each wave: blazing crewmen, haunted objects, smoke and eye nightmares, then
        /// Exact Change 3.0. Veteran: an escaped experiment shows up with each of the first three waves.
        /// </summary>
        private void NextLabWave()
        {
            labWave++;
            uint[] creatures = labWave switch
            {
                1 => [BlazingCrewman, BlazingCrewman, BlazingCrewman],
                2 => [EnragedLamp, EnragedLamp, MaskedArachnid, MaskedArachnid],
                3 => [SmokeNightmare, SmokeNightmare, EyeNightmare, EyeNightmare],
                _ => [ExactChange]
            };
            Activate(labWave switch { 1 => Objective.LabWave1, 2 => Objective.LabWave2, 3 => Objective.LabWave3, _ => Objective.LabBoss });
            SpawnWave(creatures, Lab, 10f);

            if (IsVeteran && labWave <= 3)
            {
                float angle = (float)(Random.Shared.NextDouble() * MathF.Tau);
                SpawnCreature(ExperimentCreatures[labWave - 1], Surface(Lab.X + MathF.Cos(angle) * 15f, Lab.Z + MathF.Sin(angle) * 15f, Lab.Y), angle);
            }
        }

        private void NextScrubberWave()
        {
            scrubberWave++;
            uint[] creatures = scrubberWave switch
            {
                1 => [SmokeNightmare, EyeNightmare, BlazingCrewman],
                6 => [.. theme[4], .. theme[3]],
                _ => [.. theme[scrubberWave - 2], .. theme[scrubberWave - 2]]
            };
            SpawnWave(creatures, Scrubbers, 14f);
        }

        private void SpawnWave(uint[] creatures, Vector3 centre, float radius)
        {
            wave.Clear();
            for (int i = 0; i < creatures.Length; i++)
            {
                float angle = MathF.Tau / creatures.Length * i;
                IWorldEntity unit = SpawnCreature(creatures[i], Surface(centre.X + MathF.Cos(angle) * radius, centre.Z + MathF.Sin(angle) * radius, centre.Y), angle + MathF.PI, AggroNearestPlayer);
                if (unit != null)
                    wave.Add(unit);
            }
        }

        protected override void OnObjectiveSucceeded(uint objectiveId, IPublicEventTeam team)
        {
            switch (objectiveId)
            {
                case Objective.TalkToTero:
                    publicEvent.SetPhase(Phase.Airlock);
                    break;
                case Objective.Airlock:
                    publicEvent.SetPhase(Phase.TalkToBarmy);
                    break;
                case Objective.TalkToBarmy:
                    publicEvent.SetPhase(Phase.ObservationComputer);
                    break;
                case Objective.ObservationComputer:
                    publicEvent.SetPhase(Phase.PanickedWorkers);
                    break;
                case Objective.PanickedWorkers:
                    publicEvent.SetPhase(Phase.EnterLab);
                    break;
                case Objective.EnterLab:
                    publicEvent.SetPhase(Phase.LabWaves);
                    break;
                case Objective.LabWave1:
                case Objective.LabWave2:
                case Objective.LabWave3:
                    Update(Objective.LabWaves);
                    Delay(5d, NextLabWave);
                    break;
                case Objective.LabBoss:
                    Update(Objective.LabWaves);
                    break;
                case Objective.LabWaves:
                    publicEvent.SetPhase(Phase.HazmatCloset);
                    break;
                case Objective.HazmatCloset:
                case Objective.HazmatClosetOld:
                    if ((Phase)publicEvent.Phase == Phase.HazmatCloset)
                        publicEvent.SetPhase(Phase.HazmatSuit);
                    break;
                case Objective.HazmatSuit:
                    publicEvent.SetPhase(Phase.Livestock);
                    break;
                case Objective.Livestock:
                    publicEvent.SetPhase(Phase.AirScrubber);
                    break;
                case Objective.AirScrubber:
                    publicEvent.SetPhase(Phase.ScrubberWaves);
                    break;
                case Objective.ScrubberWaves:
                    publicEvent.SetPhase(Phase.EngineeringComputer);
                    break;
                case Objective.EngineeringComputer:
                    publicEvent.SetPhase(Phase.Done);
                    PlayCinematic(CinematicSceneId.SpaceMadnessDeparture);
                    Complete(team);
                    break;
                case Objective.Slank:
                case Objective.Rockmite:
                case Objective.Downgrazer:
                    Update(Objective.Experiments);
                    break;
            }
        }

        private void Complete(IPublicEventTeam team)
        {
            bool gold = IsVeteran && !IsFailed(team, Objective.GoldMedalTimer) && IsSucceeded(team, Objective.Experiments, Objective.Datapads)
                && !failRowsdowers;

            // Save Space Station Venture, the prime dailies; Space Madness complete, Prime, Gold Medal (Prime)
            CompleteExpedition([19348], [18938, 18939], 3716, 6024, 6009, gold);
        }

        public override void OnEntityDeath(IUnitEntity entity)
        {
            uint id = entity.CreatureId;
            switch ((Phase)publicEvent.Phase)
            {
                case Phase.PanickedWorkers:
                    CheckWorkers();
                    break;
                case Phase.LabWaves when wave.Contains(entity) && wave.All(w => w == entity || w is not IUnitEntity u || !u.IsAlive):
                    Update(labWave switch { 1 => Objective.LabWave1, 2 => Objective.LabWave2, 3 => Objective.LabWave3, _ => Objective.LabBoss });
                    break;
                case Phase.Livestock when Livestock.Any(l => GetCreature(l) == id):
                    Update(Objective.Livestock);
                    break;
                case Phase.ScrubberWaves when wave.Contains(entity) && wave.All(w => w == entity || w is not IUnitEntity u || !u.IsAlive):
                    Update(Objective.ScrubberWaves);
                    if (scrubberWave < 6)
                        Delay(5d, NextScrubberWave);
                    break;
            }
        }

        public override void OnEntityActivated(IWorldEntity entity, IPlayer activator)
        {
            uint id = entity.CreatureId;
            if (id == HazmatSuitObject && (Phase)publicEvent.Phase == Phase.HazmatSuit && !suitTaken)
            {
                suitTaken = true;
                Update(Objective.HazmatSuit);
                return;
            }

            // an emergency air helm calms a hallucinating worker
            if ((id == GetCreature(HallucinatingWorker) || id == GetCreature(HallucinatingWorkerFemale)) && calmedWorkers.Add(entity.Guid))
            {
                Update(Objective.AirHelms);
                if (entity is IUnitEntity worker)
                    worker.SetFaction((Game.Static.Reputation.Faction)219);
            }

            // escaped experiments are collected
            if (ExperimentCreatures.Contains(id) && entity.InWorld)
                entity.RemoveFromMap();
        }

        protected override void OnCheck()
        {
            // the volatile rowsdowers blow up on players who get too close
            if ((Phase)publicEvent.Phase != Phase.Livestock)
                return;

            foreach (IUnitEntity rowsdower in GetEntities<IUnitEntity>(Rowsdower).Where(r => r.IsAlive).ToList())
            {
                if (!AnyPlayerNear(rowsdower.Position, 2.5f))
                    continue;

                CastOnSelf(rowsdower, 79054); // Plague Burst
                rowsdower.ModifyHealth(rowsdower.Health, Game.Static.Spell.DamageType.Physical, null);
                failRowsdowers = true;
            }
        }

        private bool failRowsdowers;
    }
}
