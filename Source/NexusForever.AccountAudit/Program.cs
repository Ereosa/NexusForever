using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using NexusForever.Game.Account;
using NexusForever.Game.Prerequisite;
using NexusForever.Game.Spell.Effect;
using NexusForever.Game.Static.AccountInventory;
using NexusForever.Game.Static.Entity;
using NexusForever.Game.Static.Prerequisite;
using NexusForever.Game.Static.Reward;
using NexusForever.Game.Static.Spell;
using NexusForever.GameTable;
using NexusForever.GameTable.Model;
using NexusForever.Network.Message;

namespace NexusForever.AccountAudit
{
    /// <summary>
    /// Audits the store and account features against what the server supports: every store offer, account item, entitlement,
    /// reward property, generic unlock, account currency and account feature packet.
    /// </summary>
    /// <remarks>
    /// Usage: NexusForever.AccountAudit --tbl &lt;tbl dir&gt; [--store &lt;file&gt;] [--loot &lt;file&gt;] [--src &lt;Source dir&gt;] [--out &lt;report.md&gt;] [--items &lt;items.tsv&gt;]
    /// The store file is the output of scripts\account-audit-store.sql (mysql -B), the loot file lists one item id with item_loot per line
    /// (select distinct id from item_loot). The source dir is scanned for packet handlers and for server packets that are sent.
    /// </remarks>
    public static class Program
    {
        // Item2Type ids of loot bags and salvageable containers, opened with ClientItemUseLootBag
        private static readonly uint[] containerCategories = [138, 194];
        private static readonly uint[] containerTypes = [212, 562];
        private const uint CostumeFamily = 26;

        private static GameTable<AccountItemEntry> accountItems;
        private static GameTable<Item2Entry> items;
        private static GameTable<ItemSpecialEntry> itemSpecials;
        private static GameTable<EntitlementEntry> entitlements;
        private static GameTable<GenericUnlockSetEntry> unlockSets;
        private static GameTable<GenericUnlockEntryEntry> unlockEntries;
        private static GameTable<PrerequisiteEntry> prerequisites;
        private static GameTable<RewardPropertyPremiumModifierEntry> modifiers;
        private static TextTable text;

        private static HashSet<PrerequisiteType> handledPrerequisites;
        private static HashSet<SpellEffectType> handledEffects;
        private static Dictionary<uint, List<Spell4EffectsEntry>> effectsBySpell;
        private static HashSet<uint> lootItems;

