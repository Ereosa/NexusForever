using NexusForever.Network.Message;
using NexusForever.Network.World.Message.Model.Fortune;

namespace NexusForever.WorldServer.Network.Message.Handler.Fortune
{
    /// <summary>
    /// Sent alongside <see cref="ClientFortuneNotifyStorefront"/> when the Fortunes window opens, that message already sends the state.
    /// </summary>
    public class ClientFortuneNotifyGameHandler : IMessageHandler<IWorldSession, ClientFortuneNotifyGame>
    {
        public void HandleMessage(IWorldSession session, ClientFortuneNotifyGame _)
        {
        }
    }
}
