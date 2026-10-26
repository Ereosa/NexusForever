using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Static.Entity;
using NexusForever.GameTable;
using NexusForever.GameTable.Model;
using NexusForever.GameTable.Static;

namespace NexusForever.Game.Crafting
{
    /// <summary>
    /// Circuit crafting rules shared by crafting and item properties.
    /// </summary>
    /// <remarks>
    /// Circuit data is a packed 64-bit value stored on the item and sent to the client as the item random circuit data:
    /// bytes 0 to 5 are the chosen property plus one for each item stat slot, byte 6 is the assault and support power split
    /// (0 is full assault and 255 full support) and byte 7 is the format version.
    /// The budget multipliers from charging sockets are stored and sent separately for each slot.
    /// Formulas are from the client circuit crafting calculator, see functions 0x1405918A0, 0x140592020, 0x140592140 and 0x14040C310.
    /// </remarks>
    public static class CircuitCrafting
    {
        public const int SlotCount = 5;
        public const byte Version  = 1;

        private const int ApSpSplitShift = 48;
        private const int VersionShift   = 56;

        /// <summary>
        /// Values that change how far sockets can be charged and how likely a circuit craft is to fail.
        /// </summary>
        /// <remarks>
        /// These come from tradeskill talent bonuses, which aren't implemented, apart from universal schematics which have fixed values.
        /// </remarks>
        public record Settings
        {
            /// <summary>
            /// Charges are this many times smaller, making each charge a smaller step (tradeskill bonus 9).
            /// </summary>
            public int ChargeIncrementShift { get; init; }
            /// <summary>
            /// Fraction of the fail chance removed (tradeskill bonus 8).
            /// </summary>
            public float FailChanceReduction { get; init; }
            /// <summary>
            /// Fraction of the overcharge fail chance removed, eg: from a Signature account.
            /// </summary>
            public float OverchargeReduction { get; init; }
            /// <summary>
            /// Reduction to the penalty for a property that doesn't match the element of its socket (tradeskill bonus 1).
            /// </summary>
            public float MismatchReduction { get; init; }
            /// <summary>
            /// The assault and support power slider has 2 to the power of this many steps (tradeskill bonus 21).
            /// </summary>
            public int ApSpSplitExponent { get; init; }
            /// <summary>
            /// Increases to the maximum charge of a socket (tradeskill bonus 3).
            /// </summary>
            public int MaxChargeBonus { get; init; }
            /// <summary>
            /// Increase to the highest fail chance a craft can be started with (tradeskill bonus 2).
            /// </summary>
            public float FailCapBonus { get; init; }
        }

        // GameFormula 1229: charge increment, base maximum charge, maximum charge per bonus, maximum charge cap and budget scale
        private static GameFormulaEntry ChargeFormula => GameTableManager.Instance.GameFormula.GetEntry(1229);
        // GameFormula 1246: mismatch penalty, base fail cap, maximum fail cap, minimum mismatch penalty and fail chance scale
        private static GameFormulaEntry FailFormula => GameTableManager.Instance.GameFormula.GetEntry(1246);
        // GameFormula 1247: power core quality that starts reducing the fail chance and the reduction per quality
        private static GameFormulaEntry PowerCoreFormula => GameTableManager.Instance.GameFormula.GetEntry(1247);
        // GameFormula 1253: crafted budget ratio below which universal schematics can't fail
        private static GameFormulaEntry UniversalFailFormula => GameTableManager.Instance.GameFormula.GetEntry(1253);
        // GameFormula 1257: share of the crafted item vendor price charged to circuit craft
        private static GameFormulaEntry CostFormula => GameTableManager.Instance.GameFormula.GetEntry(1257);
        // GameFormula 1316: overcharge fail chance reduction for Signature accounts
        private static GameFormulaEntry SignatureFormula => GameTableManager.Instance.GameFormula.GetEntry(1316);
        // GameFormula 1324: fixed settings for universal schematics
        private static GameFormulaEntry UniversalFormula => GameTableManager.Instance.GameFormula.GetEntry(1324);

        /// <summary>
        /// Return the circuit crafting settings for a schematic.
        /// </summary>
        /// <param name="universal">Universal schematics have fixed settings instead of the crafter's tradeskill bonuses.</param>
        /// <param name="signature">Signature accounts have a lower overcharge fail chance.</param>
        public static Settings GetSettings(bool universal, bool signature)
        {
            if (universal)
            {
                GameFormulaEntry formula = UniversalFormula;
                return new Settings
                {
                    ChargeIncrementShift = (int)formula.Dataint0,
                    MismatchReduction    = formula.Dataint01 * 0.01f,
                    ApSpSplitExponent    = (int)formula.Dataint02,
                    MaxChargeBonus       = (int)formula.Dataint03
                };
            }

            // TODO: tradeskill talent bonuses
            return new Settings
            {
                OverchargeReduction = signature ? SignatureFormula.Datafloat0 : 0f
            };
        }

