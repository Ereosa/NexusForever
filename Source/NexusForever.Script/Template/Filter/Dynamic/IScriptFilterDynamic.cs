namespace NexusForever.Script.Template.Filter.Dynamic
{
    public interface IScriptFilterDynamic
    {
        /// <summary>
        /// Returns if <see cref="IScriptFilterSearch"/> can match dynamic filter.
        /// </summary>
        void Filter(IScriptFilterParameters parameters);
    }
}
