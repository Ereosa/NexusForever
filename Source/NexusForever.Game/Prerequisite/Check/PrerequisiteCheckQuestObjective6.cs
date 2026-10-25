using Microsoft.Extensions.Logging;
using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Prerequisite;
using NexusForever.Game.Abstract.Quest;
using NexusForever.Game.Static.Prerequisite;
using NexusForever.Game.Static.Quest;

namespace NexusForever.Game.Prerequisite.Check
{
    /// <remarks>
    /// Same layout as <see cref="PrerequisiteType.QuestObjective"/>, object id is the quest and value the objective index.
    /// </remarks>
    [PrerequisiteCheck(PrerequisiteType.QuestObjective6)]
    public class PrerequisiteCheckQuestObjective6 : IPrerequisiteCheck
    {
        #region Dependency Injection

        private readonly ILogger<PrerequisiteCheckQuestObjective6> log;

        public PrerequisiteCheckQuestObjective6(
            ILogger<PrerequisiteCheckQuestObjective6> log)
        {
            this.log = log;
        }

        #endregion

        public bool Meets(IPlayer player, PrerequisiteComparison comparison, uint value, uint objectId, IPrerequisiteParameters parameters)
        {
            switch (comparison)
            {
                case PrerequisiteComparison.Equal:
                    return PrerequisiteCheckQuestObjective.IsObjectiveComplete(player, objectId, value);
                case PrerequisiteComparison.NotEqual:
                    return !PrerequisiteCheckQuestObjective.IsObjectiveComplete(player, objectId, value);
                default:
                    log.LogWarning($"Unhandled PrerequisiteComparison {comparison} for {PrerequisiteType.QuestObjective6}!");
                    return false;
            }
        }
    }
}
