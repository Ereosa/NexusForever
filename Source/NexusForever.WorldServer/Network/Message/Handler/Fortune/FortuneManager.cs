using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using NexusForever.Game.Static.AccountInventory;
using NexusForever.Game.Static.Fortune;
using NexusForever.GameTable;
using NexusForever.GameTable.Model;
using NexusForever.Network.World.Message.Model.Fortune;

namespace NexusForever.WorldServer.Network.Message.Handler.Fortune
{
    /// <summary>
    /// Madame Fay's Fortunes: a Fortune Coin deals three cards, each card is a guaranteed account item reward.
    /// </summary>
    /// <remarks>
    /// The reward pool is the Protostar Strong Box set, account items 131-164 which the client data flags as fortune rewards.
    /// Unflipped cards are kept per account so a game can be resumed after closing the window or relogging.
    /// </remarks>
    public static class FortuneManager
    {
        private const uint PoolFirstId = 131u;
        private const uint PoolLastId  = 164u;

        public class Deal
        {
            public uint[] AccountItemId { get; } = new uint[3];
            public RewardRarity[] Rarity { get; } = new RewardRarity[3];
            public bool[] Flipped { get; } = new bool[3];
        }

        private record Reward(uint AccountItemId, uint Item2Id, RewardRarity Rarity, uint Weight);

        private static readonly Lazy<List<Reward>> pool = new(BuildPool);
        private static readonly ConcurrentDictionary<uint, Deal> deals = new();

        private static List<Reward> BuildPool()
        {
            var rewards = new List<Reward>();
            for (uint id = PoolFirstId; id <= PoolLastId; id++)
            {
                AccountItemEntry entry = GameTableManager.Instance.AccountItem.GetEntry(id);
                if (entry == null || entry.Item2Id == 0u || entry.Flags != 1u)
                    continue;

                Item2Entry item = GameTableManager.Instance.Item.GetEntry(entry.Item2Id);
                if (item == null)
                    continue;

                // card aura follows item quality: green, blue, purple, orange
                RewardRarity rarity = (RewardRarity)Math.Clamp((int)item.ItemQualityId - 3, 0, 3);
                uint weight = rarity switch
                {
                    RewardRarity.Normal => 100u,
                    RewardRarity.Rare   => 60u,
                    RewardRarity.Epic   => 20u,
                    _                   => 5u
                };
                rewards.Add(new Reward(id, entry.Item2Id, rarity, weight));
            }
            return rewards;
        }

        public static ServerFortuneRewards BuildRewardList()
        {
            return new ServerFortuneRewards
            {
                Item2IdRewards = pool.Value.Select(r => r.Item2Id).ToList()
            };
        }

        public static Deal GetDeal(uint accountId)
        {
            return deals.TryGetValue(accountId, out Deal deal) ? deal : null;
        }

        public static Deal NewDeal(uint accountId)
        {
            var deal = new Deal();
            uint total = (uint)pool.Value.Sum(r => r.Weight);
            for (int i = 0; i < 3; i++)
            {
                uint roll = (uint)Random.Shared.Next((int)total);
                foreach (Reward reward in pool.Value)
                {
                    if (roll < reward.Weight)
                    {
                        deal.AccountItemId[i] = reward.AccountItemId;
                        deal.Rarity[i]        = reward.Rarity;
                        break;
                    }
                    roll -= reward.Weight;
                }
            }

            deals[accountId] = deal;
            return deal;
        }

        public static void Complete(uint accountId)
        {
            deals.TryRemove(accountId, out _);
        }

        public static ServerFortuneCards BuildCards(Deal deal)
        {
            var cards = new ServerFortuneCards
            {
                Operation = deal != null ? FortuneOperation.Update : FortuneOperation.Reset
            };
            if (deal != null)
            {
                for (int i = 0; i < 3; i++)
                {
                    cards.AccountItemId[i] = deal.AccountItemId[i];
                    cards.Rarity[i]        = deal.Rarity[i];
                    cards.CardFlipped[i]   = deal.Flipped[i];
                }
            }
            return cards;
        }

        public static bool HasFortuneCoin(IWorldSession session)
        {
            return session.Account.CurrencyManager.CanAfford(AccountCurrencyType.MysticShiny, 1ul);
        }
    }
}
