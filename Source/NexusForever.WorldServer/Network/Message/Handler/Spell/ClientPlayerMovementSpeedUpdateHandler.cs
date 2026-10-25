using NexusForever.Network.Message;
using NexusForever.Network.World.Message.Model.Spell;

namespace NexusForever.WorldServer.Network.Message.Handler.Spell
{
    public class ClientPlayerMovementSpeedUpdateHandler : IMessageHandler<IWorldSession, ClientPlayerMovementSpeedUpdate>
    {
        public void HandleMessage(IWorldSession session, ClientPlayerMovementSpeedUpdate movementSpeedUpdate)
        {
            if (session.Player != null)
                session.Player.MoveSpeedOption = movementSpeedUpdate.MoveSpeed;
        }
    }
}
