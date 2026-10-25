using NexusForever.Network.Message;
using NexusForever.Network.World.Message.Model.PlayerPath;

namespace NexusForever.WorldServer.Network.Message.Handler.Path
{
    public class ClientPathExplorerPowerMapProgressHandler : IMessageHandler<IWorldSession, ClientPathExplorerPowerMapProgress>
    {
        public void HandleMessage(IWorldSession session, ClientPathExplorerPowerMapProgress powerMapProgress)
        {
            session.Player.PathExplorerManager.OnPowerMapProgress(powerMapProgress.PathMissionId);
        }
    }
}
