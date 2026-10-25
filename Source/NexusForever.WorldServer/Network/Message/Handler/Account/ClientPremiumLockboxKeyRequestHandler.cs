using NexusForever.Network.Message;
using NexusForever.Network.World.Message.AccountInventory;

namespace NexusForever.WorldServer.Network.Message.Handler.Account
{
    public class ClientPremiumLockboxKeyRequestHandler : IMessageHandler<IWorldSession, ClientPremiumLockboxKeyRequest>
    {
        public void HandleMessage(IWorldSession session, ClientPremiumLockboxKeyRequest request)
        {
            session.Account.DailyLoginManager.ClaimPremiumLockboxKey();
        }
    }
}
