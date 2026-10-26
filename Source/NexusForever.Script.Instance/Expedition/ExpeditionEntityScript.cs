using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.PublicEvent;
using NexusForever.Script.Template;

namespace NexusForever.Script.Instance.Expedition
{
    /// <summary>
    /// Forwards deaths and activations of expedition creatures and objects to the expedition's event script.
    /// </summary>
    /// <remarks>
    /// Each expedition derives a script with the creature ids it needs (ScriptFilterCreatureId).
    /// </remarks>
    public abstract class ExpeditionEntityScript : IUnitScript, IOwnedScript<IUnitEntity>
    {
        protected IUnitEntity entity;

        public void OnLoad(IUnitEntity owner)
        {
            entity = owner;
        }

        public void OnDeath()
        {
            foreach (IPublicEvent publicEvent in entity.Map.PublicEventManager.GetEvents())
                publicEvent.InvokeScriptCollection<ExpeditionEventScript>(s => s.OnEntityDeath(entity));
        }

        public void OnActivateSuccess(IPlayer activator)
        {
            foreach (IPublicEvent publicEvent in entity.Map.PublicEventManager.GetEvents())
                publicEvent.InvokeScriptCollection<ExpeditionEventScript>(s => s.OnEntityActivated(entity, activator));
        }
    }
}
