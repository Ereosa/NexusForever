using NexusForever.Database.World.Model;
using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Entity.Creature;
using NexusForever.Game.Abstract.Entity.Movement;
using NexusForever.Game.Abstract.Entity.Stat;
using NexusForever.Game.Abstract.Loot;
using NexusForever.Game.Abstract.Loot.Distribute;
using NexusForever.Game.Abstract.Spell;
using NexusForever.Game.Static.Crafting;
using NexusForever.Game.Static.Entity;
using NexusForever.Game.Static.Quest;
using NexusForever.GameTable;
using NexusForever.GameTable.Model;
using NexusForever.Network.World.Entity;
using NexusForever.Network.World.Entity.Model;
using NexusForever.Shared.Game;

namespace NexusForever.Game.Entity
{
    public class HarvestUnitEntity : CreatureEntity, IHarvestUnitEntity
    {
        public override EntityType Type => EntityType.HarvestUnit;

        /// <summary>
        /// Time before a harvested node is spawned again.
        /// </summary>
        /// <remarks>
        /// Retail value is unknown, the client data has no respawn time for harvesting nodes.
        /// </remarks>
        protected override TimeSpan RespawnDelay => TimeSpan.FromMinutes(5d);

        /// <summary>
        /// Maximum time a harvested node stays in the world with loot remaining, matches loot instance expiry.
        /// </summary>
        private static readonly TimeSpan MaxHarvestedTime = TimeSpan.FromMinutes(10d);

        private UpdateTimer harvestedTimer;
        private bool despawning;

        #region Dependency Injection

        public HarvestUnitEntity(
            IMovementManager movementManager,
            IEntitySummonFactory entitySummonFactory,
            IStatUpdateManager<IUnitEntity> statUpdateManager,
            ISpellFactory spellFactory,
            ILootDistributor lootDistributor)
            : base(movementManager, entitySummonFactory, statUpdateManager, spellFactory, lootDistributor)
        {
            statUpdateManager.Initialise(this);
        }

        #endregion

        /// <summary>
        /// Harvested nodes handle their own removal and respawn once their loot has been collected.
        /// </summary>
        protected override bool RespawnsOnDeath => false;

        protected override IEntityModel BuildEntityModel()
        {
            return new HarvestUnitEntityModel
            {
                CreatureId = CreatureId
            };
        }

        /// <summary>
        /// Invoked each world tick with the delta since the previous tick occurred.
        /// </summary>
        public override void Update(double lastTick)
        {
            base.Update(lastTick);

            if (harvestedTimer == null || despawning)
                return;

            // harvested nodes stay in the world until the loot that burst from them has been collected
            harvestedTimer.Update(lastTick);
            if (!harvestedTimer.HasElapsed && HasRemainingLoot())
                return;

            despawning = true;
            if (SpawnModel != null)
                Map.ScheduleRespawn(CreatureInfo, SpawnModel, RespawnDelay);

            RemoveFromMap();
        }

        /// <summary>
        /// Harvesting nodes don't fight back, any player can harvest them with the correct tool which is enforced by the tool spell prerequisites.
        /// </summary>
        public override bool CanAttack(IUnitEntity target)
        {
            return IsAlive && target is IPlayer;
        }

        public override bool IsValidAttackTarget(IUnitEntity attacker)
        {
            return attacker is IPlayer;
        }

        protected override void OnDeath(IUnitEntity killer)
        {
            base.OnDeath(killer);
            harvestedTimer = new UpdateTimer(MaxHarvestedTime);
        }

        protected override void RewardKiller(IPlayer player)
        {
            base.RewardKiller(player);

            player.QuestManager.ObjectiveUpdate(QuestObjectiveType.GatheResource, CreatureId, 1u);

            TradeskillHarvestingInfoEntry harvestingInfo = GameTableManager.Instance.TradeskillHarvestingInfo.GetEntry(CreatureInfo.Entry.TradeskillHarvestingInfoId);
            if (harvestingInfo == null)
                return;

            TradeskillTierEntry tier = GameTableManager.Instance.TradeskillTier.GetEntry(harvestingInfo.TradeSkillTierId);
            if (tier == null)
                return;

            player.TradeskillManager.AddXp((TradeskillType)tier.TradeSkillId, tier.CraftXp);
        }

        /// <summary>
        /// Calculate default property value for supplied <see cref="Property"/>.
        /// </summary>
        /// <remarks>
        /// Default property values are not sent to the client, they are also calculated by the client and are replaced by any property updates.
        /// </remarks>
        protected override float CalculateDefaultProperty(Property property)
        {
            float value = base.CalculateDefaultProperty(property);

            if (CreatureInfo.ArcheTypeEntry != null)
                value *= CreatureInfo.ArcheTypeEntry.UnitPropertyMultiplier[(uint)property];

            if (CreatureInfo.DifficultyEntry != null)
                value *= CreatureInfo.DifficultyEntry.UnitPropertyMultiplier[(uint)property];

            if (CreatureInfo.TierEntry != null)
                value *= CreatureInfo.TierEntry.UnitPropertyMultiplier[(uint)property];

            return value;
        }
    }
}
