using System.Numerics;
using NexusForever.Game.Abstract.Crafting;
using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Entity;
using NexusForever.Game.Spell;
using NexusForever.Game.Static.Achievement;
using NexusForever.Game.Static.Crafting;
using NexusForever.Game.Static.Entity;
using NexusForever.Game.Static.Quest;
using NexusForever.Game.Static;
using NexusForever.GameTable;
using NexusForever.GameTable.Model;
using NexusForever.GameTable.Static;
using NexusForever.Network.World.Message.Model.Crafting;
using NexusForever.Network.World.Message.Static;
using NLog;

namespace NexusForever.Game.Crafting
{
    public class CraftingManager : ICraftingManager
    {
        /// <summary>
        /// Spell cast while crafting, its CraftItem effect completes the craft.
        /// </summary>
        private const uint CraftingSpell4Id = 70386u;

        /// <summary>
        /// Maximum distance from a crafting station to craft.
        /// </summary>
        private const float MaxStationDistance = 15f;

        /// <summary>
        /// Every tradeskill can be crafted at stations with this tradeskill.
        /// </summary>
        private const uint AnyTradeskillStation = uint.MaxValue;

        /// <summary>
        /// Flags from <see cref="TradeskillEntry"/>.
        /// </summary>
        [Flags]
        private enum TradeskillFlags
        {
            CircuitCrafting    = 0x04,
            CoordinateCrafting = 0x08
        }

        /// <summary>
        /// Flags from <see cref="TradeskillSchematic2Entry"/>.
        /// </summary>
        [Flags]
        private enum SchematicFlags
        {
            Discoverable = 0x02,
            /// <summary>
            /// Circuit crafts that don't need a power core and can't fail until heavily overcharged.
            /// </summary>
            Universal    = 0x04,
            AutoCraft    = 0x08
        }

        private enum CraftType
        {
            Simple,
            Coordinate,
            Circuit
        }

        private class CurrentCraft
        {
            public CraftType Type { get; init; }
            public TradeskillSchematic2Entry Schematic { get; init; }
            public uint Count { get; init; }
            public uint CatalystItemId { get; init; }
            public Vector2 Position { get; set; }
            public List<uint> Additives { get; } = [];
            public uint PowerCoreItemId { get; set; }
            public bool Completing { get; set; }

            /// <summary>
            /// Properties of each item stat slot, sockets start with a random property of their element.
            /// </summary>
            public Property?[] SlotProperties { get; } = new Property?[CircuitCrafting.SlotCount];
            /// <summary>
            /// Element of each circuit crafting socket.
            /// </summary>
            public ItemCraftingGroupFlag[] SocketGroups { get; } = new ItemCraftingGroupFlag[CircuitCrafting.SlotCount];
            public ulong CircuitData { get; set; }
            public float[] SlotMultipliers { get; set; } = [];
            public float FailChance { get; set; }
            /// <summary>
            /// Power core and crafting cost were paid by an attempt that was interrupted.
            /// </summary>
            public bool CircuitPaid { get; set; }
        }

        private static readonly ILogger log = LogManager.GetCurrentClassLogger();

        private CurrentCraft currentCraft;

        private readonly IPlayer player;

        public CraftingManager(IPlayer owner)
        {
            player = owner;
        }

        public void StartCraft(uint stationUnitId, uint schematicId, uint count, uint catalystItemId)
        {
            RecoverInterruptedCast();

            if (currentCraft != null)
            {
                // the client resumes a coordinate or circuit craft by starting it again
                if (currentCraft.Schematic.Id == schematicId && currentCraft.Type != CraftType.Simple)
                {
                    SendCurrentCraft();
                    return;
                }

                throw new InvalidOperationException($"Player {player.CharacterId} is already crafting schematic {currentCraft.Schematic.Id}!");
            }

            TradeskillSchematic2Entry schematic = GetCraftableSchematic(schematicId);
            CheckStation(stationUnitId, schematic);

            TradeskillEntry tradeskill = GameTableManager.Instance.Tradeskill.GetEntry(schematic.TradeSkillId);
            CraftType type = GetCraftType(tradeskill, schematic);

            count = type == CraftType.Simple ? Math.Max(count, 1u) : 1u;
            if (!HasMaterials(schematic, count))
            {
                player.SendGenericError(GenericError.CraftMissingMaterials);
                return;
            }

            // coordinate and circuit crafts use their materials when started, simple crafts when the craft completes
            if (type != CraftType.Simple)
                RemoveMaterials(schematic, count);

            currentCraft = new CurrentCraft
            {
                Type           = type,
                Schematic      = schematic,
                Count          = count,
                CatalystItemId = catalystItemId,
                Position       = new Vector2(schematic.VectorX, schematic.VectorY)
            };

            if (type == CraftType.Simple)
            {
                CastCraftingSpell();
                return;
            }

            if (type == CraftType.Circuit)
                RollSockets(currentCraft);

            SendCurrentCraft();
        }