        public static int Main(string[] args)
        {
            string tbl       = GetArg(args, "--tbl") ?? @"E:\Wildstar Server\data\tbl";
            string out_      = GetArg(args, "--out");
            string itemsOut  = GetArg(args, "--items");
            string storeFile = GetArg(args, "--store");
            string lootFile  = GetArg(args, "--loot");
            string src       = GetArg(args, "--src");

            accountItems  = new GameTable<AccountItemEntry>(Path.Combine(tbl, "AccountItem.tbl"));
            items         = new GameTable<Item2Entry>(Path.Combine(tbl, "Item2.tbl"));
            itemSpecials  = new GameTable<ItemSpecialEntry>(Path.Combine(tbl, "ItemSpecial.tbl"));
            entitlements  = new GameTable<EntitlementEntry>(Path.Combine(tbl, "Entitlement.tbl"));
            unlockSets    = new GameTable<GenericUnlockSetEntry>(Path.Combine(tbl, "GenericUnlockSet.tbl"));
            unlockEntries = new GameTable<GenericUnlockEntryEntry>(Path.Combine(tbl, "GenericUnlockEntry.tbl"));
            prerequisites = new GameTable<PrerequisiteEntry>(Path.Combine(tbl, "Prerequisite.tbl"));
            modifiers     = new GameTable<RewardPropertyPremiumModifierEntry>(Path.Combine(tbl, "RewardPropertyPremiumModifier.tbl"));
            var rewardProperties = new GameTable<RewardPropertyEntry>(Path.Combine(tbl, "RewardProperty.tbl"));
            var currencies       = new GameTable<AccountCurrencyTypeEntry>(Path.Combine(tbl, "AccountCurrencyType.tbl"));
            var spellEffects     = new GameTable<Spell4EffectsEntry>(Path.Combine(tbl, "Spell4Effects.tbl"));
            text = new TextTable(Path.Combine(tbl, "en-US.bin"));

            handledPrerequisites = typeof(PrerequisiteManager).Assembly.GetTypes()
                .SelectMany(t => t.GetCustomAttributes<PrerequisiteCheckAttribute>())
                .Select(a => a.Type)
                .ToHashSet();
            handledEffects = typeof(SpellEffectHandlerAttribute).Assembly.GetTypes()
                .SelectMany(t => t.GetCustomAttributes<SpellEffectHandlerAttribute>())
                .Select(a => a.SpellEffectType)
                .ToHashSet();
            effectsBySpell = spellEffects.Entries.Where(e => e != null)
                .GroupBy(e => e.SpellId)
                .ToDictionary(g => g.Key, g => g.ToList());
            lootItems = lootFile == null ? null : File.ReadLines(lootFile)
                .Select(l => uint.TryParse(l.Trim(), out uint id) ? id : 0u)
                .Where(id => id != 0u)
                .ToHashSet();

            var report = new StringBuilder();
            report.AppendLine("# Account and store coverage");
            report.AppendLine();
            report.AppendLine("Support levels: **Supported** works end to end (purchase, claim, effect applied and shown), **Partial** some of it, **Unsupported** none of it.");
            report.AppendLine();

            // account items
            var accountItemResults = accountItems.Entries
                .Where(e => e != null)
                .Select(AuditAccountItem)
                .ToDictionary(r => r.Entry.Id);

            // store offers
            List<OfferResult> offers = storeFile == null ? [] : ReadStore(storeFile, accountItemResults);
            if (storeFile != null)
                WriteStore(report, offers);

            WriteAccountItems(report, accountItemResults.Values.ToList(), offers);
            WriteItemUse(report, accountItemResults.Values.ToList(), offers);
            WriteEntitlements(report, accountItemResults.Values.ToList());
            WriteRewardProperties(report, rewardProperties);
            WriteGenericUnlocks(report, accountItemResults.Values.ToList());
            WriteCurrencies(report, currencies, accountItemResults.Values.ToList(), offers);
            if (src != null)
                WritePackets(report, src);

            if (out_ != null)
                File.WriteAllText(out_, report.ToString());
            else
                Console.Write(report);

            if (itemsOut != null)
            {
                var sb = new StringBuilder();
                sb.AppendLine("id\tname\tlevel\tinStore\tcontents\tblockers");
                foreach (AccountItemResult result in accountItemResults.Values.OrderBy(r => r.Entry.Id))
                {
                    bool inStore = offers.Any(o => o.AccountItems.Any(a => a.Id == result.Entry.Id));
                    sb.AppendLine($"{result.Entry.Id}\t{result.Name}\t{result.Level}\t{(inStore ? 1 : 0)}\t{string.Join(", ", result.Contents)}\t{string.Join("; ", result.Blockers)}");
                }
                File.WriteAllText(itemsOut, sb.ToString());
            }

            return 0;
        }

        #region Account items

        private class AccountItemResult
        {
            public AccountItemEntry Entry;
            public string Name;
            public List<string> Contents = [];
            public List<string> Blockers = [];
            public AccountSupportLevel Level = AccountSupportLevel.Supported;

            public void Add(string blocker, AccountSupportLevel level)
            {
                if (level == AccountSupportLevel.Supported)
                    return;
                Blockers.Add(blocker);
                if (level < Level)
                    Level = level;
            }
        }

