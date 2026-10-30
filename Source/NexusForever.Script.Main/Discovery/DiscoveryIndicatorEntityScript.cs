using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Entity.Creature;
using NexusForever.Game.Abstract.Loot;
using NexusForever.Script.Template;
using NexusForever.Script.Template.Filter;
using NexusForever.Shared.Game;

namespace NexusForever.Script.Main.Discovery
{
    /// <summary>
    /// Discovery indicators (the glowing holes, Tier 1 to 4), uncovering one pops a Discovery Chest that loot bursts from.
    /// </summary>
    /// <remarks>
    /// The activate spell (7105, 83956) only activates the indicator, retail spawned the chest server side.
    /// The Discovery Chest (18021) is the "Generic Discovery Chest - Object" creature, its loot comes from the indicator's entity_loot.
    /// The chest lasts as long as the Fluff effect of the activate spell (30 seconds), respawn time is a guess.
    /// </remarks>
    [ScriptFilterCreatureId(11254, 11255, 11256, 11257)]
    public class DiscoveryIndicatorEntityScript : IWorldEntityScript, IOwnedScript<IWorldEntity>
    {
        private const uint DiscoveryChestCreatureId = 18021u;
        private static readonly TimeSpan ChestDuration = TimeSpan.FromSeconds(30d);
        private static readonly TimeSpan RespawnDelay  = TimeSpan.FromMinutes(5d);

        private IWorldEntity owner;
        private UpdateTimer uncoveredTimer;
        private bool despawning;

        #region Dependency Injection

        private readonly IEntityFactory entityFactory;
        private readonly ICreatureInfoManager creatureInfoManager;

        public DiscoveryIndicatorEntityScript(
            IEntityFactory entityFactory,
            ICreatureInfoManager creatureInfoManager)
        {
            this.entityFactory       = entityFactory;
            this.creatureInfoManager = creatureInfoManager;
        }

        #endregion

        /// <summary>
        /// Invoked when <see cref="IScript"/> is loaded.
        /// </summary>
        public void OnLoad(IWorldEntity owner)
        {
            this.owner = owner;
        }

        /// <summary>
        /// Invoked when <see cref="IWorldEntity"/> is successfully activated by <see cref="IPlayer"/>.
        /// </summary>
        public void OnActivateSuccess(IPlayer activator)
        {
            // each discovery can only be uncovered once until it respawns
            if (uncoveredTimer != null || owner.Map == null)
                return;

            uncoveredTimer = new UpdateTimer(ChestDuration);

            ICreatureInfo chestInfo = creatureInfoManager.GetCreatureInfo(DiscoveryChestCreatureId);
            IWorldEntity chest = chestInfo != null ? entityFactory.CreateWorldEntity(chestInfo.Entry.CreationTypeEnum) : null;
            if (chest != null)
            {
                chest.Initialise(chestInfo);
                chest.Rotation = owner.Rotation;
                chest.AddToMap(owner.Map, owner.Position, (map, guid, vector) => map.ScheduleDespawn(guid, ChestDuration));
            }

            if (owner is IUnitEntity unit)
                owner.Map.LootManager.GenerateLoot(activator, unit);
        }

        /// <summary>
        /// Invoked each world tick with the delta since the previous tick occurred.
        /// </summary>
        public void Update(double lastTick)
        {
            if (uncoveredTimer == null || despawning)
                return;

            // the indicator stays while its loot is on the ground so it can still be picked up
            uncoveredTimer.Update(lastTick);
            if (!uncoveredTimer.HasElapsed && HasRemainingLoot())
                return;

            despawning = true;
            owner.DespawnWithRespawn(RespawnDelay);
        }

        private bool HasRemainingLoot()
        {
            foreach (ILootInstance lootInstance in owner.Map?.LootManager.GetLootInstances(owner.Guid) ?? [])
                if (lootInstance.GetLootItems().Any())
                    return true;

            return false;
        }
    }
}
