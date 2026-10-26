using NexusForever.Game.Abstract.Entity;
using NexusForever.Network.Message;
using NexusForever.Network.World.Message.Model;

namespace NexusForever.WorldServer.Network.Message.Handler.Entity
{
    public class ClientEntityCommandHandler : IMessageHandler<IWorldSession, ClientEntityCommand>
    {
        public void HandleMessage(IWorldSession session, ClientEntityCommand entityCommand)
        {
            IWorldEntity mover = session.Player;
            if (mover == null)
                return;

            if (session.Player.ControlGuid != null)
                mover = session.Player.GetVisible<IWorldEntity>(session.Player.ControlGuid.Value);

            // a taxi rider is carried by the server, during the flight the client sends world positions for the rider which
            // would be added to the taxi position again and flip the rider between two places every update
            if (mover == session.Player
                && session.Player.PlatformGuid != null
                && session.Player.GetVisible<IGridEntity>(session.Player.PlatformGuid.Value) is ITaxiEntity)
                return;

            mover?.MovementManager.HandleClientEntityCommands(entityCommand.Commands, entityCommand.Time);
        }
    }
}
