using System.Numerics;
using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Entity.Creature;
using NexusForever.Game.Abstract.Entity.Trigger;
using NexusForever.Game.Abstract.Map.Instance;
using NexusForever.Game.Abstract.PublicEvent;
using NexusForever.Game.Abstract.Quest;
using NexusForever.Game.Abstract.Spell;
using NexusForever.Game.Static;
using NexusForever.Game.Static.Entity;
using NexusForever.Game.Static.PublicEvent;
using NexusForever.Game.Static.Reputation;
using NexusForever.Game.Static.Setting;
using NexusForever.GameTable;
using NexusForever.GameTable.Model;
using NexusForever.Script.Instance.Expedition.FragmentZero.Script;
using NexusForever.Script.Template;
using NexusForever.Script.Template.Filter;
using NexusForever.Shared;
using NexusForever.Shared.Game;

namespace NexusForever.Script.Instance.Expedition.FragmentZero
{
    /// <summary>
    /// They Came from Fragment Zero! (public event 680), the expedition behind "Expedition: Lost in the Ring".
    /// </summary>
    /// <remarks>
    /// Freight Supervisor Lola lands on Fragment Zero looking for her husband Hugo and his crew. A friendly Skeech leads the players into
    /// an ambush, the Life-Overseer's projections then lead them through the Incubation Complex (Crewmate Jo, three xenobite prototypes),
    /// the Biomatics Chamber (Crewmate Syrus, Project "Matron") and the Life-Overseer's Creche (the Life-Overseer, Hugo locked up in a lab).
    /// Hugo fixes the Life-Overseer's device and the players escort him back to the ship through the Skeech hordes.
    ///
    /// Retail server data isn't available: the order follows the objective data, the communicator messages and the archived Jabbithole
    /// data (positions, abilities). Veteran (prime) difficulty swaps creatures for their veteran versions, adds the gold medal timer and the
    /// black box, which has to be found before the creche opens ("we could really use that Black Box before heading any further").
    /// </remarks>
    [ScriptFilterOwnerId(680)]
    public class FragmentZeroEventScript : IPublicEventScript, IOwnedScript<IPublicEvent>
    {
        private const ushort WorldId = 3180;

        /// <summary>
        /// Normal creatures and their veteran versions.
        /// </summary>
        private static readonly Dictionary<uint, uint> VeteranCreatures = new()
        {
            { 67516, 69629 }, { 67518, 69630 }, { 67519, 69631 }, { 67520, 69632 }, // brawler, fighter, berserker, shaman
            { 67551, 69667 }, { 67553, 69668 }, { 69092, 69670 }, { 69091, 69669 }, // crawler, creeper (scripted)
            { 69134, 69671 },                                                       // hatchling
            { 67526, 69672 }, { 67527, 69673 }, { 67528, 69674 },                   // prototypes alpha, beta, delta
            { 67999, 69681 }, { 68000, 69682 }, { 68883, 69687 }, { 69114, 69689 }, // ambusher, trapper, striker, no aggro ambusher
            { 67894, 69633 }, { 67895, 69634 },                                     // overseer's probe, detector
            { 67522, 69635 }, { 69088, 69664 },                                     // Life-Overseer, Project "Matron"
            { 69062, 69622 }, { 69063, 69623 }, { 69064, 69624 }, { 69065, 69625 }, // escape skeech
            { 67991, 69984 }, { 68860, 69695 }                                      // friendly skeech, Hugo (escape)
        };
        private static readonly HashSet<uint> VeteranOnlyCreatures = [.. VeteranCreatures.Values, 69767, 69794, 69834];

        // the friendly skeech's walk from the landing to the ambush and Hugo's escape from the creche to the ship (Jabbithole sightings)
        private static readonly Vector2[] FriendlySkeechPath =
        [
            new(9870f, -5921f), new(9871f, -5933f), new(9876f, -5952f), new(9875f, -5968f), new(9878f, -5981f), new(9879f, -5987f),
            new(9893f, -6021f), new(9895f, -6030f)
        ];
        private static readonly Vector2[] HugoEscapePath =
        [
            new(9853f, -6263f), new(9855f, -6248f), new(9861f, -6219f), new(9866f, -6194f), new(9866f, -6160f), new(9864f, -6103f),
            new(9878f, -6070f), new(9893f, -6023f), new(9876f, -5970f), new(9868f, -5906f), new(9866f, -5860f), new(9863.7f, -5836f)
        ];

        // Skeech ambushes along the way (Jabbithole sightings of the ambushers): the opening ambush, the way to the incubation complex and the creche
        private static readonly Vector2[] OpeningAmbush = [new(9896f, -6026f), new(9893f, -6031f), new(9890f, -6024f), new(9897f, -6035f), new(9894f, -6017f), new(9898f, -6022f), new(9900f, -6027f), new(9891f, -6035f)];
        private static readonly Vector2[] IncubationAmbush = [new(9817f, -6087f), new(9806f, -6090f), new(9807f, -6083f), new(9826f, -6086f), new(9813f, -6085f), new(9820f, -6092f)];
        private static readonly Vector2[] CrecheAmbush = [new(9857f, -6230f), new(9857f, -6242f), new(9852f, -6252f), new(9863f, -6232f), new(9860f, -6232f), new(9864f, -6226f)];
        private static readonly Vector2[] BoneTraps = [new(9894f, -6026f), new(9898f, -6035f), new(9893f, -6024f)];

        private IPublicEvent publicEvent;
        private IContentMapInstance mapInstance;
        private WorldDifficulty difficulty;
        private byte primeLevel;

