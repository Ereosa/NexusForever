using System.Collections.Concurrent;
using System.Diagnostics;
using System.Numerics;
using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Entity.Synchronisation;
using NexusForever.Game.Abstract.Map;
using NexusForever.Game.Entity.Synchronisation;
using NexusForever.Game.Map;
using NexusForever.Game.Map.Search;
using NexusForever.Script;
using NexusForever.Script.Template;
using NexusForever.Script.Template.Collection;
using NexusForever.Shared;
using NexusForever.Shared.Game.Events;
using NLog;

namespace NexusForever.Game.Entity
{
    public abstract class GridEntity : IGridEntity
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        public uint Guid { get; protected set; }
        public IBaseMap Map { get; private set; }
        public Vector3 Position { get; protected set; }

        public IMapInfo PreviousMap { get; private set; }

        /// <summary>
        /// Determines if the <see cref="IGridEntity"/> is on a <see cref="IBaseMap"/>.
        /// </summary>
        public bool InWorld => Map != null;

        /// <summary>
        /// Determines if the <see cref="IGridEntity"/> is pending removal from the <see cref="IBaseMap"/>.
        /// </summary>
        public bool PendingRemoval { get; private set; }

        /// <summary>
        /// Distance between <see cref="IGridEntity"/> and a <see cref="IMapGrid"/> for activation.
        /// </summary>
        public float ActivationRange { get; protected set; }

        public float? RangeCheck { get; private set; }

        private readonly Dictionary<uint, IGridEntity> inRangeEntities = [];

        /// <summary>
        /// Range checks postponed until the vision update that triggered them has added both sides of every pair.
        /// </summary>
        [ThreadStatic]
        private static List<(IGridEntity Entity, IGridEntity Target)> deferredRangeChecks;

        protected readonly Dictionary<uint, IGridEntity> visibleEntities = [];
        protected readonly Dictionary<uint, IGridEntity> invisibleEntities = [];
        private readonly HashSet<(uint GridX, uint GridZ)> visibleGrids = [];

        protected IScriptCollection scriptCollection;

        protected readonly EventQueue eventQueue = new();

        private readonly ConcurrentQueue<ISynchronisationTask> synchronisationTaskQueue = [];

        /// <summary>
        /// Initialise <see cref="IGridEntity"/>
        /// </summary>
        public void Initialise()
        {
            InitialiseScriptCollection(null);
        }

        /// <summary>
        /// Initialise <see cref="IScriptCollection"/> for <see cref="IGridEntity"/>.
        /// </summary>
        protected abstract void InitialiseScriptCollection(List<string> names);

        public virtual void Dispose()
        {
            if (scriptCollection != null)
                ScriptManager.Instance.Unload(scriptCollection);
        }

        /// <summary>
        /// Invoked each world tick with the delta since the previous tick occurred.
        /// </summary>
        public virtual void Update(double lastTick)
        {
            eventQueue.Update(lastTick);
            HandlePendingSynchronisations();

            scriptCollection?.Invoke<IUpdate>(s => s.Update(lastTick));
        }

        /// <summary>
        /// Run <see cref="Action"/> on the entity in a thread-safe manner on the world thread.
        /// </summary>
        /// <param name="action">Action to run on the world thread.</param>
        public Task SynchroniseAsync(Action action)
        {
            var synchronisationTask = new ActionSynchronisationTask();
            synchronisationTaskQueue.Enqueue(synchronisationTask);
            return synchronisationTask.Initialise(action);
        }

        /// <summary>
        /// Run <see cref="Func{T}"/> on the entity in a thread-safe manner on the world thread.
        /// </summary>
        /// <param name="func">Function to run on the world thread.</param>
        public Task<T> SynchroniseAsync<T>(Func<T> func)
        {
            var synchronisationTask = new FunctionSynchronisationTask<T>();
            synchronisationTaskQueue.Enqueue(synchronisationTask);
            return synchronisationTask.Initialise(func);
        }

        private void HandlePendingSynchronisations()
        {
            while (synchronisationTaskQueue.TryDequeue(out ISynchronisationTask synchronisationTask))
                synchronisationTask.Execute();
        }

        /// <summary>
        /// Invoke <see cref="Action{T}"/> against <see cref="IGridEntity"/> script collection.
        /// </summary>
        public void InvokeScriptCollection<T>(Action<T> action)
        {
            scriptCollection?.Invoke(action);
        }

        /// <summary>
        /// Invoke <see cref="Func{TIn, TOut}"/> against <see cref="IGridEntity"/> script collection.
        /// </summary>
        public TOut? InvokeScriptCollection<TOut, TIn>(Func<TIn, TOut> func) where TOut : struct
        {
            return scriptCollection?.Invoke(func);
        }