        /// <summary>
        /// Amount a single charge moves a socket.
        /// </summary>
        public static int GetChargeIncrement(Settings settings)
        {
            return Math.Max((int)(ChargeFormula.Dataint0 >> settings.ChargeIncrementShift), 1);
        }

        /// <summary>
        /// Largest amount a socket can be moved in either direction.
        /// </summary>
        public static int GetMaxCharge(Settings settings)
        {
            GameFormulaEntry formula = ChargeFormula;
            return (int)Math.Min(formula.Dataint01 + formula.Dataint02 * (uint)settings.MaxChargeBonus, formula.Dataint03);
        }

        /// <summary>
        /// Returns if a socket can be moved by the supplied number of charges.
        /// </summary>
        public static bool IsValidCharge(Settings settings, int charges)
        {
            long charge = (long)charges * GetChargeIncrement(settings);
            int maxCharge = GetMaxCharge(settings);
            return charge <= maxCharge && charge >= -maxCharge;
        }

        /// <summary>
        /// Return the budget multiplier for a socket moved by the supplied number of charges.
        /// </summary>
        public static float GetChargeMultiplier(Settings settings, int charges)
        {
            GameFormulaEntry formula = ChargeFormula;
            int maxCharge = GetMaxCharge(settings);
            int charge = (int)Math.Clamp((long)charges * GetChargeIncrement(settings), -maxCharge, maxCharge);

            float scale = 1f / (formula.Dataint01 * formula.Datafloat0);
            return MathF.Max(scale * charge + 1f, scale);
        }

        /// <summary>
        /// Return the number of steps of the assault and support power slider.
        /// </summary>
        public static uint GetApSpSplitMaxDelta(Settings settings)
        {
            return 1u << settings.ApSpSplitExponent;
        }

        /// <summary>
        /// Return the assault and support power split byte for the slider position.
        /// </summary>
        public static byte GetApSpSplitByte(Settings settings, uint delta)
        {
            float fraction = Math.Clamp(delta / (float)GetApSpSplitMaxDelta(settings), 0f, 1f);
            return (byte)Math.Min((int)(fraction * 255f + 0.0000099999997f), 255);
        }

        /// <summary>
        /// Return the penalty added to the crafted budget for a property that doesn't match the element of its socket.
        /// </summary>
        public static float GetMismatchPenalty(Settings settings)
        {
            GameFormulaEntry formula = FailFormula;
            return MathF.Max(formula.Datafloat03, formula.Datafloat0 - settings.MismatchReduction);
        }

        /// <summary>
        /// Return the chosen property for the item stat slot from circuit data.
        /// </summary>
        public static bool TryGetSlotProperty(ulong circuitData, int slot, out Property property)
        {
            property = default;
            if (circuitData == 0ul || slot < 0 || slot >= SlotCount)
                return false;

            byte value = (byte)(circuitData >> (slot * 8));
            if (value == 0)
                return false;

            property = (Property)(value - 1);
            return IsCraftableProperty(property);
        }

        /// <summary>
        /// Return the support power share from circuit data, -1 is full assault power and 1 is full support power.
        /// </summary>
        public static float GetApSpSplit(ulong circuitData)
        {
            byte split = (byte)(circuitData >> ApSpSplitShift);
            return split / 255f * 2f - 1f;
        }

        /// <summary>
        /// Pack circuit data from the chosen slot properties and assault and support power split.
        /// </summary>
        public static ulong BuildCircuitData(IReadOnlyList<Property?> slots, byte apSpSplit)
        {
            ulong data = 0ul;
            for (int i = 0; i < SlotCount && i < slots.Count; i++)
                if (slots[i].HasValue)
                    data |= (ulong)(byte)((uint)slots[i].Value + 1u) << (i * 8);

            data |= (ulong)apSpSplit << ApSpSplitShift;
            data |= (ulong)Version << VersionShift;
            return data;
        }

        /// <summary>
        /// Pack the element of each socket, the format of <see cref="Network.World.Message.Model.Crafting.ServerCraftingCurrentCraft.SocketGroups"/>.
        /// </summary>
        public static uint BuildSocketGroups(IReadOnlyList<ItemCraftingGroupFlag> groups)
        {
            uint data = Version;
            for (int i = 0; i < SlotCount && i < groups.Count; i++)
            {
                if (groups[i] == ItemCraftingGroupFlag.None)
                    continue;

                uint index = (uint)System.Numerics.BitOperations.TrailingZeroCount((uint)groups[i]) + 1u;
                data |= (index & 7u) << (8 + i * 3);
            }

            return data;
        }

        /// <summary>
        /// Returns if the property can be chosen for a circuit crafting socket.
        /// </summary>
        public static bool IsCraftableProperty(Property property)
        {
            UnitProperty2Entry entry = GameTableManager.Instance.UnitProperty2.GetEntry((uint)property);
            if (entry == null)
                return false;

            if ((entry.Flags & UnitPropertyFlags.Unknown04) == 0 || (entry.Flags & UnitPropertyFlags.Craftable) == 0)
                return false;

            return (entry.ItemCraftingGroupFlagBitMask & (uint)ItemCraftingGroupFlag.All) != 0u;
        }

