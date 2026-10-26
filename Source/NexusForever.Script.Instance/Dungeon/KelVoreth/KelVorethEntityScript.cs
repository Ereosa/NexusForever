using NexusForever.Script.Instance.Expedition;
using NexusForever.Script.Template.Filter;

namespace NexusForever.Script.Instance.Dungeon.KelVoreth
{
    /// <summary>
    /// Ruins of Kel Voreth creatures whose deaths drive the event, normal and veteran versions.
    /// </summary>
    [ScriptFilterCreatureId(
        51068, 51069, 50991, 50992, 51453, 51454,                // Blood Pit champions
        32534, 32535, 32536, 32539, 32531, 32533)]               // Grond the Corpsemaker, Slavemaster Drokk, Forgemaster Trogun
    public class KelVorethEntityScript : ExpeditionEntityScript
    {
    }
}
