using System.Numerics;
using NexusForever.Game.Abstract.Cinematic;
using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Entity.Creature;
using NexusForever.Game.Abstract.PublicEvent;
using NexusForever.Game.Abstract.Quest;
using NexusForever.Game.Abstract.Spell;
using NexusForever.Game.Static.Cinematic;
using NexusForever.Game.Static.PublicEvent;
using NexusForever.Game.Static.Reputation;
using NexusForever.GameTable;
using NexusForever.Script.Template.Filter;
using NexusForever.Shared;

namespace NexusForever.Script.Instance.Expedition.RageLogic
{
    /// <summary>
    /// Rage Logic, Asteroid DU-23 (public event 214, world 1627): Captain Switch's Freebots strike the asteroid where Axiom spreads the
    /// Rage Logic virus.
    /// </summary>
    /// <remarks>
    /// No packet captures exist. The order follows the objective data: obliterate the ragebots on the surface, destroy the asteroid's three
    /// engines, get into the Ragebot factory, wreck it (generators, assembly stations and transmitters), cure the infected Freebots and take
    /// out its defenders, (veteran) destroy the broadcast transmitter, hack the signal relay, take the teleporter down to the server room,
    /// enter Axiom's control room and destroy Axiom, then the departure cinematic and the escape pods.
    /// Positions and abilities come from archived Jabbithole data and the client world locations, the cutscenes from the client (GC183
    /// arrival, GC184 Axiom's introduction, GC185 departure).
    /// Guessed: what counts as a ragebot kill and as curing a Freebot (killing the infected ones and their infector bots), the mine doors
    /// opening when the engines are gone, the teleporter pad and its arrival point, Axiom's introduction played on landing, the overseerbots
    /// counting as simultaneous when they die within ten seconds, Axiom's trash crawlers (his boss adds) at 75, 50 and 25% health. Hacking drillers and ramming with hoverbikes aren't tracked.
    /// </remarks>
    [ScriptFilterOwnerId(214)]
    public class RageLogicEventScript : ExpeditionEventScript
    {
        private enum Phase : uint
        {
            Launch,
            Surface,
            Engines,
            EnterFactory,
            Factory,
            BroadcastTransmitter,
            SignalRelay,
            Teleporter,
            ControlRoom,
            Axiom,
            Done
        }

        private static class Objective
        {
            public const uint ObliterateRagebots  = 783;
            public const uint DestroyEngines      = 784;
            public const uint EnterFactory        = 786;
            public const uint DisableFactory      = 787;
            public const uint CureFreebots        = 788;
            public const uint FactoryDefenders    = 789;
            public const uint EnterServerRoom     = 790;
            public const uint DestroyAxiom        = 791;
            public const uint Scanners            = 1493;
            public const uint Overseerbots        = 1499;
            public const uint HackSignalRelay     = 1529;
            public const uint BroadcastTransmitter = 4691;
            public const uint GoldMedalTimer      = 4704;
            public const uint EnterControlRoom    = 5107;
        }

        private static readonly Vector3 Landing        = new(-297.075f, -709.762f, -35.905f);  // WorldLocation2 24756, the event's location
        private static readonly Vector3 FactoryEntrance = new(213.835f, -759.266f, -106.409f); // WorldLocation2 36417, objective 786
        private static readonly Vector3 TeleporterPad  = new(94.174f, -814.865f, -90.450f);    // WorldLocation2 24994, beside the signal relay
        private static readonly Vector3 ServerRoom     = new(-364.342f, -830.094f, -63.764f);  // WorldLocation2 24962
        private static readonly Vector3 ControlRoom    = new(-214.805f, -837.271f, -65.703f);  // WorldLocation2 51033, objective 5107

        private static readonly Dictionary<uint, uint> Veteran = new()
        {
            { 32323, 69094 }, { 32322, 69095 }, { 32302, 69096 }, { 32324, 69097 }, { 32303, 69098 }, { 32326, 69099 }, { 32325, 69100 },
            { 32368, 69101 }, { 32370, 69102 }, { 43273, 69103 }, { 43276, 69104 }, { 43284, 69105 }, { 43827, 69106 }, { 44232, 69107 },
            { 43879, 69179 }
        };

        private static readonly uint[] VeteranOnly = [69807];

        // the Rage Logic bots, normal and veteran
        private static readonly HashSet<uint> Ragebots =
        [
            32302, 32303, 32322, 32324, 32325, 43273, 43276, 43284, 43827, 44232, 43879,
            69095, 69096, 69097, 69098, 69100, 69103, 69104, 69105, 69106, 69107, 69179
        ];

        // infected Freebots, Freebots hosting an infector bot and the infector bots themselves
        private static readonly HashSet<uint> Infected = [32323, 32368, 32370, 69094, 69101, 69102];

