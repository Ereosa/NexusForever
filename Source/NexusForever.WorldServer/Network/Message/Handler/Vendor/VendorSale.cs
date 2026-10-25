using System.Collections.Generic;
using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Static.Entity;
using NexusForever.Network.World.Message.Model.Shared;
using NexusForever.Network.World.Message.Static;

namespace NexusForever.WorldServer.Network.Message.Handler.Vendor
{
    public static class VendorSale
    {
        /// <summary>
        /// Sell the supplied quantity of the item at <see cref="ItemLocation"/> to the vendor, the sold item can be bought back.
        /// </summary>
        public static void SellItem(IPlayer player, IVendorInfo vendorInfo, IBuybackManager buybackManager, ItemLocation location, uint quantity)
        {
            IItem item = player.Inventory.GetItem(location);
            if (item?.Info == null)
                return;

            if (quantity == 0u || quantity > item.StackCount)
                quantity = item.StackCount;

            float costMultiplier = vendorInfo.SellPriceMultiplier * quantity;

            // sell price is either explicit on the item or calculated, both are returned by the item
            var currencyChange = new List<(CurrencyType CurrencyTypeId, ulong CurrencyAmount)>();
            for (byte i = 0; i < item.Info.Entry.CurrencyTypeIdSellToVendor.Length; i++)
            {
                CurrencyType currencyId = item.Info.GetVendorSellCurrency(i);
                ulong currencyAmount = (ulong)(item.GetVendorSellAmount(i) * costMultiplier);
                if (currencyId == CurrencyType.None || currencyAmount == 0ul)
                    continue;

                currencyChange.Add((currencyId, currencyAmount));
            }

            foreach ((CurrencyType currencyTypeId, ulong currencyAmount) in currencyChange)
                player.CurrencyManager.CurrencyAddAmount(currencyTypeId, currencyAmount);

            IItem soldItem = player.Inventory.ItemTake(location, quantity, ItemUpdateReason.Vendor);
            buybackManager.AddItem(player, soldItem, quantity, currencyChange);
        }
    }
}
