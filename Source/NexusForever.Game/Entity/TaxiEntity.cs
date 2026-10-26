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

        private readonly Queue<ushort> splines = new();
        private Vector3 destination;
        private Vector3 destinationRotation;
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
            this.destination         = destination;
            this.destinationRotation = destinationRotation;

            foreach (ushort splineId in splineIds)
                splines.Enqueue(splineId);
        }

        protected override IEntityModel BuildEntityModel()
        {
            return new TaxiEntityModel
            {
                CreatureId    = CreatureId,
                UnitVehicleId = (ushort)VehicleEntry.Id,
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

        private void LaunchNextSpline()
        {
            if (splines.TryDequeue(out ushort splineId))
                MovementManager.LaunchSpline(splineId, SplineMode.OneShot, FlightSpeed, false);
            else
                Land();
        }

        public override void OnEntityCommandFinalise(IEntityCommand command)
        {
            base.OnEntityCommandFinalise(command);

            if (flying && command is PositionSplineCommand { IsFinalised: true })
                LaunchNextSpline();
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

            // the taxi is removed with its last passenger
            PassengerRemove(player);

            player.Rotation = destinationRotation;
            player.TeleportToLocal(destination, false);
        }
    }
}
