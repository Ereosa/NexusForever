using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Spell;
using NexusForever.Game.Abstract.Spell.Effect;
using NexusForever.Game.Abstract.Spell.Effect.Data;
using NexusForever.Game.Abstract.Spell.Target;
using NexusForever.Game.Static.Spell;
using NexusForever.Game.Static.Spell.Effect;
using NexusForever.Game.Static.Reward;

namespace NexusForever.Game.Spell.Effect.Handler
{
    [SpellEffectHandler(SpellEffectType.RewardPropertyModifier)]
    public class SpellEffectRewardPropertyModifierHandler : ISpellEffectApplyHandler<ISpellEffectDefaultData>, ISpellEffectRemoveHandler<ISpellEffectDefaultData>
    {
        /// <summary>
        /// Handle <see cref="ISpell"/> effect apply on <see cref="IUnitEntity"/> target.
        /// </summary>
        /// <remarks>
        /// DataBits00 is the reward property, DataBits01 its data (eg: currency type), DataBits02 a float scalar and DataBits03 a float discrete value,
        /// eg: Flask of Advancement adds 0.5 to XP for 6 hours.
        /// </remarks>
        public SpellEffectExecutionResult Apply(ISpellExecutionContext executionContext, IUnitEntity target, ISpellTargetEffectInfo info, ISpellEffectDefaultData data)
        {
            if (target is not IPlayer player)
                return SpellEffectExecutionResult.PreventEffect;

            player.Account.RewardPropertyManager.UpdateRewardProperty((RewardPropertyType)data.DataBits00, GetValue(data), data.DataBits01);
            return SpellEffectExecutionResult.Ok;
        }

        /// <summary>
        /// Handle <see cref="ISpell"/> effect remove on <see cref="IUnitEntity"/> target.
        /// </summary>
        public void Remove(ISpell spell, IUnitEntity target, ISpellTargetEffectInfo info, ISpellEffectDefaultData data)
        {
            if (target is IPlayer player)
                player.Account.RewardPropertyManager.UpdateRewardProperty((RewardPropertyType)data.DataBits00, -GetValue(data), data.DataBits01);
        }

        private static float GetValue(ISpellEffectDefaultData data)
        {
            return BitConverter.UInt32BitsToSingle(data.DataBits02) + BitConverter.UInt32BitsToSingle(data.DataBits03);
        }
    }
}
