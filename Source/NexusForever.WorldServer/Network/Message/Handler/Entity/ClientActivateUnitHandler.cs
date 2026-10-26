using NexusForever.Game.Abstract.Entity;
using NexusForever.Script.Template;
using NexusForever.Network;
using NexusForever.Network.Message;
using NexusForever.Network.World.Message.Model.Spell;

namespace NexusForever.WorldServer.Network.Message.Handler.Entity
{
    public class ClientActivateUnitHandler : IMessageHandler<IWorldSession, ClientActivateUnit>
    {
        public void HandleMessage(IWorldSession session, ClientActivateUnit activateUnit)
        {
            IWorldEntity entity = session.Player.GetVisible<IWorldEntity>(activateUnit.UnitId);
            if (entity == null)
                throw new InvalidPacketValueException();

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
