using NexusForever.Network.Message;

namespace NexusForever.Network.World.Message.Model
{
    /// <summary>
    /// Unlock a single taxi node for the player.
    /// </summary>
    [Message(GameMessageOpcode.ServerFlightPathAdd)]
    public class ServerFlightPathAdd : IWritable
    {
        public ushort TaxiNodeId { get; set; }

        public void Write(GamePacketWriter writer)
        {
            writer.Write(TaxiNodeId, 14u);
        }
    }
}
