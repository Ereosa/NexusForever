using NexusForever.Network.Message;
using NexusForever.Network.World.Message.Model.PlayerPath;

namespace NexusForever.WorldServer.Network.Message.Handler.Path
{
    public class ClientPathScientistRequestScanbotHandler : IMessageHandler<IWorldSession, ClientPathScientistRequestScanbot>
    {
        public void HandleMessage(IWorldSession session, ClientPathScientistRequestScanbot requestScanbot)
        {
            session.Player.PathScientistManager.SummonScanbot(requestScanbot.ScanbotProfile);
        }
    }

    public class ClientPathScientistDismissScanbotHandler : IMessageHandler<IWorldSession, ClientPathScientistDismissScanbot>
    {
        public void HandleMessage(IWorldSession session, ClientPathScientistDismissScanbot dismissScanbot)
        {
            session.Player.PathScientistManager.DismissScanbot();
        }
    }

    /// <summary>
    /// Sent by the client path action when the scanbot is summoned, the scanbot scans the current target.
    /// </summary>
    public class ClientPathScientistScanbotPathActionHandler : IMessageHandler<IWorldSession, ClientPathScientistDismissScanbotPathAction>
    {
        public void HandleMessage(IWorldSession session, ClientPathScientistDismissScanbotPathAction pathAction)
        {
            session.Player.PathScientistManager.ScanTarget();
        }
    }
}