        /// <summary>
        /// Enqueue <see cref="IGridEntity"/> for addition to the <see cref="IBaseMap"/>.
        /// </summary>
        public void AddToMap(IBaseMap map, Vector3 position, OnAddDelegate callback = null)
        {
            Debug.Assert(Map == null);

            if (callback != null)
            {
                map.EnqueueAdd(this, position, (map, guid, vector) =>
                {
                    OnAddToMap(map, guid, vector);
                    callback(map, guid, vector);
                });
            }
            else
                map.EnqueueAdd(this, position, OnAddToMap);
        }

        /// <summary>
        /// Enqueue <see cref="IGridEntity"/> for removal from the <see cref="IBaseMap"/>.
        /// </summary>
        public void RemoveFromMap(OnRemoveDelegate callback = null)
        {
            Debug.Assert(Map != null);

            if (PendingRemoval)
                return;

            PendingRemoval = true;

            if (callback != null)
            {
                Map.EnqueueRemove(this, () =>
                {
                    OnRemoveFromMap();
                    callback();
                });
            }
            else
                Map.EnqueueRemove(this, OnRemoveFromMap);
        }

        /// <summary>
        /// Enqueue <see cref="IGridEntity"/> for relocation on the <see cref="IBaseMap"/>.
        /// </summary>
        public void RelocateOnMap(Vector3 position, OnRelocateDelegate callback = null)
        {
            Debug.Assert(Map != null);

            if (callback != null)
            {
                Map.EnqueueRelocate(this, position, (vector) =>
                {
                    OnRelocate(vector);
                    callback(vector);
                });
            }
            else
                // movement relocates the entity every tick and only the latest position matters
                Map.EnqueueRelocate(this, position, OnRelocate, true);
        }

        /// <summary>
        /// Enqueue <see cref="IGridEntity"/> for visibility update on the <see cref="IBaseMap"/>.
        /// </summary>
        public void VisibilityUpdate()
        {
            Debug.Assert(Map != null);
            Map.EnqueueVisibilityUpdate(this, OnVisibilityUpdate);
        }

        /// <summary>
        /// Invoked when <see cref="IGridEntity"/> is enqueued to be added to <see cref="IBaseMap"/>.
        /// </summary>
        public virtual void OnEnqueueAddToMap()
        {
            // deliberately empty
        }

        /// <summary>
        /// Invoked when <see cref="IGridEntity"/> is added to <see cref="IBaseMap"/>.
        /// </summary>
        public virtual void OnAddToMap(IBaseMap map, uint guid, Vector3 vector)
        {
            Guid     = guid;
            Map      = map;
            Position = vector;

            UpdateVision();
            UpdateGridVision();
            UpdateRangeChecks();

            scriptCollection?.Invoke<IGridEntityScript>(s => s.OnAddToMap(map));
        }

        /// <summary>
        /// Invoked when <see cref="IGridEntity"/> is enqueued to be removed from <see cref="IBaseMap"/>.
        /// </summary>
        public virtual void OnEnqueueRemoveFromMap()
        {
            // deliberately empty
        }

        /// <summary>
        /// Invoked when <see cref="IGridEntity"/> is removed from <see cref="IBaseMap"/>.
        /// </summary>
        protected virtual void OnRemoveFromMap()
        {
            scriptCollection?.Invoke<IGridEntityScript>(s => s.OnRemoveFromMap(Map));

            foreach ((uint _, IGridEntity entity) in visibleEntities.Concat(invisibleEntities))
            {
                RemoveVisionEntity(entity);
                if (entity != this)
                    entity.RemoveVisionEntity(this);
            }

            foreach ((uint gridX, uint gridZ) in visibleGrids.ToList())
                RemoveVisible(gridX, gridZ);

            visibleGrids.Clear();

            inRangeEntities.Clear();

            PreviousMap = new MapInfo
            {
                Entry = Map.Entry
            };

            Guid           = 0;
            Map            = null;
            PendingRemoval = false;
        }

        /// <summary>
        /// Invoked when <see cref="IGridEntity"/> is relocated.
        /// </summary>
        protected virtual void OnRelocate(Vector3 vector)
        {
            Position = vector;

            UpdateVision();
            UpdateGridVision();
            UpdateRangeChecks();
        }

        /// <summary>
        /// Invoked when <see cref="IGridEntity"/> changes zone in the current <see cref="IBaseMap"/>.
        /// </summary>
        protected virtual void OnZoneUpdate()
        {
            // deliberately empty
        }

        /// <summary>
        /// Returns if <see cref="IGridEntity"/> can see supplied <see cref="IGridEntity"/>.
        /// </summary>
        protected virtual bool CanSeeEntity(IGridEntity entity)
        {
            return true;
        }

