namespace NexusForever.Game.Static.Challenges
{
    /// <summary>
    /// Flags from the ChallengeFlags column of the Challenge game table.
    /// </summary>
    [Flags]
    public enum ChallengeFlags
    {
        None              = 0x0000,

        /// <remarks>
        /// Inferred: set on 146 of 214 General and 44 of 47 Ability challenges (the ones that need a start location or grant a T-spell)
        /// and rarely on Combat challenges, these only start from the challenge log or a challenge starter object.
        /// </remarks>
        ManualStart       = 0x0004,

        /// <summary>
        /// Tier counts are milliseconds left on the timer when the challenge is finished.
        /// </summary>
        /// <remarks>
        /// Client Challenge:IsTimeTiered reads this bit, tier goals are shown as Duration - count.
        /// </remarks>
        TimeTiered        = 0x0008,

        /// <remarks>
        /// Client: Combat challenges without either of these and with a goal over 1 show their progress as a percentage.
        /// </remarks>
        NoPercentDisplay1 = 0x0040,
        NoPercentDisplay2 = 0x0100
    }
}
