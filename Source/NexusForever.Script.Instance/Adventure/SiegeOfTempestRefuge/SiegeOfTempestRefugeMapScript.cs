using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Map.Instance;
using NexusForever.Game.Abstract.PublicEvent;
using NexusForever.Game.Static.PublicEvent;
using NexusForever.Game.Static.Reputation;
using NexusForever.Script.Template;
using NexusForever.Script.Template.Filter;

namespace NexusForever.Script.Instance.Adventure.SiegeOfTempestRefuge
{
    /// <summary>
    /// The siege is played from the side of the first player in: Exiles run public event 173, Dominion players 174.
    /// </summary>
    [ScriptFilterOwnerId(1233)]
    public class SiegeOfTempestRefugeMapScript : IContentMapScript, IOwnedScript<IContentMapInstance>
    {
        private IContentMapInstance map;
        private IPublicEvent publicEvent;

        public void OnLoad(IContentMapInstance owner)
        {
            map = owner;
            map.RespawnsDisabled = true;
        }

        public void OnAddToMap(IGridEntity entity)
        {
            if (entity is not IPlayer player)
                return;

            publicEvent ??= map.PublicEventManager.CreateEvent(player.Faction1 == Faction.Dominion ? 174u : 173u);
            publicEvent.JoinEvent(player, PublicEventTeam.PublicTeam);
        }

        public void OnPublicEventFinish(IPublicEvent finished, IPublicEventTeam publicEventTeam)
        {
            if (finished == publicEvent)
                map.Match?.MatchFinish();
        }

        public void OnMatchFinish()
        {
            publicEvent?.Finish(PublicEventTeam.PublicTeam);
        }
    }
}
