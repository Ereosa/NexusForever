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
}