        private static AccountItemResult AuditAccountItem(AccountItemEntry entry)
        {
            var result = new AccountItemResult
            {
                Entry = entry,
                Name  = GetAccountItemName(entry)
            };

            bool hasContents = false;
            if (entry.Item2Id != 0u)
            {
                hasContents = true;
                (string kind, AccountSupportLevel level, string blocker) = ClassifyItem(entry.Item2Id);
                result.Contents.Add($"item:{kind}");
                AddClaim(result, "Item");
                result.Add(blocker, level);
            }

            if (entry.EntitlementId != 0u)
            {
                hasContents = true;
                EntitlementEntry entitlement = entitlements.GetEntry(entry.EntitlementId);
                result.Contents.Add($"entitlement:{entry.EntitlementId}");
                AddClaim(result, "Entitlement");
                if (entitlement == null)
                    result.Add($"entitlement {entry.EntitlementId} missing", AccountSupportLevel.Unsupported);
                else
                {
                    (AccountSupportLevel level, string how) = GetEntitlementEffect(entitlement);
                    result.Add($"entitlement {entry.EntitlementId} effect ({how})", level);
                    if (entitlement.Spell4IdPersistentBuff != 0u)
                        AddClaim(result, "EntitlementBuff");
                    if (entitlement.CharacterTitleId != 0u)
                        AddClaim(result, "EntitlementTitle");
                }

                if (entry.EntitlementScopeEnum == 1u)
                    AddClaim(result, "CharacterScope");
            }

            if (entry.AccountCurrencyEnum != 0u)
            {
                hasContents = true;
                var currency = (AccountCurrencyType)entry.AccountCurrencyEnum;
                result.Contents.Add($"currency:{currency}");
                AddClaim(result, "Currency");
                result.Add($"currency {currency} has no use", AccountFeatureSupport.GetCurrencySupport(currency) == AccountSupportLevel.Unsupported
                    ? AccountSupportLevel.Partial : AccountSupportLevel.Supported);
            }

            if (entry.GenericUnlockSetId != 0u)
            {
                hasContents = true;
                result.Contents.Add($"unlockset:{entry.GenericUnlockSetId}");
                AddClaim(result, "GenericUnlockSet");
                foreach (GenericUnlockEntryEntry unlock in GetUnlocks(entry.GenericUnlockSetId))
                    if (unlock.GenericUnlockTypeEnum != GenericUnlockType.Dye)
                        result.Add($"generic unlock type {unlock.GenericUnlockTypeEnum}", AccountSupportLevel.Unsupported);
            }

            if (entry.InstantEventEnum != 0u)
            {
                hasContents = true;
                result.Contents.Add($"instantevent:{entry.InstantEventEnum}");
                AddClaim(result, "InstantEvent");
            }

            if (entry.AccountItemCooldownGroupId != 0u)
            {
                result.Contents.Add($"cooldown:{entry.AccountItemCooldownGroupId}");
                AddClaim(result, "Cooldown");
            }

            if ((entry.Flags & 2u) != 0u)
            {
                result.Contents.Add("multiredeem");
                AddClaim(result, "MultiRedeem");
            }

            if (entry.PrerequisiteId != 0u)
            {
                AddClaim(result, "Prerequisite");
                var types = new HashSet<PrerequisiteType>();
                CollectPrerequisites(entry.PrerequisiteId, types, []);
                foreach (PrerequisiteType type in types.Where(t => !handledPrerequisites.Contains(t)))
                    result.Add($"prerequisite {type}", AccountSupportLevel.Unsupported);
            }

            if (!hasContents)
                result.Add("no contents", AccountSupportLevel.Unsupported);

            return result;
        }

        private static void AddClaim(AccountItemResult result, string feature)
        {
            (AccountSupportLevel level, string hook) = AccountFeatureSupport.ClaimFeatures[feature];
            result.Add($"claim {feature} ({hook})", level);
        }

