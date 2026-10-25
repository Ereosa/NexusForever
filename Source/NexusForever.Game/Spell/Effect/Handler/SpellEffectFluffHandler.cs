using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Spell;
using NexusForever.Game.Abstract.Spell.Effect;
using NexusForever.Game.Abstract.Spell.Effect.Data;
using NexusForever.Game.Abstract.Spell.Target;
using NexusForever.Game.Static.Spell;
using NexusForever.Game.Static.Spell.Effect;

namespace NexusForever.Game.Spell.Effect.Handler
{
    [SpellEffectHandler(SpellEffectType.Fluff)]
    public class SpellEffectFluffHandler : ISpellEffectApplyHandler<ISpellEffectDefaultData>
    {
        /// <summary>
        /// Handle <see cref="ISpell"/> effect apply on <see cref="IUnitEntity"/> target.
        /// </summary>
        public SpellEffectExecutionResult Apply(ISpellExecutionContext executionContext, IUnitEntity target, ISpellTargetEffectInfo info, ISpellEffectDefaultData data)
        {
            // bot abilities (eg: 51365 Barrage) only have a fluff effect on the selected target, the pet command is handled by the server
            if (executionContext.Spell.Caster is IPlayer player && info.Entry.TargetFlags.HasFlag(SpellEffectTargetFlags.ImplicitTarget))
                player.CombatPetManager.OnPetAbility(executionContext.Spell, target);

            return SpellEffectExecutionResult.Ok;
        }
    }
}
