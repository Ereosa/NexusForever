using System.Collections.Concurrent;
using System.Numerics;
using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Entity.Creature;
using NexusForever.Game.Abstract.Spell;
using NexusForever.Game.Abstract.Spell.Effect;
using NexusForever.Game.Abstract.Spell.Effect.Data;
using NexusForever.Game.Abstract.Spell.Target;
using NexusForever.Game.Static.Spell;
using NexusForever.Game.Static.Spell.Effect;

namespace NexusForever.Game.Spell.Effect.Handler
{
    [SpellEffectHandler(SpellEffectType.SummonCreature)]
    public class SpellEffectSummonCreatureHandler : ISpellEffectApplyHandler<ISpellEffectDefaultData>, ISpellEffectRemoveHandler<ISpellEffectDefaultData>
    {
        // creatures summoned by a cast, removed with the effect (eg: fireworks and toys last for the effect duration)
        private static readonly ConcurrentDictionary<uint, List<IWorldEntity>> summons = new();

        #region Dependency Injection

        private readonly ICreatureInfoManager creatureInfoManager;

        public SpellEffectSummonCreatureHandler(
            ICreatureInfoManager creatureInfoManager)
        {
            this.creatureInfoManager = creatureInfoManager;
        }

        #endregion

        /// <summary>
        /// Handle <see cref="ISpell"/> effect apply on <see cref="IUnitEntity"/> target.
        /// </summary>
        /// <remarks>
        /// DataBits00 is the creature and DataBits02 the number to summon around the target.
        /// </remarks>
        public SpellEffectExecutionResult Apply(ISpellExecutionContext executionContext, IUnitEntity target, ISpellTargetEffectInfo info, ISpellEffectDefaultData data)
        {
            ICreatureInfo creatureInfo = creatureInfoManager.GetCreatureInfo(data.DataBits00);
            if (creatureInfo == null || target.Map == null)
                return SpellEffectExecutionResult.PreventEffect;

            uint count = Math.Clamp(data.DataBits02, 1u, 10u);
            List<IWorldEntity> entities = summons.GetOrAdd(executionContext.Spell.CastingId, _ => []);
            for (uint i = 0u; i < count; i++)
            {
                // spread several summons in a small circle in front of the target
                float angle = target.Rotation.X + (count == 1u ? 0f : MathF.Tau * i / count);
                Vector3 position = target.Position + new Vector3(-MathF.Sin(angle), 0f, -MathF.Cos(angle)) * 2f;

                IWorldEntity entity = target.SummonFactory.Summon(creatureInfo, position, target.Rotation);
                lock (entities)
                    entities.Add(entity);
            }

            return SpellEffectExecutionResult.Ok;
        }

        /// <summary>
        /// Handle <see cref="ISpell"/> effect remove on <see cref="IUnitEntity"/> target.
        /// </summary>
        public void Remove(ISpell spell, IUnitEntity target, ISpellTargetEffectInfo info, ISpellEffectDefaultData data)
        {
            if (!summons.TryRemove(spell.CastingId, out List<IWorldEntity> entities))
                return;

            lock (entities)
                foreach (IWorldEntity entity in entities.Where(e => e.Map != null))
                    entity.RemoveFromMap();
        }
    }
}
