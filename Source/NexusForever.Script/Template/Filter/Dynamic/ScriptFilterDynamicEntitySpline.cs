using System.Collections.Immutable;
using NexusForever.Database;
using NexusForever.Database.World;
using NexusForever.Database.World.Model;

namespace NexusForever.Script.Template.Filter.Dynamic
{
    public class ScriptFilterDynamicEntitySpline : IScriptFilterDynamicEntitySpline
    {
        private HashSet<uint> creatureIds;

        #region Dependency Injection

        private readonly IDatabaseManager databaseManager;

        public ScriptFilterDynamicEntitySpline(
            IDatabaseManager databaseManager)
        {
            this.databaseManager = databaseManager;
        }

        #endregion

        public void Initialise(Type scriptType)
        {
            ImmutableList<EntityModel> entities = databaseManager.GetDatabase<WorldDatabase>().GetEntitiesWithSpline();
            if (entities.Count == 0)
                return;

            creatureIds = entities
                .Select(e => e.Creature)
                .ToHashSet();
        }

        /// <summary>
        /// Returns if <see cref="IScriptFilterSearch"/> can match dynamic filter.
        /// </summary>
        public bool Match(IScriptFilterSearch search)
        {
            if (creatureIds != null)
            {
                if (search.CreatureId == null)
                    return false;

                return creatureIds.Contains(search.CreatureId.Value);
            }

            return true;
        }
    }
}
