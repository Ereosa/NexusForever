using System.Threading.Tasks;
using NexusForever.Game.Abstract.Entity;
using NexusForever.Network.Internal.Message.Friendship;
using NexusForever.Network.World.Message.Model.Friendship;
using Rebus.Handlers;

namespace NexusForever.WorldServer.Network.Internal.Handler.Friendship
{
    public class FriendshipAccountPersonalStatusUpdatedHandler : IHandleMessages<FriendshipAccountPersonalStatusUpdatedMessage>
    {
        #region Dependency Injection

        private readonly IPlayerManager playerManager;

        public FriendshipAccountPersonalStatusUpdatedHandler(
            IPlayerManager playerManager)
        {
            this.playerManager = playerManager;
        }

        #endregion

        public Task Handle(FriendshipAccountPersonalStatusUpdatedMessage message)
        {
            IPlayer player = playerManager.GetPlayerByAccountId(message.Account.Id);
            if (player == null)
                return Task.CompletedTask;

            player.Session.EnqueueMessageEncrypted(new ServerFriendshipAccountPersonalStatus
            {
                AccountPublicStatus   = message.Account.Status ?? string.Empty,
                AccountNickname       = message.Account.Nickname ?? string.Empty,
                Presence              = message.Account.Presence,
                BlockStrangerRequests = message.Account.BlockAccountFriendRequests
            });

            // the client keeps its "ignore stranger invites" toggle from this packet (flag 2), see WorldPaketHandler case 0x3A5
            player.Session.EnqueueMessageEncrypted(new ServerFriendshipIgnoreStrangersState
            {
                Flags = message.Account.BlockAccountFriendRequests ? 2u : 0u
            });

            return Task.CompletedTask;
        }
    }
}
