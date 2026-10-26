using NexusForever.Game;
using NexusForever.Game.Abstract;
using NexusForever.Game.Abstract.Character;
using NexusForever.Game.Static.Friendship;
using NexusForever.Network.Internal;
using NexusForever.Network.Internal.Message.Friendship;
using NexusForever.Network.Message;
using NexusForever.Network.World.Message.Model.Friendship;
using NexusForever.Shared;

namespace NexusForever.WorldServer.Network.Message.Handler.Friendship
{
    public class ClientFriendshipSetNoteByNameHandler : IMessageHandler<IWorldSession, ClientFriendshipSetNoteByName>
    {
        #region Dependency Injection

        private readonly IInternalMessagePublisher messagePublisher;
        private readonly ICharacterManager characterManager;

        public ClientFriendshipSetNoteByNameHandler(
            IInternalMessagePublisher messagePublisher,
            ICharacterManager characterManager)
        {
            this.messagePublisher = messagePublisher;
            this.characterManager = characterManager;
        }

        #endregion

        public void HandleMessage(IWorldSession session, ClientFriendshipSetNoteByName message)
        {
            // characters can only be friends on the same realm, the name is resolved against the local character cache
            ICharacter character = characterManager.GetCharacter(message.Name);
            if (character == null)
            {
                session.EnqueueMessageEncrypted(new ServerFriendshipResult
                {
                    Result = FriendshipResult.PlayerNotFound
                });
                return;
            }

            var target = new Identity
            {
                Id      = character.CharacterId,
                RealmId = RealmContext.Instance.RealmId
            };

            messagePublisher.PublishAsync(new FriendshipNoteUpdateByIdentityMessage
            {
                Source = session.Player.Identity.ToInternalIdentity(),
                Target = target.ToInternalIdentity(),
                Note   = message.Note
            }).FireAndForgetAsync();
        }
    }
}
