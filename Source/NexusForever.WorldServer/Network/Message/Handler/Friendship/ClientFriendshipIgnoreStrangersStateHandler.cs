using NexusForever.Network.Internal;
using NexusForever.Network.Internal.Message.Friendship;
using NexusForever.Network.Message;
using NexusForever.Network.World.Message.Model.Friendship;
using NexusForever.Shared;

namespace NexusForever.WorldServer.Network.Message.Handler.Friendship
{
    public class ClientFriendshipIgnoreStrangersStateHandler : IMessageHandler<IWorldSession, ClientFriendshipIgnoreStrangersState>
    {
        #region Dependency Injection

        private readonly IInternalMessagePublisher messagePublisher;

        public ClientFriendshipIgnoreStrangersStateHandler(
            IInternalMessagePublisher messagePublisher)
        {
            this.messagePublisher = messagePublisher;
        }

        #endregion

        public void HandleMessage(IWorldSession session, ClientFriendshipIgnoreStrangersState message)
        {
            messagePublisher.PublishAsync(new FriendshipAccountBlockStrangersUpdateMessage
            {
                AccountId             = session.Account.Id,
                BlockStrangerRequests = message.IgnoreStrangerInvites
            }).FireAndForgetAsync();
        }
    }
}
