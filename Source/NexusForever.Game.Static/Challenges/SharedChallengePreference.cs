namespace NexusForever.Game.Static.Challenges
{
    /// <summary>
    /// Replication of the GameLib.SharedChallengePreference enum from the client, sent with <see cref="Option.OptionType.SharedChallenge"/>.
    /// </summary>
    public enum SharedChallengePreference
    {
        AutoAccept = 0,
        Prompt     = 1,
        AutoReject = 2
    }
}
