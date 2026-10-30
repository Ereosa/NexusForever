using System.Collections.Concurrent;
using System.Diagnostics;
using System.Numerics;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using NexusForever.Database.World.Model;
using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Entity.Creature;
using NexusForever.Game.Abstract.Loot;
using NexusForever.Game.Abstract.Map;
using NexusForever.Game.Abstract.Map.Instance;
using NexusForever.Game.Abstract.Map.Search;
using NexusForever.Game.Abstract.PublicEvent;
using NexusForever.Game.Configuration.Model;
using NexusForever.Game.Static.Entity;
using NexusForever.Game.Static.Map;
using NexusForever.Game.Static.Spell;
using NexusForever.GameTable.Model;
using NexusForever.IO.Map;
using NexusForever.Network.Message;
using NexusForever.Network.World.Message.Static;
using NexusForever.Script;
using NexusForever.Script.Template;
using NexusForever.Script.Template.Collection;
using NexusForever.Shared;
using NexusForever.Shared.Configuration;
using NLog;

namespace NexusForever.Game.Map
{
    public class BaseMap : IBaseMap
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        public const float DefaultVisionRange = 128f;

        /// <summary>
        /// Distance between a <see cref="IPlayer"/> and a <see cref="IGridEntity"/> before the entity can be seen.
        /// </summary>
        public virtual float? VisionRange { get; protected set; } = DefaultVisionRange;

        public WorldEntry Entry { get; private set; }
        public MapFile File { get; private set; }

        public IPublicEventManager PublicEventManager { get; }
        public IPlayerManager PlayerManager { get; }
        public ILootManager LootManager { get; }

        private readonly IMapGrid[] grids = new MapGrid[MapDefines.WorldGridCount * MapDefines.WorldGridCount];
        private readonly HashSet<(uint GridX, uint GridZ)> activeGrids = new();

        protected readonly ConcurrentQueue<IGridAction> pendingActions = new();

        // adds of non-player entities (grid activation queues thousands) wait here so they can not hold up
        // removes, teleports and player adds queued behind them
        private readonly ConcurrentQueue<IGridAction> pendingBulkAdds = new();

        /// <summary>
        /// Number of grid actions waiting to be processed.
        /// </summary>
        protected int PendingActionCount => pendingActions.Count + pendingBulkAdds.Count;

        // latest position of entities with a relocate already queued, moving entities relocate every tick and only the last
        // position matters: one queued relocate per entity keeps the queue from outgrowing the per tick threshold
        private readonly ConcurrentDictionary<IGridEntity, (Vector3 Position, OnRelocateDelegate Callback)> queuedRelocates = new();

        private readonly QueuedCounter entityCounter = new();
        protected readonly Dictionary<uint /*guid*/, IGridEntity> entities = new();
        private IEntityCache entityCache;

        private sealed class PendingRespawn
        {
            public ICreatureInfo CreatureInfo { get; init; }
            public EntityModel Model { get; init; }
            public double Timer { get; set; }
        }

        private readonly List<PendingRespawn> pendingRespawns = [];
        private readonly HashSet<EntityModel> respawningModels = [];
        private readonly List<(uint Guid, double Timer)> pendingDespawns = [];

        protected IScriptCollection scriptCollection;

        #region Dependency Injection

        private readonly IEntityFactory entityFactory;

        public BaseMap(
            IEntityFactory entityFactory,
            IPublicEventManager publicEventManager,
            IPlayerManager playerManager,
            ILootManager lootManager)
        {
            this.entityFactory = entityFactory;

            PublicEventManager = publicEventManager;
            PlayerManager      = playerManager;
            LootManager        = lootManager;
        }

        #endregion

        /// <summary>
        /// Initialise <see cref="IBaseMap"/> with <see cref="WorldEntry"/>.
        /// </summary>
        public virtual void Initialise(WorldEntry entry)
        {
            Entry       = entry;
            File        = MapIOManager.Instance.GetBaseMap(Entry.AssetPath);
            entityCache = EntityCacheManager.Instance.GetEntityCache((ushort)Entry.Id);

            PublicEventManager.Initialise(this);
            LootManager.Initialise(this);

            InitialiseScriptCollection();
        }

