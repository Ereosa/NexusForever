using System.Numerics;
using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Entity.Creature;
using NexusForever.Game.Abstract.Entity.Movement;
using NexusForever.Game.Abstract.Entity.Movement.Command;
using NexusForever.Game.Entity.Movement.Command.Position;
using NexusForever.Game.Static.Entity;
using NexusForever.Game.Static.Entity.Movement.Spline;
using NexusForever.Network.World.Entity;
using NexusForever.Network.World.Entity.Model;

namespace NexusForever.Game.Entity
{
    /// <summary>
    /// Taxi vehicle that carries a player along the splines of the taxi routes they bought and lands them at the destination node.
    /// </summary>
    public class TaxiEntity : VehicleEntity, ITaxiEntity
    {
        public override EntityType Type => EntityType.Taxi;

        /// <summary>
        /// Flight speed, the settler mini-taxi creatures are described as "Speed 30".
        /// </summary>
        private const float FlightSpeed = 30f;

        public uint OwnerGuid { get; private set; }

        private readonly Queue<TaxiLeg> legs = new();
        private TaxiLeg currentLeg;
        private double pauseTimer;
        private Action<IPlayer> onFinished;
        public Vector3 Destination { get; private set; }
        public Vector3 DestinationRotation { get; private set; }

        private bool flying;

        #region Dependency Injection

        public TaxiEntity(IMovementManager movementManager,
            IEntitySummonFactory entitySummonFactory)
            : base(movementManager, entitySummonFactory)
        {
        }

        #endregion

        /// <summary>
        /// Initialise the taxi for <see cref="IPlayer"/> to fly along the supplied splines and land at the supplied destination.
        /// </summary>
        public void Initialise(IPlayer owner, ICreatureInfo creatureInfo, IReadOnlyList<ushort> splineIds, Vector3 destination, Vector3 destinationRotation)
        {
            Initialise(creatureInfo, 0u, 0u);

            OwnerGuid                = owner.Guid;
            Rotation                 = owner.Rotation;
            Destination              = destination;
            DestinationRotation      = destinationRotation;

            // the tbl splines carry client events at their ends (98 and 99) and the client drops the connection handling them
            // on a taxi ("Failure Handling Network message 3610"), so the same points are sent as a custom path instead
            foreach (ushort splineId in splineIds)
                legs.Enqueue(new TaxiLeg(Taxi.TaxiRouteSplines.GetPoints(splineId), Taxi.TaxiRouteSplines.GetType(splineId), FlightSpeed));
        }

        /// <summary>
        /// Initialise the taxi for <see cref="IPlayer"/> to ride the supplied legs (a guided tour) and drop them at the supplied destination.
        /// </summary>
        public void Initialise(IPlayer owner, ICreatureInfo creatureInfo, IReadOnlyList<TaxiLeg> tourLegs, Vector3 destination, Vector3 destinationRotation, Action<IPlayer> onFinished)
        {
            Initialise(creatureInfo, 0u, 0u);

            OwnerGuid           = owner.Guid;
            Rotation            = owner.Rotation;
            Destination         = destination;
            DestinationRotation = destinationRotation;
            this.onFinished     = onFinished;

            foreach (TaxiLeg leg in tourLegs)
                legs.Enqueue(leg);
        }

        protected override IEntityModel BuildEntityModel()
        {
            return new TaxiEntityModel
            {
                CreatureId    = CreatureId,
                // taxis spawned from the world database (quest taxis, flavour objects) never get a vehicle entry
                UnitVehicleId = (ushort)(VehicleEntry?.Id ?? CreatureInfo?.Entry?.UnitVehicleId ?? 0u),
                OwnerId       = OwnerGuid,
                Passengers    = this
                    .Select(p => new TaxiEntityModel.Passenger
                    {
                        SeatType     = (byte)p.SeatType,
                        SeatPosition = p.SeatPosition,
                        UnitId       = p.Guid
                    })
                    .ToList()
            };
        }

        protected override void OnPassengerAdd(IPlayer player, VehicleSeatType seatType, byte seatPosition)
        {
            if (player.Guid != OwnerGuid || flying)
                return;

            flying = true;
            LaunchNextSpline();
        }

        protected override void OnPassengerRemove(IPlayer player, VehicleSeatType seatType, byte seatPosition)
        {
            // rider logged out (already saved at the destination) or was removed mid-flight, the taxi is removed with its last passenger
            if (player.Guid != OwnerGuid)
                return;

            flying = false;
            player.TaxiManager.OnLanded();
        }

        private void LaunchNextSpline()
        {
            while (legs.TryDequeue(out TaxiLeg leg))
            {
                currentLeg = leg;
                if (leg.Points.Count >= 2 && leg.Points[0] != leg.Points[^1])
                {
                    MovementManager.LaunchSpline(leg.Points.ToList(), leg.Type, SplineMode.OneShot, leg.Speed);
                    return;
                }

                // a leg without movement (a stop right after another one)
                if (OnLegFinished())
                    return;
            }

            currentLeg = null;
            Land();
        }

        /// <summary>
        /// Run the end of the current leg, returns true if the taxi now waits before the next leg.
        /// </summary>
        private bool OnLegFinished()
        {
            TaxiLeg leg = currentLeg;
            if (leg == null)
                return false;

            if (leg.OnReached != null && GetVisible<IPlayer>(OwnerGuid) is IPlayer player)
                leg.OnReached(player);

            if (leg.PauseMs == 0u)
                return false;

            pauseTimer = leg.PauseMs / 1000d;
            return true;
        }

        public override void Update(double lastTick)
        {
            base.Update(lastTick);

            if (!flying || pauseTimer <= 0d)
                return;

            pauseTimer -= lastTick;
            if (pauseTimer <= 0d && !PendingRemoval)
                LaunchNextSpline();
        }

        public override void OnEntityCommandFinalise(IEntityCommand command)
        {
            base.OnEntityCommandFinalise(command);

            if (flying && !PendingRemoval && command is PositionPathCommand { IsFinalised: true })
            {
                if (!OnLegFinished())
                    LaunchNextSpline();
            }
        }

        private void Land()
        {
            flying = false;

            IPlayer player = GetVisible<IPlayer>(OwnerGuid);
            if (player == null)
            {
                RemoveFromMap();
                return;
            }

            player.TaxiManager.OnLanded();

            // the taxi is removed with its last passenger
            PassengerRemove(player);

            player.Rotation = DestinationRotation;
            player.TeleportToLocal(Destination, false);

            onFinished?.Invoke(player);
        }
    }
}
