using NexusForever.Game.Abstract.Entity;
using NexusForever.GameTable.Model;

namespace NexusForever.Game.Abstract.Challenges
{
    /// <summary>
    /// A challenge unlocked by a player, the owner of challenge scripts.
    /// </summary>
    public interface IChallenge
    {
        ChallengeEntry Entry { get; }
        IPlayer Player { get; }

        bool Activated { get; }
        uint Count { get; }

        /// <summary>
        /// Tiers reached in the current or last run, 1 for bronze to 3 for gold.
        /// </summary>
        uint CurrentTier { get; }

        /// <summary>
        /// Goal of each tier, a count or for time tiered challenges the milliseconds that must be left on the timer.
        /// </summary>
        uint[] Tiers { get; }

        /// <summary>
        /// Milliseconds the current run has been going for.
        /// </summary>
        double Elapsed { get; }
    }
}
