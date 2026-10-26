using System.Collections.Immutable;
using NexusForever.Database;
using NexusForever.Database.Character;
using NexusForever.Game.Static.Entity;
using NexusForever.GameTable.Model;
using NexusForever.Network.Message;
using NexusForever.Network.World.Message.Model.Shared;

namespace NexusForever.Game.Abstract.Entity
{
    public interface IItem : IDatabaseCharacter, IDatabaseState, INetworkBuildable<Item>
    {
        uint Id { get; }
        IItemInfo Info { get; }
        Spell4BaseEntry SpellEntry { get; }
        ulong Guid { get; }
        ulong? CharacterId { get; set; }
        InventoryLocation Location { get; set; }
        InventoryLocation PreviousLocation { get; set; }
        uint BagIndex { get; set; }
        uint PreviousBagIndex { get; set; }
        uint StackCount { get; set; }
        uint Charges { get; set; }
        float Durability { get; set; }
        uint ExpirationTimeLeft { get; set; }

        /// <summary>
        /// Circuit data from circuit crafting.
        /// </summary>
        ulong CircuitData { get; }

        /// <summary>
        /// Budget multiplier for each item stat slot from charging sockets while circuit crafting.
        /// </summary>
        IReadOnlyList<float> CircuitMultipliers { get; }

        /// <summary>
        /// Power core used to circuit craft the item, the item takes its power level from the power core.
        /// </summary>
        uint PowerCoreItemId { get; }

        /// <summary>
        /// Properties of this item instance, circuit crafted items have properties chosen while crafting.
        /// </summary>
        ImmutableDictionary<Property, float> Properties { get; }

        /// <summary>
        /// Set the properties chosen while circuit crafting the item.
        /// </summary>
        void SetCircuit(ulong circuitData, IEnumerable<float> multipliers, uint powerCoreItemId);

        /// <summary>
        /// Return the power level of the item, circuit crafted items take their power level from the power core used to craft them.
        /// </summary>
        uint GetPowerLevel();

        // <summary>
        /// Returns the <see cref="CurrencyType"/> this <see cref="IItem"/> sells for at a vendor.
        /// </summary>
        CurrencyType GetVendorSellCurrency(byte index);

        /// <summary>
        /// Returns the amount of <see cref="CurrencyType"/> this <see cref="IItem"/> sells for at a vendor.
        /// </summary>
        uint GetVendorSellAmount(byte index);
    }
}