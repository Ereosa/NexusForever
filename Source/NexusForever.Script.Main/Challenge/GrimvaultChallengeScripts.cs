using System.Numerics;
using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Entity.Creature;
using NexusForever.GameTable;
using NexusForever.GameTable.Model;
using NexusForever.Script.Template.Filter;

namespace NexusForever.Script.Main.Challenge
{
    /// <summary>
    /// Demolition Expert (Western Grimvault): "Set off Strain Mines without getting hit" around the Boiling Cauldron, each Strain Mine
    /// set off by running into it counts.
    /// </summary>
    /// <remarks>
    /// The mines (39679) have no activate spell, they go off when walked into. Their blast isn't told apart from other damage, so
    /// getting hit doesn't fail the run.
    /// </remarks>
    [ScriptFilterOwnerId(658)]
    public class DemolitionExpertChallengeScript : TouchChallengeScript
    {
        protected override uint[] CreatureIds { get; } = [39679];
        protected override float TouchRange => 3f;
    }

    /// <summary>
    /// Resonance Runner (Blighthaven): "Follow the holorings to the Defile's entrance before time runs out".
    /// </summary>
    [ScriptFilterOwnerId(1079)]
    public class ResonanceRunnerChallengeScript : RaceToAreaChallengeScript
    {
    }

    /// <summary>
    /// Falling With Style (Western Grimvault): "Pass through vortex rings on the way down from the tower" of the Bio-Mech Facility,
    /// two towers (994, 995).
    /// </summary>
    /// <remarks>
    /// Reconstruction, the Vortex Rings (51041) aren't in any capture. The three rings hang on the line from the Butterfly Box on top
    /// of the tower down to the challenge location below, a quarter, half and three quarters of the way down.
    /// </remarks>
    [ScriptFilterOwnerId(994, 995)]
    public class FallingWithStyleChallengeScript : ChallengeScript
    {
        private const uint RingCreatureId = 51041;
        private const float RingRange     = 6f;

        private static readonly Dictionary<uint, Vector3> towerTop = new()
        {
            [994] = new(-239.43f, -520.72f, -3645.10f),
            [995] = new(-236.71f, -520.72f, -4550.46f)
        };

        private readonly ICreatureInfoManager creatureInfoManager;
        private readonly List<uint> rings = [];
        private readonly HashSet<uint> passed = [];

        public FallingWithStyleChallengeScript(ICreatureInfoManager creatureInfoManager)
        {
            this.creatureInfoManager = creatureInfoManager;
        }

        public override void OnActivate()
        {
            passed.Clear();

            WorldLocation2Entry bottom = GameTableManager.Instance.WorldLocation2.GetEntry(Challenge.Entry.WorldLocation2IdIndicator);
            ICreatureInfo creatureInfo = creatureInfoManager.GetCreatureInfo(RingCreatureId);
            if (bottom == null || creatureInfo == null || !towerTop.TryGetValue(Challenge.Entry.Id, out Vector3 top))
                return;

            var end = new Vector3(bottom.Position0, bottom.Position1, bottom.Position2);
            for (int i = 1; i <= 3; i++)
                Player.SummonFactory.Summon(creatureInfo, Vector3.Lerp(top, end, i / 4f), Vector3.Zero, (map, guid, p) => rings.Add(guid));
        }

        public override void Update(double lastTick)
        {
            foreach (uint guid in rings)
            {
                if (passed.Contains(guid))
                    continue;

                IWorldEntity ring = Player.GetVisible<IWorldEntity>(guid);
                if (ring == null || Vector3.Distance(ring.Position, Player.Position) > RingRange)
                    continue;

                passed.Add(guid);
                AddProgress();
                if (!Challenge.Activated)
                    return;
            }
        }

        public override void OnStop(bool completed)
        {
            foreach (uint guid in rings)
                Player.SummonFactory.Unsummon(guid);
            rings.Clear();
        }
    }

    /// <summary>
    /// Windwalker (Blighthaven): "Use air currents to race to the top of the floating rock formation!"
    /// </summary>
    /// <remarks>
    /// The challenge starts and ends at the same location below the rocks, their height isn't in any data, the top counts as
    /// anywhere over that location at least <see cref="ClimbHeight"/> above it.
    /// </remarks>
    [ScriptFilterOwnerId(1014)]
    public class WindwalkerChallengeScript : ChallengeScript
    {
        private const float ClimbHeight = 25f;

        public override void Update(double lastTick)
        {
            WorldLocation2Entry rocks = GameTableManager.Instance.WorldLocation2.GetEntry(Challenge.Entry.WorldLocation2IdIndicator);
            if (rocks == null || Player.Map?.Entry.Id != rocks.WorldId)
                return;

            if (HorizontalDistance(Player.Position, new Vector3(rocks.Position0, rocks.Position1, rocks.Position2)) <= rocks.Radius + 10f
                && Player.Position.Y >= rocks.Position1 + ClimbHeight)
                Complete();
        }
    }
}
