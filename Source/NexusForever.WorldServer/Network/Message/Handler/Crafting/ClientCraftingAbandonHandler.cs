using NexusForever.Network.Message;
using NexusForever.Network.World.Message.Model.Crafting;

namespace NexusForever.WorldServer.Network.Message.Handler.Crafting
{
    public class ClientCraftingAbandonHandler : IMessageHandler<IWorldSession, ClientCraftingAbandon>
    {
        public void HandleMessage(IWorldSession session, ClientCraftingAbandon message)
        {
            session.Player.CraftingManager.AbandonCraft();
        }
    }
}
