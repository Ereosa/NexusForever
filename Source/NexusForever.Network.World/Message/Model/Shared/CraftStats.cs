using NexusForever.Network.Message;

namespace NexusForever.Network.World.Message.Model.Shared
{
    /// <summary>
    /// Packed circuit crafting data, the same format as the item random circuit data.
    /// </summary>
    /// <remarks>
    /// Bytes 0 to 5 are the property plus one for each item stat slot, byte 6 is the assault and support power split and byte 7 is the format version.
    /// </remarks>
    public class CraftStats : IReadable, IWritable
    {
        public ulong Data { get; set; }

        /// <summary>
        /// Return the property plus one for the item stat slot, 0 if the slot has no property.
        /// </summary>
        public byte GetSlot(int slot)
        {
            return (byte)(Data >> (slot * 8));
        }

        public void Read(GamePacketReader reader)
        {
            Data = reader.ReadULong();
        }

        public void Write(GamePacketWriter writer)
        {
            writer.Write(Data);
        }
    }
}
