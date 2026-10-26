using System.Collections.Immutable;
using NexusForever.Game.Static.Entity;
using NexusForever.Game.Static.Item;
using NexusForever.GameTable.Model;
using NexusForever.GameTable.Static;

namespace NexusForever.Game.Abstract.Entity
{
    public interface IItemInfo
    {
        uint Id { get; }
        Item2Entry Entry { get; }
        Item2FamilyEntry FamilyEntry { get; }
        Item2CategoryEntry CategoryEntry { get; }
        Item2TypeEntry TypeEntry { get; }
        ItemSlotEntry SlotEntry { get; }
        ItemBudgetEntry BudgetEntry { get; }
        ItemStatEntry StatEntry { get; }
        SecondaryItemFlags SecondaryItemFlags { get; }

        Quality Quality { get; }

        float ItemPower { get; }
        ImmutableDictionary<Property, float> Properties { get; }

        /// <summary>
        /// Calculate item properties for an item instance with a different power level or circuit crafted properties.
        /// </summary>
        ImmutableDictionary<Property, float> CalculateProperties(uint powerLevel, ulong circuitData, IReadOnlyList<float> circuitMultipliers);

        /// <summary>
        /// Calculate item power for the supplied power level.
        /// </summary>
        float CalculateItemPower(uint powerLevel);

        /// <summary>
        /// Calculate the vendor price for the item at the supplied power level, ignoring any explicit price.
        /// </summary>
        float CalculateVendorAmount(uint powerLevel);

        /// <summary>
        /// Return the display id for <see cref="IItemInfo"/>.
        /// </summary>
        ushort GetDisplayId();

        /// <summary>
        /// Returns the <see cref="CurrencyType"/> this <see cref="IItemInfo"/> can be purchased for at a vendor.
        /// </summary>
        CurrencyType GetVendorBuyCurrency(byte index);

        /// <summary>
        /// Returns the amount of <see cref="CurrencyType"/> this <see cref="IItemInfo"/> can be purchased for at a vendor.
        /// </summary>
        uint GetVendorBuyAmount(byte index);

        /// <summary>
        /// Returns the <see cref="CurrencyType"/> this <see cref="IItemInfo"/> sells for at a vendor.
        /// </summary>
        CurrencyType GetVendorSellCurrency(byte index);

        /// <summary>
        /// Returns the amount of <see cref="CurrencyType"/> this <see cref="IItemInfo"/> sells for at a vendor.
        /// </summary>
        uint GetVendorSellAmount(byte index);

        /// <summary>
        /// Returns if item can be equipped into an item slot.
        /// </summary>
        bool IsEquippable();

        /// <summary>
        /// Returns if item can be equipped into item slot <see cref="EquippedItem"/>.
        /// </summary>
        bool IsEquippableIntoSlot(EquippedItem bagIndex);

        /// <summary>
        /// Returns if item can be stacked with other items of the same type.
        /// </summary>
        bool IsStackable();

        /// <summary>
        /// Returns if item can be used as a bag for expanding inventory slots.
        /// </summary>
        bool IsEquippableBag();
    }
}