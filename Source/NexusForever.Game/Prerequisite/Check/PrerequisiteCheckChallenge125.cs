using Microsoft.Extensions.Logging;
using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Prerequisite;
using NexusForever.Game.Static.Prerequisite;

namespace NexusForever.Game.Prerequisite.Check
{
    /// <summary>
    /// Compares 1 while the challenge with ObjectId is running, 0 otherwise.
    /// </summary>
    /// <remarks>
    /// Inferred from the objects that use it with NotEqual 0: the Grimvoid Speeder of the Owanee Speedway race, the supply crates of Supply Run and the Seaspine Mollusks of Pilfering Pearls are used during their challenge.
    /// </remarks>
    [PrerequisiteCheck(PrerequisiteType.Challenge125)]
    public class PrerequisiteCheckChallenge125 : IPrerequisiteCheck
    {
        #region Dependency Injection

        private readonly ILogger<PrerequisiteCheckChallenge125> log;

        public PrerequisiteCheckChallenge125(
            ILogger<PrerequisiteCheckChallenge125> log)
        {
            this.log = log;
        }

        #endregion

        public bool Meets(IPlayer player, PrerequisiteComparison comparison, uint value, uint objectId, IPrerequisiteParameters parameters)
        {
            uint state = player.ChallengeManager.IsActive(objectId) ? 1u : 0u;
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
                    log.LogWarning($"Unhandled PrerequisiteComparison {comparison} for {PrerequisiteType.Challenge125}!");
                    return false;
            }
        }
    }
}
