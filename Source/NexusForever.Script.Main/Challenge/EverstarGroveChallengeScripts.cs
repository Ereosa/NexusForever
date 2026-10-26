using NexusForever.Script.Template.Filter;

namespace NexusForever.Script.Main.Challenge
{
    /// <summary>
    /// Flitterfly Chaser: "Collect flitterflies before time runs out", the Everstar Butterflies (CH410 Challenge NPC) are collected by running through them.
    /// </summary>
    [ScriptFilterOwnerId(410)]
    public class FlitterflyChaserChallengeScript : TouchChallengeScript
    {
        protected override uint[] CreatureIds { get; } = [28977];
    }
}
