using Microsoft.Extensions.Logging;
using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Prerequisite;
using NexusForever.Game.Static.Prerequisite;

namespace NexusForever.Game.Prerequisite.Check
{
    /// <summary>
    /// Compares 1 if the evaluated object has already been used by the running checklist challenge with ObjectId, 0 otherwise.
    /// </summary>
    /// <remarks>
    /// Inferred: used with ChallengeRequirement Equal 0 on the objects of ChecklistActivate challenges (Powering Down, Just Keep Swinging), each object only counts once.
    /// </remarks>
    [PrerequisiteCheck(PrerequisiteType.ChallengeObject)]
    public class PrerequisiteCheckChallengeObject : IPrerequisiteCheck
    {
        #region Dependency Injection

        private readonly ILogger<PrerequisiteCheckChallengeObject> log;

        public PrerequisiteCheckChallengeObject(
            ILogger<PrerequisiteCheckChallengeObject> log)
        {
            this.log = log;
        }

        #endregion

        public bool Meets(IPlayer player, PrerequisiteComparison comparison, uint value, uint objectId, IPrerequisiteParameters parameters)
        {
            uint state = parameters?.Target is IWorldEntity entity && player.ChallengeManager.IsChecklistObjectUsed(objectId, entity) ? 1u : 0u;
            switch (comparison)
            {
                case PrerequisiteComparison.Equal:
                    return state == value;
                case PrerequisiteComparison.NotEqual:
                    return state != value;
                case PrerequisiteComparison.GreaterThan:
                    return state > value;
                case PrerequisiteComparison.GreaterThanOrEqual:
                    return state >= value;
                case PrerequisiteComparison.LessThan:
                    return state < value;
                case PrerequisiteComparison.LessThanOrEqual:
                    return state <= value;
                default:
                    log.LogWarning($"Unhandled PrerequisiteComparison {comparison} for {PrerequisiteType.ChallengeObject}!");
                    return false;
            }
        }
    }
}
