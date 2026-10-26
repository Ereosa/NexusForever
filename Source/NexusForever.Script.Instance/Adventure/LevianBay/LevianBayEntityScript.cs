using NexusForever.Script.Instance.Expedition;
using NexusForever.Script.Template.Filter;

namespace NexusForever.Script.Instance.Adventure.LevianBay
{
    /// <summary>
    /// Levian Bay creatures whose deaths and activations drive the adventure.
    /// </summary>
    [ScriptFilterCreatureId(
        67701, 68206, 68207, 67932, 67704,                                                  // Calidor, Varmintron, Gopherzoid, sentinels, the Evocator
        67574, 67580, 67582, 67584, 67590, 67576, 67579, 67583, 67588, 67593,              // the rival teams
        68337, 68325, 68326, 67700, 69994,                                                  // augmented malverine
        67691, 67692, 67694, 67696, 67697, 67698, 67752,                                    // stage one wildlife
        67747, 67748, 67749, 67750, 67751,                                                  // stage two
        67754, 67755, 67756, 68471, 68473, 67945)]                                          // stage three
    public class LevianBayEntityScript : ExpeditionEntityScript
    {
    }
}