        /// <summary>
        /// Adds the specified <see cref="IGridEntity"/> to the appropriate vision collection based on its visibility.
        /// </summary>
        public void AddVisionEntity(IGridEntity entity)
        {
            if (CanSeeEntity(entity))
                AddVisible(entity);
            else
                AddInvisible(entity);
        }

        /// <summary>
        /// Add tracked <see cref="IGridEntity"/> that is in vision range.
        /// </summary>
        protected virtual void AddVisible(IGridEntity entity)
        {
            visibleEntities.Add(entity.Guid, entity);

            scriptCollection?.Invoke<IGridEntityScript>(s => s.OnAddVisibleEntity(entity));

            CheckEntityInRange(entity);
        }

        private void AddInvisible(IGridEntity entity)
        {
            invisibleEntities.Add(entity.Guid, entity);
        }

        /// <summary>
        /// Removes the specified vision entity from the grid, regardless of its current visibility state.
        /// </summary>
        public void RemoveVisionEntity(IGridEntity entity)
        {
            if (visibleEntities.ContainsKey(entity.Guid))
                RemoveVisible(entity);
            else
                RemoveInvisible(entity);
        }

        /// <summary>
        /// Remove tracked <see cref="IGridEntity"/> that is no longer in vision range.
        /// </summary>
        protected virtual void RemoveVisible(IGridEntity entity)
        {
            visibleEntities.Remove(entity.Guid);

            scriptCollection?.Invoke<IGridEntityScript>(s => s.OnRemoveVisibleEntity(entity));

            CheckEntityInRange(entity);
        }

        private void RemoveInvisible(IGridEntity entity)
        {
            invisibleEntities.Remove(entity.Guid);
        }

        private void OnVisibilityUpdate()
        {
            foreach ((uint _, IGridEntity entity) in invisibleEntities)
            {
                if (CanSeeEntity(entity))
                {
                    RemoveInvisible(entity);
                    AddVisible(entity);
                }
            }

            foreach ((uint _, IGridEntity entity) in visibleEntities)
            {
                if (!CanSeeEntity(entity))
                {
                    RemoveVisible(entity);
                    AddInvisible(entity);
                }
            }
        }

        public IEnumerable<IGridEntity> GetVisible()
        {
            return visibleEntities.Values;
        }

        /// <summary>
        /// Return visible <see cref="IGridEntity"/> by supplied guid.
        /// </summary>
        public T GetVisible<T>(uint guid) where T : IGridEntity
        {
            if (!visibleEntities.TryGetValue(guid, out IGridEntity entity))
                return default;
            // an entity of another kind (eg: a spell asking for a unit on an instance portal) is not a match
            return entity is T match ? match : default;
        }

        /// <summary>
        /// Return visible <see cref="IWorldEntity"/> by supplied creature id.
        /// </summary>
        public IEnumerable<T> GetVisibleCreature<T>(uint creatureId) where T : IWorldEntity
        {
            foreach (IGridEntity entity in visibleEntities.Values)
                if (entity is T match && match.CreatureId == creatureId)
                    yield return match;
        }

        /// <summary>
        /// Return visible <see cref="IPlayer"/> by supplied identity.
        /// </summary>
        public IPlayer GetVisiblePlayer(Abstract.Identity identity)
        {
            foreach (var item in visibleEntities.Values)
            {
                if (item is not IPlayer player)
                    continue;

                if (player.Identity == identity)
                    return player;
            }

            return null;
        }

