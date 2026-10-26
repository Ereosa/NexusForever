using NexusForever.Network.Message;
using NexusForever.Network.World.Message.Model.Friendship;

namespace NexusForever.WorldServer.Network.Message.Handler.Friendship
{
    public class ClientFriendshipSetAutoResponseMessageHandler : IMessageHandler<IWorldSession, ClientFriendshipSetAutoResponseMessage>
    {
        public void HandleMessage(IWorldSession session, ClientFriendshipSetAutoResponseMessage message)
        {
            // the client keeps the texts in its own settings and sends them again on every login
            session.Player.AwayAutoResponse = message.AwayMessage;
            session.Player.BusyAutoResponse = message.BusyMessage;
        }
    }
}
