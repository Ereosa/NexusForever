using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Spell;
using NexusForever.Game.Abstract.Spell.Effect;
using NexusForever.Game.Abstract.Spell.Effect.Data;
using NexusForever.Game.Abstract.Spell.Target;
using NexusForever.Game.Static.Spell;
using NexusForever.Game.Static.Spell.Effect;
using NexusForever.Script.Template;

namespace NexusForever.Game.Spell.Effect.Handler
{
    /// <summary>
    /// Sends the ravel signal in DataBits01 to the scripts of the target and of the entities the caster can see.
    /// </summary>
    /// <remarks>
    /// Retail ran these through server side event scripts, the scripts of the entity running the event (a minigame console, a cannon)
    /// pick up the signals they care about.
    /// </remarks>
    [SpellEffectHandler(SpellEffectType.RavelSignal)]
    public class SpellEffectRavelSignalHandler : ISpellEffectApplyHandler<ISpellEffectDefaultData>
    {
        /// <summary>
        /// Handle <see cref="ISpell"/> effect apply on <see cref="IUnitEntity"/> target.
        /// </summary>
        public SpellEffectExecutionResult Apply(ISpellExecutionContext executionContext, IUnitEntity target, ISpellTargetEffectInfo info, ISpellEffectDefaultData data)
        {
            IUnitEntity source = executionContext.Spell.Caster;
            uint signal = data.DataBits01;

            target.InvokeScriptCollection<IWorldEntityScript>(s => s.OnRavelSignal(source, signal));
            foreach (IWorldEntity entity in source.GetVisible().OfType<IWorldEntity>().Where(e => e != target).ToList())
                entity.InvokeScriptCollection<IWorldEntityScript>(s => s.OnRavelSignal(source, signal));

            return SpellEffectExecutionResult.Ok;
        }
    }
}
