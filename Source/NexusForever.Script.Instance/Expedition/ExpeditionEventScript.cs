using System.Numerics;
using NexusForever.Game.Abstract.Cinematic;
using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Entity.Creature;
using NexusForever.Game.Abstract.Entity.Trigger;
using NexusForever.Game.Abstract.Map;
using NexusForever.Game.Abstract.Map.Instance;
using NexusForever.Game.Abstract.PublicEvent;
using NexusForever.Game.Abstract.Quest;
using NexusForever.Game.Abstract.Spell;
using NexusForever.Game.Static;
using NexusForever.Game.Static.Cinematic;
using NexusForever.Game.Static.Entity;
using NexusForever.Game.Static.Entity.Movement.Spline;
using NexusForever.Game.Static.PublicEvent;
using NexusForever.Game.Static.Setting;
using NexusForever.GameTable;
using NexusForever.GameTable.Model;
using NexusForever.Script.Template;
using NexusForever.Shared;
using NexusForever.Shared.Game;

namespace NexusForever.Script.Instance.Expedition
{
    /// <summary>
    /// Shared helpers for expedition public event scripts rebuilt without packet captures (Outpost M-13, Infestation, Rage Logic...).
    /// </summary>
    /// <remarks>
    /// Difficulty: veteran (prime) runs swap creatures for their veteran versions through <see cref="IPublicEvent.CreatureOverride"/>,
    /// veteran only spawns are left out on normal. Prime levels give veteran creatures menace (VeteranTier).
    /// Scripted spawns are tracked by reference, the map only assigns their guid once they are added.
    /// </remarks>
    public abstract class ExpeditionEventScript : IPublicEventScript, IOwnedScript<IPublicEvent>
    {
        protected IPublicEvent publicEvent;
        protected IContentMapInstance mapInstance;
        protected WorldDifficulty difficulty;
        protected byte primeLevel;

        protected bool IsVeteran => difficulty == WorldDifficulty.Veteran;

        private readonly List<(double Timer, Action Action)> delayed = [];
        private readonly List<IWorldEntity> objectiveRings = [];
        private readonly UpdateTimer checkTimer = new(TimeSpan.FromSeconds(0.5d));
        private bool noDeaths = true;

        /// <summary>
        /// World the expedition is on.
        /// </summary>
        protected abstract uint WorldId { get; }

        /// <summary>
        /// Normal creatures and their veteran versions.
        /// </summary>
        protected abstract IReadOnlyDictionary<uint, uint> VeteranCreatures { get; }

        /// <summary>
        /// Creatures that only exist on veteran, besides the veteran versions themselves.
        /// </summary>
        protected virtual IReadOnlyCollection<uint> VeteranOnlyCreatures => [];

        /// <summary>
        /// Phase the event starts in.
        /// </summary>
        protected abstract uint InitialPhase { get; }

        #region Dependency Injection

        protected readonly IGlobalQuestManager globalQuestManager;
        protected readonly ICreatureInfoManager creatureInfoManager;
        protected readonly IEntityFactory entityFactory;
        protected readonly IFactory<ISpellParameters> spellParametersFactory;
        protected readonly IGameTableManager gameTableManager;
        protected readonly ICinematicFactory cinematicFactory;

        protected ExpeditionEventScript(
            IGlobalQuestManager globalQuestManager,
            ICreatureInfoManager creatureInfoManager,
            IEntityFactory entityFactory,
            IFactory<ISpellParameters> spellParametersFactory,
            IGameTableManager gameTableManager,
            ICinematicFactory cinematicFactory)
        {
            this.globalQuestManager     = globalQuestManager;
            this.creatureInfoManager    = creatureInfoManager;
            this.entityFactory          = entityFactory;
            this.spellParametersFactory = spellParametersFactory;
            this.gameTableManager       = gameTableManager;
            this.cinematicFactory       = cinematicFactory;
        }

        #endregion