        /// <summary>
        /// Return how an item does what it is for once it is in a character's inventory.
        /// </summary>
        private static (string Kind, AccountSupportLevel Level, string Blocker) ClassifyItem(uint itemId)
        {
            Item2Entry item = items.GetEntry(itemId);
            if (item == null)
                return ("missing", AccountSupportLevel.Unsupported, $"item {itemId} missing");

            ItemSpecialEntry special = item.ItemSpecialId00 != 0u ? itemSpecials.GetEntry(item.ItemSpecialId00) : null;
            if (special != null && special.Spell4IdOnActivate != 0u)
            {
                List<Spell4EffectsEntry> effects = effectsBySpell.GetValueOrDefault(special.Spell4IdOnActivate) ?? [];
                List<SpellEffectType> missing = effects
                    .Select(e => e.EffectType)
                    .Where(t => !handledEffects.Contains(t))
                    .Distinct()
                    .ToList();
                string kind = "spell(" + string.Join("+", effects.Select(e => e.EffectType).Distinct()) + ")";
                if (missing.Count == 0)
                    return (kind, AccountSupportLevel.Supported, null);

                bool any = effects.Any(e => handledEffects.Contains(e.EffectType));
                return (kind, any ? AccountSupportLevel.Partial : AccountSupportLevel.Unsupported, "spell effect " + string.Join("+", missing));
            }

            if (item.HousingDecorInfoId != 0u)
                return ("decor", AccountSupportLevel.Supported, null);

            if (item.GenericUnlockSetId != 0u)
            {
                List<GenericUnlockEntryEntry> unlocks = GetUnlocks(item.GenericUnlockSetId).ToList();
                List<GenericUnlockType> missing = unlocks
                    .Select(u => u.GenericUnlockTypeEnum)
                    .Where(t => t != GenericUnlockType.Dye)
                    .Distinct()
                    .ToList();
                return ("genericunlock", missing.Count == 0 ? AccountSupportLevel.Supported : AccountSupportLevel.Unsupported,
                    missing.Count == 0 ? null : "generic unlock type " + string.Join("+", missing));
            }

            if (item.Item2FamilyId == CostumeFamily)
                return ("costume", AccountSupportLevel.Supported, null);

            if (containerCategories.Contains(item.Item2CategoryId) || containerTypes.Contains(item.Item2TypeId))
            {
                if (lootItems == null)
                    return ("container", AccountSupportLevel.Partial, "container loot not checked (no --loot)");
                return lootItems.Contains(itemId)
                    ? ("container", AccountSupportLevel.Supported, null)
                    : ("container", AccountSupportLevel.Unsupported, $"container {itemId} has no item_loot");
            }

            if (item.EquippedSlotFlags != 0u)
                return ("equipment", AccountSupportLevel.Supported, null);

            return ("plain", AccountSupportLevel.Supported, null);
        }

        /// <summary>
        /// How the items that account items grant (store, Fortunes and gifts) do what they are for once used: spell effects,
        /// holo-wardrobe and dye unlocks, decor, and container contents.
        /// </summary>
        private static void WriteItemUse(StringBuilder report, List<AccountItemResult> results, List<OfferResult> offers)
        {
            HashSet<uint> storeItems = offers.Where(o => o.Visible).SelectMany(o => o.AccountItems).Select(a => a.Id).ToHashSet();

            report.AppendLine("## Item use");
            report.AppendLine();
            report.AppendLine("Every item an account item grants, by how it is used.");
            report.AppendLine();
            report.AppendLine("| Use | Items | From store | Supported | Partial | Unsupported | Blockers |");
            report.AppendLine("|---|---|---|---|---|---|---|");
            var uses = results
                .Where(r => r.Entry.Item2Id != 0u)
                .GroupBy(r => r.Entry.Item2Id)
                .Select(g => (Item: g.Key, Store: g.Any(r => storeItems.Contains(r.Entry.Id)), Use: ClassifyItem(g.Key)))
                .GroupBy(x => Regex.Replace(x.Use.Kind, @"\(.*\)$", ""))
                .OrderByDescending(g => g.Count());
            foreach (var use in uses)
            {
                string blockers = string.Join("; ", use.Where(x => x.Use.Blocker != null).Select(x => Generalise(x.Use.Blocker)).Distinct().Take(5));
                report.AppendLine($"| {use.Key} | {use.Count()} | {use.Count(x => x.Store)} | {use.Count(x => x.Use.Level == AccountSupportLevel.Supported)} | "
                    + $"{use.Count(x => x.Use.Level == AccountSupportLevel.Partial)} | {use.Count(x => x.Use.Level == AccountSupportLevel.Unsupported)} | {blockers} |");
            }
            report.AppendLine();
        }

        private static (AccountSupportLevel Level, string How) GetEntitlementEffect(EntitlementEntry entitlement)
        {
            var type = (EntitlementType)entitlement.Id;
            if (AccountFeatureSupport.Entitlements.TryGetValue(type, out var direct))
                return (direct.Level, direct.Hook);

            List<RewardPropertyType> properties = modifiers.Entries
                .Where(m => m != null && m.EntitlementIdModifierCount == entitlement.Id)
                .Select(m => (RewardPropertyType)m.RewardPropertyId)
                .Distinct()
                .ToList();
            if (properties.Count > 0)
            {
                AccountSupportLevel level = properties.Select(AccountFeatureSupport.GetRewardPropertySupport).Min();
                return (level, "reward property " + string.Join("+", properties));
            }

            if (((EntitlementFlags)entitlement.Flags).HasFlag(EntitlementFlags.Disabled))
                return (AccountSupportLevel.Supported, "disabled");

            // buff and title only entitlements are covered by their claim features
            if (entitlement.Spell4IdPersistentBuff != 0u || entitlement.CharacterTitleId != 0u)
                return (AccountSupportLevel.Supported, "buff or title");

            return (AccountSupportLevel.Unsupported, "nothing reads it");
        }

