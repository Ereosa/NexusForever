using Microsoft.Extensions.Logging;
using NexusForever.Game.Abstract.Entity;
using NexusForever.Script.Template;
using NexusForever.Network;
using NexusForever.Network.Message;
using NexusForever.Network.World.Message.Model.Spell;

namespace NexusForever.WorldServer.Network.Message.Handler.Entity
{
    public class ClientActivateUnitHandler : IMessageHandler<IWorldSession, ClientActivateUnit>
    {
        #region Dependency Injection

        private readonly ILogger<ClientActivateUnitHandler> log;

        public ClientActivateUnitHandler(
            ILogger<ClientActivateUnitHandler> log)
        {
            this.log = log;
        }

        #endregion

        public void HandleMessage(IWorldSession session, ClientActivateUnit activateUnit)
        {
            IWorldEntity entity = session.Player.GetVisible<IWorldEntity>(activateUnit.UnitId);
            // the client can activate a unit the server has just removed from its vision (a respawn, a phase or grid change),
            // that isn't a bad packet and must not disconnect the player (report #47: the Northern Wilds departure console)
            if (entity == null)
            {
                log.LogDebug($"{session.Player.Name} activated unit {activateUnit.UnitId} which isn't visible to them.");
                return;
            }

            // TODO: sanity check for range etc.

            // units without an activate spell, e.g. settler depots and soldier beacons that start their holdout when used
            session.Player.PathSettlerManager.OnEntityInteract(entity);
            if (entity.CreatureInfo?.Entry?.PathMissionIdSoldier is > 0u)
                session.Player.PathSoldierManager.OnActivate(entity);
            entity.OnActivate(session.Player);

            // world vehicles may be boarded by activating them as well, the client interaction used isn't known
            if (entity is IVehicleEntity)
                entity.InvokeScriptCollection<IWorldEntityScript>(s => s.OnBoard(session.Player));
        }
    }
}
