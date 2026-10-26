using System.Numerics;
using NexusForever.Game.Abstract.Cinematic;
using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Map;
using NexusForever.Game.Abstract.Map.Instance;
using NexusForever.Game.Abstract.Map.Lock;
using NexusForever.Game.Static.Cinematic;
using NexusForever.Game.Static.Reputation;
using NexusForever.GameTable;
using NexusForever.GameTable.Model;

namespace NexusForever.Script.Instance.Expedition
{
    /// <summary>
    /// Map script for expeditions rebuilt from client data: no respawns, the arrival cinematic on entering before the event has started,
    /// and a way back for players who didn't come through a transport.
    /// </summary>
    public abstract class ExpeditionMapScript : EventBaseContentMapScript
    {
        /// <summary>
        /// Arrival cinematic played when entering before the first phase is done, null for none.
        /// </summary>
        protected abstract CinematicSceneId? ArrivalScene { get; }

        /// <summary>
        /// Where Exile players leave to when they didn't enter by a transport (the expedition transport in their zone).
        /// </summary>
        protected abstract (uint WorldId, Vector3 Position) ExileReturn { get; }

        /// <summary>
        /// Where Dominion players leave to when they didn't enter by a transport.
        /// </summary>
        protected abstract (uint WorldId, Vector3 Position) DominionReturn { get; }

        #region Dependency Injection

        protected readonly IGameTableManager gameTableManager;
        protected readonly ICinematicFactory cinematicFactory;

        protected ExpeditionMapScript(
            IGameTableManager gameTableManager,
            ICinematicFactory cinematicFactory)
        {
            this.gameTableManager = gameTableManager;
            this.cinematicFactory = cinematicFactory;
        }

        #endregion

        /// <summary>
        /// Invoked when <see cref="IContentMapInstance"/> is loaded.
        /// </summary>
        public override void OnLoad(IContentMapInstance owner)
        {
            // creatures killed and objects used in the expedition stay gone
            owner.RespawnsDisabled = true;
            base.OnLoad(owner);
        }

        /// <summary>
        /// Invoked when <see cref="IGridEntity"/> is added to map.
        /// </summary>
        public override void OnAddToMap(IGridEntity entity)
        {
            base.OnAddToMap(entity);

            if (entity is not IPlayer player)
                return;

            if (ArrivalScene.HasValue && publicEvent != null && !publicEvent.HasFinished && publicEvent.Phase == 0u)
                player.CinematicManager.QueueCinematic(cinematicFactory.CreateCinematic(ArrivalScene.Value));

            (uint worldId, Vector3 position) = player.Faction1 == Faction.Dominion ? DominionReturn : ExileReturn;
            player.InstancePortalReturn ??= new ReturnPosition
            {
                Info     = new ReturnInfo { Entry = gameTableManager.World.GetEntry(worldId) },
                Position = position
            };
        }

        private class ReturnInfo : IMapInfo
        {
            public WorldEntry Entry { get; init; }
            public IMapLock MapLock { get; init; }
        }

        private class ReturnPosition : IMapPosition
        {
            public IMapInfo Info { get; init; }
            public Vector3 Position { get; set; }
        }
    }
}
