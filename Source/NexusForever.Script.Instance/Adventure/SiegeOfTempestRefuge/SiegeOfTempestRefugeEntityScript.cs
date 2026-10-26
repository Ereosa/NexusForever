using NexusForever.Script.Instance.Expedition;
using NexusForever.Script.Template.Filter;

namespace NexusForever.Script.Instance.Adventure.SiegeOfTempestRefuge
{
    /// <summary>
    /// The assaulting troops and their commanders, their deaths beat back the divisions.
    /// </summary>
    [ScriptFilterCreatureId(
        22202, 22205, 22204, 22203, 22206, 22161, 22162, 30738, 45997,                      // Dominion troops
        21284, 21499, 21478, 21287, 21512, 21286, 21292, 30725, 45995,                      // Exile troops
        22244, 22172, 22173, 22174, 22175, 22167, 22199, 22168, 22171, 22170, 22166, 22169, 22176, // Dominion commanders
        21544, 21545, 21546, 21547, 21549, 21552, 21551, 21557, 21550, 21553, 21554, 21555, 21556)] // Exile commanders
    public class SiegeOfTempestRefugeEntityScript : ExpeditionEntityScript
    {
    }
}
