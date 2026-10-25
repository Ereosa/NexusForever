using NexusForever.Network.Message;

namespace NexusForever.Network.World.Message.Model
{
    [Message(GameMessageOpcode.ServerResurrectionForced)]
    public class ServerResurrectionForced : IWritable
    {
        public void Write(GamePacketWriter writer)
        {
        }
    }
}
