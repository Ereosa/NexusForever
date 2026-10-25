using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NexusForever.Database.Auth;
using NexusForever.Database.Auth.Model;
using NexusForever.Game.Abstract.Account;
using NexusForever.Game.Abstract.Account.Inventory;
using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Static.AccountInventory;
using NexusForever.Game.Prerequisite;
using NexusForever.Game.Static.Entity;
using NexusForever.Game.Static.Prerequisite;
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
        /// Take the <see cref="IAccountItem"/> with supplied id, granting its contents to the account or supplied <see cref="IPlayer"/>.
        /// </summary>
        /// <remarks>
        /// Items, character entitlements and prerequisites need a character, account entitlements and currency can be claimed without one.
        /// </remarks>
        public AccountOperationResult TakeItem(IPlayer player, ulong id)
        {
            if (!items.TryGetValue(id, out IAccountItem item))
                return AccountOperationResult.InvalidAccountItem;

            AccountItemEntry entry = item.Entry;
            AccountOperationResult result = CanTakeItem(player, entry);
            if (result != AccountOperationResult.Ok)
                return result;

            if (entry.Item2Id != 0u)
                player.Inventory.ItemCreate(InventoryLocation.Inventory, entry.Item2Id, 1u);

            if (entry.AccountCurrencyEnum != 0u)
                account.CurrencyManager.CurrencyAddAmount((AccountCurrencyType)entry.AccountCurrencyEnum, entry.AccountCurrencyAmount);

            if (entry.EntitlementId != 0u)
            {
                EntitlementEntry entitlementEntry = GameTableManager.Instance.Entitlement.GetEntry(entry.EntitlementId);
                int count = (int)Math.Max(entry.EntitlementCount, 1u);
                if (IsCharacterEntitlement(entitlementEntry))
                    player.EntitlementManager.UpdateEntitlement((EntitlementType)entitlementEntry.Id, count);
                else
                    account.EntitlementManager.UpdateEntitlement((EntitlementType)entitlementEntry.Id, count);
            }

            if (entry.GenericUnlockSetId != 0u)
            {
                GenericUnlockSetEntry unlockSet = GameTableManager.Instance.GenericUnlockSet.GetEntry(entry.GenericUnlockSetId);
                uint[] unlockIds = unlockSet == null ? [] :
                [
                    unlockSet.GenericUnlockEntryId00, unlockSet.GenericUnlockEntryId01, unlockSet.GenericUnlockEntryId02,
                    unlockSet.GenericUnlockEntryId03, unlockSet.GenericUnlockEntryId04, unlockSet.GenericUnlockEntryId05
                ];
                foreach (uint unlockId in unlockIds.Where(u => u != 0u))
                    account.GenericUnlockManager.Unlock((ushort)unlockId);
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

        private AccountOperationResult CanTakeItem(IPlayer player, AccountItemEntry entry)
        {
            EntitlementEntry entitlementEntry = entry.EntitlementId != 0u ? GameTableManager.Instance.Entitlement.GetEntry(entry.EntitlementId) : null;
            if (entry.EntitlementId != 0u && entitlementEntry == null)
                return AccountOperationResult.InvalidAccountItem;

            bool needsCharacter = entry.Item2Id != 0u
                || entry.EntitlementScopeEnum == 1u
                || IsCharacterEntitlement(entitlementEntry)
                || (entry.PrerequisiteId != 0u && !IsAccountPrerequisite(entry.PrerequisiteId));
            if (needsCharacter && player == null)
                return AccountOperationResult.NoCharacter;

            if (entry.Item2Id == 0u && entry.AccountCurrencyEnum == 0u && entitlementEntry == null && entry.GenericUnlockSetId == 0u)
            {
                // TODO: instant events
                log.LogWarning($"Unhandled contents for account item {entry.Id}, leaving it in the account inventory.");
                return AccountOperationResult.InvalidAccountItem;
            }

            // eg: "You have already claimed the maximum of 12 Extra Character Slots."
            if (entry.PrerequisiteId != 0u)
            {
                bool meets = player != null
                    ? PrerequisiteManager.Instance.Meets(player, entry.PrerequisiteId)
                    : MeetsAccountPrerequisite(entry.PrerequisiteId);
                if (!meets)
                    return AccountOperationResult.Prereq;
            }

            if (entitlementEntry != null)
            {
                uint current = IsCharacterEntitlement(entitlementEntry)
                    ? player.EntitlementManager.GetEntitlement((EntitlementType)entitlementEntry.Id)?.Amount ?? 0u
                    : account.EntitlementManager.GetEntitlement((EntitlementType)entitlementEntry.Id)?.Amount ?? 0u;
                if (current + Math.Max(entry.EntitlementCount, 1u) > entitlementEntry.MaxCount)
                    return AccountOperationResult.MaxEntitlementCount;
            }

            if (entry.Item2Id != 0u && player.Inventory.GetInventorySlotsRemaining(InventoryLocation.Inventory) == 0u)
                return AccountOperationResult.InvalidInventoryItem;

            return AccountOperationResult.Ok;
        }

        /// <summary>
        /// Returns if the prerequisite only checks account entitlements, so it can be evaluated without a character (eg: at character select).
        /// </summary>
        private static bool IsAccountPrerequisite(uint prerequisiteId)
        {
            PrerequisiteEntry prerequisite = GameTableManager.Instance.Prerequisite.GetEntry(prerequisiteId);
            if (prerequisite == null)
                return false;

            for (int i = 0; i < prerequisite.PrerequisiteTypeId.Length; i++)
            {
                PrerequisiteType type = prerequisite.PrerequisiteTypeId[i];
                if (type == PrerequisiteType.None)
                    continue;

                if (type != PrerequisiteType.EntitlementCount || IsCharacterEntitlement(GameTableManager.Instance.Entitlement.GetEntry(prerequisite.ObjectId[i])))
                    return false;
            }

            return true;
        }

        private bool MeetsAccountPrerequisite(uint prerequisiteId)
        {
            PrerequisiteEntry prerequisite = GameTableManager.Instance.Prerequisite.GetEntry(prerequisiteId);
            bool any = prerequisite.Flags == EvaluationMode.EvaluateOR;

            for (int i = 0; i < prerequisite.PrerequisiteTypeId.Length; i++)
            {
                if (prerequisite.PrerequisiteTypeId[i] == PrerequisiteType.None)
                    continue;

                uint count = account.EntitlementManager.GetEntitlement((EntitlementType)prerequisite.ObjectId[i])?.Amount ?? 0u;
                uint value = prerequisite.Value[i];
                bool meets = prerequisite.PrerequisiteComparisonId[i] switch
                {
                    PrerequisiteComparison.Equal              => count == value,
                    PrerequisiteComparison.NotEqual           => count != value,
                    PrerequisiteComparison.GreaterThan        => count > value,
                    PrerequisiteComparison.GreaterThanOrEqual => count >= value,
                    PrerequisiteComparison.LessThan           => count < value,
                    PrerequisiteComparison.LessThanOrEqual    => count <= value,
                    _                                         => false
                };

                if (any && meets)
                    return true;
                if (!any && !meets)
                    return false;
            }

            return !any;
        }

        /// <summary>
        /// Returns if the <see cref="IAccountItem"/> only grants account wide contents, these are applied straight away when bought at character select.
        /// </summary>
        public bool IsAccountOnly(IAccountItem item)
        {
            AccountItemEntry entry = item.Entry;
            EntitlementEntry entitlementEntry = entry.EntitlementId != 0u ? GameTableManager.Instance.Entitlement.GetEntry(entry.EntitlementId) : null;
            return entitlementEntry != null
                && entry.Item2Id == 0u
                && entry.EntitlementScopeEnum != 1u
                && !IsCharacterEntitlement(entitlementEntry)
                && (entry.PrerequisiteId == 0u || IsAccountPrerequisite(entry.PrerequisiteId));
        }

        private static bool IsCharacterEntitlement(EntitlementEntry entry)
        {
            return entry != null && ((EntitlementFlags)entry.Flags).HasFlag(EntitlementFlags.Character);
        }

        /// <summary>
        /// Returns if the account inventory contains an item for the supplied account item id.
        /// </summary>
        public bool HasItem(uint accountItemId)
        {
            return items.Values.Any(i => i.Entry.Id == accountItemId);
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
