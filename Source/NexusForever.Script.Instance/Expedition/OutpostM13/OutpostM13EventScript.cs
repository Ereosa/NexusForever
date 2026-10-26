using System.Numerics;
using NexusForever.Game.Abstract.Cinematic;
using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Entity.Creature;
using NexusForever.Game.Abstract.PublicEvent;
using NexusForever.Game.Abstract.Quest;
using NexusForever.Game.Abstract.Spell;
using NexusForever.Game.Static.Cinematic;
using NexusForever.Game.Static.PublicEvent;
using NexusForever.GameTable;
using NexusForever.Script.Template.Filter;
using NexusForever.Shared;

namespace NexusForever.Script.Instance.Expedition.OutpostM13
{
    /// <summary>
    /// Outpost M-13 (public event 108, world 1319): Captain Milo's Steady Traveler and the mining asteroid M-13.
    /// </summary>
    /// <remarks>
    /// No packet captures exist, the order follows the objective data (two game versions: the 2014 objectives and the 2015 additions):
    /// talk to Captain Milo on the bridge, Novaburn Marauders board the ship in the cargo hold ("Ransacker" Rorgh), the exit panel
    /// takes the players down to M-13, Foreman Krause in the barracks, Captain Wiko's marauders outside the mine (veteran holdout),
    /// the infested Abandoned Space Mine with the Hive Queen, then Captain Milo's shuttle outside the mine.
    /// Positions and abilities come from archived Jabbithole data, the cutscenes from the client (GC176 arrival, GC179 departure).
    /// Guessed: wave sizes, the panels' teleport spots (the Jabbithole positions of the panels), trigger radii, the bridge door opening
    /// after Captain Milo's briefing, datachrons from searching the dead miners (their activate spell), mining charges picked up by walking
    /// over them (they have no activate spell).
    /// </remarks>
    [ScriptFilterOwnerId(108)]
    public class OutpostM13EventScript : ExpeditionEventScript
    {
        private enum Phase : uint
        {
            TalkToMilo,
            GoToCargoHold,
            MaraudersAboard,
            KillRorgh,
            ExitShip,
            FindKrause,
            MaraudersOutside,
            KillWiko,
            GatherOutsideMine,
            SearchMine,
            ClearHive,
            KillQueen,
            HeadToShuttle,
            Done
        }

        private static class Objective
        {
            public const uint DefendShards        = 252;
            public const uint KillMarauders       = 253;
            public const uint KillRorgh           = 254;
            public const uint DefeatHive          = 256;
            public const uint KillQueen           = 257;
            public const uint ExitShip            = 262;
            public const uint CollectDatachrons   = 656;
            public const uint SearchMine          = 1439;
            public const uint FindKrause          = 1440;
            public const uint TalkToMilo          = 1441;
            public const uint GoToCargoHold       = 1442;
            public const uint MiningCharges       = 4696;
            public const uint KillMaraudersOnM13  = 4697;
            public const uint KillNovaburnLeader  = 4700;
            public const uint GatherOutsideMine   = 4701;
            public const uint GoldMedalTimer      = 4702;
            public const uint MarauderWave1       = 4733;
            public const uint MarauderWave2       = 4734;
            public const uint HeadToShuttle       = 5134;
        }

        // ship: the cargo hold and its marauder entry points (Jabbithole sightings of the boarders), asteroid: the panels and the holdout
        private static readonly Vector3 CargoHold      = new(9784.983f, -942.578f, 5073.07f);   // WorldLocation2 33115
        private static readonly Vector3 ShardDefence   = new(9782.113f, -939.792f, 5106.713f);  // WorldLocation2 23113, objectives 252 and 253
        private static readonly Vector3 RorghSpawn     = new(9783f, -939.792f, 5123f);          // densest Jabbithole sighting
        private static readonly Vector3 ExitPanel      = new(9681.316f, -930.104f, 5037.397f);  // WorldLocation2 28830
        private static readonly Vector3 AsteroidLanding = new(-138.9f, -526.0f, -150.0f);       // WorldLocation2 17265-17268
        private static readonly Vector3 ShipBridge     = new(9791.18f, -932.668f, 5264.279f);   // WorldLocation2 22152
        private static readonly Vector3 MarauderCamp   = new(-351.375f, -510.58f, 187.761f);    // WorldLocation2 49536
        private static readonly Vector3 MineEntrance   = new(-367.087f, -534.399f, 321.734f);   // WorldLocation2 17282
        private static readonly Vector3 Shuttle        = new(-402.379f, -487.698f, 117.481f);   // WorldLocation2 51261
        // the densest Jabbithole sightings of the boarding corsairs and plunderers, a line across the hold behind the shards
        private static readonly Vector2[] BoardingPoints = [new(9807f, 5109f), new(9751f, 5108f), new(9790f, 5110f), new(9822f, 5108f), new(9763f, 5110f), new(9736f, 5107f)];