        protected virtual void InitialiseScriptCollection()
        {
            scriptCollection = ScriptManager.Instance.InitialiseOwnedCollection<IBaseMap>(this);
            ScriptManager.Instance.InitialiseOwnedScripts<IBaseMap>(scriptCollection, Entry.Id);
        }

        /// <summary>
        /// Invoked each world tick with the delta since the previous tick occurred.
        /// </summary>
        public virtual void Update(double lastTick)
        {
            ProcessGridActions();
            UpdateGrids(lastTick);
            UpdateRespawns(lastTick);

            scriptCollection?.Invoke<IUpdate>(s => s.Update(lastTick));

            PublicEventManager.Update(lastTick);
        }

        private void ProcessGridActions()
        {
            var newActions = new List<IGridAction>();
            foreach (IGridAction action in DequeueGridActions(Stopwatch.StartNew()))
            {
                try
                {
                    switch (action)
                    {
                        case IGridActionAdd actionAdd:
                        {
                            if (!ProcessGridActionAdd(actionAdd))
                            {
                                actionAdd.RequeueCount++;
                                if (actionAdd.RequeueCount > (SharedConfiguration.Instance.Get<MapConfig>().GridActionMaxRetry ?? 5u))
                                    throw new TimeoutException($"Failed to add player to map {Entry.Id} at position X: {actionAdd.Vector.X}, Y: {actionAdd.Vector.Y}, Z: {actionAdd.Vector.Z}!");
                                else
                                    newActions.Add(action);
                            }
                            break;
                        }
                        case IGridActionRelocate actionRelocate:
                        {
                            Vector3 vector = actionRelocate.Vector;
                            OnRelocateDelegate callback = actionRelocate.Callback;
                            if (actionRelocate.Coalesced)
                            {
                                // the position was already used by an earlier action for the entity
                                if (!queuedRelocates.TryRemove(actionRelocate.Entity, out var latest))
                                    break;

                                vector   = latest.Position;
                                callback = latest.Callback;
                            }

                            // the entity left this map (teleport, logout) after the relocate was queued, relocating it
                            // would put it back in the grid as a ghost that breaks vision updates and the next login
                            if (actionRelocate.Entity.Map != this)
                                break;

                            RelocateEntity(actionRelocate.Entity, vector);
                            callback?.Invoke(vector);
                            break;
                        }
                        case IGridActionRemove actionRemove:
                        {
                            RemoveEntity(actionRemove.Entity);
                            actionRemove.Callback?.Invoke();
                            break;
                        }
                        case IGridActionVisibilityUpdate actionVisionUpdate:
                            actionVisionUpdate.Callback?.Invoke();
                            break;
                    }
                }
                catch (Exception ex)
                {
                    log.Error(ex, $"Failed to process grid action for map {Entry.Id}!");
                    action.Exception?.Invoke(ex);
                }
            }

            // new actions are added to the queue after processing so they are processed starting next update
            foreach (IGridAction action in newActions)
                QueueAction(action);
        }

        private void QueueAction(IGridAction action)
        {
            if (action is IGridActionAdd { Entity: not IPlayer })
                pendingBulkAdds.Enqueue(action);
            else
                pendingActions.Enqueue(action);
        }

        /// <summary>
        /// Dequeue the pending grid actions to process this update.
        /// </summary>
        /// <remarks>
        /// Coalesced relocates have their own, larger, budget. They are limited to one per moving entity but a busy map has hundreds of
        /// moving entities, sharing the budget with adds and removes left the position of every entity many seconds behind
        /// the position the client is shown: spells and attacks aimed at a creature missed it and creatures hit from where they stood.
        /// Removes, teleports (relocate callbacks), visibility updates and player adds keep the order they were queued in, past the
        /// <see cref="MapConfig.GridActionThreshold"/> they keep going until the update has used its time budget.
        /// Adds of other entities are in a lane of their own that gets the rest of the time budget, a grid activation queues
        /// thousands of them and a minute of waiting behind them stalled teleports, removes and every vision update.
        /// </remarks>
        private IEnumerable<IGridAction> DequeueGridActions(Stopwatch stopwatch)
        {
            var config = SharedConfiguration.Instance.Get<MapConfig>();
            uint actionThreshold = config.GridActionThreshold ?? 100u;
            uint relocateBudget  = config.GridRelocateThreshold ?? 1000u;
            long timeBudget      = config.GridActionTimeBudget ?? 30u;
            uint bulkMinimum     = config.GridBulkAddMinimum ?? 25u;

            // actions queued while processing wait for the next update
            uint processed = 0u;
            int count = pendingActions.Count;
            for (int i = 0; i < count; i++)
            {
                if (!pendingActions.TryPeek(out IGridAction action))
                    break;

                // stop at the first action over budget, later actions must not overtake it
                bool relocate = action is IGridActionRelocate { Coalesced: true };
                if (relocate)
                {
                    if (relocateBudget == 0u)
                        break;
                }
                else if (processed >= actionThreshold && stopwatch.ElapsedMilliseconds >= timeBudget)
                    break;

                if (!pendingActions.TryDequeue(out action))
                    break;

                if (relocate)
                    relocateBudget--;
                else
                    processed++;

                yield return action;
            }

            uint bulkProcessed = 0u;
            count = pendingBulkAdds.Count;
            for (int i = 0; i < count; i++)
            {
                if (bulkProcessed >= bulkMinimum && stopwatch.ElapsedMilliseconds >= timeBudget)
                    break;

                if (!pendingBulkAdds.TryDequeue(out IGridAction action))
                    break;

                bulkProcessed++;
                yield return action;
            }
        }

