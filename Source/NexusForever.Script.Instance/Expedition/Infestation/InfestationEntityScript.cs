using NexusForever.Script.Template.Filter;

namespace NexusForever.Script.Instance.Expedition.Infestation
{
    /// <summary>
    /// Infestation creatures and objects whose deaths and activations drive the expedition, normal and veteran versions.
    /// </summary>
    [ScriptFilterCreatureId(
        22669, 41722, 69018, 69022, // lumbering parasites
        26833, 69019,               // Cyclopean Parasite
        36844, 22662)]              // hull breaches, the Blue Sparrow's control panel
    public class InfestationEntityScript : ExpeditionEntityScript
    {
    }
}
