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
    public class ClientFriendshipAddByNameHandler : IMessageHandler<IWorldSession, ClientFriendshipAddByName>
    {
        #region Dependency Injection

        private readonly IInternalMessagePublisher messagePublisher;
        private readonly IPlayerManager playerManager;
        private readonly IDatabaseManager databaseManager;
        private readonly ICharacterManager characterManager;

        public ClientFriendshipAddByNameHandler(
            IInternalMessagePublisher messagePublisher,
            IPlayerManager playerManager,
            IDatabaseManager databaseManager,
            ICharacterManager characterManager)
        {
            this.messagePublisher = messagePublisher;
            this.playerManager    = playerManager;
            this.databaseManager  = databaseManager;
            this.characterManager = characterManager;
        }

        #endregion

        public void HandleMessage(IWorldSession session, ClientFriendshipAddByName message)
        {
            var request = new FriendshipNameInviteRequestMessage
            {
                Inviter     = session.Player.Identity.ToInternalIdentity(),
                InviteeName = new NexusForever.Network.Internal.Message.Shared.IdentityName
                {
                    Name      = message.Name,
                    RealmName = message.RealmName
                },
                Type = message.Type,
                Note = message.Note != string.Empty ? message.Note : null
            };

            // only friend invites respect the target's "block friend requests" lock, ignores and rivals are one sided
            ICharacter character = message.Type == FriendshipType.Friend ? characterManager.GetCharacter(message.Name) : null;
            if (character == null)
            {
                messagePublisher.PublishAsync(request).FireAndForgetAsync();
                return;
            }

            FriendRequestBlockCheck.Check(session, playerManager, databaseManager, character.CharacterId, blocked =>
            {
                if (blocked)
                    session.EnqueueMessageEncrypted(new ServerFriendshipResult { Result = FriendshipResult.FriendsBlocked });
                else
                    messagePublisher.PublishAsync(request).FireAndForgetAsync();
            });
        }
    }
}