        private static IEnumerable<GenericUnlockEntryEntry> GetUnlocks(uint setId)
        {
            GenericUnlockSetEntry set = unlockSets.GetEntry(setId);
            if (set == null)
                yield break;

            foreach (uint id in new[] { set.GenericUnlockEntryId00, set.GenericUnlockEntryId01, set.GenericUnlockEntryId02,
                set.GenericUnlockEntryId03, set.GenericUnlockEntryId04, set.GenericUnlockEntryId05 })
            {
                GenericUnlockEntryEntry unlock = id != 0u ? unlockEntries.GetEntry(id) : null;
                if (unlock != null)
                    yield return unlock;
            }
        }

        private static string GetAccountItemName(AccountItemEntry entry)
        {
            if (entry.Item2Id != 0u)
                return text.GetEntry(items.GetEntry(entry.Item2Id)?.LocalizedTextIdName ?? 0u) ?? $"item {entry.Item2Id}";
            if (entry.EntitlementId != 0u)
                return text.GetEntry(entitlements.GetEntry(entry.EntitlementId)?.LocalizedTextIdName ?? 0u) ?? $"entitlement {entry.EntitlementId}";
            if (entry.AccountCurrencyEnum != 0u)
                return $"{entry.AccountCurrencyAmount} {(AccountCurrencyType)entry.AccountCurrencyEnum}";
            if (entry.GenericUnlockSetId != 0u)
                return $"unlock set {entry.GenericUnlockSetId}";
            return $"account item {entry.Id}";
        }

        private static void WriteAccountItems(StringBuilder report, List<AccountItemResult> results, List<OfferResult> offers)
        {
            HashSet<uint> storeItems = offers.Where(o => o.Visible).SelectMany(o => o.AccountItems).Select(a => a.Id).ToHashSet();

            report.AppendLine("## Account items");
            report.AppendLine();
            report.AppendLine($"{results.Count} account items, {storeItems.Count} of them in visible store offers.");
            report.AppendLine();
            report.AppendLine("| Set | Supported | Partial | Unsupported |");
            report.AppendLine("|---|---|---|---|");
            WriteLevels(report, "all", results.Select(r => r.Level));
            WriteLevels(report, "in store", results.Where(r => storeItems.Contains(r.Entry.Id)).Select(r => r.Level));
            report.AppendLine();

            report.AppendLine("### Contents");
            report.AppendLine();
            report.AppendLine("| Contents | Account items | In store | Supported |");
            report.AppendLine("|---|---|---|---|");
            foreach (var group in results
                .SelectMany(r => r.Contents.Select(c => (Content: Regex.Replace(c, @":\d+$", ""), Result: r)))
                .GroupBy(x => x.Content)
                .OrderByDescending(g => g.Count()))
            {
                int store = group.Count(x => storeItems.Contains(x.Result.Entry.Id));
                int supported = group.Count(x => x.Result.Level == AccountSupportLevel.Supported);
                report.AppendLine($"| {group.Key} | {group.Count()} | {store} | {supported} |");
            }
            report.AppendLine();

            report.AppendLine("### Blockers");
            report.AppendLine();
            report.AppendLine("Ranked by store account items, then all account items.");
            report.AppendLine();
            report.AppendLine("| Blocker | In store | All |");
            report.AppendLine("|---|---|---|");
            foreach (var group in results
                .SelectMany(r => r.Blockers.Select(b => (Blocker: Generalise(b), Result: r)))
                .GroupBy(x => x.Blocker)
                .Select(g => (g.Key, Store: g.Select(x => x.Result.Entry.Id).Distinct().Count(storeItems.Contains), All: g.Select(x => x.Result.Entry.Id).Distinct().Count()))
                .OrderByDescending(g => g.Store)
                .ThenByDescending(g => g.All))
                report.AppendLine($"| {group.Key} | {group.Store} | {group.All} |");
            report.AppendLine();
        }

