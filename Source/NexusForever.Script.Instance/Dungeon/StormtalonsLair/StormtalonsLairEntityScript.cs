using NexusForever.Script.Instance.Expedition;
using NexusForever.Script.Template.Filter;

namespace NexusForever.Script.Instance.Dungeon.StormtalonsLair
{
    /// <summary>
    /// Stormtalon's Lair creatures whose deaths drive the event, normal and veteran versions.
    /// </summary>
    [ScriptFilterCreatureId(
        17153, 17154, 17173, 36758, 34697, 36862, 36796,        // Thundercall Pell
        34689, 34690, 34702, 37140, 37145, 37144, 37141,        // veteran Thundercall Pell
        17160, 33405, 17166, 32703, 17163, 33406,              // Blade-Wind the Invoker, Aethros, Stormtalon
        17316, 34707)]                                          // Gusts of Aethros
    public class StormtalonsLairEntityScript : ExpeditionEntityScript
    {
    }
}
