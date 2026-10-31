using System.Collections.Concurrent;
using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Spell;
using NexusForever.Game.Abstract.Spell.Effect;
using NexusForever.Game.Abstract.Spell.Effect.Data;
using NexusForever.Game.Abstract.Spell.Target;
using NexusForever.Game.Static.Spell;
using NexusForever.Game.Static.Spell.Effect;

namespace NexusForever.Game.Spell.Effect.Handler
{
    /// <summary>
    /// Shows an action bar to the player while the effect lasts, DataBits00 is the ActionBarShortcutSet and DataBits02 the ShortcutSet bar.
    /// </summary>
    /// <remarks>
    /// Eg: 36494 gives the Data Mining buttons of the Darkwhisper Database (set 328), 40526 the controls of the XAS retrofitted cannon (set 355).
    /// </remarks>
    [SpellEffectHandler(SpellEffectType.ActionBarSet)]
    public class SpellEffectActionBarSetHandler : ISpellEffectApplyHandler<ISpellEffectDefaultData>, ISpellEffectRemoveHandler<ISpellEffectDefaultData>
    {
        /// <summary>
        /// Handle <see cref="ISpell"/> effect apply on <see cref="IUnitEntity"/> target.
        /// </summary>
        public SpellEffectExecutionResult Apply(ISpellExecutionContext executionContext, IUnitEntity target, ISpellTargetEffectInfo info, ISpellEffectDefaultData data)
        {
            if (target is not IPlayer player)
                return SpellEffectExecutionResult.PreventEffect;

            player.ShowActionBar((ShortcutSet)data.DataBits02, (ushort)data.DataBits00, executionContext.Spell.Caster.Guid);

            // a bar without a duration whose spell removes itself through procs ("until you move", the Soldier Weapons Lockbox, 55640)
            // stays up until a spell force removes its spell group (its own close button, the proc), report 133
            if (IsUntilRemoved(executionContext.Spell.Parameters.SpellInfo, info))
                persistentBars[(player.Guid, (ShortcutSet)data.DataBits02)] = ((ushort)data.DataBits00, executionContext.Spell.Parameters.SpellInfo.SpellGroups.ToArray());

            return SpellEffectExecutionResult.Ok;
        }

        /// <summary>
        /// Handle <see cref="ISpell"/> effect remove on <see cref="IUnitEntity"/> target.
        /// </summary>
        public void Remove(ISpell spell, IUnitEntity target, ISpellTargetEffectInfo info, ISpellEffectDefaultData data)
        {
            if (target is not IPlayer player)
                return;

            if (persistentBars.TryGetValue((player.Guid, (ShortcutSet)data.DataBits02), out var bar) && bar.BarId == (ushort)data.DataBits00)
                return;

            player.HideActionBar((ShortcutSet)data.DataBits02, (ushort)data.DataBits00);
        }
    
        private static readonly ConcurrentDictionary<(uint PlayerGuid, ShortcutSet Set), (ushort BarId, uint[] SpellGroups)> persistentBars = new();

        private static bool IsUntilRemoved(Abstract.Spell.Info.ISpellInfo spellInfo, ISpellTargetEffectInfo info)
        {
            return (info.Entry.DurationTime == 0u)
                && spellInfo.Entry.SpellDuration == 0u
                && spellInfo.Effects.Any(e => e.EffectType == SpellEffectType.Proc);
        }

        /// <summary>
        /// A spell force removed the supplied spell group from <see cref="IPlayer"/>: hide the bars that stay up until then.
        /// </summary>
        public static void OnSpellGroupRemoved(IPlayer player, uint spellGroupId)
        {
            foreach (var entry in persistentBars.Where(e => e.Key.PlayerGuid == player.Guid && e.Value.SpellGroups.Contains(spellGroupId)).ToList())
            {
                persistentBars.TryRemove(entry.Key, out _);
                player.HideActionBar(entry.Key.Set, entry.Value.BarId);
            }
        }
}
}
