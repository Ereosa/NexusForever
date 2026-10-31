using System.Numerics;
using NexusForever.Game.Abstract;
using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Group;
using NexusForever.Game.Abstract.Loot;
using NexusForever.Game.Abstract.Loot.Generate;
using NexusForever.Game.Abstract.Map;
using NexusForever.Game.Static.Group;
using NexusForever.Game.Static.Loot;
using NexusForever.Shared;

namespace NexusForever.Game.Loot
{
    public class LootManager : ILootManager
    {
        public IBaseMap Map { get; private set; }

        private readonly Dictionary<uint, List<ILootInstance>> lootInstances = [];

        #region Dependency Injection

        private readonly IItemManager itemManager;
        private readonly ILootGroupManager lootGroupManager;
        private readonly IFactory<ILootInstance> lootInstanceFactory;

        public LootManager(
            IItemManager itemManager,
            ILootGroupManager lootGroupManager,
            IFactory<ILootInstance> lootInstanceFactory)
        {
            this.itemManager         = itemManager;
            this.lootGroupManager    = lootGroupManager;
            this.lootInstanceFactory = lootInstanceFactory;
        }

        #endregion

        public void Initialise(IBaseMap map)
        {
            Map = map;
        }

        /// <summary>
        /// Invoked each world tick with the delta since the previous tick occurred.
        /// </summary>
        public void Update(double lastTick)
        {
            foreach (List<ILootInstance> entityLootInstances in lootInstances.Values)
            {
                // reverse list iteration so we can remove expired elements
                for (int i = entityLootInstances.Count - 1; i >= 0; i--)
                {
                    ILootInstance lootInstance = entityLootInstances[i];
                    lootInstance.Update(lastTick);
                    if (lootInstance.Expired)
                    {
                        lootInstance.SendLootRemove();
                        entityLootInstances.RemoveAt(i);
                    }
                }
            }
        }

        /// <summary>
        /// Remove all loot instances associated with the specified unit, used when the unit leaves the map.
        /// </summary>
        /// <remarks>
        /// Unit guids are reused, loot outliving its corpse later told the client to remove loot from whatever new unit had the guid and the client hid that unit.
        /// </remarks>
        public void RemoveLootInstances(uint ownerUnitId)
        {
            if (!lootInstances.Remove(ownerUnitId, out List<ILootInstance> entityLootInstances))
                return;

            foreach (ILootInstance lootInstance in entityLootInstances)
                lootInstance.SendLootRemove();
        }

        /// <summary>
        /// Gets all loot instances associated with the specified unit.
        /// </summary>
        /// <param name="ownerUnitId">The id of the unit that owns the loot instances.</param>
        /// <returns>
        /// The loot instances associated with the specified unit, or an empty collection if none exist.
        /// </returns>
        public IEnumerable<ILootInstance> GetLootInstances(uint ownerUnitId)
        {
            return lootInstances.TryGetValue(ownerUnitId, out List<ILootInstance> entityLootInstances) ? entityLootInstances : [];
        }

        /// <summary>
        /// Get the loot item instance for the specified owner unit and loot id.
        /// </summary>
        /// <param name="ownerUnitId">The id of the unit that owns the loot instances.</param>
        /// <param name="lootId">The id of the loot item to retrieve.</param>
        /// <returns>
        /// The loot item instance, or null if not found.
        /// </returns>
        public ILootInstanceItem GetLootItemInstance(uint ownerUnitId, uint lootId)
        {
            foreach (ILootInstance lootInstance in GetLootInstances(ownerUnitId))
            {
                ILootInstanceItem lootInstanceItem = lootInstance.GetLootItem(lootId);
                if (lootInstanceItem != null)
                    return lootInstanceItem;
            }

            return null;
        }

        /// <summary>
        /// Generates loot for the specified player from a looted item.
        /// </summary>
        /// <param name="looter">The player who looted the item.</param>
        /// <param name="lootedItem">The item that was looted.</param>
        public bool HasItemLoot(uint itemId)
        {
            return lootGroupManager.HasItemLoot(itemId);
        }

        public void GenerateLoot(IPlayer looter, IItem lootedItem)
        {
            ILootBuilder lootBuilder = lootGroupManager.GenerateItemLoot(lootedItem.Id, [ looter ]);
            if (lootBuilder == null)
                return;

            ILootInstance lootInstance = lootInstanceFactory.Resolve();
            lootInstance.Initialise(this, looter);

            foreach (ILootBuilderItem lootBuilderItem in lootBuilder.GetItems())
            {
                ILootInstanceItem lootInstanceItem = lootInstance.AddLootItem(lootBuilderItem);
                lootInstanceItem.SetWinner(looter.Identity);
                lootInstanceItem.DeliverLoot(looter);
            }

            lootInstance.SendLootNotify();
        }

        /// <summary>
        /// Lowest Creature2Difficulty that always drops something (4 elite, 5 rare and prime).
        /// </summary>
        private const uint GuaranteedLootDifficultyId = 4u;

        /// <summary>
        /// Most rerolls of an elite's loot table that came up empty.
        /// </summary>
        private const int GuaranteedLootRolls = 10;

