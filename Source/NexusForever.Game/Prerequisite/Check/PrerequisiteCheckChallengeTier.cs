using Microsoft.Extensions.Logging;
using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Prerequisite;
using NexusForever.Game.Static.Prerequisite;

namespace NexusForever.Game.Prerequisite.Check
{
    /// <summary>
    /// Compares the tier reached by the running challenge with ObjectId, 0 to 3 for gold.
    /// </summary>
    /// <remarks>
    /// Inferred: objects of Save the Camp and Shifting Priorities stop being used once tier 2 is reached.
    /// </remarks>
    [PrerequisiteCheck(PrerequisiteType.ChallengeTier)]
    public class PrerequisiteCheckChallengeTier : IPrerequisiteCheck
    {
        #region Dependency Injection

        private readonly ILogger<PrerequisiteCheckChallengeTier> log;

        public PrerequisiteCheckChallengeTier(
            ILogger<PrerequisiteCheckChallengeTier> log)
        {
            this.log = log;
        }

        #endregion

        public bool Meets(IPlayer player, PrerequisiteComparison comparison, uint value, uint objectId, IPrerequisiteParameters parameters)
        {
            uint state = player.ChallengeManager.GetCurrentTier(objectId);
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
                    log.LogWarning($"Unhandled PrerequisiteComparison {comparison} for {PrerequisiteType.ChallengeTier}!");
                    return false;
            }
        }
    }
}