        private uint friendlySkeechGuid;
        private uint projectionGuid;
        private uint lifeOverseerGuid;
        private uint hugoLockedGuid;
        private uint hugoAuraGuid;
        private uint hugoRunnerGuid;
        private readonly Dictionary<PublicEventCreature, uint> forceFields = [];
        private readonly List<uint> biomaticsBeams = [];
        private readonly List<uint> blackBoxes = [];
        private readonly List<uint> ambushers = [];
        private readonly List<uint> objectiveRings = [];
        private readonly HashSet<int> ambushesTriggered = [];

        private bool blackBoxFound;
        private bool noDeaths = true;
        private int hugoWaypoint;
        private Vector3? hugoTarget;
        private double hugoWaveTimer;
        private double hugoZapTimer;

        private readonly List<(double Timer, Action Action)> delayed = [];
        private readonly UpdateTimer checkTimer = new(TimeSpan.FromSeconds(0.5d));

        #region Dependency Injection

        private readonly IGlobalQuestManager globalQuestManager;
        private readonly ICreatureInfoManager creatureInfoManager;
        private readonly IEntityFactory entityFactory;
        private readonly IFactory<ISpellParameters> spellParametersFactory;
        private readonly IGameTableManager gameTableManager;

        public FragmentZeroEventScript(
            IGlobalQuestManager globalQuestManager,
            ICreatureInfoManager creatureInfoManager,
            IEntityFactory entityFactory,
            IFactory<ISpellParameters> spellParametersFactory,
            IGameTableManager gameTableManager)
        {
            this.globalQuestManager     = globalQuestManager;
            this.creatureInfoManager    = creatureInfoManager;
            this.entityFactory          = entityFactory;
            this.spellParametersFactory = spellParametersFactory;
            this.gameTableManager       = gameTableManager;
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
            publicEvent.SetPhase(PublicEventPhase.SearchForMissingCrew);
        }

        private bool IsVeteran => difficulty == WorldDifficulty.Veteran;

        private uint GetCreature(uint creatureId)
        {
            if (IsVeteran)
                return VeteranCreatures.TryGetValue(creatureId, out uint veteranId) ? veteranId : creatureId;

            return VeteranOnlyCreatures.Contains(creatureId) ? 0u : creatureId;
        }

        #region Entities

        /// <summary>
        /// Invoked when a <see cref="IGridEntity"/> is added to the map the public event is on.
        /// </summary>
        public void OnAddToMap(IGridEntity entity)
        {
            if (entity is not IWorldEntity worldEntity)
                return;

            switch ((PublicEventCreature)worldEntity.CreatureId)
            {
                case PublicEventCreature.LifeOverseer:
                case PublicEventCreature.LifeOverseerVeteran:
                    lifeOverseerGuid = worldEntity.Guid;
                    break;
                case PublicEventCreature.CaptainHugoLocked:
                    hugoLockedGuid = worldEntity.Guid;
                    break;
                case PublicEventCreature.XenobiteDoorForceField:
                case PublicEventCreature.GronyxDoorForceField:
                case PublicEventCreature.PellDoorForceField:
                    forceFields[(PublicEventCreature)worldEntity.CreatureId] = worldEntity.Guid;
                    break;
                case PublicEventCreature.SecurityBeam:
                    // the facility defense control panels only turn off the beams inside the Biomatics Chamber
                    if (worldEntity.Position.X > 10100f)
                        biomaticsBeams.Add(worldEntity.Guid);
                    break;
                case PublicEventCreature.BlackBox:
                    blackBoxes.Add(worldEntity.Guid);
                    // the black box is found at one of its spots each run
                    if (blackBoxes.Count == 3)
                    {
                        int keep = Random.Shared.Next(blackBoxes.Count);
                        for (int i = 0; i < blackBoxes.Count; i++)
                            if (i != keep)
                                mapInstance.GetEntity<IWorldEntity>(blackBoxes[i])?.RemoveFromMap();
                    }
                    break;
            }

            ApplyPrimeLevel(worldEntity);
        }

