using Microsoft.Extensions.Logging;
using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Prerequisite;
using NexusForever.Game.Static.AccountInventory;
using NexusForever.Game.Static.Prerequisite;

namespace NexusForever.Game.Prerequisite.Check
{
    [PrerequisiteCheck(PrerequisiteType.LoyaltyRewards)]
    public class PrerequisiteCheckLoyaltyRewards : IPrerequisiteCheck
    {
        #region Dependency Injection

        private readonly ILogger<PrerequisiteCheckLoyaltyRewards> log;

        public PrerequisiteCheckLoyaltyRewards(
            ILogger<PrerequisiteCheckLoyaltyRewards> log)
        {
            this.log = log;
        }

        #endregion

        /// <remarks>
        /// Value is compared against the account's Cosmic Reward points, eg: "Insufficient Cosmic Reward Points".
        /// </remarks>
        public bool Meets(IPlayer player, PrerequisiteComparison comparison, uint value, uint objectId, IPrerequisiteParameters parameters)
        {
            bool? result = PrerequisiteCompare.Compare(comparison, player.Account.CurrencyManager.GetAmount(AccountCurrencyType.Loyalty), value);
            if (result == null)
                log.LogWarning($"Unhandled PrerequisiteComparison {comparison} for {PrerequisiteType.LoyaltyRewards}!");

            return result ?? false;
        }
    }
}
