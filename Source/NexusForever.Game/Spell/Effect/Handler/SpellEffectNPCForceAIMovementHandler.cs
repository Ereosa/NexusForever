using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Spell;
using NexusForever.Game.Abstract.Spell.Effect;
using NexusForever.Game.Abstract.Spell.Effect.Data;
using NexusForever.Game.Abstract.Spell.Target;
using NexusForever.Game.Static.Spell;
using NexusForever.Game.Static.Spell.Effect;

namespace NexusForever.Game.Spell.Effect.Handler
{
    [SpellEffectHandler(SpellEffectType.NPCForceAIMovement)]
    public class SpellEffectNPCForceAIMovementHandler : ISpellEffectApplyHandler<ISpellEffectDefaultData>
    {
        /// <summary>
        /// Handle <see cref="ISpell"/> effect apply on <see cref="IUnitEntity"/> target.
        /// </summary>
        /// <remarks>
        /// 41693 Pet Command - Go To Location: "Commands your bots to go to a location and stay there."
        /// </remarks>
        public SpellEffectExecutionResult Apply(ISpellExecutionContext executionContext, IUnitEntity target, ISpellTargetEffectInfo info, ISpellEffectDefaultData data)
        {
            if (target != executionContext.Spell.Caster || target is not IPlayer player)
                return SpellEffectExecutionResult.Ok;

            if (executionContext.Spell.Parameters.TargetPosition == null)
                return SpellEffectExecutionResult.PreventEffect;

            player.CombatPetManager.GoTo(executionContext.Spell.Parameters.TargetPosition.Vector);
            return SpellEffectExecutionResult.Ok;
        }
    }
}
