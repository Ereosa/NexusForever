using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Spell;
using NexusForever.Game.Abstract.Spell.Effect;
using NexusForever.Game.Abstract.Spell.Effect.Data;
using NexusForever.Game.Abstract.Spell.Target;
using NexusForever.Game.Static.Spell;
using NexusForever.Game.Static.Spell.Effect;

namespace NexusForever.Game.Spell.Effect.Handler
{
    [SpellEffectHandler(SpellEffectType.DespawnUnit)]
    public class SpellEffectDespawnUnitHandler : ISpellEffectApplyHandler<ISpellEffectDefaultData>
    {
        /// <summary>
        /// Time before a despawned unit (e.g. a used discovery) comes back.
        /// </summary>
        /// <remarks>
        /// Retail values are unknown, the client data has no respawn times.
        /// </remarks>
        private static readonly TimeSpan RespawnDelay = TimeSpan.FromMinutes(5d);

        /// <summary>
        /// Handle <see cref="ISpell"/> effect apply on <see cref="IUnitEntity"/> target.
        /// </summary>
        public SpellEffectExecutionResult Apply(ISpellExecutionContext executionContext, IUnitEntity target, ISpellTargetEffectInfo info, ISpellEffectDefaultData data)
        {
            if (target is IPlayer)
                return SpellEffectExecutionResult.PreventEffect;

            target.DespawnWithRespawn(RespawnDelay);
            return SpellEffectExecutionResult.Ok;
        }
    }
}
