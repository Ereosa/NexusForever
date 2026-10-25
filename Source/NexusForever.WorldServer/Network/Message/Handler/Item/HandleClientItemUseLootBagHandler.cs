using NexusForever.Network.World.Message.Static;
using System;
using Microsoft.Extensions.Logging;
using NexusForever.Game.Abstract.Entity;
using NexusForever.Network.Message;
using NexusForever.Network.World.Message.Model.Item;

namespace NexusForever.WorldServer.Network.Message.Handler.Item
{
    public class HandleClientItemUseLootBagHandler : IMessageHandler<IWorldSession, ClientItemUseLootBag>
    {
        #region Dependency Injection

        private readonly ILogger<HandleClientItemUseLootBagHandler> log;

        public HandleClientItemUseLootBagHandler(
            ILogger<HandleClientItemUseLootBagHandler> log)
        {
            this.log = log;
        }

        #endregion

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
            {
                // the client shows nothing happening, make the missing data easy to find (AccountAudit lists every container without item_loot)
                log.LogError($"Container item {item.Id} opened by {session.Player.Name} has no item_loot, the item was kept.");
                session.Player.SendSystemMessage("This container's contents aren't known to the server yet, so it wasn't opened.");
                return;
            }

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
