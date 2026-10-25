using System;

namespace NexusForever.Database.Auth.Model
{
    public class AccountDailyLoginModel
    {
        public uint Id { get; set; }
        public uint LoginDays { get; set; }
        public uint RewardsClaimed { get; set; }
        public DateTime? LastLoginDay { get; set; }
        public DateTime? LastLockboxKey { get; set; }

        public AccountModel Account { get; set; }
    }
}
