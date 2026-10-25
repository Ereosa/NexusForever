namespace NexusForever.Database.Character.Model
{
    public class CharacterRewardTrackModel
    {
        public ulong Id { get; set; }
        public uint RewardTrackId { get; set; }
        public uint Points { get; set; }
        public uint ClaimedRewards { get; set; }
        public uint Iteration { get; set; }

        public virtual CharacterModel Character { get; set; }
    }
}