        private const uint Axiom          = 32326;
        private const uint Scanner        = 43827;
        private const uint Overseerbot    = 44232;
        private const uint Transmitter    = 69807;
        private const uint MineDoors      = 44410;
        private const uint SpeederRocket  = 32249;
        private const uint TrashCrawler   = 43879;

        protected override uint WorldId => 1627;
        protected override uint InitialPhase => (uint)Phase.Launch;
        protected override IReadOnlyDictionary<uint, uint> VeteranCreatures => Veteran;
        protected override IReadOnlyCollection<uint> VeteranOnlyCreatures => VeteranOnly;

        private double firstOverseerDeath = -1d;
        private int overseersDead;
        private int overseerCount;
        private double clock;
        private int crawlerWaves;

        public RageLogicEventScript(
            IGlobalQuestManager globalQuestManager,
            ICreatureInfoManager creatureInfoManager,
            IEntityFactory entityFactory,
            IFactory<ISpellParameters> spellParametersFactory,
            IGameTableManager gameTableManager,
            ICinematicFactory cinematicFactory)
            : base(globalQuestManager, creatureInfoManager, entityFactory, spellParametersFactory, gameTableManager, cinematicFactory)
        {
        }

        /// <summary>
        /// The strike team chose its vehicles on the Positron, the emitters put everyone down on the asteroid.
        /// </summary>
        public void StartAssault(IReadOnlyDictionary<ulong, uint> vehicles)
        {
            if ((Phase)publicEvent.Phase != Phase.Launch)
                return;

            int i = 0;
            foreach (IPlayer player in Players.ToList())
            {
                float angle = MathF.Tau / 5f * i++;
                Vector3 position = Surface(Landing.X + MathF.Cos(angle) * 3f, Landing.Z + MathF.Sin(angle) * 3f, Landing.Y);
                player.TeleportToLocal(position);

                // the chosen speeder waits beside its rider
                uint speeder = vehicles.TryGetValue(player.CharacterId, out uint creature) ? creature : SpeederRocket;
                SpawnCreature(speeder, Surface(position.X + 3f, position.Z, position.Y), 0f);
            }

            publicEvent.SetPhase(Phase.Surface);
            Delay(3d, () => PlayCinematic(CinematicSceneId.RageLogicBossIntro));
        }

        public override void OnAddToMap(IGridEntity entity)
        {
            base.OnAddToMap(entity);

            // players joining after the launch go straight down to the asteroid
            if (entity is IPlayer player && (Phase)publicEvent.Phase != Phase.Launch)
                Delay(2d, () => { if (player.InWorld && player.Map == mapInstance) player.TeleportToLocal(Landing); });
        }

        protected override void OnPhase(uint phase)
        {
            switch ((Phase)phase)
            {
                case Phase.Surface:
                    Activate(Objective.ObliterateRagebots);
                    Activate(Objective.Scanners, (uint)Math.Max(1, GetEntities<IUnitEntity>(Scanner).Count()));
                    overseerCount = GetEntities<IUnitEntity>(Overseerbot).Count();
                    if (overseerCount > 0)
                        Activate(Objective.Overseerbots, (uint)overseerCount);
                    if (IsVeteran)
                        Activate(Objective.GoldMedalTimer);
                    break;
                case Phase.Engines:
                    Activate(Objective.DestroyEngines);
                    break;
                case Phase.EnterFactory:
                    // the engines are gone, the mine doors into the factory give way
                    foreach (IWorldEntity door in GetEntities<IWorldEntity>(MineDoors).ToList())
                        door.RemoveFromMap();
                    Activate(Objective.EnterFactory);
                    AddTrigger(3908, FactoryEntrance, 12f);
                    break;
                case Phase.Factory:
                    Activate(Objective.DisableFactory);
                    Activate(Objective.CureFreebots);
                    Activate(Objective.FactoryDefenders);
                    break;
                case Phase.BroadcastTransmitter:
                    Activate(Objective.BroadcastTransmitter);
                    foreach (IUnitEntity transmitter in GetEntities<IUnitEntity>(Transmitter))
                        AggroNearestPlayer(mapInstance, transmitter.Guid, transmitter.Position);
                    break;
                case Phase.SignalRelay:
                    Activate(Objective.HackSignalRelay);
                    break;
                case Phase.Teleporter:
                    Activate(Objective.EnterServerRoom);
                    AddTrigger(3910, ServerRoom, 10f);
                    break;
                case Phase.ControlRoom:
                    ActivateForPlayers(Objective.EnterControlRoom);
                    AddTrigger(8418, ControlRoom, 15f);
                    break;
                case Phase.Axiom:
                    Activate(Objective.DestroyAxiom);
                    foreach (IUnitEntity axiom in GetEntities<IUnitEntity>(Axiom))
                    {
                        axiom.SetFaction((Faction)233);
                        AggroNearestPlayer(mapInstance, axiom.Guid, axiom.Position);
                    }
                    break;
            }
        }