        /// <summary>
        /// Each socket of a circuit craft gets a random element allowed for its slot and starts with a random property of that element.
        /// </summary>
        private static void RollSockets(CurrentCraft craft)
        {
            IItemInfo info = ItemManager.Instance.GetItemInfo(craft.Schematic.Item2IdOutput);
            if (info?.StatEntry == null)
                return;

            for (int i = 0; i < CircuitCrafting.SlotCount && i < info.StatEntry.ItemStatTypeEnum.Length; i++)
            {
                if (info.StatEntry.ItemStatTypeEnum[i] != ItemStatType.Crafted)
                    continue;

                var mask = (ItemCraftingGroupFlag)(info.StatEntry.ItemStatData[i] & (uint)ItemCraftingGroupFlag.All);
                ItemCraftingGroupFlag[] groups = Enum.GetValues<ItemCraftingGroupFlag>()
                    .Where(g => g is not ItemCraftingGroupFlag.None and not ItemCraftingGroupFlag.All && (mask & g) != 0)
                    .Where(g => CircuitCrafting.GetCraftableProperties(info, g).Any(p => CircuitCrafting.CanSocketProperty(g, p)))
                    .ToArray();
                if (groups.Length == 0)
                    continue;

                ItemCraftingGroupFlag group = groups[Random.Shared.Next(groups.Length)];
                Property[] properties = CircuitCrafting.GetCraftableProperties(info, group)
                    .Where(p => CircuitCrafting.CanSocketProperty(group, p) && !craft.SlotProperties.Contains(p))
                    .ToArray();
                if (properties.Length == 0)
                    continue;

                craft.SocketGroups[i]   = group;
                craft.SlotProperties[i] = properties[Random.Shared.Next(properties.Length)];
            }

            craft.CircuitData = CircuitCrafting.BuildCircuitData(craft.SlotProperties, 0);
        }

        public void AddAdditive(uint stationUnitId, uint additiveItemId)
        {
            RecoverInterruptedCast();

            if (currentCraft is not { Type: CraftType.Coordinate, Completing: false })
                throw new InvalidOperationException($"Player {player.CharacterId} isn't doing a coordinate craft!");

            CheckStation(stationUnitId, currentCraft.Schematic);

            if (currentCraft.Additives.Count >= GetMaxAdditives(currentCraft.Schematic))
            {
                player.SendGenericError(GenericError.CraftTooManyAdditives);
                return;
            }

            Item2Entry additiveItem = GameTableManager.Instance.Item.GetEntry(additiveItemId);
            TradeskillAdditiveEntry additive = additiveItem != null ? GameTableManager.Instance.TradeskillAdditive.GetEntry(additiveItem.TradeskillAdditiveId) : null;
            if (additive == null || additive.TradeSkillId != currentCraft.Schematic.TradeSkillId)
                throw new InvalidOperationException($"Item {additiveItemId} isn't an additive for tradeskill {currentCraft.Schematic.TradeSkillId}!");

            ITradeskill tradeskill = player.TradeskillManager.GetTradeskill((TradeskillType)currentCraft.Schematic.TradeSkillId);
            if (additive.Tier > (tradeskill?.Tier?.Tier ?? 1u))
            {
                player.SendGenericError(GenericError.CraftTierTooLow);
                return;
            }

            if (!PayForAdditive(additiveItem))
                return;

            // additives move the marker along their vector, landing somewhere inside their radius
            float angle    = Random.Shared.NextSingle() * MathF.Tau;
            float distance = MathF.Sqrt(Random.Shared.NextSingle()) * additive.Radius;
            currentCraft.Position += new Vector2(additive.VectorX + MathF.Cos(angle) * distance, additive.VectorY + MathF.Sin(angle) * distance);
            currentCraft.Additives.Add(additiveItemId);

            SendCurrentCraft();
        }

