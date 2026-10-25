using NexusForever.Network.Message;

namespace NexusForever.Network.World.Message.Model.Spell
{
    /// <summary>
    /// Sent on login and when the player.moveSpeed client option changes (the walk/run/sprint toggle).
    /// </summary>
    /// <remarks>
    /// 0 = walk, 1 = run with shift to sprint, 2 = sprint whenever out of combat (client default).
    /// </remarks>
    [Message(GameMessageOpcode.ClientPlayerMovementSpeedUpdate)]
    public class ClientPlayerMovementSpeedUpdate : IReadable
    {
        public uint MoveSpeed { get; private set; }

        public void Read(GamePacketReader reader)
        {
            MoveSpeed = reader.ReadUInt();
        }
    }
}
