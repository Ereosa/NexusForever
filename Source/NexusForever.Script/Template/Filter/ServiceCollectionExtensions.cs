using Microsoft.Extensions.DependencyInjection;
using NexusForever.Script.Template.Filter.Dynamic;
using NexusForever.Script.Template.Filter.Standard;

namespace NexusForever.Script.Template.Filter
{
    public static class ServiceCollectionExtensions
    {
        public static void AddScriptTemplateFilter(this IServiceCollection sc)
        {
            sc.AddScriptTemplateFilterDynamic();

            sc.AddTransient<IScriptFilterMatcher, ScriptFilterMatcher>();
            sc.AddTransient<IScriptFilter, ScriptFilterActivePropId>();
            sc.AddTransient<IScriptFilter, ScriptFilterCreatureId>();
            sc.AddTransient<IScriptFilter, ScriptFilterDynamic>();
            sc.AddTransient<IScriptFilter, ScriptFilterId>();
            sc.AddTransient<IScriptFilter, ScriptFilterScriptName>();
            sc.AddTransient<IScriptFilter, ScriptFilterAssignableType>();
        }
    }
}
