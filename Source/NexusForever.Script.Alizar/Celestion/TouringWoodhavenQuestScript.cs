using System.Numerics;
using NexusForever.Database.World.Model;
using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Entity.Creature;
using NexusForever.Game.Abstract.Quest;
using NexusForever.Game.Static.Entity.Movement.Spline;
using NexusForever.Game.Static.Quest;
using NexusForever.GameTable;
using NexusForever.GameTable.Model;
using NexusForever.Script.Template;
using NexusForever.Script.Template.Filter;

namespace NexusForever.Script.Alizar.Celestion
{
    /// <summary>
    /// Touring Woodhaven: "Join Rogum as he gives you a tour of Woodhaven".
    /// </summary>
    /// <remarks>
    /// Rogum's visibility prerequisite (36825) hides the standing Rogum (53147) while the tour objective is active: retail spawned a tour
    /// guide for the player. The tour is the client's Spline2 17362, which starts at Rogum's spot and stops (EventId 1-4) at the mailbox,
    /// the taxi stand, the mount vendor and the transmat terminal. Here a Rogum is summoned for the player when they come near his spot, he
    /// walks the path, waits at each stop until the player is close (at most 30 seconds, guessed), and the objective completes when he
    /// reaches the end with the player near him. His walking speed is a guess.
    /// The summoned guide is the same creature, so he skips the visibility prerequisite: otherwise it hides him from the player he guides.
    /// </remarks>
    [ScriptFilterOwnerId(9087)]
    public class TouringWoodhavenQuestScript : IQuestScript, IOwnedScript<IQuest>
    {
        private const uint TourObjectiveId = 16462u;
        private const uint Rogum           = 53147u;
        private const uint TourSpline      = 17362u;
        private const float WalkSpeed      = 2.5f;
        private const float JoinDistance   = 12f;
        private const float StopDistance   = 15f;
        private const double StopTimeout   = 30d;

        private IQuest owner;
        private uint? guideGuid;
        private List<(Vector3 Position, bool Stop)> path;
        private int segmentEnd;
        private bool walking;
        private double waited;
        private double check;

        #region Dependency Injection

        private readonly ICreatureInfoManager creatureInfoManager;
        private readonly IGameTableManager gameTableManager;

        public TouringWoodhavenQuestScript(
            ICreatureInfoManager creatureInfoManager,
            IGameTableManager gameTableManager)
        {
            this.creatureInfoManager = creatureInfoManager;
            this.gameTableManager    = gameTableManager;
        }

        #endregion

        public void OnLoad(IQuest owner)
        {
            this.owner = owner;
        }

        public void OnUnload()
        {
            RemoveGuide();
        }

        public void Update(double lastTick)
        {
            check -= lastTick;
            if (check > 0d)
                return;
            check = 0.5d;

            IPlayer player = owner.GetOwner();
            IQuestObjective tour = owner.GetQuestObjective(TourObjectiveId);
            if (player?.Map == null || player.IsLoading || owner.State != QuestState.Accepted || tour == null || tour.IsComplete())
            {
                RemoveGuide();
                return;
            }

            path ??= gameTableManager.Spline2Node.Entries
                .Where(n => n != null && n.SplineId == TourSpline)
                .OrderBy(n => n.Ordinal)
                .Select(n => (new Vector3(n.Position0, n.Position1, n.Position2), n.EventId is >= 1u and <= 4u))
                .ToList();
            if (path.Count == 0 || player.Map.Entry.Id != 51u)
                return;

            IWorldEntity guide = guideGuid.HasValue ? player.Map.GetEntity<IWorldEntity>(guideGuid.Value) : null;
            if (guide == null)
            {
                guideGuid = null;
                if (Vector3.Distance(player.Position, path[0].Position) <= JoinDistance)
                    SummonGuide(player);
                return;
            }

            if (walking)
            {
                if (Vector3.Distance(guide.Position, path[segmentEnd].Position) > 1.5f)
                    return;
                walking = false;
                waited  = 0d;
            }

            // at a stop the guide waits for the player
            if (path[segmentEnd].Stop && Vector3.Distance(player.Position, guide.Position) > StopDistance)
            {
                waited += 0.5d;
                if (waited < StopTimeout)
                    return;
            }

            if (segmentEnd >= path.Count - 1)
            {
                if (Vector3.Distance(player.Position, guide.Position) <= StopDistance * 2f)
                    owner.ObjectiveUpdate(TourObjectiveId, 1u);
                RemoveGuide();
                return;
            }

            WalkNextSegment(guide);
        }

