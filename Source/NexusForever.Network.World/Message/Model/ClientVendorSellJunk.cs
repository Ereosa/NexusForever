using NexusForever.Network.Message;

namespace NexusForever.Network.World.Message.Model
{
    /// <summary>
    /// Sent by the Sell Junk button, the client writes no data (SellJunkToVendor).
    /// </summary>
    [Message(GameMessageOpcode.ClientSellJunkToVendor)]
    public class ClientVendorSellJunk : IReadable
    {
        public void Read(GamePacketReader reader)
        {
        }
    }
}
