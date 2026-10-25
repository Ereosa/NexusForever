using NexusForever.Game.Static.AccountInventory;
using NexusForever.Network.Message;
using NexusForever.Network.World.Message.Model.Shared;

namespace NexusForever.Network.World.Message.Model
{
    [Message(GameMessageOpcode.ClientStorefrontPurchaseCharacter)]
    public class ClientStorefrontPurchaseCharacter : IReadable
    {
        public uint OfferId { get; set; } // OfferId
        public byte Unknown1 { get; set; }
        public byte Unknown2 { get; set; }
        public AccountCurrencyType CurrencyId { get; set; }
        public uint Unknown3 { get; set; }
        public Identity Target { get; set; } = new();
        public uint Unknown6 { get; set; }

        public void Read(GamePacketReader reader)
        {
            OfferId    = reader.ReadUInt(20);
            Unknown1   = reader.ReadByte(5);
            Unknown2   = reader.ReadByte(7);
            CurrencyId = reader.ReadEnum<AccountCurrencyType>(7u);
            Unknown3   = reader.ReadUInt(20);
            Target.Read(reader);
            Unknown6   = reader.ReadUInt(20);
        }
    }
}
