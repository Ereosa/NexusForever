using System.Linq;
using NexusForever.Game.Static.Fortune;
using NexusForever.Network.Message;
using NexusForever.Network.World.Message.Model.Fortune;

namespace NexusForever.WorldServer.Network.Message.Handler.Fortune
{
    /// <summary>
    /// Sent when the player reveals one of the dealt cards, the card's reward goes to the account inventory.
    /// </summary>
    public class ClientFortuneFlipCardHandler : IMessageHandler<IWorldSession, ClientFortuneFlipCard>
    {
        public void HandleMessage(IWorldSession session, ClientFortuneFlipCard flipCard)
        {
            FortuneManager.Deal deal = FortuneManager.GetDeal(session.Account.Id);
            if (deal == null || flipCard.SelectedCardIndex >= 3u || deal.Flipped[flipCard.SelectedCardIndex])
                return;

            deal.Flipped[flipCard.SelectedCardIndex] = true;
            session.Account.ItemManager.AddItem(deal.AccountItemId[flipCard.SelectedCardIndex]);

            // Update only reveals cards, the client counts the game as finished (and lets the next click return to the
            // pedestal for another coin) once an update with Reset arrives, which also reveals any card left
            bool complete = deal.Flipped.All(f => f);
            var update = new ServerFortuneCardUpdate
            {
                Operation = complete ? FortuneOperation.Reset : FortuneOperation.Update
            };
            for (int i = 0; i < 3; i++)
                update.CardFlipped[i] = deal.Flipped[i];
            session.EnqueueMessageEncrypted(update);

            if (complete)
                FortuneManager.Complete(session.Account.Id);
        }
    }
}
