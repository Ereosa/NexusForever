using System.Numerics;
using NexusForever.Game.Abstract.Entity.Creature;

namespace NexusForever.Game.Abstract.Entity
{
    public interface ITaxiEntity : IVehicleEntity
    {
        /// <summary>
        /// Position and rotation the rider lands at, also saved if the rider logs out mid-flight.
        /// </summary>
        Vector3 Destination { get; }
        Vector3 DestinationRotation { get; }

        /// <summary>
        /// Initialise the taxi for <see cref="IPlayer"/> to fly along the supplied splines and land at the supplied destination.
        /// </summary>
        void Initialise(IPlayer owner, ICreatureInfo creatureInfo, IReadOnlyList<ushort> splineIds, Vector3 destination, Vector3 destinationRotation);

        /// <summary>
        /// Initialise the taxi for <see cref="IPlayer"/> to ride the supplied legs (a guided tour) and drop them at the supplied destination.
        /// </summary>
        /// <param name="onFinished">Invoked once the rider has been dropped at the destination, not if the ride is cut short.</param>
        void Initialise(IPlayer owner, ICreatureInfo creatureInfo, IReadOnlyList<TaxiLeg> legs, Vector3 destination, Vector3 destinationRotation, Action<IPlayer> onFinished);
    }
}
