using System.Numerics;
using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Entity.Movement;
using NexusForever.Game.Abstract.Matching;
using NexusForever.Game.Matching;
using NexusForever.Game.Static;
using NexusForever.Game.Static.Entity;
using NexusForever.Game.Static.Setting;
using NexusForever.GameTable;
using NexusForever.GameTable.Model;
using NexusForever.Network.World.Entity;
using NexusForever.Network.World.Entity.Model;
using NexusForever.Network.World.Message.Model.Instance;

namespace NexusForever.Game.Entity
{
    public class InstancePortalEntity : WorldEntity, IInstancePortalEntity
    {
        public override EntityType Type => EntityType.InstancePortal;

        #region Dependency Injection

        public InstancePortalEntity(IMovementManager movementManager,
            IEntitySummonFactory entitySummonFactory)
            : base(movementManager, entitySummonFactory)
        {
        }

        #endregion

        protected override IEntityModel BuildEntityModel()
        {
            return new InstancePortalEntityModel
            {
                CreatureId = CreatureId
            };
        }

        /// <summary>
        /// Invoked when <see cref="IWorldEntity"/>'s activate succeeds.
        /// </summary>
        /// <remarks>
        /// Instance portals of type 1 (expeditions, eg: Transport to Fragment Zero) open the client's instance settings dialog,
        /// the player enters once they confirm it (ClientSetInstanceSettings).
        /// </remarks>
        public override void OnActivateSuccess(IPlayer activator)
        {
            base.OnActivateSuccess(activator);

            InstancePortalEntry portal = GameTableManager.Instance.InstancePortal.GetEntry(CreatureInfo?.Entry?.InstancePortalId ?? 0u);
            if (portal == null || portal.InstancePortalTypeEnum != 1u || GetDestinationWorld() == null)
                return;

            activator.Session.EnqueueMessageEncrypted(new ServerShowInstanceGameModeDialog
            {
                InstancePortalUnitId = Guid,
                Difficulty           = WorldDifficulty.Normal,
                ExistingDifficulty   = WorldDifficulty.Normal
            });
        }

        /// <summary>
        /// Return the world the portal leads to: the matching map with the portal's name (InstancePortal has no world).
        /// </summary>
        public WorldEntry GetDestinationWorld()
        {
            InstancePortalEntry portal = GameTableManager.Instance.InstancePortal.GetEntry(CreatureInfo?.Entry?.InstancePortalId ?? 0u);
            if (portal == null)
                return null;

            var text = GameTableManager.Instance.GetTextTable(Language.English);
            string name = text?.GetEntry(portal.LocalizedTextIdName);
            if (string.IsNullOrEmpty(name))
                return null;

            MatchingGameMapEntry map = GameTableManager.Instance.MatchingGameMap.Entries
                .Where(m => m != null && text.GetEntry(m.LocalizedTextIdName) == name)
                .OrderBy(m => m.Id)
                .FirstOrDefault();
            return map != null ? GameTableManager.Instance.World.GetEntry(map.WorldId) : null;
        }

        /// <summary>
        /// Send the player into the portal's instance at its entrance.
        /// </summary>
        public bool Enter(IPlayer player)
        {
            WorldEntry world = GetDestinationWorld();
            if (world == null)
                return false;

            IMapEntrance entrance = MatchingDataManager.Instance.GetMapEntrance(world.Id, 0);
            if (entrance == null)
                return false;

            player.TeleportTo(world, entrance.Position);
            return true;
        }
    }
}
