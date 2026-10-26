namespace NexusForever.Database.Character.Model
{
    public class CharacterTaxiNodeModel
    {
        public ulong Id { get; set; }
        public ushort TaxiNodeId { get; set; }

        public virtual CharacterModel Character { get; set; }
    }
}
