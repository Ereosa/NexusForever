using NexusForever.Network.Message;

namespace NexusForever.Network.World.Message.Model.Spell
{
    // Sent when the sprint key is pressed or released.
    [Message(GameMessageOpcode.ClientSetSprintState)]
    public class ClientSetSprintState : IReadable
    {
        public bool Sprint { get; private set; }

        public void Read(GamePacketReader reader)
        {
            Sprint = reader.ReadBit();
        }
    }
}
