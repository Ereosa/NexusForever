using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Spell;
using NexusForever.Game.Abstract.Spell.Effect;
using NexusForever.Game.Abstract.Spell.Effect.Data;
using NexusForever.Game.Abstract.Spell.Target;
using NexusForever.Game.Static.Spell;
using NexusForever.Game.Static.Spell.Effect;

namespace NexusForever.Game.Spell.Effect.Handler
{
    [SpellEffectHandler(SpellEffectType.SummonPet)]
    public class SpellEffectSummonPetHandler : ISpellEffectApplyHandler<ISpellEffectSummonPetData>, ISpellEffectRemoveHandler<ISpellEffectSummonPetData>
    {
        /// <summary>
        /// Handle <see cref="ISpell"/> effect apply on <see cref="IUnitEntity"/> target.
        /// </summary>
        public SpellEffectExecutionResult Apply(ISpellExecutionContext executionContext, IUnitEntity target, ISpellTargetEffectInfo info, ISpellEffectSummonPetData data)
        {
            // TODO: creatures can also summon pets
            if (target is not IPlayer player)
                return SpellEffectExecutionResult.PreventEffect;

            if (!player.CombatPetManager.Summon(executionContext.Spell, info, data))
                return SpellEffectExecutionResult.PreventEffect;

            return SpellEffectExecutionResult.Ok;
        }

        /// <summary>
        /// Handle <see cref="ISpell"/> effect remove on <see cref="IUnitEntity"/> target.
        /// </summary>
        public void Remove(ISpell spell, IUnitEntity target, ISpellTargetEffectInfo info, ISpellEffectSummonPetData data)
        {
            if (target is not IPlayer player)
                return;

            player.CombatPetManager.Unsummon(info);
        }
    }
}
