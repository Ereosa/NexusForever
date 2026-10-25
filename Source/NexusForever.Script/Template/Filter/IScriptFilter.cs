namespace NexusForever.Script.Template.Filter
{
    public interface IScriptFilter
    {
        void Initialise(Type scriptType);

        /// <summary>
        /// Returns if <see cref="IScriptFilterSearch"/> can match filter.
        /// </summary>
        bool Match(IScriptFilterSearch search);
    }
}
