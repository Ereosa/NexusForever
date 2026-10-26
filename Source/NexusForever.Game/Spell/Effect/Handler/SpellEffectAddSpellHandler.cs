using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Spell;
using NexusForever.Game.Abstract.Spell.Effect;
using NexusForever.Game.Abstract.Spell.Effect.Data;
using NexusForever.Game.Abstract.Spell.Target;
using NexusForever.Game.Static.Spell;
using NexusForever.Game.Static.Spell.Effect;
using NexusForever.GameTable;
using NexusForever.GameTable.Model;

namespace NexusForever.Game.Spell.Effect.Handler
{
    [SpellEffectHandler(SpellEffectType.AddSpell)]
    public class SpellEffectAddSpellHandler : ISpellEffectApplyHandler<ISpellEffectDefaultData>
    {
        #region Dependency Injection

        private readonly IGameTableManager gameTableManager;

        public SpellEffectAddSpellHandler(
            IGameTableManager gameTableManager)
        {
            this.gameTableManager = gameTableManager;
        }

        #endregion

        /// <summary>
        /// Handle <see cref="ISpell"/> effect apply on <see cref="IUnitEntity"/> target.
        /// </summary>
        /// <remarks>
        /// DataBits00 is a Spell4 the character learns, eg: riding licenses teach a riding skill.
        /// </remarks>
        public SpellEffectExecutionResult Apply(ISpellExecutionContext executionContext, IUnitEntity target, ISpellTargetEffectInfo info, ISpellEffectDefaultData data)
        {
            if (target is not IPlayer player)
                return SpellEffectExecutionResult.PreventEffect;

            Spell4Entry spell = gameTableManager.Spell4.GetEntry(data.DataBits00);
            if (spell == null)
                return SpellEffectExecutionResult.PreventEffect;

            byte tier = (byte)Math.Max(spell.TierIndex, 1u);
            if (player.SpellManager.GetSpell(spell.Spell4BaseIdBaseSpell) == null)
                player.SpellManager.AddSpell(spell.Spell4BaseIdBaseSpell, tier);
            else if (player.SpellManager.GetSpell(spell.Spell4BaseIdBaseSpell).Tier < tier)
                player.SpellManager.UpdateSpell(spell.Spell4BaseIdBaseSpell, tier, null);

            // learnt passives (eg: riding skill) take effect straight away
            player.CastSpell(spell.Id, new SpellParameters());
            return SpellEffectExecutionResult.Ok;
        }
    }
}
