using NexusForever.Network.Message;
using NexusForever.Network.World.Message.Model.Crafting;

namespace NexusForever.WorldServer.Network.Message.Handler.Crafting
{
    public class ClientCraftingAdditiveHandler : IMessageHandler<IWorldSession, ClientCraftingAdditive>
    {
        public void HandleMessage(IWorldSession session, ClientCraftingAdditive message)
        {
            session.Player.CraftingManager.AddAdditive(message.CraftingStationUnitId, message.AdditiveItem2Id);
        }
    }
}