        private static readonly Dictionary<uint, uint> Veteran = new()
        {
            { 23453, 69074 }, { 23454, 69075 }, { 23455, 69076 }, { 23500, 69077 }, { 23513, 69078 }, { 23514, 69079 },
            { 23536, 69080 }, { 23539, 69081 }, { 23565, 69082 }, { 23626, 69083 }, { 29612, 69084 }, { 23560, 69108 },
            { 23511, 69109 }, { 23571, 69126 }
        };

        // the 2015 veteran holdout: mining charges, the marauder ship, machinegunners, smashers, Captain Wiko, the camp's force field
        private static readonly uint[] VeteranOnly = [69843, 69875, 69878, 69879, 69881, 69882, 69886, 69898, 70053];

        private const uint MarauderCorsair   = 23453;
        private const uint MarauderPlunderer = 23454;
        private const uint MarauderCorsair2  = 23565;
        private const uint Rorgh             = 23455;
        private const uint HivePod           = 23514;
        private const uint InfectedMiner     = 23500;
        private const uint HiveInfector      = 23536;
        private const uint HiveQueen         = 23513;
        private const uint AcceleriteShard   = 23451;
        private const uint ExitPanelCreature = 23470;
        private const uint EntryPanelCreature = 36676;
        private const uint MiningCharge      = 69843;
        private const uint CaptainWiko       = 69898;
        private const uint MarauderForceField = 70053;
        private const uint MachinegunnerD3   = 69878;
        private const uint SmasherD3         = 69879;
        private const uint MachinegunnerD2   = 69881;
        private const uint SmasherD2         = 69882;
        private const uint BridgeDoor        = 23591;
        private const uint BarracksDoor      = 41224;
        private const uint BarracksDoorControls = 41221;

        private static readonly uint[] DeadMiners = [29512, 29578, 29579, 29581, 29582, 29583, 29585, 29586, 29587];

        protected override uint WorldId => 1319;
        protected override uint InitialPhase => (uint)Phase.TalkToMilo;
        protected override IReadOnlyDictionary<uint, uint> VeteranCreatures => Veteran;
        protected override IReadOnlyCollection<uint> VeteranOnlyCreatures => VeteranOnly;

        private readonly List<IWorldEntity> boarders = [];
        private readonly HashSet<IWorldEntity> boardersAdded = [];
        private int boardersKilled;
        private IWorldEntity rorgh;
        private uint forceFieldGuid;
        private uint wikoGuid;
        private uint bridgeDoorGuid;
        private uint barracksDoorGuid;
        private readonly List<uint> miningCharges = [];
        private readonly HashSet<uint> searchedMiners = [];
        private int waves;

        public OutpostM13EventScript(
            IGlobalQuestManager globalQuestManager,
            ICreatureInfoManager creatureInfoManager,
            IEntityFactory entityFactory,
            IFactory<ISpellParameters> spellParametersFactory,
            IGameTableManager gameTableManager,
            ICinematicFactory cinematicFactory)
            : base(globalQuestManager, creatureInfoManager, entityFactory, spellParametersFactory, gameTableManager, cinematicFactory)
        {
        }

        public override void OnAddToMap(IGridEntity entity)
        {
            base.OnAddToMap(entity);

            if (entity is not IWorldEntity worldEntity)
                return;

            switch (worldEntity.CreatureId)
            {
                case MarauderForceField:
                    forceFieldGuid = worldEntity.Guid;
                    break;
                case CaptainWiko:
                    wikoGuid = worldEntity.Guid;
                    break;
                case BridgeDoor:
                    bridgeDoorGuid = worldEntity.Guid;
                    break;
                case BarracksDoor:
                    barracksDoorGuid = worldEntity.Guid;
                    break;
                case MiningCharge:
                    miningCharges.Add(worldEntity.Guid);
                    break;
            }
        }

