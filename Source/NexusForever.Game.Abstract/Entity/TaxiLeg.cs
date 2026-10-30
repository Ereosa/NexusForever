using System.Numerics;
using NexusForever.Game.Static.Entity.Movement.Spline;

namespace NexusForever.Game.Abstract.Entity
{
    /// <summary>
    /// Part of a taxi ride: a path, what happens at its end and how long the taxi waits there before the next leg.
    /// </summary>
    /// <remarks>
    /// A leg with a <paramref name="SplineId"/> rides that tbl spline (SetPositionSpline), the points are only used to time the leg.
    /// A leg that doesn't <paramref name="Launch"/> continues the spline of the previous leg, it only times the next part of it.
    /// </remarks>
    public record TaxiLeg(IReadOnlyList<Vector3> Points, SplineType Type, float Speed, uint PauseMs = 0u, Action<IPlayer> OnReached = null, ushort SplineId = 0, bool Launch = true);
}
