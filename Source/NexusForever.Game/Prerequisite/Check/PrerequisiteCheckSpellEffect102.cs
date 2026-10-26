using Microsoft.Extensions.Logging;
using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Prerequisite;
using NexusForever.Game.Abstract.Spell;
using NexusForever.Game.Static.Prerequisite;

namespace NexusForever.Game.Prerequisite.Check
{
    /// <summary>
    /// Object id is a spell, the value compares whether the player is under it: 0 not under it, anything else under it
    /// (eg: "SpellEffect102, Equal, 1, 36886" while under Resurrection Station, "NotEqual, 0, 45049" while under Wall of Corruption).
    /// </summary>
    /// <remarks>
    /// Values above 1 (eg: 26631 with spell 43737) look like a specific effect of the spell, they are treated as being under the spell.
    /// </remarks>
    [PrerequisiteCheck(PrerequisiteType.SpellEffect102)]
    public class PrerequisiteCheckSpellEffect102 : IPrerequisiteCheck
    {
        private readonly ILogger<PrerequisiteCheckSpellEffect102> log;

        public PrerequisiteCheckSpellEffect102(ILogger<PrerequisiteCheckSpellEffect102> log)
        {
            this.log = log;
        }

        public bool Meets(IPlayer player, PrerequisiteComparison comparison, uint value, uint objectId, IPrerequisiteParameters parameters)
        {
            bool under = objectId != 0u && player.HasSpell(s => s.Spell4Id == objectId, out ISpell _);
            bool wanted = value != 0u;
            switch (comparison)
            {
                case PrerequisiteComparison.Equal:
                    return under == wanted;
                case PrerequisiteComparison.NotEqual:
                    return under != wanted;
                default:
                    log.LogWarning($"Unhandled PrerequisiteComparison {comparison} for {PrerequisiteType.SpellEffect102}!");
                    return false;
            }
        }
    }
}
