using System;

namespace NexusForever.Database.Auth.Model
{
    public class AccountItemCooldownModel
    {
        public uint Id { get; set; }
        public uint CooldownGroupId { get; set; }
        public DateTime Expiry { get; set; }

        public AccountModel Account { get; set; }
    }
}
