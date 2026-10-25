using Microsoft.Extensions.Logging;
using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Prerequisite;
using NexusForever.Game.Static.Prerequisite;

namespace NexusForever.Game.Prerequisite.Check
{
    /// <summary>
    /// Compares 0 while the challenge with ObjectId is running, 1 otherwise.
    /// </summary>
    /// <remarks>
    /// Inferred from the objects that use it: Equal 0 shows objects that belong to a running challenge (the destination aura of challenge 422, the Veggie Delight vegetables, Primal Totems) and Beer Cans can be used when their challenge is locked or Equal 0.
    /// </remarks>
    [PrerequisiteCheck(PrerequisiteType.ChallengeRequirement)]
    public class PrerequisiteCheckChallengeRequirement : IPrerequisiteCheck
    {
        #region Dependency Injection

        private readonly ILogger<PrerequisiteCheckChallengeRequirement> log;

        public PrerequisiteCheckChallengeRequirement(
            ILogger<PrerequisiteCheckChallengeRequirement> log)
        {
            this.log = log;
        }

        #endregion

        public bool Meets(IPlayer player, PrerequisiteComparison comparison, uint value, uint objectId, IPrerequisiteParameters parameters)
        {
            uint state = player.ChallengeManager.IsActive(objectId) ? 0u : 1u;
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
                    log.LogWarning($"Unhandled PrerequisiteComparison {comparison} for {PrerequisiteType.ChallengeRequirement}!");
                    return false;
            }
        }
    }
}
