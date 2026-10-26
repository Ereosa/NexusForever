using System.Numerics;
using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Entity.Creature;
using NexusForever.Game.Static.Entity.Movement.Spline;
using NexusForever.GameTable;
using NexusForever.GameTable.Model;
using NexusForever.Script.Template;
using NexusForever.Script.Template.Filter;

namespace NexusForever.Script.Main.Challenge
{
    /// <summary>
    /// Dig Site Security (Ellevar): "Turn on the Dominion Security Lights inside the Bio-Mech Facility 498", each Dominion Bio-Mech
    /// Security (Challenge 504) used counts.
    /// </summary>
    [ScriptFilterOwnerId(504)]
    public class DigSiteSecurityChallengeScript : ChallengeScript
    {
        private const uint SecurityLightCreatureId = 33146;

        public override void OnEntityActivated(IWorldEntity entity)
        {
            if (entity.CreatureId == SecurityLightCreatureId)
                AddProgress();
        }
    }

    /// <summary>
    /// Main Flow Energy Crystal (Vein of the Flow, Ellevar), "Starts Challenge 496" in its description.
    /// </summary>
    [ScriptFilterCreatureId(32248)]
    public class MainFlowEnergyCrystalEntityScript : IWorldEntityScript, IOwnedScript<IWorldEntity>
    {
        private const uint GettingIntoTheFlowId = 496;

        public void OnLoad(IWorldEntity owner)
        {
        }

        public void OnActivateSuccess(IPlayer activator)
        {
            if (!activator.ChallengeManager.IsActive(GettingIntoTheFlowId))
                activator.ChallengeManager.Activate(GettingIntoTheFlowId);
        }
    }

    /// <summary>
    /// Getting into the Flow (Ellevar): "Jump through Flow Energy Crystals from the top of the Vein of the Flow".
    /// </summary>
    /// <remarks>
    /// Started from the Main Flow Energy Crystal, which carries the player up the Vein of the Flow along client spline 14859 (it starts
    /// at the crystal and rises 55 m). The four Flow Energy Crystals (32188) were personal phase spawns that aren't in any data, they
    /// are placed on the way down from the top of the spline to the challenge indicator below.
    /// </remarks>
    [ScriptFilterOwnerId(496)]
    public class GettingIntoTheFlowChallengeScript : TouchChallengeScript
    {
        private const ushort FlowSplineId = 14859;
        private const uint FlowCrystalId  = 32188;
        private const int CrystalCount    = 4;
        private const float RideSpeed     = 8f;

        /// <summary>
        /// Last node of spline 14859, the top of the Vein of the Flow.
        /// </summary>
        private static readonly Vector3 top = new(-3431.4f, -704.6f, -3397.0f);

        private readonly ICreatureInfoManager creatureInfoManager;
        private readonly List<uint> crystals = [];

        protected override uint[] CreatureIds { get; } = [FlowCrystalId];

        public GettingIntoTheFlowChallengeScript(ICreatureInfoManager creatureInfoManager)
        {
            this.creatureInfoManager = creatureInfoManager;
        }

        public override void OnActivate()
        {
            base.OnActivate();

            Player.MovementManager.LaunchSpline(FlowSplineId, SplineMode.OneShot, RideSpeed, false);

            WorldLocation2Entry bottom = GameTableManager.Instance.WorldLocation2.GetEntry(Challenge.Entry.WorldLocation2IdIndicator);
            ICreatureInfo creatureInfo = creatureInfoManager.GetCreatureInfo(FlowCrystalId);
            if (bottom == null || creatureInfo == null)
                return;

            var end = new Vector3(bottom.Position0, bottom.Position1, bottom.Position2);
            for (int i = 1; i <= CrystalCount; i++)
            {
                Vector3 position = Vector3.Lerp(top, end, i / (float)(CrystalCount + 1));
                Player.SummonFactory.Summon(creatureInfo, position, Vector3.Zero, (map, guid, p) => crystals.Add(guid));
            }
        }

        public override void OnStop(bool completed)
        {
            foreach (uint guid in crystals)
                Player.SummonFactory.Unsummon(guid);
            crystals.Clear();
        }
    }

