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
    [SpellEffectHandler(SpellEffectType.PetCastSpell)]
    public class SpellEffectPetCastSpellHandler : ISpellEffectApplyHandler<ISpellEffectPetCastSpellData>
    {
        #region Dependency Injection

        private readonly IGameTableManager gameTableManager;

        public SpellEffectPetCastSpellHandler(
            IGameTableManager gameTableManager)
        {
            this.gameTableManager = gameTableManager;
        }

        #endregion

        /// <summary>
        /// Handle <see cref="ISpell"/> effect apply on <see cref="IUnitEntity"/> target.
        /// </summary>
        /// <remarks>
        /// The effect names the summon spell of the pet that casts, eg: 35123 Player Cast - Barrage makes the pet summoned by 42814 cast 49502 Target finder.
        /// The pet is given the target selected by the player's spell.
        /// </remarks>
        public SpellEffectExecutionResult Apply(ISpellExecutionContext executionContext, IUnitEntity target, ISpellTargetEffectInfo info, ISpellEffectPetCastSpellData data)
        {
            if (target is not IPlayer player)
                return SpellEffectExecutionResult.PreventEffect;

            ICombatPetEntity pet = player.CombatPetManager.GetPetBySummonSpell(data.SummonSpell4Id);
            if (pet == null)
            {
                // tiers share the PetCastSpell data of their own summon spell, fall back to the same summon base spell
                Spell4Entry summonEntry = gameTableManager.Spell4.GetEntry(data.SummonSpell4Id);
                if (summonEntry != null)
                    pet = player.CombatPetManager.GetPets()
                        .FirstOrDefault(p => gameTableManager.Spell4.GetEntry(p.SummonSpell4Id)?.Spell4BaseIdBaseSpell == summonEntry.Spell4BaseIdBaseSpell);
            }

            if (pet == null || !pet.IsAlive)
                return SpellEffectExecutionResult.PreventEffect;

            uint primaryTargetId = executionContext.TargetCollection
                .GetTargets(SpellEffectTargetFlags.ImplicitTarget)
                .Select(t => t.Entity)
                .FirstOrDefault(e => e != null && e != player)?.Guid ?? executionContext.Spell.Parameters.PrimaryTargetId;

            pet.CastPetSpell(data.Spell4Id, primaryTargetId);
            return SpellEffectExecutionResult.Ok;
        }
    }
}
