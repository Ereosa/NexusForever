using NexusForever.Network.Message;
using NexusForever.Network.World.Message.Model.Pet;

namespace NexusForever.WorldServer.Network.Message.Handler.Pet
{
    public class ClientPetCommandHandler : IMessageHandler<IWorldSession, ClientPetCommand>
    {
        public void HandleMessage(IWorldSession session, ClientPetCommand petCommand)
        {
            session.Player.CombatPetManager.HandleCommand(petCommand.ShortcutSet, petCommand.Slot, false);
        }
    }
}
