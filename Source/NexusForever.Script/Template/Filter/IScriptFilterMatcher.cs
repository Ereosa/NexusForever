namespace NexusForever.Script.Template.Filter
{
    public interface IScriptFilterMatcher
    {
        void Initialise(Type type);

        /// <summary>
        /// Returns if <see cref="IScriptFilterSearch"/> can match with any defined filters.
        /// </summary>
        bool Match(IScriptFilterSearch search);
    }
}