        private bool ProcessGridActionAdd(IGridActionAdd actionAdd)
        {
            if (!CanAddEntity(actionAdd.Entity, actionAdd.Vector))
                return false;

            if (actionAdd is IGridActionPlayerAdd actionPlayerAdd)
            {
                GenericError? error = CanEnter(actionAdd.Entity as IPlayer, actionAdd.Vector);
                if (error != null)
                {
                    actionPlayerAdd.Error?.Invoke(error.Value);
                    return true;
                }
            }

            AddEntity(actionAdd.Entity, actionAdd.Vector, actionAdd.Callback);
            return true;
        }

        private void UpdateGrids(double lastTick)
        {
            var unloadedGrids = new List<IMapGrid>();
            foreach ((uint gridX, uint gridZ) in activeGrids)
            {
                IMapGrid grid = GetGrid(gridX, gridZ);
                grid.Update(lastTick);

                if (grid.UnloadStatus == MapUnloadStatus.Complete)
                    unloadedGrids.Add(grid);
            }

            foreach (IMapGrid grid in unloadedGrids)
            {
                grids[grid.Coord.Z * MapDefines.WorldGridCount + grid.Coord.X] = null;
                activeGrids.Remove(grid.Coord);

                log.Trace($"Deactivated grid at X:{grid.Coord.X}, Z:{grid.Coord.Z}.");
            }
        }

        /// <summary>
        /// Enqueue <see cref="IGridEntity"/> to be added to <see cref="IBaseMap"/>.
        /// </summary>
        public void EnqueueAdd(IGridEntity entity, Vector3 position, OnAddDelegate callback = null, OnExceptionDelegate exception = null)
        {
            entity.OnEnqueueAddToMap();

            QueueAction(new GridActionAdd
            {
                Entity    = entity,
                Vector    = position,
                Callback  = callback,
                Exception = exception
            });
        }

        /// <summary>
        /// Enqueue <see cref="IPlayer"/> to be added to <see cref="IBaseMap"/>.
        /// </summary>
        /// <remarks>
        /// Characters should not be added directly through this method.
        /// Use <see cref="IPlayer.TeleportTo(IMapPosition, TeleportReason)"/> instead.
        /// </remarks>
        public void EnqueueAdd(IPlayer player, Vector3 position, OnAddDelegate callback = null, OnGenericErrorDelegate error = null, OnExceptionDelegate exception = null)
        {
            player.OnEnqueueAddToMap();

            QueueAction(new GridActionPlayerAdd
            {
                Entity    = player,
                Vector    = position,
                Callback  = callback,
                Error     = error,
                Exception = exception
            });
        }

        /// <summary>
        /// Returns if <see cref="IGridEntity"/> can be added to <see cref="IBaseMap"/>.
        /// </summary>
        protected virtual bool CanEnter(IGridEntity entity, Vector3 position)
        {
            if (!IsValidPosition(position))
                return false;

            return true;
        }

        /// <summary>
        /// Returns if <see cref="IPlayer"/> can be added to <see cref="IBaseMap"/>.
        /// </summary>
        protected virtual GenericError? CanEnter(IPlayer player, Vector3 position)
        {
            if (!IsValidPosition(position))
                return GenericError.InstanceInvalidDestination;

            return null;
        }