        /// <summary>
        /// Invoked when <see cref="IScript"/> is loaded.
        /// </summary>
        public void OnLoad(IPublicEvent owner)
        {
            publicEvent = owner;
            mapInstance = publicEvent.Map as IContentMapInstance;

            difficulty = mapInstance?.MapLock?.Difficulty ?? WorldDifficulty.Normal;
            primeLevel = mapInstance?.MapLock?.PrimeLevel ?? 0;

            publicEvent.CreatureOverride = GetCreature;
            publicEvent.SetPhase(InitialPhase);
        }

        /// <summary>
        /// Creatures left out of this run (e.g. the hallucinations a run of Space Madness didn't pick), normal ids.
        /// </summary>
        protected virtual bool IsExcluded(uint creatureId) => false;

        protected uint GetCreature(uint creatureId)
        {
            if (IsExcluded(creatureId))
                return 0u;

            if (IsVeteran)
                return VeteranCreatures.TryGetValue(creatureId, out uint veteranId) ? veteranId : creatureId;

            if (VeteranOnlyCreatures.Contains(creatureId) || VeteranCreatures.Values.Contains(creatureId))
                return 0u;

            return creatureId;
        }

        #region Entities

        /// <summary>
        /// Invoked when a <see cref="IGridEntity"/> is added to the map the public event is on.
        /// </summary>
        public virtual void OnAddToMap(IGridEntity entity)
        {
            if (entity is not IWorldEntity worldEntity)
                return;

            ApplyPrimeLevel(worldEntity);
            if (worldEntity.CreatureId != 0u)
            {
                if (!entitiesByCreature.TryGetValue(worldEntity.CreatureId, out HashSet<uint> guids))
                    entitiesByCreature.Add(worldEntity.CreatureId, guids = []);
                guids.Add(worldEntity.Guid);
            }
        }

        private readonly Dictionary<uint, HashSet<uint>> entitiesByCreature = [];

        /// <summary>
        /// Entities of a creature (or its veteran version) currently on the map.
        /// </summary>
        protected IEnumerable<T> GetEntities<T>(uint creatureId) where T : class, IWorldEntity
        {
            if (!entitiesByCreature.TryGetValue(GetCreature(creatureId) is uint id && id != 0u ? id : creatureId, out HashSet<uint> guids))
                yield break;

            foreach (uint guid in guids.ToList())
                if (mapInstance.GetEntity<T>(guid) is T entity)
                    yield return entity;
        }

        /// <summary>
        /// Prime levels give the veteran creatures menace (VeteranTier, by the world's veteran tier scaling type).
        /// </summary>
        private void ApplyPrimeLevel(IWorldEntity entity)
        {
            if (!IsVeteran || primeLevel == 0 || entity is not IUnitEntity || entity.CreatureInfo?.Entry?.Creature2TierId is not (2u or 5u))
                return;

            WorldEntry world = gameTableManager.World.GetEntry(WorldId);
            VeteranTierEntry tier = gameTableManager.VeteranTier.Entries
                .FirstOrDefault(e => e != null && e.PrimeLevel == primeLevel && e.VeteranTierScalingType == world?.VeteranTierScalingType);
            if (tier != null)
                entity.SetBaseProperty(Property.Menace, tier.UnitPropertyOverrideMenace);
        }

        /// <summary>
        /// Invoked when a <see cref="IGridEntity"/> is removed from the map the public event is on.
        /// </summary>
        public virtual void OnRemoveFromMap(IGridEntity entity)
        {
            if (entity is IWorldEntity worldEntity && entitiesByCreature.TryGetValue(worldEntity.CreatureId, out HashSet<uint> guids))
                guids.Remove(worldEntity.Guid);
        }

