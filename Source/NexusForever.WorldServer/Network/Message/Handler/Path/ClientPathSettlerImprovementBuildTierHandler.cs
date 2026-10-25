using NexusForever.Network.Message;
using NexusForever.Network.World.Message.Model.PlayerPath;

namespace NexusForever.WorldServer.Network.Message.Handler.Path
{
    public class ClientPathSettlerImprovementBuildTierHandler : IMessageHandler<IWorldSession, ClientPathSettlerImprovementBuildTier>
    {
        public void HandleMessage(IWorldSession session, ClientPathSettlerImprovementBuildTier buildTier)
        {
            session.Player.PathSettlerManager.BuildImprovement(buildTier.PathSettlerImprovementGroupId, buildTier.BuildTier);
        }
    }
}
