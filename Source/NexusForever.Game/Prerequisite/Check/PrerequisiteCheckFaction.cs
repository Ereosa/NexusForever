using Microsoft.Extensions.Logging;
using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Prerequisite;
using NexusForever.Game.Static.Prerequisite;
using NexusForever.Game.Static.Reputation;

namespace NexusForever.Game.Prerequisite.Check
{
    [PrerequisiteCheck(PrerequisiteType.Faction)]
    public class PrerequisiteCheckFaction : IPrerequisiteCheck
    {
        #region Dependency Injection

        private readonly ILogger<PrerequisiteCheckFaction> log;

        public PrerequisiteCheckFaction(
            ILogger<PrerequisiteCheckFaction> log)
        {
            this.log = log;
        }

        #endregion

        /// <remarks>
        /// Checks the faction of the target when one is supplied (eg: spell effects that only apply to some harvesting nodes), otherwise the player.
        /// </remarks>
        public bool Meets(IPlayer player, PrerequisiteComparison comparison, uint value, uint objectId, IPrerequisiteParameters parameters)
        {
            IUnitEntity unit = parameters?.Target ?? player;
            switch (comparison)
            {
                case PrerequisiteComparison.Equal:
                    return unit.Faction1 == (Faction)value;
                case PrerequisiteComparison.NotEqual:
                    return unit.Faction1 != (Faction)value;
                default:
                    log.LogWarning($"Unhandled PrerequisiteComparison {comparison} for {PrerequisiteType.Faction}!");
                    return false;
            }
        }
    }
}
