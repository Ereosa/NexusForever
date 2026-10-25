using NexusForever.Game.Abstract.Combat;
using NexusForever.Game.Abstract.Spell.Proc;
using NexusForever.Game.Static.Spell;
using NexusForever.Shared;

namespace NexusForever.Game.Spell.Effect.Handler
{
    /// <summary>
    /// Damage spread over the targets hit (eg: Warrior Rive), the damage calculator already reads its formula like regular damage.
    /// </summary>
    [SpellEffectHandler(SpellEffectType.DistributedDamage)]
    public class SpellEffectDistributedDamageHandler : SpellEffectDamageHandler
    {
        public SpellEffectDistributedDamageHandler(
            IFactory<IDamageCalculator> damageCalculatorFactory,
            IFactory<IProcParameters> procParameterFactory)
            : base(damageCalculatorFactory, procParameterFactory)
        {
        }
    }

    /// <summary>
    /// Damage that scales with distance, the damage calculator already reads its formula like regular damage.
    /// </summary>
    [SpellEffectHandler(SpellEffectType.DistanceDependentDamage)]
    public class SpellEffectDistanceDependentDamageHandler : SpellEffectDamageHandler
    {
        public SpellEffectDistanceDependentDamageHandler(
            IFactory<IDamageCalculator> damageCalculatorFactory,
            IFactory<IProcParameters> procParameterFactory)
            : base(damageCalculatorFactory, procParameterFactory)
        {
        }
    }
}
