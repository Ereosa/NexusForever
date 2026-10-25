using Microsoft.EntityFrameworkCore.ChangeTracking;
using NexusForever.Database.Auth;
using NexusForever.Database.Auth.Model;
using NexusForever.Game.Abstract.Account;
using NexusForever.Game.Abstract.Account.DailyLogin;
using NexusForever.Game.Static;
using NexusForever.Game.Static.AccountInventory;
using NexusForever.GameTable;
using NexusForever.GameTable.Model;
using NexusForever.Network.World.Message.Model.AccountInventory;
using NLog;

namespace NexusForever.Game.Account.DailyLogin
{
    /// <summary>
    /// Daily login rewards: each day a character of the account logs in counts towards a 365 day reward calendar.
    /// </summary>
    public class AccountDailyLoginManager : IAccountDailyLoginManager
    {
        /// <summary>
        /// Account item for the daily premium lockbox key, it can only be taken while no key is in the character inventory.
        /// </summary>
        private const uint PremiumLockboxKeyAccountItemId = 2071u;
        private static readonly uint[] PremiumLockboxKeyAccountItemIds = [2071u, 2091u];

        /// <summary>
        /// Reward type for daily login rewards that grant an account item, the only type used in the client data.
        /// </summary>
        private const uint AccountItemRewardType = 0u;

        private static readonly ILogger log = LogManager.GetCurrentClassLogger();

        private uint loginDays;
        private uint rewardsClaimed;
        private DateTime? lastLoginDay;
        private DateTime? lastLockboxKey;

        private bool isNew;
        private bool isDirty;

        private readonly IAccount account;
        private readonly uint maxLoginDay;

        /// <summary>
        /// Create a new <see cref="IAccountDailyLoginManager"/> from existing <see cref="AccountModel"/> database model.
        /// </summary>
        public AccountDailyLoginManager(IAccount account, AccountModel model)
        {
            this.account = account;
            maxLoginDay  = GameTableManager.Instance.DailyLoginReward.Entries.Max(e => e.LoginDay);

            AccountDailyLoginModel dailyLogin = model.AccountDailyLogin.FirstOrDefault();
            if (dailyLogin != null)
            {
                loginDays      = dailyLogin.LoginDays;
                rewardsClaimed = dailyLogin.RewardsClaimed;
                lastLoginDay   = dailyLogin.LastLoginDay;
                lastLockboxKey = dailyLogin.LastLockboxKey;
            }
            else
                isNew = true;
        }

        public void Save(AuthContext context)
        {
            if (!isDirty)
                return;

            var model = new AccountDailyLoginModel
            {
                Id             = account.Id,
                LoginDays      = loginDays,
                RewardsClaimed = rewardsClaimed,
                LastLoginDay   = lastLoginDay,
                LastLockboxKey = lastLockboxKey
            };

            if (isNew)
                context.Add(model);
            else
            {
                EntityEntry<AccountDailyLoginModel> entity = context.Attach(model);
                entity.Property(p => p.LoginDays).IsModified      = true;
                entity.Property(p => p.RewardsClaimed).IsModified = true;
                entity.Property(p => p.LastLoginDay).IsModified   = true;
                entity.Property(p => p.LastLockboxKey).IsModified = true;
            }

            isNew   = false;
            isDirty = false;
        }

        /// <remarks>
        /// Days roll over at midnight UTC, the client counts the next day itself once the notify time sent in the update elapses.
        /// </remarks>
        private static DateTime Today => DateTime.UtcNow.Date;

        public void OnLogin()
        {
            UpdateLoginDay();
            SendDailyLoginUpdate();
        }

        private void UpdateLoginDay()
        {
            if (lastLoginDay.HasValue && lastLoginDay.Value.Date >= Today)
                return;

            lastLoginDay = Today;
            if (loginDays < maxLoginDay)
                loginDays++;

            isDirty = true;
            log.Trace($"Account {account.Id} reached daily login day {loginDays}.");
        }

        public void ClaimRewards()
        {
            UpdateLoginDay();

            for (uint day = rewardsClaimed + 1u; day <= loginDays; day++)
            {
                foreach (DailyLoginRewardEntry entry in GameTableManager.Instance.DailyLoginReward.Entries
                    .Where(e => e.LoginDay == day))
                {
                    if (entry.DailyLoginRewardTypeEnum != AccountItemRewardType
                        || GameTableManager.Instance.AccountItem.GetEntry(entry.RewardObjectValue) == null)
                    {
                        log.Warn($"Unhandled daily login reward {entry.Id} type {entry.DailyLoginRewardTypeEnum} for day {day}.");
                        continue;
                    }

                    // rewards go to the account inventory to be taken by a character
                    account.ItemManager.AddItem(entry.RewardObjectValue);
                }
            }

            if (rewardsClaimed != loginDays)
            {
                rewardsClaimed = loginDays;
                isDirty        = true;
            }

            // the claim button also claims the premium key when it is available
            if (GetPremiumLockboxKeyStatus() == PremiumLockboxKeyStatus.Available)
                GrantPremiumLockboxKey();

            SendDailyLoginUpdate();
        }

        public void ClaimPremiumLockboxKey()
        {
            UpdateLoginDay();

            if (GetPremiumLockboxKeyStatus() == PremiumLockboxKeyStatus.Available)
                GrantPremiumLockboxKey();

            SendDailyLoginUpdate();
        }

        private void GrantPremiumLockboxKey()
        {
            account.ItemManager.AddItem(PremiumLockboxKeyAccountItemId);
            lastLockboxKey = DateTime.UtcNow;
            isDirty        = true;
        }

        /// <remarks>
        /// Signature accounts receive one free premium lockbox key per day ("1 Free Lockbox Key per day").
        /// </remarks>
        private PremiumLockboxKeyStatus GetPremiumLockboxKeyStatus()
        {
            if (account.AccountTier != AccountTier.Signature)
                return PremiumLockboxKeyStatus.PremiumRequired;

            if (PremiumLockboxKeyAccountItemIds.Any(account.ItemManager.HasItem))
                return PremiumLockboxKeyStatus.InAccountInventory;

            if (lastLockboxKey.HasValue && lastLockboxKey.Value.Date >= Today)
                return PremiumLockboxKeyStatus.OnCooldown;

            return PremiumLockboxKeyStatus.Available;
        }

        public void SendDailyLoginUpdate()
        {
            TimeSpan untilNextDay = Today.AddDays(1d) - DateTime.UtcNow;
            PremiumLockboxKeyStatus keyStatus = GetPremiumLockboxKeyStatus();

            account.Session.EnqueueMessageEncrypted(new ServerDailyLoginUpdate
            {
                LoginDaysTotal            = loginDays,
                Unknown1                  = loginDays,
                CurrentLoginDays          = rewardsClaimed,
                NextDailyRewardNotifyTime = (uint)untilNextDay.TotalMilliseconds,
                NextLockboxKeyTimeInDays  = keyStatus == PremiumLockboxKeyStatus.OnCooldown ? (float)untilNextDay.TotalDays : 0f,
                PremiumKeyStatus          = keyStatus
            });
        }
    }
}
