using System.Numerics;
using NexusForever.Database.Character;
using NexusForever.Database.Character.Model;
using NexusForever.Game.Abstract.Crafting;
using NexusForever.Game.Abstract.Entity;
using NexusForever.GameTable.Model;

namespace NexusForever.Game.Crafting
{
    public class TradeskillSchematic : ITradeskillSchematic
    {
        public uint Id => Entry.Id;
        public TradeskillSchematic2Entry Entry { get; }
        public bool Discovered { get; }
        public Vector2 DiscoveryCoordinates { get; }

        private bool isDirty;

        private readonly IPlayer player;

        /// <summary>
        /// Create a new <see cref="ITradeskillSchematic"/> from an existing database model.
        /// </summary>
        public TradeskillSchematic(IPlayer player, TradeskillSchematic2Entry entry, CharacterTradeskillSchematicModel model)
        {
            this.player          = player;
            Entry                = entry;
            Discovered           = model.Discovered;
            DiscoveryCoordinates = new Vector2(model.DiscoveryX, model.DiscoveryY);
        }

        /// <summary>
        /// Create a new <see cref="ITradeskillSchematic"/> from the supplied <see cref="TradeskillSchematic2Entry"/>.
        /// </summary>
        public TradeskillSchematic(IPlayer player, TradeskillSchematic2Entry entry, Vector2? discoveryCoordinates)
        {
            this.player          = player;
            Entry                = entry;
            Discovered           = discoveryCoordinates.HasValue;
            DiscoveryCoordinates = discoveryCoordinates ?? Vector2.Zero;
            isDirty              = true;
        }

        public void Save(CharacterContext context)
        {
            if (!isDirty)
                return;

            context.Add(new CharacterTradeskillSchematicModel
            {
                Id          = player.CharacterId,
                SchematicId = Id,
                Discovered  = Discovered,
                DiscoveryX  = DiscoveryCoordinates.X,
                DiscoveryY  = DiscoveryCoordinates.Y
            });

            isDirty = false;
        }
    }
}
