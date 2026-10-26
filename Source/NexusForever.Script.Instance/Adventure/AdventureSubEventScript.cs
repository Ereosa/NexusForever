using NexusForever.Game.Abstract.PublicEvent;
using NexusForever.Script.Template;
using NexusForever.Script.Template.Filter;

namespace NexusForever.Script.Instance.Adventure
{
    /// <summary>
    /// Sub-events of the adventures driven by an <see cref="AdventureEventScript"/>: they spawn their entities and report their objectives.
    /// </summary>
    [ScriptFilterOwnerId(
        418, 420, 421, 422, 423, 424, 425, 426, 427, 428, 429, 430, 431, 432, 433, 434)] // The Hycrest Insurrection
    public class AdventureSubEventScript : IPublicEventScript, IOwnedScript<IPublicEvent>
    {
        private IPublicEvent publicEvent;

        public void OnLoad(IPublicEvent owner)
        {
            publicEvent = owner;
            publicEvent.SetPhase(0u);
        }

        public void OnPublicEventObjectiveStatus(IPublicEventObjective objective)
        {
            foreach (IPublicEvent adventure in publicEvent.Map.PublicEventManager.GetEvents().ToList())
                adventure.InvokeScriptCollection<AdventureEventScript>(s => s.OnSubObjective(publicEvent, objective));
        }
    }
}
