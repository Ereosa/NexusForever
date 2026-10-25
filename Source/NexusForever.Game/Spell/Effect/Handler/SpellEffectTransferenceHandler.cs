using NexusForever.Game.Abstract.Combat;
using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Spell;
using NexusForever.Game.Abstract.Spell.Effect.Data;
using NexusForever.Game.Abstract.Spell.Proc;
using NexusForever.Game.Abstract.Spell.Target;
using NexusForever.Game.Static.Spell;
using NexusForever.Game.Static.Spell.Effect;
using NexusForever.Shared;

namespace NexusForever.Game.Spell.Effect.Handler
{
    /// <summary>
    /// Damages the target and heals the caster for the damage dealt (eg: Stalker Nano Virus).
    /// </summary>
    [SpellEffectHandler(SpellEffectType.Transference)]
    public class SpellEffectTransferenceHandler : SpellEffectDamageHandler
    {
        public SpellEffectTransferenceHandler(
            IFactory<IDamageCalculator> damageCalculatorFactory,
            IFactory<IProcParameters> procParameterFactory)
            : base(damageCalculatorFactory, procParameterFactory)
        {
        }

        public override SpellEffectExecutionResult Apply(ISpellExecutionContext executionContext, IUnitEntity target, ISpellTargetEffectInfo info, ISpellEffectDamageData data)
        {
            SpellEffectExecutionResult result = base.Apply(executionContext, target, info, data);
            if (result == SpellEffectExecutionResult.Ok && info.Damage != null && executionContext.Spell.Caster.IsAlive)
                executionContext.Spell.Caster.ModifyHealth(info.Damage.AdjustedDamage, DamageType.Heal, executionContext.Spell.Caster);

            return result;
        }
    }
}
