using Microsoft.Extensions.Logging;
using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Prerequisite;
using NexusForever.Game.Abstract.Quest;
using NexusForever.Game.Static.Prerequisite;
using NexusForever.Game.Static.Quest;

namespace NexusForever.Game.Prerequisite.Check
{
    /// <remarks>
    /// Object id is the quest, value is the objective index, met when that objective is complete.
    /// Quest objects use it to only show or allow interaction after (or before) an objective is done, eg: "objective 0 complete, quest achieved or quest completed".
    /// </remarks>
    [PrerequisiteCheck(PrerequisiteType.QuestObjective)]
    public class PrerequisiteCheckQuestObjective : IPrerequisiteCheck
    {
        #region Dependency Injection

        private readonly ILogger<PrerequisiteCheckQuestObjective> log;

        public PrerequisiteCheckQuestObjective(
            ILogger<PrerequisiteCheckQuestObjective> log)
        {
            this.log = log;
        }

        #endregion

        public bool Meets(IPlayer player, PrerequisiteComparison comparison, uint value, uint objectId, IPrerequisiteParameters parameters)
        {
            switch (comparison)
            {
                case PrerequisiteComparison.Equal:
                    return IsObjectiveComplete(player, objectId, value);
                case PrerequisiteComparison.NotEqual:
                    return !IsObjectiveComplete(player, objectId, value);
                default:
                    log.LogWarning($"Unhandled PrerequisiteComparison {comparison} for {PrerequisiteType.QuestObjective}!");
                    return false;
            }
        }

        /// <summary>
        /// Returns if the objective index of the quest is complete, every objective of an achieved or completed quest is complete.
        /// </summary>
        public static bool IsObjectiveComplete(IPlayer player, uint questId, uint index)
        {
            QuestState? state = player.QuestManager.GetQuestState((ushort)questId);
            if (state is QuestState.Achieved or QuestState.Completed)
                return true;

            IQuest quest = player.QuestManager.GetActiveQuest((ushort)questId);
            IQuestObjective objective = quest?.GetQuestObjectiveByIndex((byte)index);
            return objective?.IsComplete() ?? false;
        }
    }
}
