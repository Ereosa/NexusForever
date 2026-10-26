using NexusForever.Script.Template.Filter;

namespace NexusForever.Script.Instance.Expedition.OutpostM13
{
    /// <summary>
    /// Outpost M-13 creatures and objects whose deaths and activations drive the expedition, normal and veteran versions.
    /// </summary>
    [ScriptFilterCreatureId(
        23451,                                                         // accelerite fragments
        23453, 23454, 23455, 23565, 69074, 69075, 69076, 69082,        // boarding marauders, "Ransacker" Rorgh
        69878, 69879, 69881, 69882, 69898,                             // veteran holdout, Captain Wiko
        23500, 23513, 23514, 23536, 69077, 69078, 69079, 69080,        // the hive
        23470, 36676, 41221,                                           // exit and entry panels, barracks door controls
        29512, 29578, 29579, 29581, 29582, 29583, 29585, 29586, 29587)] // dead miners (datachrons)
    public class OutpostM13EntityScript : ExpeditionEntityScript
    {
    }
}
