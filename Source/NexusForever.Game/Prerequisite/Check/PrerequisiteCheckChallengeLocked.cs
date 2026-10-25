using Microsoft.Extensions.Logging;
using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Prerequisite;
using NexusForever.Game.Static.Prerequisite;

namespace NexusForever.Game.Prerequisite.Check
{
    /// <summary>
    /// Compares 1 if the challenge with ObjectId is locked, 0 once it is unlocked.
    /// </summary>
    /// <remarks>
    /// Inferred from the objects that use it: challenge starter objects (EMP Pistol for challenge 208, Experimental Jellibolli Catchers, Bron's Brew) are shown with NotEqual 0, and Beer Cans can be used when NotEqual 0 or while their challenge runs.
    /// </remarks>
    [PrerequisiteCheck(PrerequisiteType.ChallengeLocked)]
    public class PrerequisiteCheckChallengeLocked : IPrerequisiteCheck
    {
        #region Dependency Injection

        private readonly ILogger<PrerequisiteCheckChallengeLocked> log;

        public PrerequisiteCheckChallengeLocked(
            ILogger<PrerequisiteCheckChallengeLocked> log)
        {
            this.log = log;
        }

        #endregion

        public bool Meets(IPlayer player, PrerequisiteComparison comparison, uint value, uint objectId, IPrerequisiteParameters parameters)
        {
            uint state = player.ChallengeManager.IsUnlocked(objectId) ? 0u : 1u;
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
                    log.LogWarning($"Unhandled PrerequisiteComparison {comparison} for {PrerequisiteType.ChallengeLocked}!");
                    return false;
            }
        }
    }
}