        private void SummonGuide(IPlayer player)
        {
            ICreatureInfo creatureInfo = creatureInfoManager.GetCreatureInfo(Rogum);
            if (creatureInfo == null)
                return;

            segmentEnd = 0;
            walking    = false;
            player.SummonFactory.Summon<INonPlayerEntity>(new TourGuideCreatureInfo(creatureInfo), path[0].Position, Vector3.Zero, (map, guid, vector) =>
            {
                guideGuid = guid;
            });
        }

        /// <summary>
        /// Walk to the next stop (or the end of the path).
        /// </summary>
        private void WalkNextSegment(IWorldEntity guide)
        {
            var nodes = new List<Vector3> { guide.Position };
            int next = segmentEnd + 1;
            for (; next < path.Count; next++)
            {
                nodes.Add(path[next].Position);
                if (path[next].Stop)
                    break;
            }
            segmentEnd = Math.Min(next, path.Count - 1);
            walking = true;
            guide.MovementManager.LaunchSpline(nodes, SplineType.Linear, SplineMode.OneShot, WalkSpeed);
        }

        private void RemoveGuide()
        {
            if (guideGuid.HasValue)
                owner?.GetOwner()?.Map?.GetEntity<IWorldEntity>(guideGuid.Value)?.RemoveFromMap();
            guideGuid = null;
            walking = false;
        }

        /// <summary>
        /// <see cref="ICreatureInfo"/> of the tour guide: Rogum without his visibility prerequisite.
        /// </summary>
        private class TourGuideCreatureInfo : ICreatureInfo
        {
            public Creature2Entry Entry => creatureInfo.Entry;
            public Creature2DifficultyEntry DifficultyEntry => creatureInfo.DifficultyEntry;
            public Creature2ArcheTypeEntry ArcheTypeEntry => creatureInfo.ArcheTypeEntry;
            public Creature2TierEntry TierEntry => creatureInfo.TierEntry;
            public Creature2ModelInfoEntry ModelEntry => creatureInfo.ModelEntry;
            public UnitVehicleEntry UnitVehicleEntry => creatureInfo.UnitVehicleEntry;
            public PrerequisiteEntry PrerequisiteVisibilityEntry => null;

            private readonly ICreatureInfo creatureInfo;

            public TourGuideCreatureInfo(ICreatureInfo creatureInfo)
            {
                this.creatureInfo = creatureInfo;
            }

            public void Initialise(Creature2Entry entry) => creatureInfo.Initialise(entry);
            public void InitialiseOverrides(IEnumerable<CreatureInfoPropertyModel> properties, IEnumerable<CreatureInfoStatModel> stats) => creatureInfo.InitialiseOverrides(properties, stats);
            public IEnumerable<ICreatureInfoProperty> GetPropertyOverrides() => creatureInfo.GetPropertyOverrides();
            public IEnumerable<ICreatureInfoStat> GetStatOverrides() => creatureInfo.GetStatOverrides();
            public uint GetLevel() => creatureInfo.GetLevel();
            public Creature2DisplayInfoEntry GetDisplayInfoEntry() => creatureInfo.GetDisplayInfoEntry();
            public Creature2OutfitInfoEntry GetOutfitInfoEntry() => creatureInfo.GetOutfitInfoEntry();
        }
    }
}
