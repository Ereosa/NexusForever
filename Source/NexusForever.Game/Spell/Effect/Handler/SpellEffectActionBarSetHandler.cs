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
            return SpellEffectExecutionResult.Ok;
        }

        /// <summary>
        /// Handle <see cref="ISpell"/> effect remove on <see cref="IUnitEntity"/> target.
        /// </summary>
        public void Remove(ISpell spell, IUnitEntity target, ISpellTargetEffectInfo info, ISpellEffectDefaultData data)
        {
            if (target is IPlayer player)
                player.HideActionBar((ShortcutSet)data.DataBits02, (ushort)data.DataBits00);
        }
    }
}
