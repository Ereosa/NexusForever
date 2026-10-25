using NexusForever.Database.Auth;
using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Static.AccountInventory;

namespace NexusForever.Game.Abstract.Account.Inventory
{
    public interface IAccountItemManager : IDatabaseAuth
    {
        /// <summary>
        /// Add a new <see cref="IAccountItem"/> for the supplied account item id.
        /// </summary>
        IAccountItem AddItem(uint accountItemId);

        /// <summary>
        /// Take the <see cref="IAccountItem"/> with supplied id, granting its contents to the supplied <see cref="IPlayer"/>.
        /// </summary>
        AccountOperationResult TakeItem(IPlayer player, ulong id);

        /// <summary>
        /// Returns if the <see cref="IAccountItem"/> only grants account wide contents, these are applied straight away when bought at character select.
        /// </summary>
        bool IsAccountOnly(IAccountItem item);

        /// <summary>
        /// Send the account inventory to the client.
        /// </summary>
        void SendInitialPackets();
    }
}
