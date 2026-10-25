namespace NexusForever.Database.Character.Model
{
    public class CharacterPathEpisodeModel
    {
        public ulong Id { get; set; }
        public ushort EpisodeId { get; set; }
        public byte RewardReceived { get; set; }

        public CharacterModel Character { get; set; }
    }
}
