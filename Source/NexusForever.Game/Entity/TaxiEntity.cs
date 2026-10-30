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

        /// <summary>
        /// Test modes for driving flights, set per character with the "taxi mode" command while the client side of the flight
        /// is worked out (reports 43 and 53): "spline" (tbl spline, default), "path" (linear custom path), "control" (the rider's
        /// client controls the taxi), "late" (the spline starts 2 seconds after the rider is seated).
        /// </summary>
        public static readonly System.Collections.Concurrent.ConcurrentDictionary<ulong, string> TestModes = new();

        private string mode = "spline";
        private bool riderControls;

        private readonly Queue<TaxiLeg> legs = new();
        private TaxiLeg currentLeg;
        private double pauseTimer;
        private double legTimer;
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
            mode                     = TestModes.TryGetValue(owner.CharacterId, out string testMode) ? testMode : "spline";

            // flights ride the route's tbl spline: the client only moves a taxi along a tbl spline ("Embark_NoSplineForTaxi"),
            // custom paths left the taxi standing still on the client (reports 43 and 53)
            foreach (ushort splineId in splineIds)
                legs.Enqueue(new TaxiLeg(Taxi.TaxiRouteSplines.GetPoints(splineId), Taxi.TaxiRouteSplines.GetType(splineId), FlightSpeed, SplineId: splineId));
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

            if (mode == "control")
            {
                player.SetControl(this);
                riderControls = true;
            }

            if (mode == "late")
                pauseTimer = 2d;
            else
                LaunchNextSpline();
        }

        protected override void OnPassengerRemove(IPlayer player, VehicleSeatType seatType, byte seatPosition)
        {
            // rider logged out (already saved at the destination) or was removed mid-flight, the taxi is removed with its last passenger
            if (player.Guid != OwnerGuid)
                return;

            flying = false;
            player.TaxiManager.OnLanded();

            if (riderControls)
            {
                riderControls = false;
                player.SetControl(player);
            }
        }

        private void LaunchNextSpline()
        {
            while (legs.TryDequeue(out TaxiLeg leg))
            {
                currentLeg = leg;
                if (leg.SplineId != 0 && mode != "path")
                {
                    MovementManager.LaunchSpline(leg.SplineId, SplineMode.OneShot, leg.Speed, false);

                    // the client reaches the end of a tbl spline after its length at the leg speed, the server's own spline
                    // finishes seconds later, the rider is landed when the client's taxi arrives
                    legTimer = GetLength(leg.Points) / leg.Speed;
                    return;
                }

                if (leg.Points.Count >= 2 && leg.Points[0] != leg.Points[^1])
                {
                    // custom paths always go out as linear: the client doesn't move a unit along a Catmull-Rom SetPositionPath
                    // (report 43, the taxi stood still and the rider snapped to the destination on landing), every custom path
                    // the client is known to follow is linear and the tbl points are dense enough to keep the shape of the route
                    MovementManager.LaunchSpline(leg.Points.ToList(), SplineType.Linear, SplineMode.OneShot, leg.Speed);
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

        private static double GetLength(IReadOnlyList<Vector3> points)
        {
            double length = 0d;
            for (int i = 1; i < points.Count; i++)
                length += Vector3.Distance(points[i - 1], points[i]);
            return length;
        }

        public override void Update(double lastTick)
        {
            base.Update(lastTick);

            if (flying && legTimer > 0d && !PendingRemoval)
            {
                legTimer -= lastTick;
                if (legTimer <= 0d)
                    FinishLeg();
            }

            if (!flying || pauseTimer <= 0d)
                return;

            pauseTimer -= lastTick;
            if (pauseTimer <= 0d && !PendingRemoval)
                LaunchNextSpline();
        }

        public override void OnEntityCommandFinalise(IEntityCommand command)
        {
            base.OnEntityCommandFinalise(command);

            if (flying && !PendingRemoval && command is PositionPathCommand { IsFinalised: true } or PositionSplineCommand { IsFinalised: true })
                FinishLeg();
        }

        private void FinishLeg()
        {
            // whichever comes first, the leg timer or the server's spline
            if (currentLeg == null)
                return;

            legTimer = 0d;
            bool paused = OnLegFinished();
            currentLeg = null;
            if (!paused)
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

            player.TaxiManager.OnLanded();

            // the taxi is removed with its last passenger
            PassengerRemove(player);

            player.Rotation = DestinationRotation;
            player.TeleportToLocal(Destination, false);

            onFinished?.Invoke(player);
        }
    }
}
