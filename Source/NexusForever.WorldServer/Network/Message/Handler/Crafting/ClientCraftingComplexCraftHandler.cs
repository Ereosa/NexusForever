using NexusForever.Network.Message;
using NexusForever.Network.World.Message.Model.Crafting;

namespace NexusForever.WorldServer.Network.Message.Handler.Crafting
{
    public class ClientCraftingComplexCraftHandler : IMessageHandler<IWorldSession, ClientCraftingComplexCraft>
    {
        public void HandleMessage(IWorldSession session, ClientCraftingComplexCraft message)
        {
            session.Player.CraftingManager.CompleteCircuitCraft(message);
        }
    }
}
