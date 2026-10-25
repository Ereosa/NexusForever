using System.Reflection;

namespace NexusForever.Script.Template.Filter.Standard
{
    public class ScriptFilterId : IScriptFilter
    {
        private HashSet<uint> ids;

        public void Initialise(Type scriptType)
        {
            ScriptFilterOwnerIdAttribute attribute = scriptType.GetCustomAttribute<ScriptFilterOwnerIdAttribute>();
            if (attribute != null)
                ids = new HashSet<uint>(attribute.Id);
        }

        /// <summary>
        /// Returns if <see cref="IScriptFilterSearch"/> can match filter.
        /// </summary>
        public bool Match(IScriptFilterSearch search)
        {
            if (ids != null)
            {
                if (search.Id == null)
                    return false;

                return ids.Contains(search.Id.Value);
            }

            return true;
        }
    }
}
