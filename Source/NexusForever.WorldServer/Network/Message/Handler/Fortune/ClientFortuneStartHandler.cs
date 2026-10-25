using NexusForever.Game.Static.AccountInventory;
using NexusForever.Network.Message;
using NexusForever.Network.World.Message.Model.Fortune;

namespace NexusForever.WorldServer.Network.Message.Handler.Fortune
{
    /// <summary>
    /// Sent when the player places a Fortune Coin on the pedestal.
    /// </summary>
    public class ClientFortuneStartHandler : IMessageHandler<IWorldSession, ClientFortuneStart>
    {
        public void HandleMessage(IWorldSession session, ClientFortuneStart _)
        {
            // an unfinished game is dealt again rather than charging another coin
            FortuneManager.Deal deal = FortuneManager.GetDeal(session.Account.Id);
            if (deal == null)
            {
                if (!FortuneManager.HasFortuneCoin(session))
                {
                    // returns the client to the idle pedestal
                    session.EnqueueMessageEncrypted(new ServerFortuneReset { Unknown = 3u });
                    return;
                }

                session.Account.CurrencyManager.CurrencySubtractAmount(AccountCurrencyType.MysticShiny, 1ul);
                deal = FortuneManager.NewDeal(session.Account.Id);
            }

            session.EnqueueMessageEncrypted(FortuneManager.BuildCards(deal));
        }
    }
}
