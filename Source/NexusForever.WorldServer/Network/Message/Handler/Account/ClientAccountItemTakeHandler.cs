using NexusForever.Game.Static.AccountInventory;
using NexusForever.Network.Message;
using NexusForever.Network.World.Message.AccountInventory;
using NexusForever.Network.World.Message.Model.AccountInventory;

namespace NexusForever.WorldServer.Network.Message.Handler.Account
{
    public class ClientAccountItemTakeHandler : IMessageHandler<IWorldSession, ClientAccountItemTake>
    {
        public void HandleMessage(IWorldSession session, ClientAccountItemTake take)
        {
            // account wide unlocks (eg: character slots) can also be claimed from character select
            AccountOperationResult result = session.Account.ItemManager.TakeItem(session.Player, take.UserInventoryId);

            session.EnqueueMessageEncrypted(new ServerAccountOperationResult
            {
                Operation = AccountOperation.TakeItem,
                Result    = result
            });
        }
    }
}