        protected override void OnObjectiveSucceeded(uint objectiveId, IPublicEventTeam team)
        {
            switch (objectiveId)
            {
                case Objective.ObliterateRagebots:
                    publicEvent.SetPhase(Phase.Engines);
                    break;
                case Objective.DestroyEngines:
                    publicEvent.SetPhase(Phase.EnterFactory);
                    break;
                case Objective.EnterFactory:
                    publicEvent.SetPhase(Phase.Factory);
                    break;
                case Objective.DisableFactory:
                case Objective.CureFreebots:
                case Objective.FactoryDefenders:
                    if (IsSucceeded(team, Objective.DisableFactory, Objective.CureFreebots, Objective.FactoryDefenders))
                        publicEvent.SetPhase(IsVeteran ? Phase.BroadcastTransmitter : Phase.SignalRelay);
                    break;
                case Objective.BroadcastTransmitter:
                    publicEvent.SetPhase(Phase.SignalRelay);
                    break;
                case Objective.HackSignalRelay:
                    publicEvent.SetPhase(Phase.Teleporter);
                    break;
                case Objective.EnterServerRoom:
                    publicEvent.SetPhase(Phase.ControlRoom);
                    break;
                case Objective.EnterControlRoom:
                    publicEvent.SetPhase(Phase.Axiom);
                    break;
                case Objective.DestroyAxiom:
                    publicEvent.SetPhase(Phase.Done);
                    PlayCinematic(CinematicSceneId.RageLogicDeparture);
                    Complete(team);
                    break;
            }
        }

        private void Complete(IPublicEventTeam team)
        {
            bool gold = IsVeteran && !IsFailed(team, Objective.GoldMedalTimer) && IsSucceeded(team, Objective.Scanners, Objective.Overseerbots);

            // Destroy the source of the Rage Logic virus (quest 6837), the prime dailies; Rage Logic complete, Prime, Gold Medal (Prime)
            CompleteExpedition([12547], [18936, 18937], 3713, 6022, 6008, gold);
        }

        public override void OnEntityDeath(IUnitEntity entity)
        {
            uint id = entity.CreatureId;
            Phase phase = (Phase)publicEvent.Phase;

            if (id == GetCreature(Scanner))
                Update(Objective.Scanners);

            if (id == GetCreature(Overseerbot))
            {
                if (firstOverseerDeath < 0d || clock - firstOverseerDeath > 10d)
                {
                    firstOverseerDeath = clock;
                    overseersDead = 0;
                }

                overseersDead++;
                if (overseersDead >= overseerCount)
                    Update(Objective.Overseerbots, overseerCount);
            }

            switch (phase)
            {
                case Phase.Surface when Ragebots.Contains(id):
                    Update(Objective.ObliterateRagebots);
                    break;
                case Phase.Factory when Infected.Contains(id):
                    Update(Objective.CureFreebots);
                    break;
                case Phase.Factory when Ragebots.Contains(id):
                    Update(Objective.FactoryDefenders);
                    break;
                case Phase.BroadcastTransmitter when id == Transmitter:
                    Update(Objective.BroadcastTransmitter);
                    break;
                case Phase.Axiom when id == GetCreature(Axiom):
                    Update(Objective.DestroyAxiom);
                    break;
            }
        }

        protected override void OnTick(double lastTick)
        {
            clock += lastTick;

            if ((Phase)publicEvent.Phase != Phase.Axiom || crawlerWaves >= 3)
                return;

            // Axiom calls trash crawlers out of the scrap at three quarters, half and a quarter of his health
            IUnitEntity axiom = GetEntities<IUnitEntity>(Axiom).FirstOrDefault(a => a.IsAlive && a.InCombat);
            if (axiom == null || axiom.Health > axiom.MaxHealth * (0.75f - 0.25f * crawlerWaves))
                return;

            crawlerWaves++;
            for (int i = 0; i < 3; i++)
            {
                float angle = MathF.Tau / 3f * i;
                SpawnCreature(TrashCrawler, Surface(axiom.Position.X + MathF.Cos(angle) * 8f, axiom.Position.Z + MathF.Sin(angle) * 8f, axiom.Position.Y), angle, AggroNearestPlayer);
            }
        }

        protected override void OnCheck()
        {
            if ((Phase)publicEvent.Phase != Phase.Teleporter)
                return;

            // the teleporter beneath the factory takes the players down to the server room
            foreach (IPlayer player in Players.Where(p => p.IsAlive && Vector3.Distance(p.Position, TeleporterPad) < 3f).ToList())
                player.TeleportToLocal(ServerRoom);
        }
    }
}