        public void CompleteCoordinateCraft(uint stationUnitId, uint schematicId)
        {
            RecoverInterruptedCast();

            // simple crafts don't have a craft in progress to complete
            if (currentCraft == null)
            {
                StartCraft(stationUnitId, schematicId, 1u, 0u);
                return;
            }

            if (currentCraft is not { Type: CraftType.Coordinate, Completing: false } || currentCraft.Schematic.Id != schematicId)
                throw new InvalidOperationException($"Player {player.CharacterId} isn't doing a coordinate craft of schematic {schematicId}!");

            CheckStation(stationUnitId, currentCraft.Schematic);
            CastCraftingSpell();
        }

        public void CompleteCircuitCraft(ClientCraftingComplexCraft complexCraft)
        {
            RecoverInterruptedCast();

            if (currentCraft is not { Type: CraftType.Circuit, Completing: false } || currentCraft.Schematic.Id != complexCraft.TradeskillSchematic2Id)
                throw new InvalidOperationException($"Player {player.CharacterId} isn't doing a circuit craft of schematic {complexCraft.TradeskillSchematic2Id}!");

            CheckStation(complexCraft.CraftingStationUnitId, currentCraft.Schematic);

            TradeskillSchematic2Entry schematic = currentCraft.Schematic;
            IItemInfo info = ItemManager.Instance.GetItemInfo(schematic.Item2IdOutput);
            if (info == null)
                throw new InvalidOperationException($"Schematic {schematic.Id} doesn't have a valid output item!");

            if (player.Inventory.IsInventoryFull(InventoryLocation.Inventory))
            {
                player.SendGenericError(GenericError.ItemInventoryFull);
                return;
            }

            bool universal = IsUniversal(schematic);
            CircuitCrafting.Settings settings = CircuitCrafting.GetSettings(universal, player.Account.AccountTier == AccountTier.Signature);

            // non universal schematics need a power core, the crafted item takes its power level from the power core
            IItemInfo powerCore = null;
            if (complexCraft.PowerCoreItem2Id != 0u)
            {
                powerCore = ItemManager.Instance.GetItemInfo(complexCraft.PowerCoreItem2Id);
                if (powerCore?.CategoryEntry?.Id != PowerCoreItemCategory || (!currentCraft.CircuitPaid && !HasItem(complexCraft.PowerCoreItem2Id, 1u)))
                {
                    player.SendGenericError(GenericError.CraftInvalidPowerCore);
                    return;
                }
            }
            else if (!universal)
            {
                player.SendGenericError(GenericError.CraftInvalidPowerCore);
                return;
            }

            GenericError? error = BuildCircuit(complexCraft, info, settings, out Property?[] slots, out float[] multipliers);
            if (error.HasValue)
            {
                player.SendGenericError(error.Value);
                return;
            }

            if (complexCraft.ApSpSplitDelta > CircuitCrafting.GetApSpSplitMaxDelta(settings))
            {
                player.SendGenericError(GenericError.CraftBadParams);
                return;
            }

            uint powerLevel = powerCore?.Entry.PowerLevel ?? info.Entry.PowerLevel;
            float failChance = GetFailChance(info, settings, slots, multipliers, powerLevel, powerCore, universal);

            // universal schematics can't be crafted with any chance to fail
            bool canFail = ((SchematicFlags)schematic.Flags & SchematicFlags.Universal) != 0;
            if (canFail ? failChance > 0f : failChance > CircuitCrafting.GetFailCap(settings))
            {
                player.SendGenericError(GenericError.CraftFailChanceTooHigh);
                return;
            }

            // circuit crafts cost a share of the crafted item value, universal schematics are free
            ulong cost = universal ? 0ul : CircuitCrafting.GetCraftingCost(info.CalculateVendorAmount(powerLevel));
            if (!currentCraft.CircuitPaid)
            {
                if (cost > 0ul && !player.CurrencyManager.CanAfford(CurrencyType.Credits, cost))
                {
                    player.SendGenericError(GenericError.VendorNotEnoughCash);
                    return;
                }

                if (cost > 0ul)
                    player.CurrencyManager.CurrencySubtractAmount(CurrencyType.Credits, cost);

                // power cores are consumed by the craft
                if (powerCore != null)
                    RemoveItem(powerCore.Id, 1u);

                currentCraft.PowerCoreItemId = powerCore?.Id ?? 0u;
                currentCraft.CircuitPaid     = true;
            }
            else if ((powerCore?.Id ?? 0u) != currentCraft.PowerCoreItemId)
            {
                // a retried craft keeps the power core already used
                player.SendGenericError(GenericError.CraftInvalidPowerCore);
                return;
            }

            byte apSpSplit = CircuitCrafting.GetApSpSplitByte(settings, complexCraft.ApSpSplitDelta);
            currentCraft.CircuitData     = CircuitCrafting.BuildCircuitData(slots, apSpSplit);
            currentCraft.SlotMultipliers = multipliers;
            currentCraft.FailChance      = failChance;

            CastCraftingSpell();
        }

