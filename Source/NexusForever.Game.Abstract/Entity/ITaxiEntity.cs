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
    }
}