        private bool IsValidPosition(Vector3 position)
        {
            // make sure player position falls within a valid grid
            int radius = (MapDefines.GridSize * MapDefines.WorldGridCount) / 2;
            if (position.X < -radius || position.X > radius
                || position.Z < -radius || position.Z > radius)
                return false;

            // TODO: check if map file has a grid at position?

            return true;
        }

        /// <summary>
        /// Enqueue <see cref="IGridEntity"/> to be removed from <see cref="IBaseMap"/>.
        /// </summary>
        public void EnqueueRemove(IGridEntity entity, OnRemoveDelegate callback = null)
        {
            entity.OnEnqueueRemoveFromMap();

            pendingActions.Enqueue(new GridActionRemove
            {
                Entity   = entity,
                Callback = callback
            });
        }

        /// <summary>
        /// Enqueue <see cref="IGridEntity"/> to be relocated in <see cref="IBaseMap"/> to <see cref="Vector3"/>.
        /// </summary>
        public void EnqueueRelocate(IGridEntity entity, Vector3 position, OnRelocateDelegate callback = null, bool coalesce = false)
        {
            if (coalesce)
            {
                // when a coalesced relocate is already queued for the entity it moves to the latest position instead
                bool added = false;
                queuedRelocates.AddOrUpdate(entity,
                    _ =>
                    {
                        added = true;
                        return (position, callback);
                    },
                    (_, _) => (position, callback));

                if (!added)
                    return;
            }

            pendingActions.Enqueue(new GridActionRelocate
            {
                Entity    = entity,
                Vector    = position,
                Callback  = callback,
                Coalesced = coalesce
            });
        }

        /// <summary>
        /// Enqueue <see cref="IGridEntity"/> for visibility update.
        /// </summary>
        public void EnqueueVisibilityUpdate(IGridEntity entity, OnVisibilityUpdateDelegate callback = null)
        {
            pendingActions.Enqueue(new GridActionVisibilityUpdate
            {
                Entity   = entity,
                Callback = callback
            });
        }

        /// <summary>
        /// Return all <see cref="IGridEntity"/>'s from <see cref="Vector3"/> in range that satisfy <see cref="ISearchCheck{T}"/>.
        /// </summary>
        public IEnumerable<T> Search<T>(Vector3 vector, float? radius, ISearchCheck<T> check) where T : IGridEntity
        {
            // no radius is unlimited distance
            if (radius == null)
            {
                foreach (T entity in entities.Values.OfType<T>().Where(check.CheckEntity))
                    yield return entity;
                yield break;
            }

            for (float z = vector.Z - radius.Value; z < vector.Z + radius.Value + MapDefines.GridCellSize; z += MapDefines.GridCellSize)
            {
                for (float x = vector.X - radius.Value; x < vector.X + radius.Value + MapDefines.GridCellSize; x += MapDefines.GridCellSize)
                {
                    var searchVector = new Vector3(x, 0f, z);

                    // don't activate new grids during search
                    IMapGrid grid = GetGrid(searchVector);
                    if (grid == null)
                        continue;

                    foreach (T entity in grid.Search(searchVector, check))
                        yield return entity;
                }
            }
        }

        /// <summary>
        /// Return all <see cref="IMapGrid"/>'s from <see cref="Vector3"/> in range.
        /// </summary>
        public void GridSearch(Vector3 vector, float? radius, out List<IMapGrid> intersectedGrids)
        {
            // negative radius is unlimited distance
            if (radius == null)
            {
                intersectedGrids = GetActiveGrids().ToList();
                return;
            }

            intersectedGrids = new List<IMapGrid>();
            for (float z = vector.Z - radius.Value; z < vector.Z + radius.Value + MapDefines.GridSize; z += MapDefines.GridSize)
            {
                for (float x = vector.X - radius.Value; x < vector.X + radius.Value + MapDefines.GridSize; x += MapDefines.GridSize)
                {
                    IMapGrid grid = GetGrid(new Vector3(x, 0f, z));
                    if (grid != null)
                        intersectedGrids.Add(grid);
                }
            }
        }

