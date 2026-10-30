using System.Numerics;
using NexusForever.Game.Abstract.Map;
using NexusForever.IO.Map;

namespace NexusForever.Game.Map
{
    /// <summary>
    /// Adds of non-player entities waiting to be processed, grouped by grid.
    /// </summary>
    /// <remarks>
    /// A grid activation queues thousands of adds. Draining the grid closest to a player first, and inside it the entities
    /// closest to a player first, makes what the player can see spawn in seconds while far grids fill in behind it.
    /// </remarks>
    public class BulkAddQueue
    {
        private sealed class Bucket
        {
            public (uint X, uint Z) Coord { get; init; }
            public List<(IGridAction Action, Vector3 Position)> Actions { get; } = [];
            public bool Dirty { get; set; }
        }

        private readonly object sync = new();
        private readonly Dictionary<(uint X, uint Z), Bucket> buckets = [];
        private int count;

        public int Count => Volatile.Read(ref count);

        public void Enqueue(IGridAction action, Vector3 position)
        {
            (uint X, uint Z) coord;
            try
            {
                coord = MapGrid.GetGridCoord(position);
            }
            catch (ArgumentOutOfRangeException)
            {
                // the add reports the invalid position when it is processed
                coord = (0u, 0u);
            }

            lock (sync)
            {
                if (!buckets.TryGetValue(coord, out Bucket bucket))
                    buckets.Add(coord, bucket = new Bucket { Coord = coord });

                bucket.Actions.Add((action, position));
                bucket.Dirty = true;
                count++;
            }
        }

        /// <summary>
        /// Take the queued actions closest to the supplied player positions first, the actions are removed as they are returned.
        /// </summary>
        public IEnumerable<IGridAction> Drain(IReadOnlyList<Vector3> players)
        {
            List<Bucket> order;
            lock (sync)
            {
                order = buckets.Values
                    .OrderBy(b => GridDistance(b.Coord, players))
                    .ToList();
            }

            foreach (Bucket bucket in order)
            {
                while (true)
                {
                    IGridAction action;
                    lock (sync)
                    {
                        if (bucket.Actions.Count == 0)
                        {
                            buckets.Remove(bucket.Coord);
                            break;
                        }

                        if (bucket.Dirty)
                        {
                            // farthest first so the closest can be taken off the end
                            if (players.Count > 0)
                                bucket.Actions.Sort((a, b) => Nearest(b.Position, players).CompareTo(Nearest(a.Position, players)));
                            else
                                bucket.Actions.Reverse();

                            bucket.Dirty = false;
                        }

                        int last = bucket.Actions.Count - 1;
                        action = bucket.Actions[last].Action;
                        bucket.Actions.RemoveAt(last);
                        count--;
                    }

                    yield return action;
                }
            }
        }

        private static float Nearest(Vector3 position, IReadOnlyList<Vector3> players)
        {
            float nearest = float.MaxValue;
            foreach (Vector3 player in players)
                nearest = MathF.Min(nearest, Vector2.DistanceSquared(new Vector2(position.X, position.Z), new Vector2(player.X, player.Z)));
            return nearest;
        }

        /// <summary>
        /// Distance from the closest player to the edge of the grid, zero when a player is inside it.
        /// </summary>
        private static float GridDistance((uint X, uint Z) coord, IReadOnlyList<Vector3> players)
        {
            float minX = ((int)coord.X - MapDefines.WorldGridOrigin) * MapDefines.GridSize;
            float minZ = ((int)coord.Z - MapDefines.WorldGridOrigin) * MapDefines.GridSize;

            float nearest = float.MaxValue;
            foreach (Vector3 player in players)
            {
                float dx = MathF.Max(MathF.Max(minX - player.X, player.X - (minX + MapDefines.GridSize)), 0f);
                float dz = MathF.Max(MathF.Max(minZ - player.Z, player.Z - (minZ + MapDefines.GridSize)), 0f);
                nearest = MathF.Min(nearest, dx * dx + dz * dz);
            }

            return players.Count == 0 ? 0f : nearest;
        }
    }
}
