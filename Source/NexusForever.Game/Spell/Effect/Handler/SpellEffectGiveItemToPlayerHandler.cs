using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Spell;
using NexusForever.Game.Abstract.Spell.Effect;
using NexusForever.Game.Abstract.Spell.Effect.Data;
using NexusForever.Game.Abstract.Spell.Target;
using NexusForever.Game.Static.Entity;
using NexusForever.Game.Static.Spell;
using NexusForever.Game.Static.Spell.Effect;
using NexusForever.Network.World.Message.Static;

namespace NexusForever.Game.Spell.Effect.Handler
{
    /// <remarks>
    /// DataBits00 is the Item2 id and DataBits01 the count (eg: quest objects handing over a quest item).
    /// </remarks>
    [SpellEffectHandler(SpellEffectType.GiveItemToPlayer)]
    public class SpellEffectGiveItemToPlayerHandler : ISpellEffectApplyHandler<ISpellEffectDefaultData>
    {
        /// <summary>
        /// Handle <see cref="ISpell"/> effect apply on <see cref="IUnitEntity"/> target.
        /// </summary>
        public SpellEffectExecutionResult Apply(ISpellExecutionContext executionContext, IUnitEntity target, ISpellTargetEffectInfo info, ISpellEffectDefaultData data)
        {
            IPlayer player = target as IPlayer ?? executionContext.Spell.Caster as IPlayer;
            if (player == null || data.DataBits00 == 0u)
                return SpellEffectExecutionResult.Ok;

            player.Inventory.ItemCreate(InventoryLocation.Inventory, data.DataBits00, Math.Max(1u, data.DataBits01), ItemUpdateReason.SpellEffect);
            return SpellEffectExecutionResult.Ok;
        }
    }
}
