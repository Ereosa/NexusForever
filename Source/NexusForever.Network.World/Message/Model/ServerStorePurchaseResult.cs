using NexusForever.Network.Message;

namespace NexusForever.Network.World.Message.Model
{
    /// <summary>
    /// Result of a storefront purchase, fires the client StorePurchaseOfferResult event which closes the purchase spinner.
    /// </summary>
    [Message(GameMessageOpcode.ServerStorePurchaseResult)]
    public class ServerStorePurchaseResult : IWritable
    {
        public bool Success { get; set; }
        public byte StoreError { get; set; }

        public void Write(GamePacketWriter writer)
        {
            writer.Write(Success);
            writer.Write(StoreError, 5u);
        }
    }
}
