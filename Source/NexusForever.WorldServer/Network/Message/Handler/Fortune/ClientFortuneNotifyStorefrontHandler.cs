using NexusForever.Network.Message;
using NexusForever.Network.World.Message.Model.Fortune;

namespace NexusForever.WorldServer.Network.Message.Handler.Fortune
{
    /// <summary>
    /// Sent when the Fortunes window opens, the client waits on the cards before it finishes loading.
    /// </summary>
    public class ClientFortuneNotifyStorefrontHandler : IMessageHandler<IWorldSession, ClientFortuneNotifyStorefront>
    {
        public void HandleMessage(IWorldSession session, ClientFortuneNotifyStorefront _)
        {
            session.EnqueueMessageEncrypted(FortuneManager.BuildRewardList());
            session.EnqueueMessageEncrypted(FortuneManager.BuildCards(FortuneManager.GetDeal(session.Account.Id)));
        }
    }
}