        /// <summary>
        /// Returns if the property can be chosen for a circuit crafting socket of the supplied item.
        /// </summary>
        public static bool IsCraftableProperty(Property property, IItemInfo info)
        {
            if (!IsCraftableProperty(property))
                return false;

            // some properties are limited to items equipped in certain slots
            UnitProperty2Entry entry = GameTableManager.Instance.UnitProperty2.GetEntry((uint)property);
            uint slotFlags = info.SlotEntry?.EquippedSlotFlags ?? 0u;
            return entry.EquippedSlotFlags == 0u || (entry.EquippedSlotFlags & slotFlags) != 0u;
        }

        /// <summary>
        /// Return the elements of the supplied property.
        /// </summary>
        public static ItemCraftingGroupFlag GetPropertyGroup(Property property)
        {
            UnitProperty2Entry entry = GameTableManager.Instance.UnitProperty2.GetEntry((uint)property);
            return (ItemCraftingGroupFlag)((entry?.ItemCraftingGroupFlagBitMask ?? 0u) & (uint)ItemCraftingGroupFlag.All);
        }

        /// <summary>
        /// Return every property that can be chosen for a socket of the supplied element for the supplied item.
        /// </summary>
        public static IEnumerable<Property> GetCraftableProperties(IItemInfo info, ItemCraftingGroupFlag group)
        {
            return GameTableManager.Instance.UnitProperty2.Entries
                .Where(e => e != null)
                .Select(e => (Property)e.Id)
                .Where(p => IsCraftableProperty(p, info) && (GetPropertyGroup(p) & group) != 0);
        }

        /// <summary>
        /// Returns if a property can be chosen for a socket, fusion sockets only take fusion properties and other sockets only take other properties.
        /// </summary>
        public static bool CanSocketProperty(ItemCraftingGroupFlag socketGroup, Property property)
        {
            bool fusionSocket   = (socketGroup & ItemCraftingGroupFlag.Fusion) != 0;
            bool fusionProperty = (GetPropertyGroup(property) & ItemCraftingGroupFlag.Fusion) != 0;
            return fusionSocket == fusionProperty;
        }

        /// <summary>
        /// Returns if a property matches the element of its socket, fusion sockets match every element.
        /// </summary>
        public static bool IsMatchingGroup(ItemCraftingGroupFlag socketGroup, Property property)
        {
            ItemCraftingGroupFlag group = (socketGroup & ItemCraftingGroupFlag.Fusion) != 0 ? ItemCraftingGroupFlag.All : socketGroup;
            return (GetPropertyGroup(property) & group) != 0;
        }

        /// <summary>
        /// Return the fail chance of a circuit craft.
        /// </summary>
        /// <param name="settings">Circuit crafting settings of the craft.</param>
        /// <param name="budget">Item power of the crafted item.</param>
        /// <param name="craftedBudget">Sum of the charged slot budgets including mismatch penalties.</param>
        /// <param name="powerCore">Power core used for the craft, higher quality cores reduce the fail chance.</param>
        /// <param name="universal">Universal schematics don't fail until the crafted budget passes a threshold.</param>
        public static float GetFailChance(Settings settings, float budget, float craftedBudget, IItemInfo powerCore, bool universal)
        {
            if (budget <= 0f)
                return 0f;

            float coreModifier = 1f;
            if (powerCore != null)
            {
                GameFormulaEntry formula = PowerCoreFormula;
                uint quality = Math.Clamp(powerCore.Entry.ItemQualityId, 1u, 7u);
                if (quality >= formula.Dataint0)
                    coreModifier = 1f - formula.Datafloat0 * (quality - formula.Dataint0 + 1f);
            }

            float ratio = craftedBudget / budget;
            float unbuffed = Math.Clamp((ratio - 1f) * FailFormula.Datafloat04 * coreModifier * (1f - settings.FailChanceReduction), 0f, 1f);
            float failChance = Math.Clamp((1f - settings.OverchargeReduction) * unbuffed, 0f, 1f);

            if (universal && failChance > 0f && ratio < UniversalFailFormula.Datafloat0)
                failChance = 0f;

            return failChance;
        }

        /// <summary>
        /// Return the highest fail chance a circuit craft can be started with.
        /// </summary>
        public static float GetFailCap(Settings settings)
        {
            GameFormulaEntry formula = FailFormula;
            float cap = MathF.Min(formula.Datafloat02, formula.Datafloat01 + settings.FailCapBonus) * (1f - settings.FailChanceReduction);
            return (1f - settings.OverchargeReduction) * cap;
        }

        /// <summary>
        /// Return the credits charged to circuit craft an item with the supplied calculated vendor price.
        /// </summary>
        public static ulong GetCraftingCost(float vendorPrice)
        {
            // tradeskill bonus 22 also scales the cost
            return (ulong)Math.Max((int)(vendorPrice * CostFormula.Datafloat0), 0);
        }
    }
}
