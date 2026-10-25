using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Entity.Movement;
using NexusForever.Game.Abstract.Entity.Stat;
using NexusForever.Game.Abstract.Spell;
using NexusForever.Game.Static.Entity;
using NexusForever.Network.World.Entity;
using NexusForever.Network.World.Entity.Model;

namespace NexusForever.Game.Entity
{
    /// <remarks>
    /// Turrets are combat units (health, interrupt armour, can be killed for quest credit), they only differ from <see cref="NonPlayerEntity"/> by the network model.
    /// </remarks>
    public class AiTurretEntity : NonPlayerEntity, IAiTurretEntity
    {
        public override EntityType Type => EntityType.AiTurret;

        #region Dependency Injection

        public AiTurretEntity(
            IMovementManager movementManager,
            IEntitySummonFactory entitySummonFactory,
            IStatUpdateManager<IUnitEntity> statUpdateManager,
            ISpellFactory spellFactory)
            : base(movementManager, entitySummonFactory, statUpdateManager, spellFactory)
        {
        }

        #endregion

        protected override IEntityModel BuildEntityModel()
        {
            return new AiTurretEntityModel
            {
                CreatureId        = CreatureId,
                QuestChecklistIdx = QuestChecklistIdx
            };
        }
    }
}
