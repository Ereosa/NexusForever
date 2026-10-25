using NexusForever.Network.World.Message.Static;
using System;
using NexusForever.Game.Abstract.Entity;
using NexusForever.Network.Message;
using NexusForever.Network.World.Message.Model.Item;

namespace NexusForever.WorldServer.Network.Message.Handler.Item
{
    public class HandleClientItemUseLootBagHandler : IMessageHandler<IWorldSession, ClientItemUseLootBag>
    {
        public void HandleMessage(IWorldSession session, ClientItemUseLootBag useLootBag)
        {
            IItem item = session.Player.Inventory.GetItem(useLootBag.ItemLocation);
            if (item == null)
                throw new ArgumentException($"Item missing at Inventory Location {useLootBag.ItemLocation.Location} and Index {useLootBag.ItemLocation.BagIndex}.");

            if (useLootBag.Guid != item.Guid)
                throw new InvalidOperationException($"Guid {useLootBag.Guid} received does not match the Item found at Inventory Location {useLootBag.ItemLocation.Location} and Index {useLootBag.ItemLocation.BagIndex}.");

            // TODO: understand the logic the client uses to determine what items can be used
            /*if (item.Info.Entry.Item2CategoryId != 138)
                throw new NotImplementedException();*/

            // don't consume bags whose contents aren't known yet
            if (!session.Player.Map.LootManager.HasItemLoot(item.Id))
                return;

            // loot bags are single use, ItemUse treats unstackable items without charges as reusable (quest clickies)
            if (item.Info.Entry.MaxStackCount > 1 || item.Info.Entry.MaxCharges > 0)
            {
                if (!session.Player.Inventory.ItemUse(item))
                    return;
            }
            else
                session.Player.Inventory.ItemDelete(useLootBag.ItemLocation, ItemUpdateReason.ConsumeCharge);

            session.Player.Map.LootManager.GenerateLoot(session.Player, item);
        }
    }
}
