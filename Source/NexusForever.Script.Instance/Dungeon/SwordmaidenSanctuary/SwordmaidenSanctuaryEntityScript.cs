using NexusForever.Script.Instance.Expedition;
using NexusForever.Script.Template.Filter;

namespace NexusForever.Script.Instance.Dungeon.SwordmaidenSanctuary
{
    /// <summary>
    /// Sanctuary of the Swordmaiden bosses whose deaths drive the event, normal and veteran versions.
    /// </summary>
    [ScriptFilterCreatureId(
        28600, 28599, 28721, 28720, 28727, 28728,               // Deadringer Shallaos, Ondu Lifeweaver, Moldwood Overlord Skash
        28733, 28732, 28735, 28736)]                            // Rayna Darkspeaker, Spiritmother Selene the Corrupted
    public class SwordmaidenSanctuaryEntityScript : ExpeditionEntityScript
    {
    }
}
