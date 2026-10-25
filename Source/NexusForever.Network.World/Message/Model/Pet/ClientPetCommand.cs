using NexusForever.Game.Static.Spell;
using NexusForever.Network.Message;

namespace NexusForever.Network.World.Message.Model.Pet
{
    /// <summary>
    /// Sent when a pet command shortcut (type 12) is pressed on a shown action bar, the command applies to all of the player's pets.
    /// </summary>
    [Message(GameMessageOpcode.ClientPetCommand)]
    public class ClientPetCommand : IReadable
    {
        public ShortcutSet ShortcutSet { get; private set; }
        public uint Slot { get; private set; }

        public void Read(GamePacketReader reader)
        {
            ShortcutSet = reader.ReadEnum<ShortcutSet>(32u);
            Slot        = reader.ReadUInt();
        }
    }
}
