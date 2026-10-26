using NexusForever.Game.Abstract.Entity;
using NexusForever.Network.Message;
using NexusForever.Network.World.Message.Model.Instance;

namespace NexusForever.WorldServer.Network.Message.Handler.Instance
{
    /// <summary>
    /// The player confirmed the instance settings dialog of an instance portal (ServerShowInstanceGameModeDialog).
    /// </summary>
    /// <remarks>
    /// Difficulty, prime level and rally are not applied, instances only have their normal version.
    /// </remarks>
    public class ClientSetInstanceSettingsHandler : IMessageHandler<IWorldSession, ClientSetInstanceSettings>
    {
        public void HandleMessage(IWorldSession session, ClientSetInstanceSettings setInstanceSettings)
        {
            IInstancePortalEntity portal = session.Player?.GetVisible<IInstancePortalEntity>(setInstanceSettings.InstancePortalUnitId);
            portal?.Enter(session.Player);
        }
    }
}
