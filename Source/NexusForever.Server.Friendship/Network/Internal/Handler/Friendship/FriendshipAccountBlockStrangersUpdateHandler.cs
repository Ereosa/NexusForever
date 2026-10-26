using NexusForever.Database.Friendship;
using NexusForever.Network.Internal.Message.Friendship;
using NexusForever.Server.Friendship.Game.Account;
using Rebus.Handlers;

namespace NexusForever.Server.Friendship.Network.Internal.Handler.Friendship
{
    public class FriendshipAccountBlockStrangersUpdateHandler : IHandleMessages<FriendshipAccountBlockStrangersUpdateMessage>
    {
        #region Dependency Injection

        private readonly FriendshipContext _context;
        private readonly AccountManager _accountManager;

        public FriendshipAccountBlockStrangersUpdateHandler(
            FriendshipContext context,
            AccountManager accountManager)
        {
            _context        = context;
            _accountManager = accountManager;
        }

        #endregion

        public async Task Handle(FriendshipAccountBlockStrangersUpdateMessage message)
        {
            Account account = await _accountManager.GetAccountAsync(message.AccountId);
            if (account == null)
                return;

            await account.SetBlockAccountFriendRequestsAsync(message.BlockStrangerRequests);

            await _context.SaveChangesAsync();
        }
    }
}