        private static string Generalise(string blocker)
        {
            // keep the kind of blocker, drop ids
            blocker = Regex.Replace(blocker, @"^(item|container|entitlement) \d+ ", "$1 ");
            return blocker;
        }

        #endregion

        #region Store

        private class OfferResult
        {
            public uint Id;
            public string Name;
            public bool Visible;
            public string Categories;
            public List<AccountItemEntry> AccountItems = [];
            public Dictionary<uint, float> Prices = [];
            public AccountSupportLevel Level = AccountSupportLevel.Supported;
            public List<string> Blockers = [];
        }

        private static List<OfferResult> ReadStore(string file, Dictionary<uint, AccountItemResult> accountItemResults)
        {
            // offerId groupId visible categories name items(accountItemXamount,...) prices(currency:price,...)
            var results = new List<OfferResult>();
            foreach (string line in File.ReadLines(file).Skip(1))
            {
                string[] parts = line.Split('\t');
                if (parts.Length < 7 || !uint.TryParse(parts[0], out uint offerId))
                    continue;

                var offer = new OfferResult
                {
                    Id         = offerId,
                    Visible    = parts[2] == "1",
                    Categories = parts[3],
                    Name       = parts[4]
                };

                foreach (string item in parts[5].Split(',', StringSplitOptions.RemoveEmptyEntries))
                {
                    uint accountItemId = uint.Parse(item.Split('x')[0]);
                    if (!accountItemResults.TryGetValue(accountItemId, out AccountItemResult result))
                    {
                        offer.Blockers.Add($"account item {accountItemId} missing");
                        offer.Level = AccountSupportLevel.Unsupported;
                        continue;
                    }

                    offer.AccountItems.Add(result.Entry);
                    if (result.Level < offer.Level)
                        offer.Level = result.Level;
                    offer.Blockers.AddRange(result.Blockers.Select(Generalise));
                }

                if (offer.AccountItems.Count == 0 && offer.Blockers.Count == 0)
                {
                    offer.Blockers.Add("no account items");
                    offer.Level = AccountSupportLevel.Unsupported;
                }

                foreach (string price in parts[6].Split(',', StringSplitOptions.RemoveEmptyEntries))
                {
                    string[] p = price.Split(':');
                    offer.Prices[uint.Parse(p[0])] = float.Parse(p[1], System.Globalization.CultureInfo.InvariantCulture);
                }

                if (offer.Prices.Count == 0)
                {
                    offer.Blockers.Add("no price");
                    offer.Level = AccountSupportLevel.Unsupported;
                }

                offer.Blockers = offer.Blockers.Distinct().ToList();
                results.Add(offer);
            }

            return results;
        }

        private static void WriteStore(StringBuilder report, List<OfferResult> offers)
        {
            List<OfferResult> visible = offers.Where(o => o.Visible).ToList();

            report.AppendLine("## Store offers");
            report.AppendLine();
            report.AppendLine($"{offers.Count} offers, {visible.Count} visible. Purchase is supported for every priced offer, the level below is for claiming and using what it grants.");
            report.AppendLine();
            report.AppendLine("| Set | Supported | Partial | Unsupported |");
            report.AppendLine("|---|---|---|---|");
            WriteLevels(report, "visible offers", visible.Select(o => o.Level));
            report.AppendLine();

            report.AppendLine("| Blocker | Visible offers |");
            report.AppendLine("|---|---|");
            foreach (var group in visible.SelectMany(o => o.Blockers.Select(b => (b, o))).GroupBy(x => x.b).OrderByDescending(g => g.Count()))
                report.AppendLine($"| {group.Key} | {group.Count()} |");
            report.AppendLine();

            report.AppendLine("<details><summary>Visible offers that aren't fully supported</summary>");
            report.AppendLine();
            report.AppendLine("| Offer | Name | Level | Blockers |");
            report.AppendLine("|---|---|---|---|");
            foreach (OfferResult offer in visible.Where(o => o.Level != AccountSupportLevel.Supported).OrderBy(o => o.Id))
                report.AppendLine($"| {offer.Id} | {offer.Name} | {offer.Level} | {string.Join("; ", offer.Blockers)} |");
            report.AppendLine();
            report.AppendLine("</details>");
            report.AppendLine();
        }

