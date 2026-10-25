using System.Reflection;

namespace NexusForever.Script.Template.Filter.Standard
{
    public class ScriptFilterCreatureId : IScriptFilter
    {
        private HashSet<uint> creatureIds;

        public void Initialise(Type scriptType)
        {
            ScriptFilterCreatureIdAttribute attribute = scriptType.GetCustomAttribute<ScriptFilterCreatureIdAttribute>();
            if (attribute != null)
                creatureIds = new HashSet<uint>(attribute.CreatureId);
        }

        /// <summary>
        /// Returns if <see cref="IScriptFilterSearch"/> can match filter.
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