        protected IWorldEntity SpawnCreature(uint creatureId, Vector3 position, float yaw = 0f, OnAddDelegate add = null)
        {
            uint id = GetCreature(creatureId);
            if (id == 0u)
                return null;

            ICreatureInfo creatureInfo = creatureInfoManager.GetCreatureInfo(id);
            if (creatureInfo == null)
                return null;

            IWorldEntity entity = entityFactory.CreateWorldEntity(creatureInfo.Entry.CreationTypeEnum);
            // vehicles need their vehicle entry (the creature's UnitVehicleId) or their entity model can't be built
            if (entity is IVehicleEntity vehicle)
                vehicle.Initialise(creatureInfo, 0u, 0u);
            else
                entity.Initialise(creatureInfo);
            entity.Rotation = new Vector3(yaw, 0f, 0f);
            entity.AddToMap(mapInstance, position, add);
            return entity;
        }

        protected static void RemoveEntity<T>(ref T entity) where T : class, IWorldEntity
        {
            if (entity?.InWorld == true)
                entity.RemoveFromMap();
            entity = null;
        }

        protected void RemoveEntity(uint guid)
        {
            if (guid != 0u)
                mapInstance.GetEntity<IWorldEntity>(guid)?.RemoveFromMap();
        }

        /// <summary>
        /// Return a surface position at terrain height, or at the supplied height where the map has no terrain.
        /// </summary>
        protected Vector3 Surface(float x, float z, float fallbackY)
        {
            float? y = mapInstance.GetTerrainHeight(x, z);
            return new Vector3(x, y.HasValue && MathF.Abs(y.Value - fallbackY) < 15f ? y.Value : fallbackY, z);
        }

        #region Doors

        private enum DoorKind
        {
            /// <summary>
            /// Open, shuts while its boss fights and opens again when the boss dies or leaves combat.
            /// </summary>
            Encounter,

            /// <summary>
            /// Shut until its boss dies.
            /// </summary>
            Progression
        }

        private class Door
        {
            public DoorKind Kind { get; init; }
            public uint BossCreatureId { get; init; }
            public double Failsafe { get; init; }
            public IDoorEntity Entity { get; set; }
            public double ClosedFor { get; set; }
            public bool BossSeen { get; set; }
        }

        private readonly List<Door> doors = [];
        private readonly UpdateTimer doorTimer = new(TimeSpan.FromSeconds(0.5d));

        /// <summary>
        /// A door that shuts while the boss fights (the party is locked in with it) and opens when it dies or the fight resets.
        /// Failsafe: it opens after <paramref name="failsafe"/> seconds shut, whatever the fight does.
        /// </summary>
        protected void AddEncounterDoor(uint doorCreatureId, Vector3 position, float yaw, uint bossCreatureId, double failsafe = 600d)
        {
            AddDoor(DoorKind.Encounter, doorCreatureId, position, yaw, bossCreatureId, failsafe);
        }

        /// <summary>
        /// A door that stays shut until the boss dies. Failsafe: it opens after <paramref name="failsafe"/> seconds, and when the boss
        /// isn't on the map at all (not spawned, left out on this difficulty).
        /// </summary>
        protected void AddProgressionDoor(uint doorCreatureId, Vector3 position, float yaw, uint bossCreatureId, double failsafe = 2400d)
        {
            AddDoor(DoorKind.Progression, doorCreatureId, position, yaw, bossCreatureId, failsafe);
        }

        private void AddDoor(DoorKind kind, uint doorCreatureId, Vector3 position, float yaw, uint bossCreatureId, double failsafe)
        {
            var door = new Door
            {
                Kind           = kind,
                BossCreatureId = bossCreatureId,
                Failsafe       = failsafe
            };
            doors.Add(door);

            SpawnCreature(doorCreatureId, position, yaw, (map, guid, vector) =>
            {
                door.Entity = mapInstance.GetEntity<IDoorEntity>(guid);
                if (kind == DoorKind.Encounter)
                    door.Entity?.OpenDoor();
                else
                    door.Entity?.CloseDoor();
            });
        }

