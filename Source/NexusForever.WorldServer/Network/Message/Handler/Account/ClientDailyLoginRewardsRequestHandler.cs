using NexusForever.Network.Message;
using NexusForever.Network.World.Message.AccountInventory;

namespace NexusForever.WorldServer.Network.Message.Handler.Account
{
    public class ClientDailyLoginRewardsRequestHandler : IMessageHandler<IWorldSession, ClientDailyLoginRewardsRequest>
    {
        public void HandleMessage(IWorldSession session, ClientDailyLoginRewardsRequest request)
        {
            session.Account.DailyLoginManager.ClaimRewards();
        }
    }
}