        protected override void OnPhase(uint phase)
        {
            switch ((Phase)phase)
            {
                case Phase.TalkToMilo:
                    // optional objectives for the whole run
                    Activate(Objective.CollectDatachrons, 10u);
                    if (IsVeteran)
                    {
                        Activate(Objective.MiningCharges);
                        Activate(Objective.GoldMedalTimer);
                    }
                    break;
                case Phase.GoToCargoHold:
                    // the bridge door opens once Captain Milo has sent the players down to the hold
                    mapInstance.GetEntity<IDoorEntity>(bridgeDoorGuid)?.OpenDoor();
                    ActivateForPlayers(Objective.GoToCargoHold);
                    AddTrigger(4912, CargoHold, 18f);
                    break;
                case Phase.MaraudersAboard:
                    Activate(Objective.DefendShards);
                    Activate(Objective.KillMarauders, 12u);
                    SpawnBoarders(6);
                    Delay(25d, () => SpawnBoarders(6));
                    break;
                case Phase.KillRorgh:
                    Activate(Objective.KillRorgh);
                    rorgh = SpawnCreature(Rorgh, RorghSpawn, MathF.PI, AggroNearestPlayer);
                    break;
                case Phase.ExitShip:
                    Activate(Objective.ExitShip);
                    break;
                case Phase.FindKrause:
                    Activate(Objective.FindKrause);
                    break;
                case Phase.MaraudersOutside:
                    Activate(Objective.KillMaraudersOnM13);
                    Activate(Objective.MarauderWave1);
                    SpawnHoldoutWave();
                    break;
                case Phase.KillWiko:
                    Activate(Objective.KillNovaburnLeader);
                    RemoveEntity(forceFieldGuid);
                    if (mapInstance.GetEntity<IUnitEntity>(wikoGuid) is IUnitEntity wiko)
                        AggroNearestPlayer(mapInstance, wiko.Guid, wiko.Position);
                    break;
                case Phase.GatherOutsideMine:
                    ActivateForPlayers(Objective.GatherOutsideMine);
                    AddTrigger(8090, GetLocation(49538), 10f, 69886);
                    break;
                case Phase.SearchMine:
                    Activate(Objective.SearchMine);
                    break;
                case Phase.ClearHive:
                    Activate(Objective.DefeatHive, 30u);
                    break;
                case Phase.KillQueen:
                    Activate(Objective.KillQueen);
                    break;
                case Phase.HeadToShuttle:
                    ActivateForPlayers(Objective.HeadToShuttle);
                    AddTrigger(8464, Shuttle, 10f);
                    break;
            }
        }

        private void SpawnBoarders(int count)
        {
            uint[] creatures = [MarauderPlunderer, MarauderCorsair, MarauderPlunderer, MarauderCorsair2];
            for (int i = 0; i < count; i++)
            {
                Vector2 point = BoardingPoints[i % BoardingPoints.Length] + new Vector2((float)(Random.Shared.NextDouble() * 3d - 1.5d), (float)(Random.Shared.NextDouble() * 3d - 1.5d));
                IWorldEntity boarder = SpawnCreature(creatures[i % creatures.Length], Surface(point.X, point.Y, ShardDefence.Y), 0f, AggroNearestPlayer);
                if (boarder != null)
                    boarders.Add(boarder);
            }
        }

        private void SpawnHoldoutWave()
        {
            uint[] creatures = [MachinegunnerD3, SmasherD3, MachinegunnerD2, SmasherD2, SmasherD3];
            for (int i = 0; i < 5; i++)
            {
                float angle = MathF.Tau / 5f * i;
                Vector3 position = Surface(MarauderCamp.X + MathF.Cos(angle) * 10f, MarauderCamp.Z + MathF.Sin(angle) * 10f, MarauderCamp.Y);
                IWorldEntity attacker = SpawnCreature(creatures[i % creatures.Length], position, angle + MathF.PI, AggroNearestPlayer);
                if (attacker != null)
                    boarders.Add(attacker);
            }
        }

        protected override void OnObjectiveSucceeded(uint objectiveId, IPublicEventTeam team)
        {
            switch (objectiveId)
            {
                case Objective.TalkToMilo:
                    publicEvent.SetPhase(Phase.GoToCargoHold);
                    break;
                case Objective.GoToCargoHold:
                    publicEvent.SetPhase(Phase.MaraudersAboard);
                    break;
                case Objective.KillMarauders:
                    // the shards made it through the boarding
                    if (!failedShards)
                        Update(Objective.DefendShards);
                    publicEvent.SetPhase(Phase.KillRorgh);
                    break;
                case Objective.KillRorgh:
                    publicEvent.SetPhase(Phase.ExitShip);
                    break;
                case Objective.ExitShip:
                    publicEvent.SetPhase(Phase.FindKrause);
                    break;
                case Objective.FindKrause:
                    publicEvent.SetPhase(IsVeteran ? Phase.MaraudersOutside : Phase.SearchMine);
                    break;
                case Objective.MarauderWave1:
                    Activate(Objective.MarauderWave2);
                    SpawnHoldoutWave();
                    break;
                case Objective.MarauderWave2:
                    Update(Objective.KillMaraudersOnM13);
                    break;
                case Objective.KillMaraudersOnM13:
                    publicEvent.SetPhase(Phase.KillWiko);
                    break;
                case Objective.KillNovaburnLeader:
                    publicEvent.SetPhase(Phase.GatherOutsideMine);
                    break;
                case Objective.GatherOutsideMine:
                    publicEvent.SetPhase(Phase.SearchMine);
                    break;
                case Objective.SearchMine:
                    publicEvent.SetPhase(Phase.ClearHive);
                    break;
                case Objective.DefeatHive:
                    publicEvent.SetPhase(Phase.KillQueen);
                    break;
                case Objective.KillQueen:
                    publicEvent.SetPhase(Phase.HeadToShuttle);
                    break;
                case Objective.HeadToShuttle:
                    publicEvent.SetPhase(Phase.Done);
                    PlayCinematic(CinematicSceneId.OutpostM13Departure);
                    Complete(team);
                    break;
            }
        }

