using System.Collections.Immutable;
using System.Numerics;
using NexusForever.Database.World.Model;
using NexusForever.Game.Abstract.Map;

namespace NexusForever.Game.Map
{
    public class EntityCache : IEntityCache
    {
        public uint GridCount => (uint)entities.Count;
        public uint EntityCount { get; }

        private readonly Dictionary<(uint GridX, uint GridZ), HashSet<EntityModel>> entities = new();

        // checklist indexes are assigned on first use, caches can be built before the AssetManager is initialised (precached maps)
        private ImmutableList<EntityModel> models;

        public EntityCache(ImmutableList<EntityModel> models)
        {
            EntityCount = (uint)models.Count;
            this.models = models;
            foreach (EntityModel model in models)
                AddEntity(model);
        }

        /// <remarks>
        /// Checklist objective progress is a bit per entity (QuestChecklistIdx), the client shows every entity whose bit is set as done
        /// (eg: lit torches for Lighting the Way). Captures sometimes have every entity of a checklist target on index 0,
        /// so one use marked them all done, those get distinct indexes.
        /// </remarks>
        private static void AssignChecklistIndexes(IEnumerable<EntityModel> models)
        {
            foreach (IGrouping<uint, EntityModel> group in models.GroupBy(m => m.Creature))
            {
                if (group.Count() < 2 || group.Select(m => m.QuestChecklistIdx).Distinct().Count() > 1)
                    continue;

                if (!AssetManager.Instance.IsChecklistTarget(group.Key))
                    continue;

                byte index = 0;
                foreach (EntityModel model in group.OrderBy(m => m.Id))
                    model.QuestChecklistIdx = (byte)(index++ % 32);
            }
        }

        /// <summary>
        /// Add <see cref="EntityModel"/> to be spawned when parent <see cref="IMapGrid"/> is activated.
        /// </summary>
        public void AddEntity(EntityModel model)
        {
            var vector = new Vector3(model.X, model.Y, model.Z);
            (uint GridX, uint GridZ) coord = MapGrid.GetGridCoord(vector);

            if (!entities.ContainsKey(coord))
                entities.Add(coord, new HashSet<EntityModel>());

            entities[coord].Add(model);
        }

        /// <summary>
        /// Return all <see cref="EntityModel"/>'s to be spawned for parent <see cref="IMapGrid"/>.
        /// </summary>
        public IEnumerable<EntityModel> GetEntities(uint gridX, uint gridZ)
        {
            if (models != null)
            {
                lock (entities)
                {
                    if (models != null)
                    {
                        AssignChecklistIndexes(models);
                        models = null;
                    }
                }
            }

            return entities.TryGetValue((gridX, gridZ), out HashSet<EntityModel> cellEntities) ? cellEntities : Enumerable.Empty<EntityModel>();
        }
    }
}