        /// <summary>
        /// Return <see cref="IGridEntity"/> by guid.
        /// </summary>
        public T GetEntity<T>(uint guid) where T : IGridEntity
        {
            if (!entities.TryGetValue(guid, out IGridEntity entity))
                return default;
            return (T)entity;
        }

        /// <summary>
        /// Enqueue broadcast of <see cref="IWritable"/> to all <see cref="IPlayer"/>'s on the map.
        /// </summary>
        public void EnqueueToAll(IWritable message)
        {
            foreach (IGridEntity entity in entities.Values)
            {
                IPlayer player = entity as IPlayer;
                player?.Session.EnqueueMessageEncrypted(message);
            }
        }

        /// <summary>
        /// Notify <see cref="IMapGrid"/> at coordinates of the addition of new <see cref="IPlayer"/> that is in vision range.
        /// </summary>
        public void GridAddVisiblePlayer(uint gridX, uint gridZ)
        {
            IMapGrid grid = GetGrid(gridX, gridZ);
            grid.AddVisiblePlayer();
        }

        /// <summary>
        /// Notify <see cref="IMapGrid"/> at coordinates of the removal of an existing <see cref="IPlayer"/> that is no longer in vision range.
        /// </summary>
        public void GridRemoveVisiblePlayer(uint gridX, uint gridZ)
        {
            IMapGrid grid = GetGrid(gridX, gridZ);
            grid.RemoveVisiblePlayer();
        }

        private IMapGrid GetGrid(uint gridX, uint gridZ)
        {
            return grids[gridZ * MapDefines.WorldGridCount + gridX];
        }

        private IMapGrid GetGrid(Vector3 vector)
        {
            (uint gridX, uint gridZ) = MapGrid.GetGridCoord(vector);
            return GetGrid(gridX, gridZ);
        }

        protected IEnumerable<IMapGrid> GetActiveGrids()
        {
            return activeGrids
                .Select(g => GetGrid(g.GridX, g.GridZ));
        }

        /// <summary>
        /// Activate one or more <see cref="IMapGrid"/>'s at <see cref="Vector"/> depending on vision range of <see cref="IGridEntity"/>.
        /// </summary>
        private void ActivateGrid(IGridEntity entity, Vector3 vector)
        {
            float range = entity.ActivationRange;
            for (float z = vector.Z - range; z < vector.Z + range + MapDefines.GridSize; z += MapDefines.GridSize)
            {
                for (float x = vector.X - range; x < vector.X + range + MapDefines.GridSize; x += MapDefines.GridSize)
                {
                    (uint gridX, uint gridZ) = MapGrid.GetGridCoord(new Vector3(x, 0f, z));
                    IMapGrid grid = GetGrid(gridX, gridZ);
                    if (grid == null)
                        ActivateGrid(gridX, gridZ);
                }
            }
        }

        /// <summary>
        /// Activate single <see cref="IMapGrid"/> at position.
        /// </summary>
        private void ActivateGrid(uint gridX, uint gridZ)
        {
            // instance grids are not unloaded, the entire instance is unloaded instead during inactivity
            var grid = new MapGrid(gridX, gridZ, this is not IMapInstance);
            grids[gridZ * MapDefines.WorldGridCount + gridX] = grid;
            activeGrids.Add(grid.Coord);

            log.Trace($"Activated grid at X:{gridX}, Z:{gridZ}.");

            SpawnGrid(gridX, gridZ);
        }

        protected virtual void SpawnGrid(uint gridX, uint gridZ)
        {
            var creatureInfoManager = LegacyServiceProvider.Provider.GetService<ICreatureInfoManager>();

            foreach (EntityModel model in entityCache.GetEntities(gridX, gridZ))
            {
                // entity will be spawned when the pending respawn completes
                if (respawningModels.Contains(model))
                    continue;

                ICreatureInfo creatureInfo = creatureInfoManager.GetCreatureInfo(model.Creature);
                if (creatureInfo == null)
                    continue;

                SpawnEntity(creatureInfo, model);
            }
        }

        /// <summary>
        /// Disable respawns on the map, dead creatures and used objects stay gone (eg: expeditions and dungeons).
        /// </summary>
        public bool RespawnsDisabled { get; set; }

        private void SpawnEntity(ICreatureInfo creatureInfo, EntityModel model)
        {
            IWorldEntity entity = entityFactory.CreateWorldEntity(model.Type);
            entity.Initialise(creatureInfo, model);
            entity.AddToMap(this, new Vector3(model.X, model.Y, model.Z));
        }