        /// <summary>
        /// Item category of power cores.
        /// </summary>
        private const uint PowerCoreItemCategory = 143u;

        private static bool IsUniversal(TradeskillSchematic2Entry schematic)
        {
            return ((SchematicFlags)schematic.Flags & SchematicFlags.Universal) != 0 && schematic.Tier == 0u;
        }

        /// <summary>
        /// Validate the properties and charges chosen for each socket of the current circuit craft.
        /// </summary>
        private GenericError? BuildCircuit(ClientCraftingComplexCraft complexCraft, IItemInfo info, CircuitCrafting.Settings settings, out Property?[] slots, out float[] multipliers)
        {
            slots       = (Property?[])currentCraft.SlotProperties.Clone();
            multipliers = new float[Math.Min(complexCraft.ChargeCounts.Length, CircuitCrafting.SlotCount)];

            ItemStatEntry statEntry = info.StatEntry;
            for (int i = 0; i < CircuitCrafting.SlotCount; i++)
            {
                bool crafted = statEntry != null && i < statEntry.ItemStatTypeEnum.Length && statEntry.ItemStatTypeEnum[i] == ItemStatType.Crafted;
                int charges  = i < complexCraft.ChargeCounts.Length ? complexCraft.ChargeCounts[i] : 0;

                if (!crafted)
                {
                    // only sockets can be charged or have a property chosen
                    if (charges != 0 || complexCraft.CraftStats.GetSlot(i) != 0)
                        return GenericError.CraftMicrochipInvalidSocket;
                }
                else
                {
                    if (!CircuitCrafting.IsValidCharge(settings, charges))
                        return GenericError.CraftBadParams;

                    byte chosen = complexCraft.CraftStats.GetSlot(i);
                    if (chosen != 0)
                    {
                        var property = (Property)(chosen - 1);
                        if (!CircuitCrafting.IsCraftableProperty(property, info) || !CircuitCrafting.CanSocketProperty(currentCraft.SocketGroups[i], property))
                            return GenericError.CraftMicrochipInvalidSocket;

                        slots[i] = property;
                    }

                    if (!slots[i].HasValue)
                        return GenericError.CraftIncompleteCircuit;
                }

                if (i < multipliers.Length)
                    multipliers[i] = CircuitCrafting.GetChargeMultiplier(settings, charges);
            }

            // each property can only be chosen once
            if (slots.Where(p => p.HasValue).GroupBy(p => p.Value).Any(g => g.Count() > 1))
                return GenericError.CraftDuplicateChip;

            return null;
        }

        /// <summary>
        /// Return the chance for the circuit craft to fail, charging sockets above their budget and properties that don't match their socket increase the chance.
        /// </summary>
        private float GetFailChance(IItemInfo info, CircuitCrafting.Settings settings, Property?[] slots, float[] multipliers, uint powerLevel, IItemInfo powerCore, bool universal)
        {
            if (info.BudgetEntry == null || info.StatEntry == null)
                return 0f;

            float itemPower = info.CalculateItemPower(powerLevel);
            float penalty   = CircuitCrafting.GetMismatchPenalty(settings);

            float craftedBudget = 0f;
            for (int i = 0; i < CircuitCrafting.SlotCount && i < info.StatEntry.ItemStatTypeEnum.Length; i++)
            {
                ItemStatType type = info.StatEntry.ItemStatTypeEnum[i];
                if (type is not ItemStatType.Standard and not ItemStatType.Crafted)
                    continue;

                if (type == ItemStatType.Crafted && !slots[i].HasValue)
                    continue;

                float budget = info.BudgetEntry.Budgets[i] * itemPower * (i < multipliers.Length ? multipliers[i] : 1f);
                craftedBudget += budget;

                if (type == ItemStatType.Crafted && !CircuitCrafting.IsMatchingGroup(currentCraft.SocketGroups[i], slots[i].Value))
                    craftedBudget += penalty * budget;
            }

            return CircuitCrafting.GetFailChance(settings, itemPower, craftedBudget, powerCore, universal);
        }

        public void AbandonCraft()
        {
            if (currentCraft == null || currentCraft.Completing)
                return;

            log.Trace($"Player {player.CharacterId} abandoned craft of schematic {currentCraft.Schematic.Id}.");
            currentCraft = null;
            SendCurrentCraft();
        }

