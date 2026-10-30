using System.Numerics;

namespace NexusForever.Game.Abstract.Map
{
    public interface IGridActionRelocate : IGridAction
    {
        Vector3 Vector { get; init; }
        OnRelocateDelegate Callback { get; init; }

        /// <summary>
        /// Determines if the relocate is the single pending relocate of the entity, the map keeps the latest position for it.
        /// </summary>
        bool Coalesced { get; init; }
    }
}