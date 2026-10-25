using System;

namespace NexusForever.Database.Auth.Model
{
    public class AccountItemModel
    {
        public uint Id { get; set; }
        public ulong ItemId { get; set; }
        public uint AccountItemId { get; set; }
        public DateTime CreateTime { get; set; }

        public AccountModel Account { get; set; }
    }
}
