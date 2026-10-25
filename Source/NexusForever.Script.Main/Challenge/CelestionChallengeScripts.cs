using NexusForever.Script.Template.Filter;

namespace NexusForever.Script.Main.Challenge
{
    /// <summary>
    /// Chasing Fireflies: "Collect Fireflies near Celestial Falls", the fireflies are collected by jumping through them like in quest 6680.
    /// </summary>
    [ScriptFilterOwnerId(493)]
    public class ChasingFirefliesChallengeScript : TouchChallengeScript
    {
        protected override uint[] CreatureIds { get; } = [30974];
    }
}
