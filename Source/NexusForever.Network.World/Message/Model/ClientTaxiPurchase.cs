using NexusForever.Network.Message;

namespace NexusForever.Network.World.Message.Model
{
    /// <summary>
    /// Sent when a flight path is bought from a taxi vendor ("PurchaseFlightPath") or a shuttle is taken ("TakeShuttle").
    /// </summary>
    /// <remarks>
    /// The client works out the cheapest route from the vendor's taxi node and sends the TaxiRoute ids in order,
    /// a shuttle is always a single route.
    /// </remarks>
    [Message(GameMessageOpcode.ClientTaxiPurchase)]
    public class ClientTaxiPurchase : IReadable
    {
        public List<uint> TaxiRoutes { get; } = new();

        public void Read(GamePacketReader reader)
        {
            uint count = reader.ReadUInt();
            for (uint i = 0u; i < count && i < 64u; i++)
                TaxiRoutes.Add(reader.ReadUInt());
        }
    }
}