        #endregion

        #region Entitlements, reward properties, unlocks and currencies

        private static void WriteEntitlements(StringBuilder report, List<AccountItemResult> results)
        {
            report.AppendLine("## Entitlements");
            report.AppendLine();
            report.AppendLine("| Id | Name | Max | Flags | Account items | Effect | Level |");
            report.AppendLine("|---|---|---|---|---|---|---|");
            foreach (EntitlementEntry entitlement in entitlements.Entries.Where(e => e != null))
            {
                int count = results.Count(r => r.Entry.EntitlementId == entitlement.Id);
                (AccountSupportLevel level, string how) = GetEntitlementEffect(entitlement);
                var extras = new List<string> { how };
                if (entitlement.Spell4IdPersistentBuff != 0u)
                    extras.Add($"buff {entitlement.Spell4IdPersistentBuff}: {AccountFeatureSupport.ClaimFeatures["EntitlementBuff"].Level}");
                if (entitlement.CharacterTitleId != 0u)
                    extras.Add($"title {entitlement.CharacterTitleId}: {AccountFeatureSupport.ClaimFeatures["EntitlementTitle"].Level}");
                string name = text.GetEntry(entitlement.LocalizedTextIdName) ?? "";
                report.AppendLine($"| {entitlement.Id} | {name} | {entitlement.MaxCount} | {(EntitlementFlags)entitlement.Flags} | {count} | {string.Join("; ", extras)} | {level} |");
            }
            report.AppendLine();
        }

        private static void WriteRewardProperties(StringBuilder report, GameTable<RewardPropertyEntry> rewardProperties)
        {
            report.AppendLine("## Reward properties");
            report.AppendLine();
            report.AppendLine("| Id | Property | Premium modifiers | Entitlements | Server use | Level |");
            report.AppendLine("|---|---|---|---|---|---|");
            foreach (RewardPropertyEntry entry in rewardProperties.Entries.Where(e => e != null))
            {
                var type = (RewardPropertyType)entry.Id;
                List<RewardPropertyPremiumModifierEntry> mods = modifiers.Entries.Where(m => m != null && m.RewardPropertyId == entry.Id).ToList();
                string linked = string.Join(",", mods.Where(m => m.EntitlementIdModifierCount != 0u).Select(m => m.EntitlementIdModifierCount).Distinct());
                string hook = AccountFeatureSupport.RewardProperties.TryGetValue(type, out var support) ? support.Hook : "";
                report.AppendLine($"| {entry.Id} | {type} | {mods.Count} | {linked} | {hook} | {AccountFeatureSupport.GetRewardPropertySupport(type)} |");
            }
            report.AppendLine();
        }

        private static void WriteGenericUnlocks(StringBuilder report, List<AccountItemResult> results)
        {
            report.AppendLine("## Generic unlocks");
            report.AppendLine();
            int sets = results.Count(r => r.Entry.GenericUnlockSetId != 0u);
            int itemSets = items.Entries.Count(i => i != null && i.GenericUnlockSetId != 0u);
            report.AppendLine($"{sets} account items and {itemSets} items grant generic unlock sets.");
            report.AppendLine();
            report.AppendLine("| Type | Entries | Level |");
            report.AppendLine("|---|---|---|");
            foreach (var group in unlockEntries.Entries.Where(e => e != null).GroupBy(e => e.GenericUnlockTypeEnum))
                report.AppendLine($"| {group.Key} | {group.Count()} | {(group.Key == GenericUnlockType.Dye ? "Supported" : "Unsupported")} |");
            report.AppendLine();
        }

        private static void WriteCurrencies(StringBuilder report, GameTable<AccountCurrencyTypeEntry> currencies, List<AccountItemResult> results, List<OfferResult> offers)
        {
            report.AppendLine("## Account currencies");
            report.AppendLine();
            report.AppendLine("| Id | Currency | Account items granting | Visible store prices | Server use | Level |");
            report.AppendLine("|---|---|---|---|---|---|");
            foreach (AccountCurrencyTypeEntry entry in currencies.Entries.Where(e => e != null))
            {
                var type = (AccountCurrencyType)entry.Id;
                int granting = results.Count(r => r.Entry.AccountCurrencyEnum == entry.Id);
                int prices = offers.Count(o => o.Visible && o.Prices.ContainsKey(entry.Id));
                string hook = AccountFeatureSupport.Currencies.TryGetValue(type, out var support) ? support.Hook : "";
                report.AppendLine($"| {entry.Id} | {text.GetEntry(entry.LocalizedTextId)} | {granting} | {prices} | {hook} | {AccountFeatureSupport.GetCurrencySupport(type)} |");
            }
            report.AppendLine();
        }

