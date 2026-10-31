using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Loot;
using NexusForever.Network.Message;
using NexusForever.Network.World.Message.Model.Loot;

namespace NexusForever.WorldServer.Network.Message.Handler.Loot
{
    public class ClientLootItemHandler : IMessageHandler<IWorldSession, ClientLootItem>
    {
        /// <summary>
        /// How far from a chest the loot it stands for can lie (a Discovery Chest's loot belongs to the Discovery it popped out of).
        /// </summary>
        private const float ChestLootRange = 5f;

        public void HandleMessage(IWorldSession session, ClientLootItem lootItem)
        {
            ILootInstanceItem lootInstanceItem =
                session.Player.Map.LootManager.GetLootItemInstance(lootItem.OwnerUnitId, lootItem.LootUnitId);
            if (lootInstanceItem == null)
            {
                // the client interacting with a chest (or a pinata) the loot of which it doesn't know asks for the chest itself: owner
                // and loot are both the chest's unit (decompiled client, the interact handler sending 0x14F, report 132)
                if (lootItem.OwnerUnitId == lootItem.LootUnitId && !lootItem.Request)
                    TakeChestLoot(session.Player, lootItem.OwnerUnitId);
                return;
            }

            if (lootItem.Request)
            {
                // TODO
            }
            else
                lootInstanceItem.TakeLoot(session.Player);
        }

        private static void TakeChestLoot(IPlayer player, uint chestGuid)
        {
            IWorldEntity chest = player.GetVisible<IWorldEntity>(chestGuid);
            if (chest == null || Vector3.Distance(chest.Position, player.Position) > 15f)
                return;

            List<ILootInstanceItem> items = player.Map.LootManager.GetLootInstances(chestGuid)
                .SelectMany(i => i.GetLootItems())
                .ToList();

            if (items.Count == 0)
            {
                // loot of the unit the chest stands for, right where the chest is
                foreach (IWorldEntity entity in player.GetVisible().OfType<IWorldEntity>()
                    .Where(e => e.Guid != chestGuid && Vector3.Distance(e.Position, chest.Position) <= ChestLootRange))
                    items.AddRange(player.Map.LootManager.GetLootInstances(entity.Guid).SelectMany(i => i.GetLootItems()));
            }

            foreach (ILootInstanceItem item in items)
                item.TakeLoot(player);
        }
    }
}