        /// <summary>
        /// A crafting spell that was interrupted (eg: by moving) leaves the craft as it was before completing.
        /// </summary>
        private void RecoverInterruptedCast()
        {
            if (currentCraft is not { Completing: true } || player.IsCasting())
                return;

            if (currentCraft.Type == CraftType.Simple)
                currentCraft = null;
            else
                currentCraft.Completing = false;
        }

        private void CastCraftingSpell()
        {
            currentCraft.Completing = true;
            player.CastSpell(CraftingSpell4Id, new SpellParameters
            {
                UserInitiatedSpellCast = false
            });
        }

        public void OnCraftSpell()
        {
            if (currentCraft is not { Completing: true })
                return;

            CurrentCraft craft = currentCraft;
            currentCraft = null;

            switch (craft.Type)
            {
                case CraftType.Coordinate:
                    FinishCoordinateCraft(craft);
                    break;
                case CraftType.Simple:
                {
                    if (!HasMaterials(craft.Schematic, craft.Count))
                    {
                        player.SendGenericError(GenericError.CraftMissingMaterials);
                        break;
                    }

                    RemoveMaterials(craft.Schematic, craft.Count);
                    FinishCraft(craft, craft.Schematic, craft.Schematic.Item2IdOutput, craft.Schematic.OutputCount * craft.Count, CraftingDiscovery.Cold, CraftingDirection.None);
                    break;
                }
                default:
                    FinishCircuitCraft(craft);
                    break;
            }

            SendCurrentCraft();
        }

        private void FinishCoordinateCraft(CurrentCraft craft)
        {
            TradeskillSchematic2Entry crafted = craft.Schematic;

            // variants of the schematic are circles on the grid, the craft produces the variant the marker landed in
            TradeskillSchematic2Entry[] variants = GameTableManager.Instance.TradeskillSchematic2.Entries
                .Where(s => s.TradeskillSchematic2IdParent == craft.Schematic.Id)
                .ToArray();

            TradeskillSchematic2Entry hit = variants
                .Where(v => player.TradeskillManager.IsSchematicKnown(v.Id) && v.Radius > 0f)
                .Select(v => (Variant: v, Distance: Vector2.Distance(craft.Position, GetVariantPosition(v))))
                .Where(v => v.Distance <= v.Variant.Radius)
                .OrderBy(v => v.Distance)
                .Select(v => v.Variant)
                .FirstOrDefault();

            CraftingDiscovery discovery = CraftingDiscovery.Cold;
            CraftingDirection direction = CraftingDirection.None;

            // undiscovered variants give hot or cold feedback towards the closest one, landing on one discovers it
            TradeskillSchematic2Entry undiscovered = variants
                .Where(v => IsDiscoverable(v) && !player.TradeskillManager.IsSchematicKnown(v.Id))
                .OrderBy(v => Vector2.Distance(craft.Position, GetVariantPosition(v)))
                .FirstOrDefault();
            if (undiscovered != null)
            {
                Vector2 target = GetVariantPosition(undiscovered);
                float distance = Vector2.Distance(craft.Position, target);
                float radius   = Math.Max(undiscovered.Radius, 0.01f);

                discovery = GetDiscovery(distance, radius);
                direction = GetDirection(craft.Position, target);

                if (discovery == CraftingDiscovery.Success && hit == null)
                {
                    player.TradeskillManager.LearnSchematic(undiscovered.Id, target);
                    hit = undiscovered;
                }
            }

            if (hit != null)
                crafted = hit;

            FinishCraft(craft, crafted, crafted.Item2IdOutput, crafted.OutputCount, discovery, direction);
        }

        private void FinishCircuitCraft(CurrentCraft craft)
        {
            TradeskillSchematic2Entry schematic = craft.Schematic;

            // overcharged circuits can fail, producing the failed craft item instead
            if (craft.FailChance > 0f && Random.Shared.NextSingle() < craft.FailChance)
            {
                if (schematic.Item2IdOutputFail != 0u)
                    player.Inventory.ItemCreate(InventoryLocation.Inventory, schematic.Item2IdOutputFail, 1u, ItemUpdateReason.Crafting);

                player.Session.EnqueueMessageEncrypted(new ServerCraftingFinish
                {
                    Pass                          = false,
                    TradeskillSchematic2IdCrafted = schematic.Id,
                    Item2IdCrafted                = schematic.Item2IdOutputFail
                });

                log.Trace($"Player {player.CharacterId} failed circuit craft of schematic {schematic.Id} with a {craft.FailChance:P0} chance to fail.");
                return;
            }

            IItemInfo info = ItemManager.Instance.GetItemInfo(schematic.Item2IdOutput);
            if (info == null || player.Inventory.IsInventoryFull(InventoryLocation.Inventory))
            {
                player.SendGenericError(GenericError.ItemInventoryFull);
                return;
            }

            var item = new Item(player.CharacterId, info);
            item.SetCircuit(craft.CircuitData, craft.SlotMultipliers, craft.PowerCoreItemId);
            player.Inventory.AddItem(item, InventoryLocation.Inventory, ItemUpdateReason.Crafting);

            // the crafted item is created here so only the rewards are handled when finishing the craft
            FinishCraft(craft, schematic, schematic.Item2IdOutput, 0u, CraftingDiscovery.Cold, CraftingDirection.None);
        }