        #endregion

        #region Packets

        private static readonly Regex accountPacket = new(
            "Account|Store|Entitlement|Credd|DailyLogin|Omnibit|Premium|GenericUnlock|Lockbox|Fortune|CostumeItem|ServiceToken|CharacterRename|RealmTransfer|Gacha",
            RegexOptions.Compiled);

        private static void WritePackets(StringBuilder report, string src)
        {
            string[] files = Directory.GetFiles(src, "*.cs", SearchOption.AllDirectories)
                .Where(f => !f.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}") && !f.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}"))
                .ToArray();
            string all = string.Join("\n", files.Select(File.ReadAllText));

            // message models in NexusForever.Network.World
            var models = typeof(NexusForever.Network.World.Message.Model.ServerStoreFinalise).Assembly.GetTypes()
                .Select(t => (Type: t, Attribute: t.GetCustomAttribute<MessageAttribute>()))
                .Where(x => x.Attribute != null)
                .GroupBy(x => x.Attribute.Opcode)
                .ToDictionary(g => g.Key, g => g.First().Type.Name);

            report.AppendLine("## Account feature packets");
            report.AppendLine();
            report.AppendLine("Client packets need a model and a handler, server packets need a model that something sends.");
            report.AppendLine();
            report.AppendLine("| Opcode | Packet | Model | Handled / sent | Level |");
            report.AppendLine("|---|---|---|---|---|");
            foreach (GameMessageOpcode opcode in Enum.GetValues<GameMessageOpcode>().Distinct().Where(o => accountPacket.IsMatch(o.ToString()) && !o.ToString().Contains("Friendship")))
            {
                string name = opcode.ToString();
                bool client = name.StartsWith("Client");
                string model = models.GetValueOrDefault(opcode);
                bool used = model != null && (client
                    ? Regex.IsMatch(all, $@"IMessageHandler<\s*IWorldSession\s*,\s*{model}\s*>")
                    : Regex.IsMatch(all, $@"new\s+{model}\b"));
                string level = model == null ? "Unsupported" : used ? "Supported" : "Partial";
                report.AppendLine($"| 0x{(uint)opcode:X4} | {name} | {(model != null ? "yes" : "no")} | {(used ? "yes" : "no")} | {level} |");
            }
            report.AppendLine();
        }

        #endregion

        private static void WriteLevels(StringBuilder report, string set, IEnumerable<AccountSupportLevel> levels)
        {
            List<AccountSupportLevel> list = levels.ToList();
            string Pct(AccountSupportLevel level) => list.Count == 0 ? "0" : $"{list.Count(l => l == level)} ({100d * list.Count(l => l == level) / list.Count:0}%)";
            report.AppendLine($"| {set} ({list.Count}) | {Pct(AccountSupportLevel.Supported)} | {Pct(AccountSupportLevel.Partial)} | {Pct(AccountSupportLevel.Unsupported)} |");
        }

        private static void CollectPrerequisites(uint prerequisiteId, HashSet<PrerequisiteType> types, HashSet<uint> visited)
        {
            if (prerequisiteId == 0u || !visited.Add(prerequisiteId))
                return;

            PrerequisiteEntry entry = prerequisites.GetEntry(prerequisiteId);
            if (entry == null)
                return;

            for (int i = 0; i < entry.PrerequisiteTypeId.Length; i++)
            {
                PrerequisiteType type = entry.PrerequisiteTypeId[i];
                if (type == PrerequisiteType.None)
                    continue;

                types.Add(type);
                if (type == PrerequisiteType.OtherPrerequisite)
                    CollectPrerequisites(entry.ObjectId[i], types, visited);
            }
        }

        private static string GetArg(string[] args, string name)
        {
            int index = Array.IndexOf(args, name);
            return index >= 0 && index + 1 < args.Length ? args[index + 1] : null;
        }
    }
}