        /// <summary>
        /// A door placed on the way between two points of the dungeon, <paramref name="distance"/> metres before <paramref name="to"/>,
        /// facing along the way (positions guessed from the room layout).
        /// </summary>
        protected (Vector3 Position, float Yaw) DoorOnWay(Vector3 from, Vector3 to, float distance)
        {
            Vector3 direction = to - from;
            direction.Y = 0f;
            float length = direction.Length();
            if (length < 1f)
                return (to, 0f);

            direction /= length;
            float t = Math.Clamp((length - distance) / length, 0f, 1f);
            Vector3 point = Vector3.Lerp(from, to, t);
            return (Surface(point.X, point.Z, point.Y), MathF.Atan2(direction.X, direction.Z));
        }

        private void UpdateDoors(double lastTick)
        {
            if (doors.Count == 0)
                return;

            foreach (Door door in doors)
                if (door.Entity != null && !door.Entity.IsOpen)
                    door.ClosedFor += lastTick;

            doorTimer.Update(lastTick);
            if (!doorTimer.HasElapsed)
                return;
            doorTimer.Reset();

            foreach (Door door in doors)
            {
                if (door.Entity == null)
                    continue;

                List<IUnitEntity> bosses = GetEntities<IUnitEntity>(door.BossCreatureId).ToList();
                if (bosses.Count > 0)
                    door.BossSeen = true;
                bool bossAlive = bosses.Any(b => b.IsAlive);

                bool shut = door.Kind switch
                {
                    DoorKind.Encounter   => bossAlive && bosses.Any(b => b.InCombat) && door.ClosedFor < door.Failsafe,
                    DoorKind.Progression => bossAlive && door.ClosedFor < door.Failsafe,
                    _                    => false
                };
                // a progression door whose boss never showed up opens after a minute (boss left out on this difficulty)
                if (door.Kind == DoorKind.Progression && !door.BossSeen && door.ClosedFor > 60d)
                    shut = false;

                if (shut && door.Entity.IsOpen)
                    door.Entity.CloseDoor();
                else if (!shut && !door.Entity.IsOpen)
                {
                    door.Entity.OpenDoor();
                    if (door.Kind == DoorKind.Encounter)
                        door.ClosedFor = 0d;
                }
            }
        }

        #endregion

        /// <summary>
        /// Nodes of a client Spline2 path (creature paths and platforms of the world), in order.
        /// </summary>
        protected List<Vector3> GetSplineNodes(uint splineId)
        {
            return gameTableManager.Spline2Node.Entries
                .Where(n => n != null && n.SplineId == splineId)
                .OrderBy(n => n.Ordinal)
                .Select(n => new Vector3(n.Position0, n.Position1, n.Position2))
                .ToList();
        }

        protected Vector3 GetLocation(uint worldLocationId)
        {
            WorldLocation2Entry entry = gameTableManager.WorldLocation2.GetEntry(worldLocationId);
            return entry != null ? new Vector3(entry.Position0, entry.Position1, entry.Position2) : Vector3.Zero;
        }

        /// <summary>
        /// Add a participant trigger volume for an objective, optionally marked by an objective ring creature.
        /// </summary>
        protected void AddTrigger(uint objectId, Vector3 position, float range, uint ringCreatureId = 0u)
        {
            var trigger = publicEvent.CreateEntity<IVolumeGridTriggerEntity>();
            trigger.Initialise(objectId, range, objectId);
            trigger.AddToMap(mapInstance, position);

            if (ringCreatureId == 0u)
                return;

            IWorldEntity ring = SpawnCreature(ringCreatureId, position);
            if (ring != null)
                objectiveRings.Add(ring);
        }

        /// <summary>
        /// Scripted attackers go straight for the closest player.
        /// </summary>
        protected void AggroNearestPlayer(IBaseMap map, uint guid, Vector3 position)
        {
            if (mapInstance.GetEntity<IUnitEntity>(guid) is not IUnitEntity unit)
                return;

            IPlayer player = mapInstance.GetPlayers()
                .Where(p => p.IsAlive)
                .OrderBy(p => p.Position.GetDistance(position))
                .FirstOrDefault();
            if (player != null && player.Position.GetDistance(position) < 60f)
                unit.ThreatManager.UpdateThreat(player, 1);
        }

