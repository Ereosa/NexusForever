using NexusForever.Network.Message;
using NexusForever.Network.World.Message.Model.Crafting;

namespace NexusForever.WorldServer.Network.Message.Handler.Crafting
{
    public class ClientCraftingCraftItemAutoCraftHandler : IMessageHandler<IWorldSession, ClientCraftingCraftItemAutoCraft>
    {
        public void HandleMessage(IWorldSession session, ClientCraftingCraftItemAutoCraft message)
        {
            session.Player.CraftingManager.StartCraft(message.CraftingStationUnitId, message.TradeskillSchematic2Id, message.SchematicCount, message.CatalystItem2Id);
        }
    }
}
