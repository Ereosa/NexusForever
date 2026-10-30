using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Spell;
using NexusForever.Game.Abstract.Spell.Effect;
using NexusForever.Game.Abstract.Spell.Effect.Data;
using NexusForever.Game.Abstract.Spell.Target;
using NexusForever.Game.Static.Entity;
using NexusForever.Game.Static.Spell;
using NexusForever.Game.Static.Spell.Effect;
using NexusForever.Network.World.Combat;
using NexusForever.Network.World.Message.Model;

namespace NexusForever.Game.Spell.Effect.Handler
{
    [SpellEffectHandler(SpellEffectType.Stealth)]
    public class SpellEffectStealthHandler : ISpellEffectApplyHandler<ISpellEffectDefaultData>, ISpellEffectRemoveHandler<ISpellEffectDefaultData>
    {
        /// <summary>
        /// Handle <see cref="ISpell"/> effect apply on <see cref="IUnitEntity"/> target.
        /// </summary>
        public SpellEffectExecutionResult Apply(ISpellExecutionContext executionContext, IUnitEntity target, ISpellTargetEffectInfo info, ISpellEffectDefaultData data)
        {
            if (!target.CreateFlags.HasFlag(EntityCreateFlag.IsStealthed))
            {
                // flag is only read by the client when the entity is created, so late observers see the right state
                target.CreateFlags |= EntityCreateFlag.IsStealthed;
                SendCombatLog(target, false);
            }

            return SpellEffectExecutionResult.Ok;
        }

        /// <summary>
        /// Handle <see cref="ISpell"/> effect remove on <see cref="IUnitEntity"/> target.
        /// </summary>
        public void Remove(ISpell spell, IUnitEntity target, ISpellTargetEffectInfo info, ISpellEffectDefaultData data)
        {
            // another active spell can still be keeping the unit in stealth
            if (target.GetSpellsByEffect(SpellEffectType.Stealth).Any(s => s != spell && !s.IsFinished))
                return;

            if (!target.CreateFlags.HasFlag(EntityCreateFlag.IsStealthed))
                return;

            target.CreateFlags &= ~EntityCreateFlag.IsStealthed;
            SendCombatLog(target, true);
        }

        internal static void SendCombatLog(IUnitEntity target, bool exiting)
        {
            target.EnqueueToVisible(new ServerCombatLog
            {
                CombatLog = new CombatLogStealth
                {
                    UnitId   = target.Guid,
                    BExiting = exiting
                }
            }, true);
        }
    }
}