        protected void CastOnSelf(IUnitEntity unit, uint spellId)
        {
            ISpellParameters parameters = spellParametersFactory.Resolve();
            parameters.PrimaryTargetId = unit.Guid;
            unit.CastSpell(spellId, parameters);
        }

        protected void Walk(IUnitEntity unit, IEnumerable<Vector3> nodes, float speed)
        {
            var path = new List<Vector3> { unit.Position };
            path.AddRange(nodes);
            unit.MovementManager.LaunchSpline(path, SplineType.Linear, SplineMode.OneShot, speed);
        }

        protected IEnumerable<IPlayer> Players => mapInstance.GetPlayers();

        protected bool AnyPlayerNear(Vector3 position, float range)
        {
            return mapInstance.GetPlayers().Any(p => p.IsAlive && p.Position.GetDistance(position) < range);
        }

        #endregion

        /// <summary>
        /// A creature with an <see cref="ExpeditionEntityScript"/> died.
        /// </summary>
        public virtual void OnEntityDeath(IUnitEntity entity)
        {
        }

        /// <summary>
        /// An object with an <see cref="ExpeditionEntityScript"/> was activated.
        /// </summary>
        public virtual void OnEntityActivated(IWorldEntity entity, IPlayer activator)
        {
        }

        #region Phases and objectives

        /// <summary>
        /// Invoked when the public event phase changes.
        /// </summary>
        public void OnPublicEventPhase(uint phase)
        {
            foreach (IWorldEntity ring in objectiveRings)
                if (ring.InWorld)
                    ring.RemoveFromMap();
            objectiveRings.Clear();

            OnPhase(phase);
        }

        protected abstract void OnPhase(uint phase);

        /// <summary>
        /// Invoked when the <see cref="IPublicEventObjective"/> status changes.
        /// </summary>
        public void OnPublicEventObjectiveStatus(IPublicEventObjective objective)
        {
            if (objective.Status == PublicEventStatus.Succeeded)
                OnObjectiveSucceeded(objective.Entry.Id, objective.Team);
        }

        protected abstract void OnObjectiveSucceeded(uint objectiveId, IPublicEventTeam team);

        protected static bool IsSucceeded(IPublicEventTeam team, params uint[] objectives)
        {
            return objectives.All(o => team.GetObjectives().FirstOrDefault(t => t.Entry.Id == o)?.Status == PublicEventStatus.Succeeded);
        }

        protected static bool IsFailed(IPublicEventTeam team, uint objective)
        {
            return team.GetObjectives().FirstOrDefault(t => t.Entry.Id == objective)?.Status == PublicEventStatus.Failed;
        }

        protected void Activate(uint objectiveId, uint max = 0u)
        {
            publicEvent.ActivateObjective(objectiveId, max);
        }

        protected void ActivateForPlayers(uint objectiveId)
        {
            publicEvent.ActivateObjective(objectiveId, mapInstance.PlayerCount);
        }

        protected void Update(uint objectiveId, int count = 1)
        {
            publicEvent.UpdateObjective(objectiveId, count);
        }

