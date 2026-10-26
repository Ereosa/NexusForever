using System.Numerics;
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
