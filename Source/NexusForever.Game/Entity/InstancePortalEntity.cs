using System.Numerics;
using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Entity.Movement;
using NexusForever.Game.Abstract.Map;
using NexusForever.Game.Abstract.Map.Lock;
using NexusForever.Game.Map;
using NexusForever.Game.Map.Lock;
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
            if (portal == null)
                return;

            switch (portal.InstancePortalTypeEnum)
            {
                case 1u:
                    ShowInstanceSettings(activator);
                    break;
                case 3u:
                    Exit(activator);
                    break;
            }
        }

        /// <summary>
        /// Open the client's instance settings dialog for the portal's instance.
        /// </summary>
        /// <remarks>
        /// Difficulty is the mask of allowed difficulties (bit 0 normal, bit 1 veteran), prime levels replace veteran for worlds that have them.
        /// Prime difficulty needs the level of the world's prime matching game type (50), a completed prime level unlocks the next one.
        /// </remarks>
        private void ShowInstanceSettings(IPlayer activator)
        {
            WorldEntry world = GetDestinationWorld();
            if (world == null)
                return;

            bool hasPrimeLevels = world.PrimeLevelMax > 0;
            bool primeAllowed   = hasPrimeLevels && activator.Level >= GetPrimeMinLevel(world.Id);

            var parameters = InstanceParameters.Scaling;
            if (hasPrimeLevels)
                parameters |= InstanceParameters.HasPrimeLevels;

            byte unlocked = (byte)Math.Min(activator.PrimeLevelManager.GetPrimeLevelUnlocked(world.Id), world.PrimeLevelMax);

            // an instance still running for the player keeps its settings, the client then only offers to enter it
            IMapLock mapLock = MapLockManager.Instance.GetSoloLock(activator.Identity, world.Id);
            bool existing = mapLock is { IsInstanceLoaded: true };

            activator.Session.EnqueueMessageEncrypted(new ServerShowInstanceGameModeDialog
            {
                InstancePortalUnitId = Guid,
                Difficulty           = primeAllowed ? (WorldDifficulty)3 : (WorldDifficulty)1,
                Parameters           = parameters,
                ExistingDifficulty   = existing ? mapLock.Difficulty : WorldDifficulty.Count,
                ExistingPrimeLevel   = existing ? mapLock.PrimeLevel : (byte)0,
                MaxPrimeLevelWorld   = (byte)world.PrimeLevelMax,
                MaxPrimeLevelGroup   = unlocked,
                PrimeAllowed         = primeAllowed
            });
        }

        private static uint GetPrimeMinLevel(uint worldId)
        {
            uint? level = GameTableManager.Instance.MatchingGameMap.Entries
                .Where(m => m != null && m.WorldId == worldId)
                .Select(m => GameTableManager.Instance.MatchingGameType.GetEntry(m.MatchingGameTypeId))
                .Where(t => t != null && t.MatchTypeEnum is Static.Matching.MatchType.PrimeLevelExpedition
                    or Static.Matching.MatchType.PrimeLevelDungeon or Static.Matching.MatchType.PrimeLevelAdventure)
                .Select(t => (uint?)t.MinLevel)
                .Min();
            return level ?? 50u;
        }

        /// <summary>
        /// Exit portals (type 3) send the player back to where they entered the instance.
        /// </summary>
        private void Exit(IPlayer activator)
        {
            if (activator.InstancePortalReturn == null)
                return;

            IMapPosition position = activator.InstancePortalReturn;
            activator.InstancePortalReturn = null;
            activator.TeleportTo(position);
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
        /// Send the player into the portal's instance at its entrance with the supplied difficulty and prime level.
        /// </summary>
        public bool Enter(IPlayer player, WorldDifficulty difficulty, byte primeLevel)
        {
            WorldEntry world = GetDestinationWorld();
            if (world == null)
                return false;

            IMapEntrance entrance = MatchingDataManager.Instance.GetMapEntrance(world.Id, 0);
            if (entrance == null)
                return false;

            if (difficulty == WorldDifficulty.Veteran)
            {
                // prime levels can't go past the one the player has unlocked
                if (world.PrimeLevelMax == 0 || player.Level < GetPrimeMinLevel(world.Id))
                    difficulty = WorldDifficulty.Normal;
                primeLevel = (byte)Math.Min(primeLevel, player.PrimeLevelManager.GetPrimeLevelUnlocked(world.Id));
            }
            if (difficulty != WorldDifficulty.Veteran)
                primeLevel = 0;

            // a finished or unloaded instance is replaced by a new one with the new settings, a running one is kept
            IMapLock mapLock = MapLockManager.Instance.GetSoloLock(player.Identity, world.Id);
            if (mapLock != null && !mapLock.IsInstanceLoaded && (mapLock.Difficulty != difficulty || mapLock.PrimeLevel != primeLevel))
            {
                MapLockManager.Instance.RemoveSoloLock(player.Identity, world.Id);
                mapLock = null;
            }

            if (mapLock == null)
            {
                mapLock = MapLockManager.Instance.CreateSoloLock(player.Identity, world.Id);
                mapLock.Difficulty = difficulty;
                mapLock.PrimeLevel = primeLevel;
            }

            player.InstancePortalReturn = new MapPosition
            {
                Info     = new MapInfo { Entry = Map.Entry },
                Position = player.Position
            };

            player.TeleportTo(world, entrance.Position, mapLock);
            return true;
        }
    }
}
