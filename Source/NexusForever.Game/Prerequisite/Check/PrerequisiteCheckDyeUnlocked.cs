using Microsoft.Extensions.Logging;
using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Prerequisite;
using NexusForever.Game.Static.Prerequisite;

namespace NexusForever.Game.Prerequisite.Check
{
    [PrerequisiteCheck(PrerequisiteType.DyeUnlocked)]
    public class PrerequisiteCheckDyeUnlocked : IPrerequisiteCheck
    {
        #region Dependency Injection

        private readonly ILogger<PrerequisiteCheckDyeUnlocked> log;

        public PrerequisiteCheckDyeUnlocked(
            ILogger<PrerequisiteCheckDyeUnlocked> log)
        {
            this.log = log;
        }

        #endregion

        /// <remarks>
        /// Object is a DyeColorRamp, met when the account has (Equal) or hasn't (NotEqual) unlocked the dye, eg: "Dye already acquired!".
        /// </remarks>
        public bool Meets(IPlayer player, PrerequisiteComparison comparison, uint value, uint objectId, IPrerequisiteParameters parameters)
        {
            bool? result = PrerequisiteCompare.Compare(comparison, player.Account.GenericUnlockManager.IsDyeUnlocked(objectId));
            if (result == null)
                log.LogWarning($"Unhandled PrerequisiteComparison {comparison} for {PrerequisiteType.DyeUnlocked}!");

            return result ?? false;
        }
    }
}
