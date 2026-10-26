using NexusForever.Game.Abstract.PublicEvent;
using NexusForever.Script.Template;
using NexusForever.Script.Template.Filter;

namespace NexusForever.Script.Instance.Adventure.RiotInTheVoid
{
    /// <summary>
    /// The intro and stage sub-events of Riot in the Void: they spawn their entities and report their objectives to the adventure's script.
    /// </summary>
    [ScriptFilterOwnerId(178, 403, 404, 405, 406, 407, 408, 409, 410, 411, 412, 413, 414, 415, 416, 417)]
    public class RiotInTheVoidSubEventScript : IPublicEventScript, IOwnedScript<IPublicEvent>
    {
        private IPublicEvent publicEvent;

        public void OnLoad(IPublicEvent owner)
        {
            publicEvent = owner;
            publicEvent.SetPhase(0u);
        }

        public void OnPublicEventObjectiveStatus(IPublicEventObjective objective)
        {
            IPublicEvent adventure = publicEvent.Map.PublicEventManager.GetEvent(179u);
            adventure?.InvokeScriptCollection<RiotInTheVoidEventScript>(s => s.OnSubObjective(publicEvent, objective));
        }
    }
}