        /// <summary>
        /// Spawn a new entity from the supplied <see cref="EntityModel"/> after a delay, used for entities that respawn after being removed (eg: harvesting nodes).
        /// </summary>
        /// <remarks>
        /// The entity isn't spawned with its grid while the respawn is pending.
        /// </remarks>
        public void ScheduleRespawn(ICreatureInfo creatureInfo, EntityModel model, TimeSpan delay)
        {
            if (RespawnsDisabled)
                return;

            if (!respawningModels.Add(model))
                return;

            pendingRespawns.Add(new PendingRespawn
            {
                CreatureInfo = creatureInfo,
                Model        = model,
                Timer        = delay.TotalSeconds
            });
        }

        public void ScheduleDespawn(uint guid, TimeSpan delay)
        {
            pendingDespawns.Add((guid, delay.TotalSeconds));
        }

        private void UpdateRespawns(double lastTick)
        {
            for (int i = pendingDespawns.Count - 1; i >= 0; i--)
            {
                (uint guid, double timer) = pendingDespawns[i];
                timer -= lastTick;
                if (timer > 0d)
                {
                    pendingDespawns[i] = (guid, timer);
                    continue;
                }

                pendingDespawns.RemoveAt(i);
                GetEntity<IWorldEntity>(guid)?.RemoveFromMap();
            }

            for (int i = pendingRespawns.Count - 1; i >= 0; i--)
            {
                PendingRespawn respawn = pendingRespawns[i];
                respawn.Timer -= lastTick;
                if (respawn.Timer > 0d)
                    continue;

                pendingRespawns.RemoveAt(i);
                respawningModels.Remove(respawn.Model);

                // if the grid isn't active the entity will be spawned with the grid when it next activates
                var position = new Vector3(respawn.Model.X, respawn.Model.Y, respawn.Model.Z);
                IMapGrid grid = GetGrid(position);
                if (grid == null || grid.UnloadStatus.HasValue)
                    continue;

                SpawnEntity(respawn.CreatureInfo, respawn.Model);
            }
        }

        private bool CanAddEntity(IGridEntity entity, Vector3 vector)
        {
            ActivateGrid(entity, vector);

            // if the grid doesn't exist we can't add the new entity to it
            IMapGrid grid = GetGrid(vector);
            if (grid == null)
                throw new InvalidOperationException($"Failed to get grid for map {Entry.Id} at at position X: {vector.X}, Y: {vector.Y}, Z: {vector.Z}!");

            // if the grid is unloading we can't add the new entity to it
            // we will need to wait for the grid to fully unload
            if (grid.UnloadStatus.HasValue)
                return false;

            return true;
        }

        protected virtual void AddEntity(IGridEntity entity, Vector3 vector, OnAddDelegate add = null)
        {
            Debug.Assert(entity.Map == null);

            IMapGrid grid = GetGrid(vector);
            grid.AddEntity(entity, vector);

            uint guid = entityCounter.Dequeue();
            entities.Add(guid, entity);

            // RemoveEntity drops the player from the player manager on every map change, so add them back here
            // or identity lookups (friends, whispers, groups, guilds) stop finding them until they relog
            if (entity is IPlayer player)
                PlayerManager.AddPlayer(player);

            add?.Invoke(this, guid, vector);

            PublicEventManager.OnAddToMap(entity);
            scriptCollection?.Invoke<IMapScript>(s => s.OnAddToMap(entity));

            log.Trace($"Added entity {entity.Guid} to map {Entry.Id} at {vector.X},{vector.Y},{vector.Z}.");
        }

        protected virtual void RemoveEntity(IGridEntity entity, OnRemoveDelegate remove = null)
        {
            Debug.Assert(entity.Map != null);

            // removing from the grid at the entity's position failed with an overflow every tick when the entity had moved to another
            // grid without a relocate, the entity then never left the map; look through the active grids for it instead
            if (GetGrid(entity.Position)?.RemoveEntity(entity) != true)
            {
                foreach ((uint gridX, uint gridZ) in activeGrids.ToList())
                    if (GetGrid(gridX, gridZ)?.RemoveEntity(entity) == true)
                        break;
            }

            log.Trace($"Removed entity {entity.Guid} from map {Entry.Id}.");

            entityCounter.Enqueue(entity.Guid);
            entities.Remove(entity.Guid);

            if (entity is IPlayer player)
                PlayerManager.RemovePlayer(player);

            scriptCollection?.Invoke<IMapScript>(s => s.OnRemoveFromMap(entity));
            PublicEventManager.OnRemoveFromMap(entity);

            remove?.Invoke();
        }

