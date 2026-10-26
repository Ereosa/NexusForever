using NexusForever.Network.Message;
using NexusForever.Network.World.Message.Model.Shared;
using System.Numerics;

namespace NexusForever.Network.World.Message.Model.Crafting
{
    [Message(GameMessageOpcode.ServerCraftingCurrentCraft)]
    public class ServerCraftingCurrentCraft : IWritable
    {
        public uint TradeskillSchematic2Id { get; set; }
        public CraftStats CraftStats { get; set; } = new CraftStats();
        /// <summary>
        /// Element of each circuit crafting socket, the low byte is the format version followed by 3 bits for each socket with the element bit index plus one.
        /// </summary>
        public uint SocketGroups { get; set; }
        public uint SchematicCount { get; set; }
        public uint AdditiveCount { get; set; }
        /// <summary>
        /// Number of <see cref="Item2IdModifiers"/> in use, the client reads the additives from this count.
        /// </summary>
        public uint Item2IdModifierCount { get; set; }
        public uint[] Item2IdModifiers { get; set; } = new uint[5];
        public uint Item2Id { get; set; }
        public Vector2 DiscoveryCoordinates { get; set; }
        public Vector2 DiscoveryVectorMultiplier { get; set; }
        public float DiscoveryRadiusMultiplier { get; set; }

        public void Write(GamePacketWriter writer)
        {
            writer.Write(TradeskillSchematic2Id, 15);
            CraftStats.Write(writer);
            writer.Write(SocketGroups);
            writer.Write(SchematicCount);
            writer.Write(AdditiveCount);
            writer.Write(Item2IdModifierCount);
            foreach (var id in Item2IdModifiers)
            {
                writer.Write(id);
            }
            writer.Write(Item2Id, 18);
            writer.Write(DiscoveryCoordinates.X);
            writer.Write(DiscoveryCoordinates.Y);
            writer.Write(DiscoveryVectorMultiplier.X);
            writer.Write(DiscoveryVectorMultiplier.Y);
            writer.Write(DiscoveryRadiusMultiplier);
        }
    }
}
