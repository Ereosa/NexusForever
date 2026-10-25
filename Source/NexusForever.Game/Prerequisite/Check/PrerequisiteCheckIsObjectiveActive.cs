using Microsoft.Extensions.Logging;
using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Prerequisite;
using NexusForever.Game.Abstract.Quest;
using NexusForever.Game.Static.Prerequisite;

namespace NexusForever.Game.Prerequisite.Check
{
    /// <remarks>
    /// Object id is the quest, value is the objective index.
    /// Used by spell runners to swap abilities for quest specific versions while an objective is in progress (e.g. Crush on the tutorial turrets).
    /// </remarks>
    [PrerequisiteCheck(PrerequisiteType.IsObjectiveActive)]
    public class PrerequisiteCheckIsObjectiveActive : IPrerequisiteCheck
    {
        #region Dependency Injection

        private readonly ILogger<PrerequisiteCheckIsObjectiveActive> log;

        public PrerequisiteCheckIsObjectiveActive(
            ILogger<PrerequisiteCheckIsObjectiveActive> log)
        {
            this.log = log;
        }

        #endregion

        public bool Meets(IPlayer player, PrerequisiteComparison comparison, uint value, uint objectId, IPrerequisiteParameters parameters)
        {
            switch (comparison)
            {
                case PrerequisiteComparison.Equal:
                    return IsObjectiveActive(player, objectId, value);
                case PrerequisiteComparison.NotEqual:
                    return !IsObjectiveActive(player, objectId, value);
                default:
                    log.LogWarning($"Unhandled PrerequisiteComparison {comparison} for {PrerequisiteType.IsObjectiveActive}!");
                    return false;
            }
        }

        private static bool IsObjectiveActive(IPlayer player, uint questId, uint index)
        {
            IQuest quest = player.QuestManager.GetActiveQuest((ushort)questId);
            if (quest == null)
                return false;

            IQuestObjective objective = quest.GetQuestObjectiveByIndex((byte)index);
            if (objective == null || objective.IsComplete())
                return false;

            // sequential objectives only become active once every earlier objective is complete
            if (objective.ObjectiveInfo.IsSequential())
                for (byte i = 0; i < index; i++)
                    if (!(quest.GetQuestObjectiveByIndex(i)?.IsComplete() ?? true))
                        return false;

            return true;
        }
    }
}