        protected virtual void RelocateEntity(IGridEntity entity, Vector3 vector)
        {
            Debug.Assert(entity.Map != null);

            ActivateGrid(entity, vector);
            IMapGrid newGrid = GetGrid(vector);
            IMapGrid oldGrid = GetGrid(entity.Position);

            // the old grid can be gone (unloaded) when the entity's position moved on without a relocate
            if (oldGrid == null
                || newGrid.Coord.X != oldGrid.Coord.X
                || newGrid.Coord.Z != oldGrid.Coord.Z)
            {
                oldGrid?.RemoveEntity(entity);
                newGrid.AddEntity(entity, vector);
            }
            else
                oldGrid.RelocateEntity(entity, vector);
        }

        /// <summary>
        /// Return terrain height at supplied position.
        /// </summary>
        public float? GetTerrainHeight(float x, float z)
        {
            // TODO: handle cases for water and props
            return File.GetTerrainHeight(new Vector3(x, 0, z));
        }

        /// <summary>
        /// Invoked when a <see cref="IPlayer"/> on the map dies.
        /// </summary>
        public virtual void OnDeath(IPlayer player)
        {
            ResurrectionType type = GetResurrectionType();

            if ((type & ResurrectionType.WakeHere | ResurrectionType.WakeHereServiceToken) != 0)
            {
                // TODO: Replace with DelayEvent (of 2 seconds) with map updates.
                IGhostEntity ghost = entityFactory.CreateEntity<IGhostEntity>();
                ghost.Initialise(player);
                ghost.AddToMap(this, player.Position);
            }
            else
                player.ResurrectionManager.ShowResurrection(type, null, null);
        }

        /// <summary>
        /// Return <see cref="ResurrectionType"/> applicable to this map.
        /// </summary>
        public virtual ResurrectionType GetResurrectionType()
        {
            // TODO: add support for Holocrypts
            return ResurrectionType.WakeHere | ResurrectionType.WakeHereServiceToken;
        }

        /// <summary>
        /// Resurrect <see cref="IPlayer"/> with supplied <see cref="ResurrectionType"/>.
        /// </summary>
        public virtual void Resurrect(ResurrectionType type, IPlayer player)
        {
            switch (type)
            {
                /*case ResurrectionType.Holocrypt:
                    OnResurrectHolocrypt(player);
                    break;*/
                default:
                    OnResurrect(player);
                    break;
            }
        }

        private void OnResurrectHolocrypt(IPlayer player)
        {
            // TODO: add support for Holocrypts
            throw new NotImplementedException();
        }

        private void OnResurrect(IPlayer player)
        {
            player.ModifyHealth(player.MaxHealth / 2, DamageType.Heal, null);
            player.Shield = 0;
        }

        /// <summary>
        /// Invoked when <see cref="IPublicEvent"/> finishes with the winning <see cref="IPublicEventTeam"/>.
        /// </summary>
        public virtual void OnPublicEventFinish(IPublicEvent publicEvent, IPublicEventTeam publicEventTeam)
        {
            scriptCollection.Invoke<IMapScript>(s => s.OnPublicEventFinish(publicEvent, publicEventTeam));
        }

        /// <summary>
        /// Invoked when a <see cref="IPlayer"/> on the map moves into a different zone.
        /// </summary>
        public virtual void OnPlayerZoneChange(IPlayer player, uint zoneId)
        {
            scriptCollection?.Invoke<IMapScript>(s => s.OnPlayerZoneChange(player, zoneId));
        }

        /// <summary>
        /// Return a string containing debug information about the map.
        /// </summary>
        public virtual string WriteDebugInformation()
        {
            var sb = new StringBuilder();
            sb.AppendLine($"World Id: {Entry.Id}");
            sb.AppendLine($"Grid Count: {activeGrids.Count}");
            sb.Append($"Entity Count: {entities.Count}");
            return sb.ToString();
        }        
    }
}
