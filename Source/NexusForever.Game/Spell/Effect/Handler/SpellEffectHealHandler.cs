using NexusForever.Game.Abstract.Combat;
using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Spell;
using NexusForever.Game.Abstract.Spell.Effect;
using NexusForever.Game.Abstract.Spell.Effect.Data;
using NexusForever.Game.Abstract.Spell.Target;
using NexusForever.Game.Static.Quest;
using NexusForever.Game.Static.Spell;
using NexusForever.Game.Static.Spell.Effect;
using NexusForever.Shared;

using NexusForever.Game.Spell.Proc;

namespace NexusForever.Game.Spell.Effect.Handler
{
    [SpellEffectHandler(SpellEffectType.Heal)]
    public class SpellEffectHealHandler : ISpellEffectApplyHandler<ISpellEffectDefaultData>
    {
        #region Dependency Injection

        private readonly IFactory<IDamageCalculator> damageCalculatorFactory;

        public SpellEffectHealHandler(
            IFactory<IDamageCalculator> damageCalculatorFactory)
        {
            this.damageCalculatorFactory = damageCalculatorFactory;
        }

        #endregion

        /// <summary>
        /// Handle <see cref="ISpell"/> effect apply on <see cref="IUnitEntity"/> target.
        /// </summary>
        public SpellEffectExecutionResult Apply(ISpellExecutionContext executionContext, IUnitEntity target, ISpellTargetEffectInfo info, ISpellEffectDefaultData data)
        {
            IUnitEntity caster = executionContext.Spell.Caster;
            if (!target.IsAlive || (target != caster && caster.CanAttack(target)))
                return SpellEffectExecutionResult.PreventEffect;

            IDamageCalculator damageCalculator = damageCalculatorFactory.Resolve();
            damageCalculator.CalculateHeal(executionContext, target, info);
            if (info.Damage == null)
                return SpellEffectExecutionResult.Ok;

            bool wasHurt = target.Health < target.MaxHealth;
            target.ModifyHealth(info.Damage.AdjustedDamage, DamageType.Heal, caster);

            // "Heal X to full health" objectives (Support Training: Healing), target-less activate objectives listing X in their reward pane
            if (wasHurt && target.Health >= target.MaxHealth && caster is IPlayer player && target is not IPlayer)
                foreach (uint targetGroupId in AssetManager.Instance.GetTargetGroupsForCreatureId(target.CreatureId))
                    player.QuestManager.ObjectiveUpdateRewardPane(QuestObjectiveType.ActivateEntity, targetGroupId, 1u);
            ProcTrigger.OnHeal(caster, target, executionContext.Spell.Parameters.SpellInfo, info.Damage, executionContext.Spell.Parameters.IsProcTriggered);
            return SpellEffectExecutionResult.Ok;
        }
    }
}
