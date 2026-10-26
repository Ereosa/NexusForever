using System.Numerics;
using NexusForever.Game.Abstract.Cinematic;
using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Entity.Creature;
using NexusForever.Game.Abstract.PublicEvent;
using NexusForever.Game.Abstract.Quest;
using NexusForever.Game.Abstract.Spell;
using NexusForever.Game.Static.Entity;
using NexusForever.GameTable;
using NexusForever.Script.Template.Filter;
using NexusForever.Shared;

namespace NexusForever.Script.Instance.Expedition.RageLogic
{
    /// <summary>
    /// Rage Logic, the Positron (public event 213): choose an assault vehicle in the hangar, then the Positron's emitters put the strike
    /// team down on Asteroid DU-23 (public event 214).
    /// </summary>
    /// <remarks>
    /// The three speeders and their places in the hangar come from the archived Jabbithole sightings. Guessed: the vehicle is chosen by
    /// boarding one, the teleport down a few seconds later with the chosen speeders waiting at the landing, and players who stay in the hangar
    /// for half a minute without boarding go down anyway (vehicle riding and weapons have no captured packets to check against).
    /// </remarks>
    [ScriptFilterOwnerId(213)]
    public class RageLogicLaunchEventScript : ExpeditionEventScript
    {
        private const uint ChooseVehicle = 781;

        private static readonly Vector3 Hangar = new(1250.673f, 9.056f, -1789.085f); // WorldLocation2 24890, objective 781

        // Rocket Launcher, Electro-Magnetic Pulse and Rail Gun speeders, two of each along the hangar's launch side
        private static readonly (uint Creature, Vector3 Position)[] Speeders =
        [
            (32249, new(1252f, 9.056f, -1788f)), (32249, new(1252f, 9.056f, -1796f)),
            (43809, new(1251f, 9.056f, -1804f)), (43809, new(1251f, 9.056f, -1780f)),
            (43810, new(1251f, 9.056f, -1814f)), (43810, new(1252f, 9.056f, -1770f))
        ];

        protected override uint WorldId => 1627;
        protected override uint InitialPhase => 0u;
        protected override IReadOnlyDictionary<uint, uint> VeteranCreatures => new Dictionary<uint, uint>();

        private readonly Dictionary<ulong, uint> chosen = [];
        private double hangarTime;
        private bool launched;

        public RageLogicLaunchEventScript(
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
            if (phase != 0u)
                return;

            Activate(ChooseVehicle);
            foreach ((uint creature, Vector3 position) in Speeders)
                SpawnCreature(creature, position, -MathF.PI / 2f);
        }

        /// <summary>
        /// A player boarded one of the hangar speeders.
        /// </summary>
        public void OnSpeederBoarded(IVehicleEntity speeder, IPlayer player)
        {
            if (launched)
                return;

            if (speeder.GetPassenger(VehicleSeatType.Pilot, 0) != null)
                return;

            chosen[player.CharacterId] = speeder.CreatureId;
            speeder.EnqueuePassengerAdd(player, VehicleSeatType.Pilot, 0);
            Launch();
        }

        private void Launch()
        {
            if (launched)
                return;

            launched = true;
            Update(ChooseVehicle);

            Delay(4d, () =>
            {
                foreach (IVehicleEntity speeder in Speeders.Select(s => s.Creature).Distinct().SelectMany(GetEntities<IVehicleEntity>))
                    foreach (IPlayer player in Players.Where(p => speeder.GetPassenger(p.Guid) != null).ToList())
                        speeder.PassengerRemove(player);

                IPublicEvent assault = mapInstance.PublicEventManager.GetEvent(214u);
                assault?.InvokeScriptCollection<RageLogicEventScript>(s => s.StartAssault(chosen));
            });
        }

        protected override void OnObjectiveSucceeded(uint objectiveId, IPublicEventTeam team)
        {
        }

        protected override void OnTick(double lastTick)
        {
            if (launched)
                return;

            if (!AnyPlayerNear(Hangar, 27f))
            {
                hangarTime = 0d;
                return;
            }

            hangarTime += lastTick;
            if (hangarTime >= 30d)
                Launch();
        }
    }
}
