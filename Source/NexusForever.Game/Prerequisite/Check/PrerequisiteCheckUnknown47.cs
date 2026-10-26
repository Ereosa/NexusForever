using Microsoft.Extensions.Logging;
using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Prerequisite;
using NexusForever.Game.Static.Prerequisite;

namespace NexusForever.Game.Prerequisite.Check
{
    /// <summary>
    /// The state this type checks is unknown, its value is an id that looks like a spell (eg: "Unknown47, NotEqual, 20704" on the
    /// visibility of the Voreth Flame in Fire in the Sky, "Unknown47, Equal, 10014; Unknown47, NotEqual, 10015" on ability tiers).
    /// </summary>
    /// <remarks>
    /// The player is assumed to never be in that state: NotEqual passes, Equal fails. This is a guess, it lets the quest targets
    /// that are hidden while the state is set show up instead of staying hidden for good.
    /// </remarks>
    [PrerequisiteCheck(PrerequisiteType.Unknown47)]
    public class PrerequisiteCheckUnknown47 : IPrerequisiteCheck
    {
        private readonly ILogger<PrerequisiteCheckUnknown47> log;

        public PrerequisiteCheckUnknown47(ILogger<PrerequisiteCheckUnknown47> log)
        {
            this.log = log;
        }

        public bool Meets(IPlayer player, PrerequisiteComparison comparison, uint value, uint objectId, IPrerequisiteParameters parameters)
        {
            switch (comparison)
            {
                case PrerequisiteComparison.Equal:
                    return false;
                case PrerequisiteComparison.NotEqual:
                    return true;
                default:
                    log.LogWarning($"Unhandled PrerequisiteComparison {comparison} for {PrerequisiteType.Unknown47}!");
                    return false;
            }
        }
    }
}
