using System.Collections.Immutable;
using NexusForever.Game.Abstract.Quest;
using NexusForever.Game.Static.Prerequisite;
using NexusForever.Game.Static.Quest;
using NexusForever.GameTable;
using NexusForever.GameTable.Model;

namespace NexusForever.Game.Quest
{
    public class QuestInfo : IQuestInfo
    {
        public Quest2Entry Entry { get; }
        public Quest2DifficultyEntry DifficultyEntry { get; }

        public ImmutableList<Quest2Entry> PrerequisiteQuests { get; private set; }
        public ImmutableList<IQuestObjectiveInfo> Objectives { get; private set; }
        public ImmutableDictionary<uint, Quest2RewardEntry> Rewards { get; private set; }

        public bool IsQuestMentioned => GlobalQuestManager.Instance.GetQuestCommunicatorMessages((ushort)Entry.Id).ToList().Count > 0u;

        /// <summary>
        /// Create a new <see cref="IQuestInfo"/> using supplied <see cref="Quest2Entry"/>.
        /// </summary>
        public QuestInfo(Quest2Entry entry)
        {
            Entry           = entry;
            DifficultyEntry = GameTableManager.Instance.Quest2Difficulty.GetEntry(Entry.Quest2DifficultyId);

            InitialisePrerequisiteQuests();
            InitialiseObjectives();
            InitialiseRewards();
        }

        private void InitialisePrerequisiteQuests()
        {
            ImmutableList<Quest2Entry>.Builder builder = ImmutableList.CreateBuilder<Quest2Entry>();
            foreach (uint questId in Entry.PrerequisiteQuests.Where(q => q != 0u))
                builder.Add(GameTableManager.Instance.Quest2.GetEntry(questId));

            PrerequisiteQuests = builder.ToImmutable();
        }

        private void InitialiseObjectives()
        {
            ImmutableList<IQuestObjectiveInfo>.Builder builder = ImmutableList.CreateBuilder<IQuestObjectiveInfo>();
            foreach (uint objectiveId in Entry.Objectives.Where(o => o != 0u))
                builder.Add(new QuestObjectiveInfo(GameTableManager.Instance.QuestObjective.GetEntry(objectiveId)));

            Objectives = builder.ToImmutable();
        }

        private void InitialiseRewards()
        {
            ImmutableDictionary<uint, Quest2RewardEntry>.Builder builder = ImmutableDictionary.CreateBuilder<uint, Quest2RewardEntry>();
            foreach (Quest2RewardEntry rewardEntry in GameTableManager.Instance.Quest2Reward.Entries
                .Where(e => e.Quest2Id == Entry.Id))
                builder.Add(rewardEntry.Id, rewardEntry);

            Rewards = builder.ToImmutable();
        }

        private uint? workOrderTradeskillTierId;

        /// <summary>
        /// Return the TradeskillTier a work order (objective type 31) asks to craft for, from the quest's TradeSkillProfession prerequisite.
        /// </summary>
        /// <remarks>
        /// The objective data points at a table missing from this client build (one row per work order), the quest's tradeskill tier is the
        /// closest data: crafting any schematic of that tradeskill and tier fills the order (inferred).
        /// </remarks>
        public uint GetWorkOrderTradeskillTierId()
        {
            workOrderTradeskillTierId ??= FindTradeskillTier(Entry.PrerequisiteId, 0);
            return workOrderTradeskillTierId.Value;
        }

        private static uint FindTradeskillTier(uint prerequisiteId, int depth)
        {
            PrerequisiteEntry prerequisite = GameTableManager.Instance.Prerequisite.GetEntry(prerequisiteId);
            if (prerequisite == null || depth > 4)
                return 0u;

            for (int i = 0; i < prerequisite.PrerequisiteTypeId.Length; i++)
            {
                if (prerequisite.PrerequisiteTypeId[i] == PrerequisiteType.TradeSkillProfession)
                    return prerequisite.ObjectId[i];
                if (prerequisite.PrerequisiteTypeId[i] == PrerequisiteType.OtherPrerequisite)
                {
                    uint tier = FindTradeskillTier(prerequisite.ObjectId[i], depth + 1);
                    if (tier != 0u)
                        return tier;
                }
            }

            return 0u;
        }

        public bool IsAutoComplete()
        {
            return ((QuestFlags)Entry.Flags & QuestFlags.AutoComplete) != 0;
        }

        /// <summary>
        /// Returns if the quest can be accepted and completed via the communicator.
        /// </summary>
        public bool IsCommunicatorReceived()
        {
            return (Entry.Flags & 0x08) != 0 ;
        }

        public bool CanBeCalledBack()
        {
            return (Entry.Flags & 0x04) != 0;
        }

        public bool CannotAbandon()
        {
            return (Entry.Flags & 0x4000) != 0;
        }

        public bool CannotAbandonWhenAchieved()
        {
            return (Entry.Flags & 0x8000) != 0;
        }

        public bool IsContract()
        {
            return (Entry.Flags & 0x80000) != 0;
        }

        /// <summary>
        /// Return experience rewarded on completion.
        /// </summary>
        public uint GetRewardExperience()
        {
            if (Entry.RewardXpOverride != 0u)
                return Entry.RewardXpOverride;

            XpPerLevelEntry entry = GameTableManager.Instance.XpPerLevel.GetEntry(Entry.ConLevel);
            return (uint)(DifficultyEntry.XpMultiplier * entry.BaseQuestXpPerLevel);
        }

        /// <summary>
        /// Return money rewarded on completion.
        /// </summary>
        public uint GetRewardMoney()
        {
            if (Entry.RewardCashOverride != 0u)
                return Entry.RewardCashOverride;

            GameFormulaEntry entry = GameTableManager.Instance.GameFormula.GetEntry(530);
            return (uint)(MathF.Pow(Entry.ConLevel, entry.Datafloat0) * DifficultyEntry.CashRewardMultiplier);
        }

        /// <summary>
        /// Return the factions and reputation rewarded on completion.
        /// </summary>
        /// <remarks>
        /// Like experience the amount is the quest level's base reputation (XpPerLevel) scaled by the difficulty, unless overridden.
        /// Quests of difficulty 6 (multiplier 0) carry the base amount as their override (Survival of the Fittest, level 18: 226),
        /// which backs the formula. That a second faction without an override receives the same amount is a guess.
        /// </remarks>
        public IEnumerable<(uint FactionId, float Amount)> GetRewardReputation()
        {
            float amount = (GameTableManager.Instance.XpPerLevel.GetEntry(Entry.ConLevel)?.BaseRepRewardPerLevel ?? 0u)
                * (DifficultyEntry?.RepRewardMultiplier ?? 0f);

            if (Entry.Faction2IdRewardReputation00 != 0u)
            {
                float reward = Entry.RewardReputationOverride00 != 0f ? Entry.RewardReputationOverride00 : amount;
                if (reward > 0f)
                    yield return (Entry.Faction2IdRewardReputation00, reward);
            }
            if (Entry.Faction2IdRewardReputation01 != 0u)
            {
                float reward = Entry.RewardReputationOverride01 != 0f ? Entry.RewardReputationOverride01 : amount;
                if (reward > 0f)
                    yield return (Entry.Faction2IdRewardReputation01, reward);
            }
        }
    }
}
