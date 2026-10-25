using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Entity.Creature;
using NexusForever.Game.Abstract.Entity.Movement;
using NexusForever.Game.Static.Entity;
using NexusForever.Network.World.Entity;
using NexusForever.Network.World.Entity.Model;
using NexusForever.Shared;
using NexusForever.Shared.Game;
using NLog;

namespace NexusForever.Game.Entity
{
    public class ScannerUnitEntity : WorldEntity, IScannerUnitEntity
    {
        private const float FollowDistance = 2.5f;
        private const float FollowMinRecalculateDistance = 4f;
        private const float TargetDistance = 2f;

        private static readonly ILogger log = LogManager.GetCurrentClassLogger();

        public override EntityType Type => EntityType.ScannerUnit;

        public uint OwnerGuid { get; private set; }

        private string name;
        private uint? targetGuid;
        private readonly UpdateTimer followTimer = new(1d);

        #region Dependency Injection

        public ScannerUnitEntity(IMovementManager movementManager,
            IEntitySummonFactory entitySummonFactory)
            : base(movementManager, entitySummonFactory)
        {
        }

        #endregion

        public void Initialise(IPlayer owner, ICreatureInfo creatureInfo, string name)
        {
            OwnerGuid = owner.Guid;
            this.name = name ?? "";
            Initialise(creatureInfo);

            SetBaseProperty(Property.BaseHealth, 800.0f);

            SetStat(Static.Entity.Stat.Health, 800u);
            SetStat(Static.Entity.Stat.Level, owner.Level);
            SetStat(Static.Entity.Stat.Sheathed, 0u);

            Faction1 = owner.Faction1;
            Faction2 = owner.Faction2;
        }

        protected override IEntityModel BuildEntityModel()
        {
            return new ScannerUnitEntityModel
            {
                CreatureId = CreatureId,
                OwnerId    = OwnerGuid,
                Name       = name
            };
        }

        public override void OnEnqueueRemoveFromMap()
        {
            followTimer.Reset(false);
            targetGuid = null;
        }

        public void MoveToTarget(IWorldEntity target)
        {
            targetGuid = target.Guid;
            MovementManager.Follow(target, TargetDistance);
        }

        public void ReturnToOwner()
        {
            targetGuid = null;
            followTimer.Reset();

            IPlayer owner = GetVisible<IPlayer>(OwnerGuid);
            if (owner != null)
                MovementManager.Follow(owner, FollowDistance);
        }

        public override void Update(double lastTick)
        {
            base.Update(lastTick);
            Follow(lastTick);
        }

        private void Follow(double lastTick)
        {
            if (targetGuid != null)
                return;

            followTimer.Update(lastTick);
            if (!followTimer.HasElapsed)
                return;

            followTimer.Reset();

            IPlayer owner = GetVisible<IPlayer>(OwnerGuid);
            if (owner == null)
            {
                log.Trace($"Scanner unit {Guid} has lost its owner {OwnerGuid}.");
                RemoveFromMap();
                return;
            }

            if (owner.Position.GetDistance(Position) < FollowMinRecalculateDistance)
                return;

            MovementManager.Follow(owner, FollowDistance);
        }
    }
}
