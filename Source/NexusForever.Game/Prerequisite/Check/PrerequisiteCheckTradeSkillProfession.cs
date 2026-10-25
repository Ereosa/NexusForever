using Microsoft.Extensions.Logging;
using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Prerequisite;
using NexusForever.Game.Static.Prerequisite;

namespace NexusForever.Game.Prerequisite.Check
{
    [PrerequisiteCheck(PrerequisiteType.TradeSkillProfession)]
    public class PrerequisiteCheckTradeSkillProfession : IPrerequisiteCheck
    {
        #region Dependency Injection

        private readonly ILogger<PrerequisiteCheckTradeSkillProfession> log;

        public PrerequisiteCheckTradeSkillProfession(
            ILogger<PrerequisiteCheckTradeSkillProfession> log)
        {
            this.log = log;
        }

        #endregion

        /// <remarks>
        /// Object id is a TradeskillTier id, the value compared is the progress through that tier (see ITradeskillManager.GetTierProgress).
        /// </remarks>
        public bool Meets(IPlayer player, PrerequisiteComparison comparison, uint value, uint objectId, IPrerequisiteParameters parameters)
        {
            uint progress = player.TradeskillManager.GetTierProgress(objectId);
            switch (comparison)
            {
                case PrerequisiteComparison.Equal:
                    return progress == value;
                case PrerequisiteComparison.NotEqual:
                    return progress != value;
                case PrerequisiteComparison.GreaterThan:
                    return progress > value;
                case PrerequisiteComparison.GreaterThanOrEqual:
                    return progress >= value;
                case PrerequisiteComparison.LessThan:
                    return progress < value;
                case PrerequisiteComparison.LessThanOrEqual:
                    return progress <= value;
                default:
                    log.LogWarning($"Unhandled PrerequisiteComparison {comparison} for {PrerequisiteType.TradeSkillProfession}!");
                    return false;
            }
        }
    }
}
