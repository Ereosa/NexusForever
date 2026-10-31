using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Spell;
using NexusForever.Game.Abstract.Spell.Effect;
using NexusForever.Game.Abstract.Spell.Effect.Data;
using NexusForever.Game.Abstract.Spell.Target;
using NexusForever.Game.Static.Spell;
using NexusForever.Game.Static.Spell.Effect;

namespace NexusForever.Game.Spell.Effect.Handler
{
    [SpellEffectHandler(SpellEffectType.Activate)]
    public class SpellEffectActivateHandler : ISpellEffectApplyHandler<ISpellEffectDefaultData>
    {
        /// <summary>
        /// Handle <see cref="ISpell"/> effect apply on <see cref="IUnitEntity"/> target.
        /// </summary>
        public SpellEffectExecutionResult Apply(ISpellExecutionContext executionContext, IUnitEntity target, ISpellTargetEffectInfo info, ISpellEffectDefaultData data)
        {
            if (executionContext.Spell.Parameters.ClientSideInteraction != null)
                executionContext.Spell.Parameters.ClientSideInteraction.HandleSuccess(executionContext.Spell);
            // quest items used on an object activate it without an interaction of their own (eg: Fiery Vengeance's Incendiary
            // Grenade thrown at a Cartel Slaver Shack, report 116)
            else if (executionContext.Spell.Caster is IPlayer player && target is IWorldEntity entity && entity != player)
                entity.OnActivateSuccess(player);

            return SpellEffectExecutionResult.Ok;
        }
    }
}
