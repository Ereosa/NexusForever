namespace NexusForever.Script.Template.Filter
{
    public class ScriptFilterMatcher : IScriptFilterMatcher
    {
        #region Dependency Injection

        private readonly IEnumerable<IScriptFilter> filters;

        public ScriptFilterMatcher(
            IEnumerable<IScriptFilter> filters)
        {
            this.filters = filters;
        }

        #endregion

        public void Initialise(Type type)
        {
            foreach (IScriptFilter filter in filters)
                filter.Initialise(type);
        }

        /// <summary>
        /// Returns if <see cref="IScriptFilterSearch"/> can match with any defined filters.
        /// </summary>
        public bool Match(IScriptFilterSearch search)
        {
            return filters.All(f => f.Match(search));
        }
    }
}