        private static bool IsDiscoverable(TradeskillSchematic2Entry schematic)
        {
            return ((SchematicFlags)schematic.Flags & SchematicFlags.Discoverable) != 0;
        }

        /// <summary>
        /// Return the centre of the supplied variant on the crafting grid.
        /// </summary>
        /// <remarks>
        /// Discoverable variants don't have a fixed position, for them the vector is the distance range from the centre of the grid and
        /// the variant is somewhere in the quadrant around the discovery angle. Each character has their own position which becomes
        /// the variant position once discovered.
        /// </remarks>
        private Vector2 GetVariantPosition(TradeskillSchematic2Entry variant)
        {
            if (!IsDiscoverable(variant))
                return new Vector2(variant.VectorX, variant.VectorY);

            ITradeskillSchematic learned = player.TradeskillManager.GetSchematic(variant.Id);
            if (learned is { Discovered: true })
                return learned.DiscoveryCoordinates;

            float centreAngle = variant.DiscoverableAngle > 0f ? variant.DiscoverableAngle : variant.DiscoverableQuadrant * 90f + 45f;
            float minDistance = Math.Min(variant.VectorX, variant.VectorY);
            float maxDistance = Math.Max(variant.VectorX, variant.VectorY);

            // stable for the character so hot and cold feedback is consistent between crafts
            var random = new Random(unchecked((int)(player.CharacterId * 397u) ^ (int)variant.Id));
            float angle    = (centreAngle - 45f + (float)random.NextDouble() * 90f) * MathF.PI / 180f;
            float distance = minDistance + (float)random.NextDouble() * (maxDistance - minDistance);
            return new Vector2(MathF.Cos(angle) * distance, MathF.Sin(angle) * distance);
        }

        /// <remarks>
        /// Thresholds are relative to the radius of the undiscovered variant, retail values are unknown.
        /// </remarks>
        private static CraftingDiscovery GetDiscovery(float distance, float radius)
        {
            if (distance <= radius)
                return CraftingDiscovery.Success;
            if (distance <= radius * 2f)
                return CraftingDiscovery.Hot;
            if (distance <= radius * 4f)
                return CraftingDiscovery.Warm;
            return CraftingDiscovery.Cold;
        }

        private static CraftingDirection GetDirection(Vector2 from, Vector2 to)
        {
            Vector2 delta = to - from;
            if (delta.LengthSquared() < 0.0001f)
                return CraftingDirection.None;

            // compass direction in 45 degree sectors, east is 0 degrees and north is 90 degrees
            float degrees = MathF.Atan2(delta.Y, delta.X) * 180f / MathF.PI;
            int sector = (int)MathF.Round(((degrees % 360f) + 360f) % 360f / 45f) % 8;
            return sector switch
            {
                0 => CraftingDirection.E,
                1 => CraftingDirection.NE,
                2 => CraftingDirection.N,
                3 => CraftingDirection.NW,
                4 => CraftingDirection.W,
                5 => CraftingDirection.SW,
                6 => CraftingDirection.S,
                _ => CraftingDirection.SE
            };
        }

