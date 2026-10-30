using Microsoft.Extensions.Logging;
using NexusForever.Game.Abstract.Entity;
using NexusForever.Network;
using NexusForever.Network.Message;
using NexusForever.Network.World.Message.Model.Spell;

namespace NexusForever.WorldServer.Network.Message.Handler.Entity
{
    public class ClientActivateUnitCastHandler : IMessageHandler<IWorldSession, ClientActivateUnitCast>
    {
        #region Dependency Injection

        private readonly ILogger<ClientActivateUnitCastHandler> log;

        public ClientActivateUnitCastHandler(
            ILogger<ClientActivateUnitCastHandler> log)
        {
            this.log = log;
        }

        #endregion

        public void HandleMessage(IWorldSession session, ClientActivateUnitCast activateUnitCast)
        {
            IWorldEntity entity = session.Player.GetVisible<IWorldEntity>(activateUnitCast.UnitId);
            // the client can activate a unit the server has just removed from its vision (a respawn, a phase or grid change),
            // that isn't a bad packet and must not disconnect the player (report #47: the Northern Wilds departure console)
            if (entity == null)
            {
                log.LogDebug($"{session.Player.Name} activated unit {activateUnitCast.UnitId} which isn't visible to them.");
                return;
            }

            entity.OnActivateCast(session.Player, activateUnitCast.ClientSpellCastUniqueId);
        }
    }
}
