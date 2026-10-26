using NexusForever.Script.Instance.Expedition;
using NexusForever.Script.Template.Filter;

namespace NexusForever.Script.Instance.Dungeon.Skullcano
{
    /// <summary>
    /// Skullcano creatures whose deaths drive the event, normal and veteran versions.
    /// </summary>
    [ScriptFilterCreatureId(
        24510, 24908,                                           // Redmoon Jailors
        24475, 24893, 24486, 24894, 24489, 24895)]              // Thunderfoot, Bosun Octog, Mordechai Redmoon
    public class SkullcanoEntityScript : ExpeditionEntityScript
    {
    }
}
