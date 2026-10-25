using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using NexusForever.Script.Template.Filter.Dynamic;

namespace NexusForever.Script.Template.Filter.Standard
{
    public class ScriptFilterDynamic : IScriptFilter
    {
        private IScriptFilter dynamicFilter;

        #region Dependency Injection

        private readonly IServiceProvider serviceProvider;

        public ScriptFilterDynamic(
            IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        #endregion

        public void Initialise(Type scriptType)
        {
            Attribute attribute = scriptType.GetCustomAttribute(typeof(ScriptFilterDynamicAttribute<>));
            if (attribute == null)
                return;

            Type dynamicFilterType = attribute.GetType().GetGenericArguments().First();
            dynamicFilter = (IScriptFilter)serviceProvider.GetRequiredService(dynamicFilterType);
            dynamicFilter.Initialise(scriptType);
        }

        /// <summary>
        /// Returns if <see cref="IScriptFilterSearch"/> can match filter.
        /// </summary>
        public bool Match(IScriptFilterSearch search)
        {
            if (dynamicFilter == null)
                return true;

            return dynamicFilter.Match(search);
        }
    }
}