        /// <summary>
        /// Update all <see cref="IGridEntity"/>'s in vision range.
        /// </summary>
        private void UpdateVision()
        {
            // a relocate for an entity that already left its map has nothing to see
            if (Map == null)
                return;

            var check = new SearchCheckRange<IGridEntity>();
            check.Initialise(Position, Map.VisionRange);

            // entities still being added have no guid yet, a duplicate key here used to stop the whole map processing grid actions
            Dictionary<uint, IGridEntity> entities = Map.Search(Position, Map.VisionRange, check)
                .Where(e => e != null && e.Guid != 0)
                .GroupBy(e => e.Guid)
                .ToDictionary(g => g.Key, g => g.First());

            // range events (eg: a creature aggroing a player) fire once both sides have the other in vision, otherwise the
            // combat messages reach a client that hasn't received the creature yet and it never learns who the creature targets
            bool deferring = deferredRangeChecks == null;
            if (deferring)
                deferredRangeChecks = [];

            try
            {
                // new entities now in vision range
                foreach ((uint guid, IGridEntity entity) in entities)
                {
                    if (visibleEntities.ContainsKey(guid)
                        || invisibleEntities.ContainsKey(guid))
                        continue;

                    // one broken entity must not stop the rest of the vision update
                    try
                    {
                        AddVisionEntity(entity);
                        if (entity != this)
                            entity.AddVisionEntity(this);
                    }
                    catch (Exception ex)
                    {
                        log.Error(ex, $"Failed to add entity {guid} to the vision of {Guid}!");
                    }
                }
            }
            finally
            {
                List<(IGridEntity Entity, IGridEntity Target)> pending = deferring ? deferredRangeChecks : [];
                if (deferring)
                    deferredRangeChecks = null;

                foreach ((IGridEntity checker, IGridEntity target) in pending)
                {
                    try
                    {
                        checker.CheckEntityInRange(target);
                    }
                    catch (Exception ex)
                    {
                        log.Error(ex, $"Failed range check of entity {target.Guid} for {checker.Guid}!");
                    }
                }
            }

            // old entities now out of vision range
            foreach ((uint guid, IGridEntity entity) in visibleEntities
                .Concat(invisibleEntities)
                .ToList())
            {
                if (entities.ContainsKey(guid))
                    continue;

                try
                {
                    RemoveVisionEntity(entity);
                    if (entity != this)
                        entity.RemoveVisionEntity(this);
                }
                catch (Exception ex)
                {
                    log.Error(ex, $"Failed to remove entity {guid} from the vision of {Guid}!");
                }
            }
        }

        /// <summary>
        /// Update all <see cref="IMapGrid"/>'s in vision range.
        /// </summary>
        private void UpdateGridVision()
        {
            Map.GridSearch(Position, Map.VisionRange, out List<IMapGrid> intersectedGrids);
            List<(uint X, uint Z)> visibleGridCoords = intersectedGrids
                .Select(g => g.Coord)
                .ToList();

            // new grids now in vision range
            foreach ((uint gridX, uint gridZ) in visibleGridCoords.Except(visibleGrids))
                AddVisible(gridX, gridZ);

            // old grids now out of vision range
            foreach ((uint gridX, uint gridZ) in visibleGrids.Except(visibleGridCoords).ToList())
                RemoveVisible(gridX, gridZ);
        }

        protected virtual void AddVisible(uint gridX, uint gridZ)
        {
            visibleGrids.Add((gridX, gridZ));
        }

        protected virtual void RemoveVisible(uint gridX, uint gridZ)
        {
            visibleGrids.Remove((gridX, gridZ));
        }

        protected void UpdateRangeChecks()
        {
            foreach ((uint _, IGridEntity entity) in visibleEntities)
                entity.CheckEntityInRange(this);
        }

        /// <summary>
        /// Set range check for <see cref="IGridEntity"/>.
        /// </summary>
        public void SetInRangeCheck(float range)
        {
            if (Map?.VisionRange < range)
                throw new ArgumentOutOfRangeException();

            RangeCheck = range;
        }

        /// <summary>
        /// Checks if the provided <see cref="IGridEntity"/> is at a range to trigger an event on this <see cref="IGridEntity"/>.
        /// </summary>
        public virtual void CheckEntityInRange(IGridEntity target)
        {
            if (!RangeCheck.HasValue)
                return;

            if (deferredRangeChecks != null)
            {
                deferredRangeChecks.Add((this, target));
                return;
            }

            if (!HasEnteredRange(target) && IsInRange(target))
                AddToRange(target);
            else if (HasEnteredRange(target) && !IsInRange(target))
                RemoveFromRange(target);
        }

        private bool IsInRange(IGridEntity target)
        {
            if (target is IUnitEntity unitEntity)
                if (!unitEntity.IsAlive)
                    return false;

            return Position.GetDistance(target.Position) < RangeCheck;
        }

        private bool HasEnteredRange(IGridEntity target)
        {
            return inRangeEntities.ContainsKey(target.Guid);
        }

        protected virtual void AddToRange(IGridEntity entity)
        {
            inRangeEntities.Add(entity.Guid, entity);

            scriptCollection?.Invoke<IGridEntityScript>(s => s.OnEnterRange(entity));
        }

        protected virtual void RemoveFromRange(IGridEntity entity)
        {
            scriptCollection?.Invoke<IGridEntityScript>(s => s.OnExitRange(entity));

            inRangeEntities.Remove(entity.Guid);
        }

        /// <summary>
        /// Returns all <see cref="IGridEntity"/> in range of this <see cref="IGridEntity"/>.
        /// </summary>
        public IEnumerable<T> GetInRange<T>(uint guid) where T : IGridEntity
        {
            return visibleEntities.Values.Cast<T>();
        }
    }
}