        private void FinishCraft(CurrentCraft craft, TradeskillSchematic2Entry crafted, uint itemId, uint count, CraftingDiscovery discovery, CraftingDirection direction)
        {
            if (itemId != 0u && count > 0u)
                player.Inventory.ItemCreate(InventoryLocation.Inventory, itemId, count, ItemUpdateReason.Crafting);
            else if (itemId != 0u)
                count = 1u;

            var tradeskillId = (TradeskillType)craft.Schematic.TradeSkillId;
            uint xp = (GetTier(tradeskillId, craft.Schematic.Tier)?.CraftXp ?? 0u) * craft.Count;
            player.TradeskillManager.AddXp(tradeskillId, xp);

            player.QuestManager.ObjectiveUpdate(QuestObjectiveType.CraftSchematic, crafted.Id, craft.Count);
            player.QuestManager.ObjectiveUpdate(QuestObjectiveType.Unknown20, crafted.Id, craft.Count);
            if (itemId != 0u)
            {
                player.AchievementManager.CheckAchievements(player, AchievementType.CraftItem, itemId, count: craft.Count);
                player.AchievementManager.CheckAchievements(player, AchievementType.CraftItemChecklist, itemId);
            }
            if (crafted.Id != craft.Schematic.Id)
            {
                player.QuestManager.ObjectiveUpdate(QuestObjectiveType.CraftSchematic, craft.Schematic.Id, craft.Count);
                player.QuestManager.ObjectiveUpdate(QuestObjectiveType.Unknown20, craft.Schematic.Id, craft.Count);
            }

            player.Session.EnqueueMessageEncrypted(new ServerCraftingFinish
            {
                Pass                          = true,
                TradeskillSchematic2IdCrafted = crafted.Id,
                Item2IdCrafted                = itemId,
                HotOrCold                     = discovery,
                Direction                     = direction,
                EarnedXp                      = xp
            });

            log.Trace($"Player {player.CharacterId} crafted schematic {crafted.Id} ({count}x item {itemId}) for {xp} xp.");
        }

        private static TradeskillTierEntry GetTier(TradeskillType tradeskillId, uint tier)
        {
            return GameTableManager.Instance.TradeskillTier.Entries
                .FirstOrDefault(t => t.TradeSkillId == (uint)tradeskillId && t.Tier == Math.Max(tier, 1u));
        }

        private TradeskillSchematic2Entry GetCraftableSchematic(uint schematicId)
        {
            TradeskillSchematic2Entry schematic = GameTableManager.Instance.TradeskillSchematic2.GetEntry(schematicId);
            if (schematic == null)
                throw new InvalidOperationException($"Invalid tradeskill schematic {schematicId}!");

            if (!player.TradeskillManager.IsSchematicKnown(schematicId))
            {
                player.SendGenericError(GenericError.CraftUnknownSchematic);
                throw new InvalidOperationException($"Player {player.CharacterId} doesn't know schematic {schematicId}!");
            }

            ITradeskill tradeskill = player.TradeskillManager.GetTradeskill((TradeskillType)schematic.TradeSkillId);
            if (tradeskill?.Active != true)
                throw new InvalidOperationException($"Player {player.CharacterId} doesn't have tradeskill {schematic.TradeSkillId} active!");

            if (!tradeskill.IsHobby && schematic.Tier > (tradeskill.Tier?.Tier ?? 1u))
            {
                player.SendGenericError(GenericError.CraftTierTooLow);
                throw new InvalidOperationException($"Player {player.CharacterId} tier is too low for schematic {schematicId}!");
            }

            return schematic;
        }

        private static CraftType GetCraftType(TradeskillEntry tradeskill, TradeskillSchematic2Entry schematic)
        {
            if (((SchematicFlags)schematic.Flags & SchematicFlags.AutoCraft) != 0)
                return CraftType.Simple;

            var flags = (TradeskillFlags)(tradeskill?.Flags ?? 0u);
            if ((flags & TradeskillFlags.CoordinateCrafting) != 0)
                return CraftType.Coordinate;
            if ((flags & TradeskillFlags.CircuitCrafting) != 0)
                return CraftType.Circuit;

            return CraftType.Simple;
        }

        private void CheckStation(uint stationUnitId, TradeskillSchematic2Entry schematic)
        {
            IWorldEntity station = player.GetVisible<IWorldEntity>(stationUnitId);
            uint stationTradeskill = station?.CreatureInfo?.Entry.TradeSkillIdStation ?? 0u;
            if (stationTradeskill == 0u || (stationTradeskill != AnyTradeskillStation && stationTradeskill != schematic.TradeSkillId))
                throw new InvalidOperationException($"Unit {stationUnitId} isn't a crafting station for tradeskill {schematic.TradeSkillId}!");

            if (Vector3.Distance(player.Position, station.Position) > MaxStationDistance)
            {
                player.SendGenericError(GenericError.CraftTooFar);
                throw new InvalidOperationException($"Player {player.CharacterId} is too far from crafting station {stationUnitId}!");
            }
        }

        private static uint GetMaxAdditives(TradeskillSchematic2Entry schematic)
        {
            if (schematic.MaxAdditives != 0u)
                return schematic.MaxAdditives;

            return GameTableManager.Instance.Tradeskill.GetEntry(schematic.TradeSkillId)?.MaxAdditives ?? 0u;
        }

