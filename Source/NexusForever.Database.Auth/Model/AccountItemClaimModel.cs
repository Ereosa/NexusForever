namespace NexusForever.Database.Auth.Model
{
    public class AccountItemClaimModel
    {
        public uint Id { get; set; }
        public ulong ItemId { get; set; }
        public ulong CharacterId { get; set; }

        public AccountModel Account { get; set; }
    }
}
