using NexusForever.Game.Abstract.PublicEvent;
using NexusForever.Script.Template;
using NexusForever.Script.Template.Filter;

namespace NexusForever.Script.Instance.Adventure
{
    /// <summary>
    /// Sub-events of the adventures driven by an <see cref="AdventureEventScript"/>: they spawn their entities and report their objectives.
    /// </summary>
    [ScriptFilterOwnerId(
        418, 420, 421, 422, 423, 424, 425, 426, 427, 428, 429, 430, 431, 432, 433, 434,  // The Hycrest Insurrection
        57, 58, 59, 60, 61, 62, 63, 64, 65, 66, 67, 68, 69, 70, 71, 72, 73, 74, 75, 76, // The Siege of Tempest Refuge
        78, 83, 84, 90, 91, 204,
        672, 674, 675, 676, 695)]                                                        // Levian Bay
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