        /// <remarks>
        /// The creature loot tables are archived drop rates, each item rolled on its own, so a kill can come up empty: a third of the
        /// Frostshard's kills (Algoroc rare, report 83) dropped nothing. Elites and rares always left something on retail, their table
        /// is rolled again until it drops (guessed rule).
        /// </remarks>
        private ILootBuilder GenerateEntityLoot(IUnitEntity lootedEntity, List<IPlayer> players)
        {
            ILootBuilder lootBuilder = lootGroupManager.GenerateEntityLoot(lootedEntity.CreatureId, players);
            if (lootBuilder == null || (lootedEntity.CreatureInfo?.Entry?.Creature2DifficultyId ?? 0u) < GuaranteedLootDifficultyId)
                return lootBuilder;

            for (int i = 0; i < GuaranteedLootRolls && !lootBuilder.GetItems().Any(); i++)
                lootBuilder = lootGroupManager.GenerateEntityLoot(lootedEntity.CreatureId, players) ?? lootBuilder;

            return lootBuilder;
        }

        /// <summary>
        /// Generates loot for the specified player from a looted entity.
        /// </summary>
        /// <param name="looter">The player who looted the entity.</param>
        /// <param name="lootedEntity">The entity that was looted.</param>
        public void GenerateLoot(IPlayer looter, IUnitEntity lootedEntity)
        {
            ILootBuilder lootBuilder = GenerateEntityLoot(lootedEntity, [ looter ]);
            if (lootBuilder == null)
                return;

            ILootInstance lootInstance = lootInstanceFactory.Resolve();
            lootInstance.Initialise(this, lootedEntity);

            if (!lootInstances.ContainsKey(lootedEntity.Guid))
                lootInstances.Add(lootedEntity.Guid, []);

            lootInstances[lootedEntity.Guid].Add(lootInstance);

            foreach (ILootBuilderItem lootBuilderItem in lootBuilder.GetItems())
            {
                ILootInstanceItem lootInstanceItem = lootInstance.AddLootItem(lootBuilderItem);
                lootInstanceItem.SetLootable();
            }

            lootInstance.SendLootNotify();
        }

        /// <summary>
        /// Generates loot for the specified group from a looted entity.
        /// </summary>
        /// <remarks>
        /// Group loot is personal: the drop is generated once and every group member in range gets their own copy of each item they are eligible for.
        /// The group loot rules (need before greed, master loot, etc.) are not used.
        /// </remarks>
        /// <param name="group">The group that looted the entity.</param>
        /// <param name="lootedEntity">The entity that was looted.</param>
        public void GenerateLoot(IGroup group, IUnitEntity lootedEntity)
        {
            List<IPlayer> players = [];
            foreach (IGroupMember member in group)
            {
                IPlayer player = lootedEntity.GetVisiblePlayer(member.Identity);
                if (player != null)
                    players.Add(player);
            }

            if (players.Count == 0)
                return;

            ILootBuilder lootBuilder = GenerateEntityLoot(lootedEntity, players);
            if (lootBuilder == null)
                return;

            if (!lootInstances.ContainsKey(lootedEntity.Guid))
                lootInstances.Add(lootedEntity.Guid, []);

            foreach (IPlayer player in players)
            {
                ILootInstance lootInstance = null;
                foreach (ILootBuilderItem lootBuilderItem in lootBuilder.GetItems())
                {
                    // conditional loot (eg: quest items) is only eligible for some of the group
                    if (!lootBuilderItem.Looters.Contains(player.Identity))
                        continue;

                    if (lootInstance == null)
                    {
                        lootInstance = lootInstanceFactory.Resolve();
                        lootInstance.Initialise(this, lootedEntity);
                        lootInstances[lootedEntity.Guid].Add(lootInstance);
                    }

                    ILootInstanceItem lootInstanceItem = lootInstance.AddLootItem(lootBuilderItem.StaticId, lootBuilderItem.Type, lootBuilderItem.Count, [ player.Identity ]);
                    lootInstanceItem.SetLootable();
                }

                lootInstance?.SendLootNotify();
            }
        }

        /// <summary>
        /// Distance from a unit whose loot is vacuumed though the unit isn't visible to the player.
        /// </summary>
        private const float VacuumRange = 15f;

        /// <summary>
        /// Vacuums all loot in range for the specified player.
        /// </summary>
        /// <param name="player">The player to vacuum loot for.</param>
        public void VacuumLoot(IPlayer player)
        {
            // loot of units in range counts too when the unit itself isn't visible to the player any more, eg: a Discovery's loot lies on
            // its indicator, the client offers it at the Discovery Chest standing on top of it (report 95)
            var looted = new HashSet<uint>();
            foreach (IGridEntity entity in player.GetVisible()
                .Concat(lootInstances.Keys.ToList()
                    .Select(guid => Map?.GetEntity<IUnitEntity>(guid))
                    .Where(e => e != null && Vector3.Distance(e.Position, player.Position) <= VacuumRange)))
            {
                if (entity is not IUnitEntity || !looted.Add(entity.Guid))
                    continue;

                if (!lootInstances.TryGetValue(entity.Guid, out List<ILootInstance> entityLootInstances))
                    continue;

                foreach (ILootInstance lootInstance in entityLootInstances)
                {
                    foreach (ILootInstanceItem lootInstanceItem in lootInstance.GetLootItems())
                    {
                        if (lootInstanceItem.CanLoot)
                            lootInstanceItem.TakeLoot(player);
                    }
                }
            }
        }
    }
}
