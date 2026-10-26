using NexusForever.Network.Message;

namespace NexusForever.Network.World.Message.Model
{
    /// <summary>
    /// Remove all unlocked taxi nodes for the player.
    /// </summary>
    [Message(GameMessageOpcode.ServerFlightPathClear)]
    public class ServerFlightPathClear : IWritable
    {
        public void Write(GamePacketWriter writer)
        {
        }
    }
}
