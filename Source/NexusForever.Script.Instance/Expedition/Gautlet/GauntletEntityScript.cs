using NexusForever.Script.Template.Filter;

namespace NexusForever.Script.Instance.Expedition.Gautlet
{
    /// <summary>
    /// Gauntlet combatants whose deaths drive the expedition, normal and veteran versions.
    /// </summary>
    [ScriptFilterCreatureId(
        48461, 48765, 48793, 48833, 48835, 48842, 48843, 48844, 48863, 48865, 48866, 48867, // Swarm Pit fodder
        69237, 69239, 69240, 69241, 69242, 69243, 69244, 69246, 69247, 69248, 69251,
        48454, 48458, 48666, 48667, 48669, 48670, 48689, 48695, 48697, 48721, 48723, 48728, // Faction Friction
        69280, 69281, 69282, 69283, 69284, 69285, 69286, 69287, 69288, 69289, 69290, 69291,
        49209, 49212, 49213, 49214, 49399, 49401, 69273, 69274, 69275, 69276, 69277, 69278, // Darkspur
        69688)]                                                                             // "Brick" Braggor
    public class GauntletEntityScript : ExpeditionEntityScript
    {
    }
}
