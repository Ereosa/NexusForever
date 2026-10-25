using System.Reflection;

namespace NexusForever.Script.Template.Filter.Standard
{
    public class ScriptFilterActivePropId : IScriptFilter
    {
        private HashSet<ulong> activePropIds;

        public void Initialise(Type scriptType)
        {
            ScriptFilterActivePropIdAttribute attribute = scriptType.GetCustomAttribute<ScriptFilterActivePropIdAttribute>();
            if (attribute != null)
                activePropIds = new HashSet<ulong>(attribute.ActivePropId);
        }

        /// <summary>
        /// Returns if <see cref="IScriptFilterSearch"/> can match filter.
        /// </summary>
        public bool Match(IScriptFilterSearch search)
        {
            if (activePropIds != null)
            {
                if (search.ActivePropId == null)
                    return false;

                return activePropIds.Contains(search.ActivePropId.Value);
            }

            return true;
        }
    }
}