        /// <summary>
        /// Finish the expedition: quest objectives, achievements, prime level unlock, the run counts as completed.
        /// </summary>
        /// <param name="questObjectives">Quest objectives credited to every player (the expedition quest).</param>
        /// <param name="primeQuestObjectives">Quest objectives credited on prime (the prime dailies).</param>
        protected void CompleteExpedition(uint[] questObjectives, uint[] primeQuestObjectives, ushort completeAchievement, ushort primeAchievement, ushort primeGoldAchievement, bool gold)
        {
            foreach (IPlayer player in mapInstance.GetPlayers())
            {
                foreach (uint objectiveId in questObjectives)
                    player.QuestManager.ObjectiveUpdate(objectiveId, 1u);
                if (IsVeteran)
                    foreach (uint objectiveId in primeQuestObjectives)
                        player.QuestManager.ObjectiveUpdate(objectiveId, 1u);

                GrantAchievement(player, completeAchievement);
                if (IsVeteran)
                {
                    GrantAchievement(player, primeAchievement);
                    player.PrimeLevelManager.CompletePrimeLevel(WorldId, primeLevel);
                    if (gold && noDeaths)
                        GrantAchievement(player, primeGoldAchievement);
                }
            }

            if (mapInstance.MapLock != null)
                mapInstance.MapLock.IsCompleted = true;

            publicEvent.Finish(PublicEventTeam.PublicTeam);
        }

        private static void GrantAchievement(IPlayer player, ushort achievementId)
        {
            if (achievementId != 0 && !player.AchievementManager.HasCompletedAchievement(achievementId))
                player.AchievementManager.GrantAchievement(achievementId);
        }

        #endregion

        #region Update

        /// <summary>
        /// Invoked each world tick with the delta since the previous tick occurred.
        /// </summary>
        public void Update(double lastTick)
        {
            if (publicEvent == null)
                return;

            // doors keep following their encounters after the event finished, so nobody stays shut in
            UpdateDoors(lastTick);

            if (publicEvent.HasFinished)
                return;

            for (int i = delayed.Count - 1; i >= 0; i--)
            {
                (double timer, Action action) = delayed[i];
                timer -= lastTick;
                if (timer > 0d)
                {
                    delayed[i] = (timer, action);
                    continue;
                }

                delayed.RemoveAt(i);
                action();
            }

            OnTick(lastTick);

            checkTimer.Update(lastTick);
            if (!checkTimer.HasElapsed)
                return;
            checkTimer.Reset();

            if (mapInstance.GetPlayers().Any(p => !p.IsAlive))
                noDeaths = false;

            OnCheck();
        }

        /// <summary>
        /// Invoked every tick.
        /// </summary>
        protected virtual void OnTick(double lastTick)
        {
        }

        /// <summary>
        /// Invoked twice a second, for position checks.
        /// </summary>
        protected virtual void OnCheck()
        {
        }

        protected void Delay(double seconds, Action action)
        {
            delayed.Add((seconds, action));
        }

        #endregion

        #region Messages and cinematics

        protected void BroadcastCommunicatorMessage(uint messageId)
        {
            ICommunicatorMessage communicatorMessage = globalQuestManager.GetCommunicatorMessage(messageId);
            if (communicatorMessage == null)
                return;

            foreach (IPlayer player in mapInstance.GetPlayers())
                communicatorMessage.Send(player.Session);
        }

        protected void BroadcastSystemMessage(uint textId)
        {
            string text = GetText(textId);
            if (string.IsNullOrEmpty(text))
                return;

            foreach (IPlayer player in mapInstance.GetPlayers())
                player.SendSystemMessage(text);
        }

        /// <summary>
        /// A creature on the map yells a line to the whole expedition (the expeditions are small, the range covers them).
        /// </summary>
        protected void Yell(uint creatureId, uint textId, float range = 500f)
        {
            IWorldEntity speaker = GetEntities<IWorldEntity>(creatureId).FirstOrDefault();
            string text = GetText(textId);
            if (speaker != null && !string.IsNullOrEmpty(text))
                speaker.NpcYell(text, range);
        }

        protected string GetText(uint textId)
        {
            return gameTableManager.GetTextTable(Language.English)?.GetEntry(textId) ?? string.Empty;
        }

        protected void PlayCinematic(CinematicSceneId id)
        {
            foreach (IPlayer player in mapInstance.GetPlayers())
                player.CinematicManager.QueueCinematic(cinematicFactory.CreateCinematic(id));
        }

        #endregion
    }
}
