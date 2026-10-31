using NexusForever.Network.Message;
using NexusForever.Network.World.Message.Model;

namespace NexusForever.WorldServer.Network.Message.Handler.Quest
{
    /// <summary>
    /// The client queued a communicator call and set its quest to mentioned (engaged), or ignored it.
    /// </summary>
    public class ClientCommunicatorActionHandler : IMessageHandler<IWorldSession, ClientCommunicatorAction>
    {
        public void HandleMessage(IWorldSession session, ClientCommunicatorAction communicatorAction)
        {
            session.Player.QuestManager.OnCommunicatorAction((ushort)communicatorAction.QuestId, communicatorAction.Engaged);
        }
    }
}
