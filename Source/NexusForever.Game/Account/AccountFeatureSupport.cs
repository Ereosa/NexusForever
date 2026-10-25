using System.Collections.Immutable;
using NexusForever.Game.Static.AccountInventory;
using NexusForever.Game.Static.Entity;
using NexusForever.Game.Static.Reward;

namespace NexusForever.Game.Account
{
    public enum AccountSupportLevel
    {
        Unsupported,
        Partial,
        Supported
    }

    /// <summary>
    /// Which account features (reward properties, entitlements and account currencies) the server applies.
    /// </summary>
    /// <remarks>
    /// Keep this in sync with the code that reads them, it is used for the runtime warnings when an account item is claimed and by the AccountAudit tool.
    /// Entitlements that only feed a reward property (<see cref="GameTable.Model.RewardPropertyPremiumModifierEntry.EntitlementIdModifierCount"/>) take the support of that reward property.
    /// </remarks>
    public static class AccountFeatureSupport
    {
        /// <summary>
        /// Reward properties the server reads, and where.
        /// </summary>
        public static ImmutableDictionary<RewardPropertyType, (AccountSupportLevel Level, string Hook)> RewardProperties { get; } =
            new Dictionary<RewardPropertyType, (AccountSupportLevel, string)>
            {
                [RewardPropertyType.RewardTrackId]           = (AccountSupportLevel.Supported, "RewardTrackManager point multiplier"),
                [RewardPropertyType.RewardTrackType]         = (AccountSupportLevel.Supported, "RewardTrackManager point multiplier"),
                [RewardPropertyType.CostumeSlots]            = (AccountSupportLevel.Supported, "CostumeManager.CostumeCap"),
                [RewardPropertyType.TradeskillMatStackLimit] = (AccountSupportLevel.Supported, "SupplySatchelManager stack limit"),
                [RewardPropertyType.CharacterSlots]          = (AccountSupportLevel.Supported, "CharacterListManager character limit"),
                [RewardPropertyType.XP]                      = (AccountSupportLevel.Supported, "XpManager.GrantXp boost (eg: Flask of Advancement)"),
                [RewardPropertyType.BankSlots]               = (AccountSupportLevel.Supported, "Inventory.CanMoveItem bank bag slots"),
                [RewardPropertyType.BagSlots]                = (AccountSupportLevel.Supported, "all 4 inventory bag slots are open"),
                [RewardPropertyType.ExtraDecorSlots]         = (AccountSupportLevel.Partial,   "housing has no decor limit, so extra slots change nothing"),
            }.ToImmutableDictionary();

        /// <summary>
        /// Entitlements the server reads directly, and where.
        /// </summary>
        public static ImmutableDictionary<EntitlementType, (AccountSupportLevel Level, string Hook)> Entitlements { get; } =
            new Dictionary<EntitlementType, (AccountSupportLevel, string)>
            {
                [EntitlementType.Signature]                = (AccountSupportLevel.Partial,   "account tier comes from RBAC permission Signature, the entitlement is display only"),
                [EntitlementType.AdditionalCostumeUnlocks] = (AccountSupportLevel.Supported, "AccountCostumeManager holo-wardrobe unlock limit"),
                [EntitlementType.ChuaWarriorUnlock]        = (AccountSupportLevel.Supported, "ClientCharacterCreateHandler, CharacterCreation.EntitlementIdRequired"),
                [EntitlementType.AurinEngineerUnlock]      = (AccountSupportLevel.Supported, "ClientCharacterCreateHandler, CharacterCreation.EntitlementIdRequired"),
            }.ToImmutableDictionary();

        /// <summary>
        /// Account currencies, how they are earned and spent.
        /// </summary>
        public static ImmutableDictionary<AccountCurrencyType, (AccountSupportLevel Level, string Hook)> Currencies { get; } =
            new Dictionary<AccountCurrencyType, (AccountSupportLevel, string)>
            {
                [AccountCurrencyType.MysticShiny]    = (AccountSupportLevel.Supported, "spent on Madame Fay's Fortunes, bought in the store"),
                [AccountCurrencyType.ServiceToken]   = (AccountSupportLevel.Supported, "spent on rapid transport, appearance change, path change, guild registration and resurrection"),
                [AccountCurrencyType.Omnibits]       = (AccountSupportLevel.Partial,   "spent in the store, nothing earns it"),
                [AccountCurrencyType.Protobucks]     = (AccountSupportLevel.Supported, "spent in the store"),
                [AccountCurrencyType.PromotionToken] = (AccountSupportLevel.Partial,   "level 50 character creation, the currency isn't loaded"),
            }.ToImmutableDictionary();

        /// <summary>
        /// What claiming an account item does with each part of its <see cref="GameTable.Model.AccountItemEntry"/>, see AccountItemManager.TakeItem.
        /// </summary>
        public static ImmutableDictionary<string, (AccountSupportLevel Level, string Hook)> ClaimFeatures { get; } =
            new Dictionary<string, (AccountSupportLevel, string)>
            {
                ["Item"]             = (AccountSupportLevel.Supported,   "item created in the character's inventory"),
                ["Entitlement"]      = (AccountSupportLevel.Supported,   "account or character entitlement count"),
                ["CharacterScope"]   = (AccountSupportLevel.Supported,   "EntitlementScopeEnum 1 entitlements are claimed to the character"),
                ["EntitlementBuff"]  = (AccountSupportLevel.Supported,   "EntitlementEffects casts Spell4IdPersistentBuff on claim and when entering the world"),
                ["EntitlementTitle"] = (AccountSupportLevel.Supported,   "EntitlementEffects grants CharacterTitleId on claim and when entering the world"),
                ["Currency"]         = (AccountSupportLevel.Supported,   "account currency added"),
                ["GenericUnlockSet"] = (AccountSupportLevel.Supported,   "generic unlocks added to the account"),
                ["InstantEvent"]     = (AccountSupportLevel.Unsupported, "no instant events, only a test account item uses one (logged)"),
                ["Cooldown"]         = (AccountSupportLevel.Supported,   "AccountItemCooldownGroup, one claim a day (duration isn't in the client data)"),
                ["MultiRedeem"]      = (AccountSupportLevel.Supported,   "AccountItem.Flags 0x2, claimed once by each character"),
                ["Prerequisite"]     = (AccountSupportLevel.Supported,   "PrerequisiteManager"),
            }.ToImmutableDictionary();

        public static AccountSupportLevel GetRewardPropertySupport(RewardPropertyType type)
        {
            return RewardProperties.TryGetValue(type, out var support) ? support.Level : AccountSupportLevel.Unsupported;
        }

        public static AccountSupportLevel GetEntitlementSupport(EntitlementType type)
        {
            return Entitlements.TryGetValue(type, out var support) ? support.Level : AccountSupportLevel.Unsupported;
        }

        public static AccountSupportLevel GetCurrencySupport(AccountCurrencyType type)
        {
            return Currencies.TryGetValue(type, out var support) ? support.Level : AccountSupportLevel.Unsupported;
        }
    }
}
