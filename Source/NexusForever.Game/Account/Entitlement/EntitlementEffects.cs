using NexusForever.Game.Abstract.Entitlement;
using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Spell;
using NexusForever.GameTable.Model;

namespace NexusForever.Game.Account.Entitlement
{
    /// <summary>
    /// Applies the character side effects of an entitlement, its title and persistent buff.
    /// </summary>
    public static class EntitlementEffects
    {
        /// <summary>
        /// Grant the title and cast the persistent buff of every account and character entitlement the <see cref="IPlayer"/> has.
        /// </summary>
        public static void ApplyAll(IPlayer player)
        {
            foreach (IEntitlement entitlement in player.Account.EntitlementManager.Cast<IEntitlement>().Concat(player.EntitlementManager.Cast<IEntitlement>()))
                if (entitlement.Amount > 0u)
                    Apply(player, entitlement.Entry, true);
        }

        /// <summary>
        /// Grant the title and cast the persistent buff of the supplied entitlement.
        /// </summary>
        public static void Apply(IPlayer player, EntitlementEntry entry, bool suppress = false)
        {
            if (entry.CharacterTitleId != 0u && !player.TitleManager.HasTitle((ushort)entry.CharacterTitleId))
                player.TitleManager.AddTitle((ushort)entry.CharacterTitleId, suppress);

            if (entry.Spell4IdPersistentBuff != 0u && !player.HasSpell(entry.Spell4IdPersistentBuff, out _))
                player.CastSpell(entry.Spell4IdPersistentBuff, new SpellParameters());
        }
    }
}
