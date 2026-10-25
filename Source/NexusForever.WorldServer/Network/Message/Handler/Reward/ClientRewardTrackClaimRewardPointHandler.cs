using NexusForever.Network.Message;
using NexusForever.Network.World.Message.Model.Reward;

namespace NexusForever.WorldServer.Network.Message.Handler.Reward
{
    public class ClientRewardTrackClaimRewardPointHandler : IMessageHandler<IWorldSession, ClientRewardTrackClaimRewardPoint>
    {
        public void HandleMessage(IWorldSession session, ClientRewardTrackClaimRewardPoint claimRewardPoint)
        {
            session.Player.RewardTrackManager.ClaimReward(claimRewardPoint.RewardTrackRewardsId, claimRewardPoint.ChoiceIndex);
        }
    }
}
