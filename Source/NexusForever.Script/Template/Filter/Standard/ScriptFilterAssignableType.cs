namespace NexusForever.Script.Template.Filter.Standard
{
    public class ScriptFilterAssignableType : IScriptFilter
    {
        private Type scriptType;

        public void Initialise(Type scriptType)
        {
            this.scriptType = scriptType;
        }

        /// <summary>
        /// Returns if <see cref="IScriptFilterSearch"/> can match filter.
        /// </summary>
        public bool Match(IScriptFilterSearch search)
        {
            if (search.ScriptType == null)
                return true;

            return search.ScriptType.IsAssignableFrom(scriptType);
        }
    }
}