    /// <summary>
    /// Kickin' It (Ellevar): "Kick the ball through each waypoint", the Stormseeker Ball game of the Stormseeker Broodlings in the Larallen Highlands.
    /// </summary>
    /// <remarks>
    /// The ball (26153) and the Stormseeker Ball Goals (31628) were event spawns that aren't in the world spawns. The ball starts at the
    /// challenge start location, running into it kicks it a few metres away from the player. The waypoints are where Jabbithole's
    /// sightings of the ball cluster away from the kick off spot (the ball rests in a goal after going through it), in order of distance.
    /// </remarks>
    [ScriptFilterOwnerId(470)]
    public class KickinItChallengeScript : ChallengeScript
    {
        private const uint BallCreatureId = 26153;
        private const uint GoalCreatureId = 31628;
        private const float KickRange     = 2f;
        private const float KickDistance  = 10f;
        private const float KickSpeed     = 12f;
        private const float GoalRange     = 3f;

        private static readonly Vector2[] waypoints =
        [
            new(-2809f, -3555f),
            new(-2827f, -3537f),
            new(-2884f, -3498f),
            new(-2854f, -3445f)
        ];

        private readonly ICreatureInfoManager creatureInfoManager;
        private readonly List<uint> goals = [];
        private uint ball;
        private int next;
        private double kickCooldown;
        private float groundHeight;

        public KickinItChallengeScript(ICreatureInfoManager creatureInfoManager)
        {
            this.creatureInfoManager = creatureInfoManager;
        }

        public override void OnActivate()
        {
            next = 0;
            ball = 0u;
            goals.Clear();

            WorldLocation2Entry start = GameTableManager.Instance.WorldLocation2.GetEntry(Challenge.Entry.WorldLocation2IdStartLocation);
            if (start == null)
                return;

            groundHeight = start.Position1;

            ICreatureInfo ballInfo = creatureInfoManager.GetCreatureInfo(BallCreatureId);
            if (ballInfo != null)
                Player.SummonFactory.Summon(ballInfo, new Vector3(start.Position0, groundHeight, start.Position2), Vector3.Zero, (map, guid, p) => ball = guid);

            ICreatureInfo goalInfo = creatureInfoManager.GetCreatureInfo(GoalCreatureId);
            if (goalInfo == null)
                return;

            foreach (Vector2 waypoint in waypoints)
                Player.SummonFactory.Summon(goalInfo, new Vector3(waypoint.X, groundHeight, waypoint.Y), Vector3.Zero, (map, guid, p) => goals.Add(guid));
        }

        public override void Update(double lastTick)
        {
            kickCooldown -= lastTick;

            IWorldEntity ballEntity = ball != 0u ? Player.GetVisible<IWorldEntity>(ball) : null;
            if (ballEntity == null)
                return;

            if (kickCooldown <= 0d && HorizontalDistance(ballEntity.Position, Player.Position) <= KickRange)
            {
                Vector3 direction = ballEntity.Position - Player.Position;
                direction.Y = 0f;
                if (direction.LengthSquared() > 0.0001f)
                {
                    Vector3 target = ballEntity.Position + Vector3.Normalize(direction) * KickDistance;
                    target.Y = groundHeight;
                    ballEntity.MovementManager.LaunchSpline([ballEntity.Position, target], SplineType.Linear, SplineMode.OneShot, KickSpeed);
                    kickCooldown = KickDistance / KickSpeed;
                }
            }

            if (next >= waypoints.Length || Vector2.Distance(new Vector2(ballEntity.Position.X, ballEntity.Position.Z), waypoints[next]) > GoalRange)
                return;

            if (next < goals.Count)
                Player.SummonFactory.Unsummon(goals[next]);

            next++;
            AddProgress();
            if (Challenge.Activated && next >= waypoints.Length)
                Complete();
        }

        public override void OnStop(bool completed)
        {
            if (ball != 0u)
                Player.SummonFactory.Unsummon(ball);
            foreach (uint guid in goals)
                Player.SummonFactory.Unsummon(guid);
            goals.Clear();
            ball = 0u;
        }
    }
}
