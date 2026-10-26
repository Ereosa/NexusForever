namespace NexusForever.Database.Character.Model
{
    public class CharacterChallengeModel
    {
        public ulong Id { get; set; }
        public uint ChallengeId { get; set; }
        public uint CompletionCount { get; set; }
        public uint LastRewardTier { get; set; }
        public uint CooldownRemaining { get; set; }

        public virtual CharacterModel Character { get; set; }
    }
}
