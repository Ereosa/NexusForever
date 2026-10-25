using NexusForever.Database.Character;

namespace NexusForever.Game.Abstract.Reward
{
    public interface IRewardTrackManager : IDatabaseCharacter
    {
        void SendInitialPackets();

        /// <summary>
        /// Invoked when the player enters a new zone, the challenge reward track of the zone becomes the active one.
        /// </summary>
        void OnZoneUpdate();

        /// <summary>
        /// Add reward points to the supplied reward track, applying the player's reward property multipliers.
        /// </summary>
        uint AddPoints(uint rewardTrackId, uint points);

        /// <summary>
        /// Claim the reward of a reward point that has been reached, with the supplied choice for rewards with several choices.
        /// </summary>
        void ClaimReward(uint rewardTrackRewardsId, uint choiceIndex);
    }
}
