using NexusForever.Network.Message;
using NexusForever.Network.World.Message.Model.Pet;

namespace NexusForever.WorldServer.Network.Message.Handler.Pet
{
    public class ClientPetSetStanceHandler : IMessageHandler<IWorldSession, ClientPetSetStance>
    {
        public void HandleMessage(IWorldSession session, ClientPetSetStance petSetStance)
        {
            session.Player.CombatPetManager.SetStance(petSetStance.PetUnitId, petSetStance.Stance);
        }
    }
}
