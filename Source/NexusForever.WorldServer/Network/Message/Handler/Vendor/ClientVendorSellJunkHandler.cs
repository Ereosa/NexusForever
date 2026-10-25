using System.Linq;
using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Static.Entity;
using NexusForever.Network.Message;
using NexusForever.Network.World.Message.Model;
using NexusForever.Network.World.Message.Model.Shared;

namespace NexusForever.WorldServer.Network.Message.Handler.Vendor
{
    public class ClientVendorSellJunkHandler : IMessageHandler<IWorldSession, ClientVendorSellJunk>
    {
        /// <summary>
        /// Item2Category of items the client counts as junk (Item.CodeEnumItem2Category.Junk).
        /// </summary>
        private const uint JunkItem2CategoryId = 94u;

        #region Dependency Injection

        private readonly IBuybackManager buybackManager;

        public ClientVendorSellJunkHandler(
            IBuybackManager buybackManager)
        {
            this.buybackManager = buybackManager;
        }

        #endregion

        public void HandleMessage(IWorldSession session, ClientVendorSellJunk vendorSellJunk)
        {
            IVendorInfo vendorInfo = session.Player.SelectedVendorInfo;
            if (vendorInfo == null)
                return;

            ItemLocation[] junk = session.Player.Inventory
                .Where(b => b.Location == InventoryLocation.Inventory)
                .SelectMany(b => b)
                .Where(i => i.Info?.Entry.Item2CategoryId == JunkItem2CategoryId)
                .Select(i => new ItemLocation
                {
                    Location = i.Location,
                    BagIndex = i.BagIndex
                })
                .ToArray();

            foreach (ItemLocation location in junk)
                VendorSale.SellItem(session.Player, vendorInfo, buybackManager, location, 0u);
        }
    }
}
