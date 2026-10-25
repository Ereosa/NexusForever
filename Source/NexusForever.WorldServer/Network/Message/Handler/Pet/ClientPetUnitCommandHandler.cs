using NexusForever.Network.Message;
using NexusForever.Network.World.Message.Model.Pet;

namespace NexusForever.WorldServer.Network.Message.Handler.Pet
{
    public class ClientPetUnitCommandHandler : IMessageHandler<IWorldSession, ClientPetUnitCommand>
    {
        public void HandleMessage(IWorldSession session, ClientPetUnitCommand petUnitCommand)
        {
            session.Player.CombatPetManager.HandleCommand(petUnitCommand.ShortcutSet, petUnitCommand.Slot, true);
        }
    }
}
