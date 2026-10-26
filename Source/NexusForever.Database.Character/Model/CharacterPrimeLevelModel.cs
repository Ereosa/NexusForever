namespace NexusForever.Database.Character.Model
{
    public class CharacterPrimeLevelModel
    {
        public ulong Id { get; set; }
        public ushort WorldId { get; set; }
        public byte PrimeLevel { get; set; }

        public CharacterModel Character { get; set; }
    }
}
