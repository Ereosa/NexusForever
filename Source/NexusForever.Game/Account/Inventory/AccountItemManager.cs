using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NexusForever.Database.Auth;
using NexusForever.Database.Auth.Model;
using NexusForever.Game.Abstract.Account;
using NexusForever.Game.Abstract.Account.Inventory;
using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Static.AccountInventory;
using NexusForever.Game.Static.Entity;
using NexusForever.GameTable;
using NexusForever.GameTable.Model;
using NexusForever.Network.World.Message.Model.AccountInventory;
using NexusForever.Shared;

namespace NexusForever.Game.Account.Inventory
{
    /// <summary>
    /// Account inventory, holds store purchases until they are taken by a character.
    /// </summary>
    public class AccountItemManager : IAccountItemManager
    {
        private static readonly ILogger log = LegacyServiceProvider.Provider.GetService<ILogger<AccountItemManager>>();

        private readonly IAccount account;
        private readonly Dictionary<ulong, IAccountItem> items = [];
        private readonly List<IAccountItem> deletedItems = [];
        private ulong nextId = 1;

        /// <summary>
        /// Create a new <see cref="IAccountItemManager"/> from an existing database model.
        /// </summary>
        public AccountItemManager(IAccount account, AccountModel model)
        {
            this.account = account;

            foreach (AccountItemModel itemModel in model.AccountItem)
            {
                var item = new AccountItem(account, itemModel);
                if (item.Entry == null)
                    continue;

                items.Add(item.Id, item);
                nextId = Math.Max(nextId, item.Id + 1);
            }
        }

        public void Save(AuthContext context)
        {
            foreach (IAccountItem item in deletedItems)
                item.Save(context);
            deletedItems.Clear();

            foreach (IAccountItem item in items.Values)
                item.Save(context);
        }

        /// <summary>
        /// Add a new <see cref="IAccountItem"/> for the supplied account item id.
        /// </summary>
        public IAccountItem AddItem(uint accountItemId)
        {
            AccountItemEntry entry = GameTableManager.Instance.AccountItem.GetEntry(accountItemId);
            if (entry == null)
                throw new ArgumentException($"Invalid account item {accountItemId}!");

            var item = new AccountItem(account, nextId++, entry);
            items.Add(item.Id, item);

            account.Session.EnqueueMessageEncrypted(new ServerAccountItemAdd
            {
                Item = item.Build()
            });

            return item;
        }

        /// <summary>
        /// Take the <see cref="IAccountItem"/> with supplied id, granting its contents to the supplied <see cref="IPlayer"/>.
        /// </summary>
        public AccountOperationResult TakeItem(IPlayer player, ulong id)
        {
            if (!items.TryGetValue(id, out IAccountItem item))
                return AccountOperationResult.InvalidAccountItem;

            AccountItemEntry entry = item.Entry;
            if (entry.Item2Id != 0u)
                player.Inventory.ItemCreate(InventoryLocation.Inventory, entry.Item2Id, 1u);
            else if (entry.AccountCurrencyEnum != 0u)
                account.CurrencyManager.CurrencyAddAmount((AccountCurrencyType)entry.AccountCurrencyEnum, entry.AccountCurrencyAmount);
            else
            {
                // TODO: entitlements, generic unlock sets and instant events
                log.LogWarning($"Unhandled contents for account item {entry.Id}, leaving it in the account inventory.");
                return AccountOperationResult.InvalidAccountItem;
            }

            items.Remove(id);
            item.Delete();
            deletedItems.Add(item);

            account.Session.EnqueueMessageEncrypted(new ServerAccountItemRemove
            {
                UserInventoryId = id
            });

            return AccountOperationResult.Ok;
        }

        /// <summary>
        /// Send the account inventory to the client.
        /// </summary>
        public void SendInitialPackets()
        {
            account.Session.EnqueueMessageEncrypted(new ServerAccountItems
            {
                AccountItems = items.Values.Select(i => i.Build()).ToList()
            });
        }
    }
}
