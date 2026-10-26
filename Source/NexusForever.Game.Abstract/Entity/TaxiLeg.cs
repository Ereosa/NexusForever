using System.Numerics;
using NexusForever.Game.Static.Entity.Movement.Spline;

namespace NexusForever.Game.Abstract.Entity
{
    /// <summary>
    /// Part of a taxi ride: a path, what happens at its end and how long the taxi waits there before the next leg.
    /// </summary>
    public record TaxiLeg(IReadOnlyList<Vector3> Points, SplineType Type, float Speed, uint PauseMs = 0u, Action<IPlayer> OnReached = null);
}
