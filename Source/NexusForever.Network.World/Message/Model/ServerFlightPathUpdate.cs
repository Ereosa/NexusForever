using NexusForever.Network.Message;

namespace NexusForever.Network.World.Message.Model
{
    /// <summary>
    /// Replace the unlocked taxi nodes of the player, the client ignores nodes of the other faction.
    /// </summary>
    [Message(GameMessageOpcode.ServerFlightPathUpdate)]
    public class ServerFlightPathUpdate : IWritable
    {
        public List<uint> TaxiNodes { get; set; } = new();

        public void Write(GamePacketWriter writer)
        {
            writer.Write((uint)TaxiNodes.Count);
            TaxiNodes.ForEach(n => writer.Write(n));
        }
    }
}