        /// <summary>
        /// Additives owned by the player are used first, otherwise they are bought for their vendor price.
        /// </summary>
        private bool PayForAdditive(Item2Entry additiveItem)
        {
            if (HasItem(additiveItem.Id, 1u))
            {
                RemoveItem(additiveItem.Id, 1u);
                return true;
            }

            CurrencyType currency = additiveItem.CurrencyTypeId[0];
            ulong cost = additiveItem.CurrencyAmount[0];
            if (currency == CurrencyType.None || cost == 0u)
                return true;

            if (!player.CurrencyManager.CanAfford(currency, cost))
            {
                player.SendGenericError(GenericError.VendorNotEnoughCash);
                return false;
            }

            player.CurrencyManager.CurrencySubtractAmount(currency, cost);
            return true;
        }

        private IEnumerable<(uint ItemId, uint Count)> GetMaterials(TradeskillSchematic2Entry schematic, uint count)
        {
            uint[] items = [schematic.Item2IdMaterial00, schematic.Item2IdMaterial01, schematic.Item2IdMaterial02, schematic.Item2IdMaterial03, schematic.Item2IdMaterial04];
            uint[] costs = [schematic.MaterialCost00, schematic.MaterialCost01, schematic.MaterialCost02, schematic.MaterialCost03, schematic.MaterialCost04];
            for (int i = 0; i < items.Length; i++)
                if (items[i] != 0u && costs[i] != 0u)
                    yield return (items[i], costs[i] * count);
        }

        private bool HasMaterials(TradeskillSchematic2Entry schematic, uint count)
        {
            return GetMaterials(schematic, count).All(m => HasItem(m.ItemId, m.Count));
        }

        private void RemoveMaterials(TradeskillSchematic2Entry schematic, uint count)
        {
            foreach ((uint itemId, uint amount) in GetMaterials(schematic, count))
                RemoveItem(itemId, amount);
        }

        /// <summary>
        /// Returns if the player has the supplied amount of an item, counting both the supply satchel and inventory.
        /// </summary>
        private bool HasItem(uint itemId, uint count)
        {
            uint satchel = GetSatchelAmount(itemId, out _);
            if (satchel >= count)
                return true;

            return player.Inventory.HasItemCount(itemId, count - satchel);
        }

        /// <summary>
        /// Remove the supplied amount of an item, taking from the supply satchel before the inventory.
        /// </summary>
        private void RemoveItem(uint itemId, uint count)
        {
            uint satchel = GetSatchelAmount(itemId, out ushort materialId);
            uint fromSatchel = Math.Min(satchel, count);
            if (fromSatchel > 0u)
                player.SupplySatchelManager.RemoveAmount(materialId, fromSatchel);

            if (count > fromSatchel)
                player.Inventory.ItemDelete(itemId, count - fromSatchel, ItemUpdateReason.Crafting);
        }

        private uint GetSatchelAmount(uint itemId, out ushort materialId)
        {
            TradeskillMaterialEntry material = SupplySatchelManager.GetMaterialEntry(itemId);
            materialId = (ushort)(material?.Id ?? 0u);
            return material != null ? player.SupplySatchelManager.GetAmount(materialId) : 0u;
        }

        private void SendCurrentCraft()
        {
            var message = new ServerCraftingCurrentCraft();
            if (currentCraft != null)
            {
                message.TradeskillSchematic2Id    = currentCraft.Schematic.Id;
                message.CraftStats.Data           = currentCraft.CircuitData;
                message.SocketGroups              = currentCraft.Type == CraftType.Circuit ? CircuitCrafting.BuildSocketGroups(currentCraft.SocketGroups) : 0u;
                message.SchematicCount            = currentCraft.Count;
                message.AdditiveCount             = (uint)currentCraft.Additives.Count;
                message.Item2IdModifierCount      = (uint)currentCraft.Additives.Count;
                message.Item2Id                   = currentCraft.PowerCoreItemId;
                message.DiscoveryCoordinates      = currentCraft.Position;
                message.DiscoveryVectorMultiplier = Vector2.One;
                message.DiscoveryRadiusMultiplier = 1f;

                for (int i = 0; i < currentCraft.Additives.Count && i < message.Item2IdModifiers.Length; i++)
                    message.Item2IdModifiers[i] = currentCraft.Additives[i];
            }

            player.Session.EnqueueMessageEncrypted(message);
        }
    }
}
