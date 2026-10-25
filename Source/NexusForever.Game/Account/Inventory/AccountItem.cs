using NexusForever.Database.Auth;
using NexusForever.Database.Auth.Model;
using NexusForever.Game.Abstract.Account;
using NexusForever.Game.Abstract.Account.Inventory;
using NexusForever.GameTable;
using NexusForever.GameTable.Model;
using NexusForever.Network.World.Message.Model.AccountInventory;

namespace NexusForever.Game.Account.Inventory
{
    public class AccountItem : IAccountItem
    {
        [Flags]
        private enum AccountItemSaveMask
        {
            None   = 0x00,
            Create = 0x01,
            Delete = 0x02
        }

        public ulong Id { get; }
        public AccountItemEntry Entry { get; }

        private readonly IAccount account;
        private AccountItemSaveMask saveMask;

        /// <summary>
        /// Create a new <see cref="IAccountItem"/> from an existing database model.
        /// </summary>
        public AccountItem(IAccount account, AccountItemModel model)
        {
            this.account = account;
            Id           = model.ItemId;
            Entry        = GameTableManager.Instance.AccountItem.GetEntry(model.AccountItemId);
        }

        /// <summary>
        /// Create a new <see cref="IAccountItem"/>.
        /// </summary>
        public AccountItem(IAccount account, ulong id, AccountItemEntry entry)
        {
            this.account = account;
            Id           = id;
            Entry        = entry;
            saveMask     = AccountItemSaveMask.Create;
        }

        /// <summary>
        /// Mark the <see cref="IAccountItem"/> for deletion.
        /// </summary>
        public void Delete()
        {
            // never saved, nothing to remove from the database
            if ((saveMask & AccountItemSaveMask.Create) != 0)
                saveMask = AccountItemSaveMask.None;
            else
                saveMask = AccountItemSaveMask.Delete;
        }

        public void Save(AuthContext context)
        {
            if (saveMask == AccountItemSaveMask.None)
                return;

            var model = new AccountItemModel
            {
                Id            = account.Id,
                ItemId        = Id,
                AccountItemId = Entry.Id
            };

            if ((saveMask & AccountItemSaveMask.Create) != 0)
                context.Add(model);
            else if ((saveMask & AccountItemSaveMask.Delete) != 0)
                context.Remove(model);

            saveMask = AccountItemSaveMask.None;
        }

        public AccountInventoryItem Build()
        {
            return new AccountInventoryItem
            {
                UserInventoryId = Id,
                AccountItemId   = Entry.Id
            };
        }
    }
}
