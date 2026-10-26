using NexusForever.Database.Auth;

namespace NexusForever.Game.Abstract.Account.DailyLogin
{
    public interface IAccountDailyLoginManager : IDatabaseAuth
    {
        /// <summary>
        /// Invoked when a character of the account enters the world, counting the login day if it is a new day.
        /// </summary>
        void OnLogin();

        /// <summary>
        /// Claim every daily login reward earned since the last claim, and the premium lockbox key if it is available.
        /// </summary>
        void ClaimRewards();

        /// <summary>
        /// Claim the daily premium lockbox key for Signature accounts.
        /// </summary>
        void ClaimPremiumLockboxKey();

        void SendDailyLoginUpdate();
    }
}
