namespace NexusForever.Database.Character.Model
{
    public class CharacterTradeskillSchematicModel
    {
        public ulong Id { get; set; }
        public uint SchematicId { get; set; }
        public bool Discovered { get; set; }
        public float DiscoveryX { get; set; }
        public float DiscoveryY { get; set; }

        public virtual CharacterModel Character { get; set; }
    }
}
