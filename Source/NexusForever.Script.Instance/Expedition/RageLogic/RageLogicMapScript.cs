using System.Numerics;
using NexusForever.Game.Abstract.Cinematic;
using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Map.Instance;
using NexusForever.Game.Abstract.PublicEvent;
using NexusForever.Game.Static.Cinematic;
using NexusForever.Game.Static.PublicEvent;
using NexusForever.GameTable;
using NexusForever.Script.Template.Filter;

namespace NexusForever.Script.Instance.Expedition.RageLogic
{
    /// <summary>
    /// Rage Logic has two public events: 213 on the Positron (choose a vehicle) and 214 on Asteroid DU-23 (the assault), both run together.
    /// </summary>
    [ScriptFilterOwnerId(1627)]
    public class RageLogicMapScript : ExpeditionMapScript
    {
        public override uint PublicEventId => 214u;

        protected override CinematicSceneId? ArrivalScene => CinematicSceneId.RageLogicArrival;

        // Transport to the Positron in Whitevale (creature 32244), used by both factions
        protected override (uint WorldId, Vector3 Position) ExileReturn => (51, new Vector3(3579.52f, -944.842f, 647.994f));
        protected override (uint WorldId, Vector3 Position) DominionReturn => (51, new Vector3(3579.52f, -944.842f, 647.994f));

        private IPublicEvent launchEvent;

        public RageLogicMapScript(
            IGameTableManager gameTableManager,
            ICinematicFactory cinematicFactory)
            : base(gameTableManager, cinematicFactory)
        {
        }

        public override void OnLoad(IContentMapInstance owner)
        {
            base.OnLoad(owner);
            launchEvent = owner.PublicEventManager.CreateEvent(213u);
        }

        public override void OnAddToMap(IGridEntity entity)
        {
            base.OnAddToMap(entity);

            if (entity is IPlayer player && launchEvent != null && !launchEvent.HasFinished)
                launchEvent.JoinEvent(player, PublicEventTeam.PublicTeam);
        }
    }
}
