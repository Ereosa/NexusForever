using System.Numerics;
using NexusForever.Game.Abstract.Map;
using NexusForever.Shared;

namespace NexusForever.Game.Entity.Movement.Generator
{
    /// <summary>
    /// Builds a path that follows the terrain between two points.
    /// </summary>
    /// <remarks>
    /// A path made of only its end points is a straight line through the air or the ground, the client moves the unit along it as is.
    /// </remarks>
    public static class GroundPathBuilder
    {
        private const float StepSize = 2f;
        private const int MaxNodes = 64;

        /// <summary>
        /// Distance the terrain may differ from the straight line between the end points before it is distrusted: structures, bridges and
        /// interiors sit above or below the terrain height map.
        /// </summary>
        private const float MaxDeviation = 6f;

        /// <summary>
        /// Return terrain height at the position plus the supplied clearance, or null if the map has no terrain there.
        /// </summary>
        public static float? GetHeight(IBaseMap map, float x, float z, float clearance)
        {
            float? height = map?.GetTerrainHeight(x, z);
            return height.HasValue ? height.Value + clearance : null;
        }

        /// <summary>
        /// Return the nodes of a path from <paramref name="begin"/> to <paramref name="final"/> that stays <paramref name="clearance"/> above the terrain.
        /// </summary>
        /// <remarks>
        /// Without terrain data, or where it disagrees with the end points, the height is interpolated between them.
        /// </remarks>
        public static List<Vector3> Build(IBaseMap map, Vector3 begin, Vector3 final, float clearance)
        {
            var nodes = new List<Vector3> { begin };

            float distance = MathF.Sqrt(MathF.Pow(final.X - begin.X, 2) + MathF.Pow(final.Z - begin.Z, 2));
            int steps = Math.Min((int)MathF.Floor(distance / StepSize), MaxNodes);

            for (int i = 1; i <= steps; i++)
            {
                float t = (float)i / (steps + 1);
                nodes.Add(Node(map, begin, final, t, clearance));
            }

            nodes.Add(Node(map, begin, final, 1f, clearance));
            return nodes;
        }

        private static Vector3 Node(IBaseMap map, Vector3 begin, Vector3 final, float t, float clearance)
        {
            float x = MathsExtensions.Lerp(begin.X, final.X, t);
            float z = MathsExtensions.Lerp(begin.Z, final.Z, t);
            float linear = MathsExtensions.Lerp(begin.Y, final.Y, t);

            float? height = GetHeight(map, x, z, clearance);
            float y = height.HasValue && MathF.Abs(height.Value - linear) <= MaxDeviation ? height.Value : linear;
            return new Vector3(x, y, z);
        }
    }
}
