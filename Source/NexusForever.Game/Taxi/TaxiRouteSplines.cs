using System.Numerics;
using NexusForever.GameTable;
using NexusForever.GameTable.Model;
using NLog;

namespace NexusForever.Game.Taxi
{
    /// <summary>
    /// Links each <see cref="TaxiRouteEntry"/> to the tbl spline its taxi flies along.
    /// </summary>
    /// <remarks>
    /// The link is server data that isn't in the client tables, every flight route has a spline in the same world that starts at
    /// the source node location and ends at the destination node location (within a few dozen units), the next best spline is
    /// hundreds of units off. Routes without a close spline (shuttles between worlds, long range transfers) have no flight.
    /// </remarks>
    public static class TaxiRouteSplines
    {
        /// <summary>
        /// Largest combined distance of the spline end points from the two taxi node locations.
        /// </summary>
        private const float MatchDistance = 200f;

        private static readonly ILogger log = LogManager.GetCurrentClassLogger();

        private static readonly Lazy<(Dictionary<uint, ushort> Routes, Dictionary<ushort, Vector3> Starts)> cache = new(Build);

        /// <summary>
        /// Return the spline for the supplied taxi route, 0 if the route has no flight.
        /// </summary>
        public static ushort GetSpline(uint taxiRouteId)
        {
            return cache.Value.Routes.TryGetValue(taxiRouteId, out ushort splineId) ? splineId : (ushort)0;
        }

        /// <summary>
        /// Return the position of the first point of the supplied spline.
        /// </summary>
        public static Vector3 GetStart(ushort splineId)
        {
            return cache.Value.Starts.TryGetValue(splineId, out Vector3 start) ? start : Vector3.Zero;
        }

        private static (Dictionary<uint, ushort>, Dictionary<ushort, Vector3>) Build()
        {
            var ends = new Dictionary<uint, (Vector3 Start, Vector3 End, uint StartOrdinal, uint EndOrdinal)>();
            foreach (Spline2NodeEntry node in GameTableManager.Instance.Spline2Node.Entries)
            {
                var position = new Vector3(node.Position0, node.Position1, node.Position2);
                if (!ends.TryGetValue(node.SplineId, out var end))
                {
                    ends.Add(node.SplineId, (position, position, node.Ordinal, node.Ordinal));
                    continue;
                }

                if (node.Ordinal < end.StartOrdinal)
                    end = (position, end.End, node.Ordinal, end.EndOrdinal);
                if (node.Ordinal > end.EndOrdinal)
                    end = (end.Start, position, end.StartOrdinal, node.Ordinal);
                ends[node.SplineId] = end;
            }

            var splinesByWorld = GameTableManager.Instance.Spline2.Entries
                .Where(s => s.Id <= ushort.MaxValue && ends.ContainsKey(s.Id))
                .GroupBy(s => s.WorldId)
                .ToDictionary(g => g.Key, g => g.Select(s => s.Id).ToList());

            var routes = new Dictionary<uint, ushort>();
            var starts = new Dictionary<ushort, Vector3>();
            foreach (TaxiRouteEntry route in GameTableManager.Instance.TaxiRoute.Entries)
            {
                WorldLocation2Entry source      = GetLocation(route.TaxiNodeIdSource);
                WorldLocation2Entry destination = GetLocation(route.TaxiNodeIdDestination);
                if (source == null || destination == null || source.WorldId != destination.WorldId)
                    continue;

                if (!splinesByWorld.TryGetValue(source.WorldId, out List<uint> splines))
                    continue;

                var sourcePosition      = new Vector3(source.Position0, source.Position1, source.Position2);
                var destinationPosition = new Vector3(destination.Position0, destination.Position1, destination.Position2);

                uint bestSpline = 0u;
                float bestDistance = MatchDistance;
                foreach (uint splineId in splines)
                {
                    var end = ends[splineId];
                    float distance = Vector3.Distance(end.Start, sourcePosition) + Vector3.Distance(end.End, destinationPosition);
                    if (distance < bestDistance)
                    {
                        bestDistance = distance;
                        bestSpline   = splineId;
                    }
                }

                if (bestSpline == 0u)
                    continue;

                routes.Add(route.Id, (ushort)bestSpline);
                starts[(ushort)bestSpline] = ends[bestSpline].Start;
            }

            log.Info($"Matched {routes.Count} taxi routes to flight splines.");
            return (routes, starts);
        }

        private static WorldLocation2Entry GetLocation(uint taxiNodeId)
        {
            TaxiNodeEntry node = GameTableManager.Instance.TaxiNode.GetEntry(taxiNodeId);
            return node != null ? GameTableManager.Instance.WorldLocation2.GetEntry(node.WorldLocation2Id) : null;
        }
    }
}