        /// <summary>
        /// Prime levels give the veteran creatures menace (VeteranTier, by the world's veteran tier scaling type).
        /// </summary>
        private void ApplyPrimeLevel(IWorldEntity entity)
        {
            if (!IsVeteran || primeLevel == 0 || entity is not IUnitEntity || entity.CreatureInfo?.Entry?.Creature2TierId != 2u)
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
        public void OnRemoveFromMap(IGridEntity entity)
        {
            if (entity is not IWorldEntity worldEntity)
                return;

            biomaticsBeams.Remove(worldEntity.Guid);
            blackBoxes.Remove(worldEntity.Guid);
        }

        private IWorldEntity SpawnCreature(uint creatureId, Vector3 position, float yaw = 0f)
        {
            ICreatureInfo creatureInfo = creatureInfoManager.GetCreatureInfo(GetCreature(creatureId));
            if (creatureInfo == null)
                return null;

            IWorldEntity entity = entityFactory.CreateWorldEntity(creatureInfo.Entry.CreationTypeEnum);
            entity.Initialise(creatureInfo);
            entity.Rotation = new Vector3(yaw, 0f, 0f);
            entity.AddToMap(mapInstance, position);
            return entity;
        }

        private IWorldEntity SpawnCreature(PublicEventCreature creature, Vector3 position, float yaw = 0f)
        {
            return SpawnCreature((uint)creature, position, yaw);
        }

        private void RemoveEntity(ref uint guid)
        {
            if (guid == 0u)
                return;

            mapInstance.GetEntity<IWorldEntity>(guid)?.RemoveFromMap();
            guid = 0u;
        }

        private void RemoveForceField(PublicEventCreature forceField)
        {
            if (forceFields.Remove(forceField, out uint guid))
                mapInstance.GetEntity<IWorldEntity>(guid)?.RemoveFromMap();
        }

        /// <summary>
        /// Return the position of a surface point at terrain height.
        /// </summary>
        private Vector3 Surface(Vector2 point)
        {
            float y = mapInstance.GetTerrainHeight(point.X, point.Y) ?? -783f;
            return new Vector3(point.X, y, point.Y);
        }

        private Vector3 GetLocation(uint worldLocationId)
        {
            WorldLocation2Entry entry = gameTableManager.WorldLocation2.GetEntry(worldLocationId);
            return entry != null ? new Vector3(entry.Position0, entry.Position1, entry.Position2) : Vector3.Zero;
        }

        /// <summary>
        /// Spawn the Life-Overseer's projection at a world location, it shows the players the way to the next wing.
        /// </summary>
        private void MoveProjection(uint worldLocationId)
        {
            RemoveEntity(ref projectionGuid);
            projectionGuid = SpawnCreature(PublicEventCreature.LifeOverseerProjection, GetLocation(worldLocationId), MathF.PI)?.Guid ?? 0u;
        }

        /// <summary>
        /// The airlock cycles: the gas vents for a few seconds.
        /// </summary>
        private void CycleAirlock(Vector3 position, uint textId)
        {
            IWorldEntity gas = SpawnCreature(PublicEventCreature.AirlockGas, position);
            uint guid = gas?.Guid ?? 0u;
            Delay(6d, () => RemoveEntity(ref guid));

            if (textId != 0u)
                BroadcastSystemMessage(textId);
        }

        /// <summary>
        /// Add a participant trigger volume for an objective, with the objective ring that marks it.
        /// </summary>
        private void AddTrigger(uint objectId, Vector3 position, float range, PublicEventCreature ring)
        {
            var trigger = publicEvent.CreateEntity<IVolumeGridTriggerEntity>();
            trigger.Initialise(objectId, range, objectId);
            trigger.AddToMap(mapInstance, position);

            IWorldEntity ringEntity = SpawnCreature(ring, position);
            if (ringEntity != null)
                objectiveRings.Add(ringEntity.Guid);
        }

        private void SpawnAmbush(Vector2[] positions, int count)
        {
            for (int i = 0; i < count; i++)
            {
                PublicEventCreature creature = i % 2 == 0 ? PublicEventCreature.AmbusherOpening : PublicEventCreature.TrapperOpening;
                Vector2 point = positions[i % positions.Length] + new Vector2((float)(Random.Shared.NextDouble() * 3d - 1.5d), (float)(Random.Shared.NextDouble() * 3d - 1.5d));
                IWorldEntity ambusher = SpawnCreature(creature, Surface(point), (float)(Random.Shared.NextDouble() * MathF.Tau));
                if (ambusher != null)
                    ambushers.Add(ambusher.Guid);
            }
        }

        #endregion

        #region Phases

        /// <summary>
        /// Invoked when the public event phase changes.
        /// </summary>
        public void OnPublicEventPhase(uint phase)
        {
            // the previous phase's objective ring has served its purpose
            foreach (uint guid in objectiveRings)
                mapInstance.GetEntity<IWorldEntity>(guid)?.RemoveFromMap();
            objectiveRings.Clear();

            switch ((PublicEventPhase)phase)
            {
                case PublicEventPhase.SearchForMissingCrew:
                    OnPhaseSearchForMissingCrew();
                    break;
                case PublicEventPhase.FollowFriendlySkeech:
                    OnPhaseFollowFriendlySkeech();
                    break;
                case PublicEventPhase.SkeechAmbush:
                    OnPhaseSkeechAmbush();
                    break;
                case PublicEventPhase.FollowProjectionsIncubation:
                    OnPhaseFollowProjectionsIncubation();
                    break;
                case PublicEventPhase.IncubationComplex:
                    OnPhaseIncubationComplex();
                    break;
                case PublicEventPhase.ReturnToIncubationAirlock:
                    OnPhaseReturnToIncubationAirlock();
                    break;
                case PublicEventPhase.FollowProjectionsBiomatics:
                    OnPhaseFollowProjectionsBiomatics();
                    break;
                case PublicEventPhase.BiomaticsChamber:
                    OnPhaseBiomaticsChamber();
                    break;
                case PublicEventPhase.ReturnToBiomaticsAirlock:
                    OnPhaseReturnToBiomaticsAirlock();
                    break;
                case PublicEventPhase.FollowProjectionsCreche:
                    OnPhaseFollowProjectionsCreche();
                    break;
                case PublicEventPhase.LifeOverseersCreche:
                    OnPhaseLifeOverseersCreche();
                    break;
                case PublicEventPhase.ReturnToCrecheAirlock:
                    OnPhaseReturnToCrecheAirlock();
                    break;
                case PublicEventPhase.SeeIfHugoHasAWayOut:
                    OnPhaseSeeIfHugoHasAWayOut();
                    break;
                case PublicEventPhase.WaitForHugo:
                    OnPhaseWaitForHugo();
                    break;
                case PublicEventPhase.StayCloseToHugo:
                    OnPhaseStayCloseToHugo();
                    break;
                case PublicEventPhase.SpeakWithHugo:
                    OnPhaseSpeakWithHugo();
                    break;
            }
        }

        private void OnPhaseSearchForMissingCrew()
        {
            // the search for the missing crew (4397) is the initial objective, the optional objectives run for the whole expedition
            publicEvent.ActivateObjective(PublicEventObjective.EliminateSkeech);
            publicEvent.ActivateObjective(PublicEventObjective.CollectCargoCrates, 10u);
            publicEvent.ActivateObjective(PublicEventObjective.DiscoverLostRecordings);
            publicEvent.ActivateObjective(PublicEventObjective.OhmnasRecording);
            publicEvent.ActivateObjective(PublicEventObjective.LucentsRecording);

            if (IsVeteran)
            {
                publicEvent.ActivateObjective(PublicEventObjective.KillExperimentalSkeech);
                publicEvent.ActivateObjective(PublicEventObjective.GoldMedalTimer);
                publicEvent.ActivateObjective(PublicEventObjective.LocateBlackBox, 1u);
            }

            friendlySkeechGuid = SpawnCreature(PublicEventCreature.FriendlySkeech, Surface(FriendlySkeechPath[0]), MathF.PI)?.Guid ?? 0u;
        }

        private void OnPhaseFollowFriendlySkeech()
        {
            publicEvent.ActivateObjective(PublicEventObjective.FollowFriendlySkeech, mapInstance.PlayerCount);
            AddTrigger(7902, GetLocation(49225), 12f, PublicEventCreature.RingSkeechAmbush);

            BroadcastCommunicatorMessage(CommunicatorMessage.LolaFollowHim);
            Delay(4d, () => BroadcastCommunicatorMessage(7523u));

            // the friendly skeech runs ahead to the crash site... straight into the ambush
            if (mapInstance.GetEntity<IUnitEntity>(friendlySkeechGuid) is IUnitEntity skeech)
                skeech.MovementManager.LaunchSpline(BuildPath(FriendlySkeechPath), Game.Static.Entity.Movement.Spline.SplineType.Linear,
                    Game.Static.Entity.Movement.Spline.SplineMode.OneShot, 5f);
        }

        private void OnPhaseSkeechAmbush()
        {
            publicEvent.ActivateObjective(PublicEventObjective.SurviveSkeechAmbush);

            // bone traps snap shut on everyone at the crash site
            foreach (Vector2 point in BoneTraps)
            {
                IWorldEntity trap = SpawnCreature(PublicEventCreature.BoneTrap, Surface(point));
                if (trap is not IUnitEntity trapUnit)
                    continue;

                foreach (IPlayer player in mapInstance.GetPlayers().Where(p => p.Position.GetDistance(trapUnit.Position) < 12f))
                {
                    ISpellParameters parameters = spellParametersFactory.Resolve();
                    parameters.PrimaryTargetId = player.Guid;
                    trapUnit.CastSpell(76786, parameters);
                }

                uint guid = trap.Guid;
                Delay(8d, () => RemoveEntity(ref guid));
            }

            projectionGuid = SpawnCreature(PublicEventCreature.LifeOverseerProjection, Surface(new Vector2(9902f, -6040f)), 2.5f)?.Guid ?? 0u;
            BroadcastCommunicatorMessage(CommunicatorMessage.OverseerUnworthy);
            Delay(12d, () => BroadcastCommunicatorMessage(CommunicatorMessage.HugoRoughWaters));

            RemoveEntity(ref friendlySkeechGuid);
            SpawnAmbush(OpeningAmbush, 8);
        }

        private void OnPhaseFollowProjectionsIncubation()
        {
            publicEvent.ActivateObjective(PublicEventObjective.FollowProjectionsIncubation, mapInstance.PlayerCount);
            AddTrigger(7903, GetLocation(48711), 8f, PublicEventCreature.RingIncubationAirlock);

            MoveProjection(48711);
            RemoveForceField(PublicEventCreature.XenobiteDoorForceField);

            BroadcastCommunicatorMessage(CommunicatorMessage.LolaMadeHisPoint);
            Delay(8d, () => BroadcastCommunicatorMessage(CommunicatorMessage.HugoLastSawJo));
            Delay(20d, () => BroadcastCommunicatorMessage(CommunicatorMessage.LolaGatherCargo));
        }

        private void OnPhaseIncubationComplex()
        {
            RemoveEntity(ref projectionGuid);
            CycleAirlock(new Vector3(9687f, -767.6f, -6094f), 0u);

            publicEvent.ActivateObjective(PublicEventObjective.SearchIncubationForJo);
            publicEvent.ActivateObjective(PublicEventObjective.DefeatPrototypeAlpha);
            publicEvent.ActivateObjective(PublicEventObjective.DefeatPrototypeBeta);
            publicEvent.ActivateObjective(PublicEventObjective.DefeatPrototypeDelta);
            publicEvent.ActivateObjective(PublicEventObjective.SmashXenobiteEggs);
        }

        private void OnPhaseReturnToIncubationAirlock()
        {
            publicEvent.ActivateObjective(PublicEventObjective.ReturnToIncubationAirlock, mapInstance.PlayerCount);
            AddTrigger(7896, GetLocation(48717), 8f, PublicEventCreature.RingReturnToAirlock);
        }

        private void OnPhaseFollowProjectionsBiomatics()
        {
            publicEvent.ActivateObjective(PublicEventObjective.FollowProjectionsBiomatics, mapInstance.PlayerCount);
            AddTrigger(7904, GetLocation(48712), 8f, PublicEventCreature.RingBiomaticsAirlock);

            MoveProjection(48712);
            RemoveForceField(PublicEventCreature.GronyxDoorForceField);

            BroadcastCommunicatorMessage(CommunicatorMessage.HugoSyrusStillOutThere);
        }

        private void OnPhaseBiomaticsChamber()
        {
            RemoveEntity(ref projectionGuid);
            CycleAirlock(new Vector3(10119f, -776.5f, -6119f), 639390); // Airlock cycle complete. Welcome to the Biomatics Chamber.

            publicEvent.ActivateObjective(PublicEventObjective.SearchBiomaticsForSyrus);
            publicEvent.ActivateObjective(PublicEventObjective.DefeatProjectMatron);
            publicEvent.ActivateObjective(PublicEventObjective.DeactivateDefenses);

            BroadcastCommunicatorMessage(CommunicatorMessage.OverseerDefenses);
            Delay(6d, () => BroadcastCommunicatorMessage(CommunicatorMessage.HugoSyrusHeadingDown));
            Delay(12d, () => BroadcastCommunicatorMessage(CommunicatorMessage.LolaBeamsWarning));
        }

        private void OnPhaseReturnToBiomaticsAirlock()
        {
            publicEvent.ActivateObjective(PublicEventObjective.ReturnToBiomaticsAirlock, mapInstance.PlayerCount);
            AddTrigger(7916, GetLocation(48657), 11.76f, PublicEventCreature.RingReturnToAirlock);
        }

        private void OnPhaseFollowProjectionsCreche()
        {
            publicEvent.ActivateObjective(PublicEventObjective.FollowProjectionsCreche, mapInstance.PlayerCount);
            AddTrigger(7905, GetLocation(48713), 8f, PublicEventCreature.RingCrecheAirlock);

            MoveProjection(48713);
            if (!IsVeteran || blackBoxFound)
                RemoveForceField(PublicEventCreature.PellDoorForceField);

            BroadcastCommunicatorMessage(CommunicatorMessage.HugoLastOne);
        }

        private void OnPhaseLifeOverseersCreche()
        {
            RemoveEntity(ref projectionGuid);
            CycleAirlock(new Vector3(9829f, -780.7f, -6316f), 639387); // Airlock cycle complete. Welcome to Life-Overseer's Creche.

            publicEvent.ActivateObjective(PublicEventObjective.SearchCrecheForHugo, mapInstance.PlayerCount);
            AddTrigger(7891, GetLocation(48680), 12f, PublicEventCreature.RingInvestigateCreche);

            BroadcastCommunicatorMessage(CommunicatorMessage.HugoLockedUp);
        }

        private void OnPhaseReturnToCrecheAirlock()
        {
            publicEvent.ActivateObjective(PublicEventObjective.ReturnToCrecheAirlock, mapInstance.PlayerCount);
            AddTrigger(7893, GetLocation(48682), 18f, PublicEventCreature.RingReturnToAirlock);

            // Hugo leaves his lab with the players
            RemoveEntity(ref hugoLockedGuid);
        }

        private void OnPhaseSeeIfHugoHasAWayOut()
        {
            publicEvent.ActivateObjective(PublicEventObjective.SeeIfHugoHasAWayOut);
            hugoAuraGuid = SpawnCreature(PublicEventCreature.CaptainHugoAura, GetLocation(49299), 0f)?.Guid ?? 0u;
        }

        private void OnPhaseWaitForHugo()
        {
            publicEvent.ActivateObjective(PublicEventObjective.WaitForHugo);

            // Hugo gets the Life-Overseer's device working (communicator 7528 to 7531)
            BroadcastCommunicatorMessage(CommunicatorMessage.HugoGizmo);
            Delay(4d, () => BroadcastCommunicatorMessage(7529u));
            Delay(8d, () => BroadcastCommunicatorMessage(7530u));
            Delay(12d, () => BroadcastCommunicatorMessage(7531u));
            Delay(15d, () => BroadcastCommunicatorMessage(CommunicatorMessage.HugoSomethingYouDontSee));
            Delay(16d, () => publicEvent.UpdateObjective(PublicEventObjective.WaitForHugo, 1));
        }

        private void OnPhaseStayCloseToHugo()
        {
            publicEvent.ActivateObjective(PublicEventObjective.StayCloseToHugo);

            Vector3 position = mapInstance.GetEntity<IWorldEntity>(hugoAuraGuid)?.Position ?? Surface(HugoEscapePath[0]);
            RemoveEntity(ref hugoAuraGuid);

            IWorldEntity hugo = SpawnCreature(PublicEventCreature.CaptainHugoRunner, position, 0f);
            hugoRunnerGuid = hugo?.Guid ?? 0u;
            hugoWaypoint   = 1;
            hugoTarget     = null;
            hugoWaveTimer  = 6d;
            hugoZapTimer   = 10d;

            // the Life-Overseer's device: an aura that protects and empowers everyone close to Hugo
            if (hugo is IUnitEntity hugoUnit)
                CastOnSelf(hugoUnit, 76947);
        }

        private void OnPhaseSpeakWithHugo()
        {
            publicEvent.ActivateObjective(PublicEventObjective.SpeakWithHugo);

            Vector3 position = mapInstance.GetEntity<IWorldEntity>(hugoRunnerGuid)?.Position ?? GetLocation(48683);
            RemoveEntity(ref hugoRunnerGuid);
            SpawnCreature(PublicEventCreature.CaptainHugoEnd, position, 0f);
        }

        #endregion

        #region Objectives

        /// <summary>
        /// Invoked when the <see cref="IPublicEventObjective"/> status changes.
        /// </summary>
        public void OnPublicEventObjectiveStatus(IPublicEventObjective objective)
        {
            if (objective.Status != PublicEventStatus.Succeeded)
                return;

            switch ((PublicEventObjective)objective.Entry.Id)
            {
                case PublicEventObjective.SearchForMissingCrew:
                    publicEvent.SetPhase(PublicEventPhase.FollowFriendlySkeech);
                    break;
                case PublicEventObjective.FollowFriendlySkeech:
                    publicEvent.SetPhase(PublicEventPhase.SkeechAmbush);
                    break;
                case PublicEventObjective.SurviveSkeechAmbush:
                    publicEvent.SetPhase(PublicEventPhase.FollowProjectionsIncubation);
                    break;
                case PublicEventObjective.FollowProjectionsIncubation:
                    publicEvent.SetPhase(PublicEventPhase.IncubationComplex);
                    break;
                case PublicEventObjective.SearchIncubationForJo:
                case PublicEventObjective.DefeatPrototypeAlpha:
                case PublicEventObjective.DefeatPrototypeBeta:
                case PublicEventObjective.DefeatPrototypeDelta:
                    // Jo's body is found once the three xenobite prototypes are dead
                    if (IsSucceeded(objective.Team, PublicEventObjective.SearchIncubationForJo, PublicEventObjective.DefeatPrototypeAlpha,
                        PublicEventObjective.DefeatPrototypeBeta, PublicEventObjective.DefeatPrototypeDelta))
                        publicEvent.ActivateObjective(PublicEventObjective.SearchJoCorpse);
                    break;
                case PublicEventObjective.SearchJoCorpse:
                    publicEvent.SetPhase(PublicEventPhase.ReturnToIncubationAirlock);
                    break;
                case PublicEventObjective.ReturnToIncubationAirlock:
                    publicEvent.SetPhase(PublicEventPhase.FollowProjectionsBiomatics);
                    break;
                case PublicEventObjective.FollowProjectionsBiomatics:
                    publicEvent.SetPhase(PublicEventPhase.BiomaticsChamber);
                    break;
                case PublicEventObjective.SearchBiomaticsForSyrus:
                case PublicEventObjective.DefeatProjectMatron:
                    if (IsSucceeded(objective.Team, PublicEventObjective.SearchBiomaticsForSyrus, PublicEventObjective.DefeatProjectMatron))
                        publicEvent.ActivateObjective(PublicEventObjective.SearchSyrusCorpse);
                    break;
                case PublicEventObjective.SearchSyrusCorpse:
                    publicEvent.SetPhase(PublicEventPhase.ReturnToBiomaticsAirlock);
                    break;
                case PublicEventObjective.ReturnToBiomaticsAirlock:
                    publicEvent.SetPhase(PublicEventPhase.FollowProjectionsCreche);
                    break;
                case PublicEventObjective.FollowProjectionsCreche:
                    publicEvent.SetPhase(PublicEventPhase.LifeOverseersCreche);
                    break;
                case PublicEventObjective.SearchCrecheForHugo:
                    OnSearchCrecheForHugo();
                    break;
                case PublicEventObjective.DefeatLifeOverseer:
                    publicEvent.ActivateObjective(PublicEventObjective.LocateHugo);
                    break;
                case PublicEventObjective.LocateHugo:
                    mapInstance.GetEntity<IWorldEntity>(hugoLockedGuid)?.NpcSay(GetText(640044)); // That overseer fella was working on some kind of gizmo...
                    publicEvent.SetPhase(PublicEventPhase.ReturnToCrecheAirlock);
                    break;
                case PublicEventObjective.ReturnToCrecheAirlock:
                    publicEvent.SetPhase(PublicEventPhase.SeeIfHugoHasAWayOut);
                    break;
                case PublicEventObjective.SeeIfHugoHasAWayOut:
                    publicEvent.SetPhase(PublicEventPhase.WaitForHugo);
                    break;
                case PublicEventObjective.WaitForHugo:
                    publicEvent.SetPhase(PublicEventPhase.StayCloseToHugo);
                    break;
                case PublicEventObjective.StayCloseToHugo:
                    publicEvent.SetPhase(PublicEventPhase.SpeakWithHugo);
                    break;
                case PublicEventObjective.SpeakWithHugo:
                    OnExpeditionComplete();
                    break;

                // optional objectives
                case PublicEventObjective.EliminateSkeech:
                    publicEvent.ActivateObjective(PublicEventObjective.EliminateSkeech50);
                    break;
                case PublicEventObjective.EliminateSkeech50:
                    publicEvent.ActivateObjective(PublicEventObjective.EliminateSkeech100);
                    break;
                case PublicEventObjective.EliminateSkeech100:
                    publicEvent.ActivateObjective(PublicEventObjective.EliminateSkeech200);
                    break;
                case PublicEventObjective.CollectCargoCrates:
                    BroadcastCommunicatorMessage(CommunicatorMessage.LolaCargoGathered);
                    break;
                case PublicEventObjective.OhmnasRecording:
                case PublicEventObjective.LucentsRecording:
                    publicEvent.UpdateObjective(PublicEventObjective.DiscoverLostRecordings, 1);
                    break;
                case PublicEventObjective.DeactivateDefenses:
                    foreach (uint guid in biomaticsBeams.ToList())
                        mapInstance.GetEntity<IWorldEntity>(guid)?.RemoveFromMap();
                    BroadcastCommunicatorMessage(CommunicatorMessage.LolaBeamsGone);
                    break;
                case PublicEventObjective.LocateBlackBox:
                    blackBoxFound = true;
                    BroadcastCommunicatorMessage(CommunicatorMessage.LolaBlackBoxFound);
                    if (publicEvent.Phase == (uint)PublicEventPhase.FollowProjectionsCreche)
                        RemoveForceField(PublicEventCreature.PellDoorForceField);
                    break;
            }
        }

        private static bool IsSucceeded(IPublicEventTeam team, params PublicEventObjective[] objectives)
        {
            return objectives.All(o => team.GetObjectives().FirstOrDefault(t => t.Entry.Id == (uint)o)?.Status == PublicEventStatus.Succeeded);
        }

        private void OnSearchCrecheForHugo()
        {
            publicEvent.ActivateObjective(PublicEventObjective.DefeatLifeOverseer);

            // the Life-Overseer stops watching and fights
            IUnitEntity lifeOverseer = mapInstance.GetEntity<IUnitEntity>(lifeOverseerGuid);
            lifeOverseer?.InvokeScriptCollection<LifeOverseerEntityScript>(s => s.StartEncounter());
        }

        private void OnExpeditionComplete()
        {
            BroadcastCommunicatorMessage(CommunicatorMessage.HugoYouveDoneIt);

            bool gold = IsVeteran && noDeaths
                && publicEvent.GetTeams().All(t => IsSucceeded(t, PublicEventObjective.SmashXenobiteEggs, PublicEventObjective.CollectCargoCrates,
                    PublicEventObjective.DeactivateDefenses) && t.GetObjectives().FirstOrDefault(o => o.Entry.Id == (uint)PublicEventObjective.GoldMedalTimer)?.Status != PublicEventStatus.Failed);

            foreach (IPlayer player in mapInstance.GetPlayers())
            {
                // Expedition: They Came from Fragment Zero! (10126) and the prime dailies (9909 Exile, 9912 Dominion)
                player.QuestManager.ObjectiveUpdate(19344u, 1u);
                if (IsVeteran)
                {
                    player.QuestManager.ObjectiveUpdate(18922u, 1u);
                    player.QuestManager.ObjectiveUpdate(18926u, 1u);
                }

                GrantAchievement(player, 6020); // Expedition: They Came from Fragment Zero!
                if (IsVeteran)
                {
                    GrantAchievement(player, 6027); // Expedition: They Came from Fragment Zero! (Prime)
                    player.PrimeLevelManager.CompletePrimeLevel(WorldId, primeLevel);
                }
                if (gold)
                    GrantAchievement(player, 6005); // Gold Medal: They Came from Fragment Zero! (Prime)
            }

            publicEvent.Finish(PublicEventTeam.PublicTeam);
        }

        private static void GrantAchievement(IPlayer player, ushort achievementId)
        {
            if (!player.AchievementManager.HasCompletedAchievement(achievementId))
                player.AchievementManager.GrantAchievement(achievementId);
        }

        #endregion

        #region Events from entity scripts

        /// <summary>
        /// A cargo crate or the black box was collected.
        /// </summary>
        public void OnCollect(IWorldEntity entity)
        {
            uint virtualItemId = entity.CreatureId == (uint)PublicEventCreature.BlackBox ? 1190u : 1176u;
            publicEvent.UpdateObjective(PublicEventObjectiveType.VirtualCollect, virtualItemId, 1);
        }

        /// <summary>
        /// A xenobite egg was smashed, some of them hatch.
        /// </summary>
        public void OnEggSmashed(IWorldEntity egg, bool hatches)
        {
            publicEvent.UpdateObjective(PublicEventObjective.SmashXenobiteEggs, 1);
            if (!hatches)
                return;

            for (int i = 0; i < 2; i++)
            {
                Vector3 offset = new((float)(Random.Shared.NextDouble() * 2d - 1d), 0f, (float)(Random.Shared.NextDouble() * 2d - 1d));
                SpawnCreature(PublicEventCreature.Hatchling, egg.Position + offset, (float)(Random.Shared.NextDouble() * MathF.Tau));
            }
        }

        /// <summary>
        /// The Life-Overseer calls his creations to fight for him.
        /// </summary>
        public void OnLifeOverseerSummon(IWorldEntity lifeOverseer)
        {
            uint[] creatures = [67516, 67518, 67520];
            for (int i = 0; i < creatures.Length; i++)
            {
                float angle = MathF.Tau / creatures.Length * i;
                Vector3 position = lifeOverseer.Position + new Vector3(MathF.Cos(angle) * 8f, 0f, MathF.Sin(angle) * 8f);
                SpawnCreature(creatures[i], position, angle + MathF.PI);
            }
        }

        #endregion

        #region Update

        /// <summary>
        /// Invoked each world tick with the delta since the previous tick occurred.
        /// </summary>
        public void Update(double lastTick)
        {
            if (publicEvent == null || publicEvent.HasFinished)
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

            if ((PublicEventPhase)publicEvent.Phase == PublicEventPhase.StayCloseToHugo)
                UpdateHugoEscape(lastTick);

            checkTimer.Update(lastTick);
            if (!checkTimer.HasElapsed)
                return;
            checkTimer.Reset();

            List<IPlayer> players = mapInstance.GetPlayers().ToList();
            if (players.Any(p => !p.IsAlive))
                noDeaths = false;

            switch ((PublicEventPhase)publicEvent.Phase)
            {
                case PublicEventPhase.SearchForMissingCrew:
                    // the players find the friendly skeech near the crash site
                    if (players.Any(p => p.Position.GetDistance(GetLocation(48709)) < 15f))
                        publicEvent.UpdateObjective(PublicEventObjective.SearchForMissingCrew, 1);
                    break;
                case PublicEventPhase.SkeechAmbush:
                    ambushers.RemoveAll(g => mapInstance.GetEntity<IUnitEntity>(g) is not { IsAlive: true });
                    if (ambushers.Count == 0)
                        publicEvent.UpdateObjective(PublicEventObjective.SurviveSkeechAmbush, 1);
                    break;
                case PublicEventPhase.FollowProjectionsIncubation:
                    CheckAmbush(players, 1, IncubationAmbush);
                    break;
                case PublicEventPhase.IncubationComplex:
                    if (players.Any(p => p.Position.GetDistance(GetLocation(48715)) < 25.5f))
                        publicEvent.UpdateObjective(PublicEventObjective.SearchIncubationForJo, 1);
                    break;
                case PublicEventPhase.BiomaticsChamber:
                    if (players.Any(p => p.Position.GetDistance(GetLocation(48732)) < 15f))
                        publicEvent.UpdateObjective(PublicEventObjective.SearchBiomaticsForSyrus, 1);
                    break;
                case PublicEventPhase.FollowProjectionsCreche:
                    CheckAmbush(players, 2, CrecheAmbush);
                    // veteran: Lola wants the black box before going further
                    if (IsVeteran && !blackBoxFound && !ambushesTriggered.Contains(100)
                        && players.Any(p => p.Position.GetDistance(GetLocation(48713)) < 30f))
                    {
                        ambushesTriggered.Add(100);
                        BroadcastCommunicatorMessage(CommunicatorMessage.LolaBlackBox);
                    }
                    break;
            }
        }

        private void CheckAmbush(List<IPlayer> players, int id, Vector2[] positions)
        {
            if (ambushesTriggered.Contains(id))
                return;

            Vector3 centre = Surface(positions[0]);
            if (!players.Any(p => p.Position.GetDistance(centre) < 20f))
                return;

            ambushesTriggered.Add(id);
            SpawnAmbush(positions, 6);
        }

        /// <summary>
        /// Hugo walks back to the ship while players stay close, Skeech keep coming at him.
        /// </summary>
        private void UpdateHugoEscape(double lastTick)
        {
            if (mapInstance.GetEntity<IUnitEntity>(hugoRunnerGuid) is not IUnitEntity hugo)
                return;

            // the Skeech can knock Hugo down but he gets back up, the escort carries on from where he fell
            if (!hugo.IsAlive)
            {
                Vector3 position = hugo.Position;
                RemoveEntity(ref hugoRunnerGuid);
                hugo = SpawnCreature(PublicEventCreature.CaptainHugoRunner, position, 0f) as IUnitEntity;
                if (hugo == null)
                    return;

                hugoRunnerGuid = hugo.Guid;
                hugoTarget     = null;
                CastOnSelf(hugo, 76947);
            }

            List<IPlayer> close = mapInstance.GetPlayers().Where(p => p.IsAlive && p.Position.GetDistance(hugo.Position) < 20f).ToList();

            hugoWaveTimer -= lastTick;
            if (hugoWaveTimer <= 0d && close.Count > 0)
            {
                hugoWaveTimer = 14d;
                SpawnEscapeWave(hugo);
            }

            hugoZapTimer -= lastTick;
            if (hugoZapTimer <= 0d)
            {
                hugoZapTimer = 20d;
                CastOnSelf(hugo, 78626); // aura zap
            }

            // waiting for Hugo to reach the waypoint he is walking to
            if (hugoTarget.HasValue && hugo.Position.GetDistance(hugoTarget.Value) > 1.5f)
                return;

            if (hugoTarget.HasValue)
            {
                hugoTarget = null;
                hugoWaypoint++;
            }

            if (hugoWaypoint >= HugoEscapePath.Length)
            {
                publicEvent.UpdateObjective(PublicEventObjective.StayCloseToHugo, 1);
                return;
            }

            // Hugo only moves on while the players stay close to him
            if (close.Count == 0)
                return;

            List<Vector3> nodes = BuildPath([new Vector2(hugo.Position.X, hugo.Position.Z), HugoEscapePath[hugoWaypoint]]);
            hugoTarget = nodes[^1];
            hugo.MovementManager.LaunchSpline(nodes, Game.Static.Entity.Movement.Spline.SplineType.Linear,
                Game.Static.Entity.Movement.Spline.SplineMode.OneShot, 4f);
        }

        private void SpawnEscapeWave(IUnitEntity hugo)
        {
            PublicEventCreature[] creatures = [PublicEventCreature.EscapeBrawler, PublicEventCreature.EscapeFighter, PublicEventCreature.EscapeBerserker, PublicEventCreature.EscapeShaman];
            int count = 2 + (int)Math.Min(3u, mapInstance.PlayerCount);
            for (int i = 0; i < count; i++)
            {
                float angle = (float)(Random.Shared.NextDouble() * MathF.Tau);
                var point = new Vector2(hugo.Position.X + MathF.Cos(angle) * 18f, hugo.Position.Z + MathF.Sin(angle) * 18f);
                SpawnCreature(creatures[Random.Shared.Next(creatures.Length)], Surface(point), angle + MathF.PI);
            }

            if (Random.Shared.Next(3) == 0)
                BroadcastCommunicatorMessage(CommunicatorMessage.OverseerRemade);
        }

        /// <summary>
        /// Build a surface path through the supplied points with a node every 2 metres at terrain height.
        /// </summary>
        private List<Vector3> BuildPath(Vector2[] points)
        {
            var nodes = new List<Vector3> { Surface(points[0]) };
            for (int i = 1; i < points.Length; i++)
            {
                Vector2 from = points[i - 1], to = points[i];
                int steps = Math.Max(1, (int)(Vector2.Distance(from, to) / 2f));
                for (int s = 1; s <= steps; s++)
                    nodes.Add(Surface(Vector2.Lerp(from, to, s / (float)steps)));
            }

            return nodes;
        }

        private void CastOnSelf(IUnitEntity unit, uint spellId)
        {
            ISpellParameters parameters = spellParametersFactory.Resolve();
            parameters.PrimaryTargetId = unit.Guid;
            unit.CastSpell(spellId, parameters);
        }

        private void Delay(double seconds, Action action)
        {
            delayed.Add((seconds, action));
        }

        #endregion

        #region Messages

        private void BroadcastCommunicatorMessage(CommunicatorMessage message)
        {
            BroadcastCommunicatorMessage((uint)message);
        }

        private void BroadcastCommunicatorMessage(uint messageId)
        {
            ICommunicatorMessage communicatorMessage = globalQuestManager.GetCommunicatorMessage(messageId);
            if (communicatorMessage == null)
                return;

            foreach (IPlayer player in mapInstance.GetPlayers())
                communicatorMessage.Send(player.Session);
        }

        private void BroadcastSystemMessage(uint textId)
        {
            string text = GetText(textId);
            if (string.IsNullOrEmpty(text))
                return;

            foreach (IPlayer player in mapInstance.GetPlayers())
                player.SendSystemMessage(text);
        }

        private string GetText(uint textId)
        {
            return gameTableManager.GetTextTable(Language.English)?.GetEntry(textId);
        }

        #endregion
    }
}
