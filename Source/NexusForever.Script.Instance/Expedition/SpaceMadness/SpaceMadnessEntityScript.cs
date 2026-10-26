using NexusForever.Script.Template.Filter;

namespace NexusForever.Script.Instance.Expedition.SpaceMadness
{
    /// <summary>
    /// Space Madness creatures and objects whose deaths and activations drive the expedition, normal and veteran versions.
    /// </summary>
    [ScriptFilterCreatureId(
        46123, 46124, 46126, 46127, 46128, 46130, 46131, 46132, 46133, 46135, 46137, 58783, 58798, 46721, // nightmares
        69213, 69214, 69215, 69216, 69217, 69218, 69219, 69220, 69221, 69222, 69223, 69207,
        45724, 45726, 45727, 45728, 45729, 45730, 69137, 69138, 69171, 69172, 69139, 69140,               // livestock
        46714, 46720, 46722, 46723, 46724, 46483, 69205, 69206, 69208, 69209, 69210, 69204,               // laboratory waves
        46856, 46858, 46881, 46882, 46883, 46884, 46885, 46887, 46889,                                    // fire waves
        69145, 69146, 69147, 69148, 69149, 69150, 69151, 69152, 69153,
        48098, 48099, 48101, 48102, 48103, 48104, 48105, 48106, 48107,                                    // electric waves
        69154, 69155, 69156, 69157, 69158, 69159, 69160, 69161, 69162,
        48345, 48346, 48347, 48348, 48349, 48350, 48351, 48352, 48353,                                    // shadow waves
        69163, 69164, 69165, 69173, 69166, 69167, 69168, 69169, 69170,
        45981, 45903, 45904, 69141, 69142, 69899, 69900, 69901)]                                          // hazmat suit, workers, experiments
    public class SpaceMadnessEntityScript : ExpeditionEntityScript
    {
    }
}
