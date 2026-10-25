using System.Numerics;
using NexusForever.Database.Character;
using NexusForever.GameTable.Model;

namespace NexusForever.Game.Abstract.Crafting
{
    public interface ITradeskillSchematic : IDatabaseCharacter
    {
        uint Id { get; }
        TradeskillSchematic2Entry Entry { get; }

        /// <summary>
        /// Schematic was learned by being discovered during a coordinate craft.
        /// </summary>
        bool Discovered { get; }

        /// <summary>
        /// Coordinates on the discovery panel where the schematic was discovered.
        /// </summary>
        Vector2 DiscoveryCoordinates { get; }
    }
}
