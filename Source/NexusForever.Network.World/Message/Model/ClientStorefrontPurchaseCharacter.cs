using NexusForever.Game.Static.AccountInventory;
using NexusForever.Network.Message;
using NexusForever.Network.World.Message.Model.Shared;

namespace NexusForever.Network.World.Message.Model
{
    [Message(GameMessageOpcode.ClientStorefrontPurchaseCharacter)]
    public class ClientStorefrontPurchaseCharacter : IReadable
    {
        public uint OfferId { get; set; }
        public AccountCurrencyType CurrencyId { get; set; }
        public float Price { get; set; }
        public uint CategoryId { get; set; }
        public uint Count { get; set; }
        public Identity Target { get; set; } = new();
        public uint FriendId { get; set; }

        public void Read(GamePacketReader reader)
        {
            OfferId    = reader.ReadUInt();
            CurrencyId = reader.ReadEnum<AccountCurrencyType>(5u);
            Price      = reader.ReadSingle();
            CategoryId = reader.ReadUInt(14u);
            Count      = reader.ReadUInt();
            Target.Read(reader);
            FriendId   = reader.ReadUInt();
        }
    }
}
