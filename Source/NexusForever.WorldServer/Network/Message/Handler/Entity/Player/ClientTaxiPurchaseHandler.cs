using NexusForever.Network.Message;
using NexusForever.Network.World.Message.Model;

namespace NexusForever.WorldServer.Network.Message.Handler.Entity.Player
{
    public class ClientTaxiPurchaseHandler : IMessageHandler<IWorldSession, ClientTaxiPurchase>
    {
        public void HandleMessage(IWorldSession session, ClientTaxiPurchase taxiPurchase)
        {
            session.Player.TaxiManager.Purchase(taxiPurchase.TaxiRoutes);
        }
    }
}
