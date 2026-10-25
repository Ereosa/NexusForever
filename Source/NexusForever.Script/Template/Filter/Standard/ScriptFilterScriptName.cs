using System.Reflection;

namespace NexusForever.Script.Template.Filter.Standard
{
    public class ScriptFilterScriptName : IScriptFilter
    {
        private string scriptName;

        public void Initialise(Type scriptType)
        {
            ScriptFilterScriptNameAttribute attribute = scriptType.GetCustomAttribute<ScriptFilterScriptNameAttribute>();
            if (attribute != null)
                scriptName = attribute.ScriptName;
        }

        /// <summary>
        /// Returns if <see cref="IScriptFilterSearch"/> can match filter.
        /// </summary>
        public bool Match(IScriptFilterSearch search)
        {
            if (search.ScriptNames != null)
            {
                if (scriptName == null)
                    return false;

                return search.ScriptNames.Contains(scriptName);
            }

            return scriptName == null;
        }
    }
}
