using NexusForever.Database;
using NexusForever.Game;
using NexusForever.Game.Abstract.Character;
using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Static.Friendship;
using NexusForever.Network.Internal;
using NexusForever.Network.Internal.Message.Friendship;
using NexusForever.Network.Message;
using NexusForever.Network.World.Message.Model.Friendship;
using NexusForever.Shared;
using NexusForever.WorldServer.Network.Internal;

namespace NexusForever.WorldServer.Network.Message.Handler.Friendship
{
    public class ClientFriendshipAddByIdentityHandler : IMessageHandler<IWorldSession, ClientFriendshipAddByIdentity>
    {
        #region Dependency Injection

        private readonly IInternalMessagePublisher messagePublisher;
        private readonly IPlayerManager playerManager;
        private readonly IDatabaseManager databaseManager;

        public ClientFriendshipAddByIdentityHandler(
            IInternalMessagePublisher messagePublisher,
            IPlayerManager playerManager,
            IDatabaseManager databaseManager)
        {
            this.messagePublisher = messagePublisher;
            this.playerManager    = playerManager;
            this.databaseManager  = databaseManager;
        }

        #endregion

        public void HandleMessage(IWorldSession session, ClientFriendshipAddByIdentity message)
        {
            var request = new FriendshipIdentityInviteRequestMessage
            {
                Inviter = session.Player.Identity.ToInternalIdentity(),
                Invitee = message.Target.ToInternalIdentity(),
                Type    = message.Type,
                Note    = message.Note != string.Empty ? message.Note : null
            };

            // only friend invites respect the target's "block friend requests" lock, ignores and rivals are one sided
            if (message.Type != FriendshipType.Friend || message.Target.RealmId != RealmContext.Instance.RealmId)
            {
                messagePublisher.PublishAsync(request).FireAndForgetAsync();
                return;
            }

            FriendRequestBlockCheck.Check(session, playerManager, databaseManager, message.Target.Id, blocked =>
            {
                if (blocked)
                    session.EnqueueMessageEncrypted(new ServerFriendshipResult { Result = FriendshipResult.FriendsBlocked });
                else
                    messagePublisher.PublishAsync(request).FireAndForgetAsync();
            });
        }
    }
}