        private void Complete(IPublicEventTeam team)
        {
            bool gold = IsVeteran && !IsFailed(team, Objective.GoldMedalTimer)
                && IsSucceeded(team, Objective.CollectDatachrons, Objective.MiningCharges, Objective.DefendShards);

            // Expedition: Outpost M-13 (10125), the prime dailies (9915, 9916); Bug Hunt, Outpost M-13 (Prime), Gold Medal (Prime)
            CompleteExpedition([19343], [18932, 18933], 1937, 6021, 6006, gold);
        }

        public override void OnEntityDeath(IUnitEntity entity)
        {
            uint id = entity.CreatureInfo?.Entry?.Id ?? 0u;
            switch ((Phase)publicEvent.Phase)
            {
                case Phase.MaraudersAboard when boarders.Contains(entity):
                    Update(Objective.KillMarauders);
                    break;
                case Phase.KillRorgh when entity == rorgh:
                    Update(Objective.KillRorgh);
                    break;
                case Phase.MaraudersOutside when boarders.Contains(entity):
                    boardersKilled++;
                    if (boardersKilled % 5 == 0)
                        Update(waves++ == 0 ? Objective.MarauderWave1 : Objective.MarauderWave2);
                    break;
                case Phase.KillWiko when entity.Guid == wikoGuid:
                    Update(Objective.KillNovaburnLeader);
                    break;
                case Phase.ClearHive when IsHive(entity.CreatureId):
                    Update(Objective.DefeatHive);
                    break;
                case Phase.KillQueen when entity.CreatureId == GetCreature(HiveQueen):
                    Update(Objective.KillQueen);
                    break;
            }

            // the accelerite shards were destroyed by the boarders
            if (id == AcceleriteShard && (Phase)publicEvent.Phase == Phase.MaraudersAboard)
                failedShards = true;
        }

        private bool failedShards;

        private bool IsHive(uint creatureId)
        {
            return creatureId == GetCreature(HivePod) || creatureId == GetCreature(InfectedMiner) || creatureId == GetCreature(HiveInfector);
        }

        public override void OnEntityActivated(IWorldEntity entity, IPlayer activator)
        {
            switch (entity.CreatureId)
            {
                // the exit panel takes the players down to the asteroid, the entry panel back up to the ship
                case ExitPanelCreature:
                    activator.TeleportToLocal(AsteroidLanding);
                    if ((Phase)publicEvent.Phase == Phase.ExitShip)
                        Update(Objective.ExitShip);
                    break;
                case EntryPanelCreature:
                    activator.TeleportToLocal(ExitPanel + new Vector3(3f, 0f, 3f));
                    break;
                // the barracks exit, Foreman Krause's door out to the marauder camp and the mine
                case BarracksDoorControls:
                    mapInstance.GetEntity<IDoorEntity>(barracksDoorGuid)?.OpenDoor();
                    break;
                default:
                    if (DeadMiners.Contains(entity.CreatureId) && searchedMiners.Add(entity.Guid))
                        Update(Objective.CollectDatachrons);
                    break;
            }
        }

        protected override void OnCheck()
        {
            switch ((Phase)publicEvent.Phase)
            {
                case Phase.MaraudersAboard:
                    foreach (IWorldEntity boarder in boarders.Where(b => b.InWorld))
                        boardersAdded.Add(boarder);
                    break;
                case Phase.SearchMine:
                    if (AnyPlayerNear(MineEntrance, 15f))
                        Update(Objective.SearchMine);
                    break;
            }

            if (!IsVeteran)
                return;

            // mining charges are picked up by walking over them
            foreach (uint guid in miningCharges.ToList())
            {
                IWorldEntity charge = mapInstance.GetEntity<IWorldEntity>(guid);
                if (charge == null)
                {
                    miningCharges.Remove(guid);
                    continue;
                }

                if (!Players.Any(p => Vector3.DistanceSquared(p.Position, charge.Position) < 9f))
                    continue;

                miningCharges.Remove(guid);
                charge.RemoveFromMap();
                Update(Objective.MiningCharges);
            }
        }
    }
}
