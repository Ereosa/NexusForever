using Microsoft.Extensions.Logging;
using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Prerequisite;
using NexusForever.Game.Static.Prerequisite;

namespace NexusForever.Game.Prerequisite.Check
{
    /// <summary>
    /// Compares the current count of the challenge with ObjectId.
    /// </summary>
    /// <remarks>
    /// Prerequisite 13017 has the failure text "Ellevar - Ch515 isActive and >4" for ChallengeRequirement Equal 0 and this GreaterThan 4.
    /// </remarks>
    [PrerequisiteCheck(PrerequisiteType.Challenge160)]
    public class PrerequisiteCheckChallenge160 : IPrerequisiteCheck
    {
        #region Dependency Injection

        private readonly ILogger<PrerequisiteCheckChallenge160> log;

        public PrerequisiteCheckChallenge160(
            ILogger<PrerequisiteCheckChallenge160> log)
        {
            this.log = log;
        }

        #endregion

        public bool Meets(IPlayer player, PrerequisiteComparison comparison, uint value, uint objectId, IPrerequisiteParameters parameters)
        {
            uint state = player.ChallengeManager.GetCurrentCount(objectId);
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
                    log.LogWarning($"Unhandled PrerequisiteComparison {comparison} for {PrerequisiteType.Challenge160}!");
                    return false;
            }
        }
    }
}
