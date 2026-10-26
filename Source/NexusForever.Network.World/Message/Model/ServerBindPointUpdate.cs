using NexusForever.Network.Message;

namespace NexusForever.Network.World.Message.Model
{
    /// <summary>
    /// Sets the player's bind point (the Transmat Terminal "Recall - Transmat" returns to).
    /// </summary>
    /// <remarks>
    /// The client stores the id for GameLib.HasBindPoint/GetBindPoint and the Transmat Terminal "current bind point" check.
    /// </remarks>
    [Message(GameMessageOpcode.ServerBindPointUpdate)]
    public class ServerBindPointUpdate : IWritable
    {
        public ushort BindPointId { get; set; }

        public void Write(GamePacketWriter writer)
        {
            writer.Write(BindPointId);
        }
    }
}
