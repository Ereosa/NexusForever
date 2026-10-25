using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Spell;
using NexusForever.Game.Abstract.Spell.Effect;
using NexusForever.Game.Abstract.Spell.Effect.Data;
using NexusForever.Game.Abstract.Spell.Target;
using NexusForever.Game.Static.Spell;
using NexusForever.Game.Static.Spell.Effect;

namespace NexusForever.Game.Spell.Effect.Handler
{
    [SpellEffectHandler(SpellEffectType.GiveAugmentPowerToPlayer)]
    public class SpellEffectGiveAugmentPowerToPlayerHandler : ISpellEffectApplyHandler<ISpellEffectDefaultData>
    {
        /// <summary>
        /// Handle <see cref="ISpell"/> effect apply on <see cref="IUnitEntity"/> target.
        /// </summary>
        /// <remarks>
        /// DataBits00 is the amount of permanent AMP power, eg: "AMP Power Upgrade".
        /// </remarks>
        public SpellEffectExecutionResult Apply(ISpellExecutionContext executionContext, IUnitEntity target, ISpellTargetEffectInfo info, ISpellEffectDefaultData data)
        {
            if (target is not IPlayer player)
                return SpellEffectExecutionResult.PreventEffect;

            return player.SpellManager.AddBonusAmpPower(data.DataBits00) ? SpellEffectExecutionResult.Ok : SpellEffectExecutionResult.PreventEffect;
        }
    }
}
