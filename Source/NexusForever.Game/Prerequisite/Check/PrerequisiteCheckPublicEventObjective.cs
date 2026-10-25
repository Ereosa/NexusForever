using Microsoft.Extensions.Logging;
using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Prerequisite;
using NexusForever.Game.Abstract.PublicEvent;
using NexusForever.Game.Static.Prerequisite;
using NexusForever.Game.Static.PublicEvent;

namespace NexusForever.Game.Prerequisite.Check
{
    /// <remarks>
    /// Object id is the public event objective, value is a <see cref="PublicEventStatus"/> (1 = active), eg: event props only usable while their objective runs.
    /// </remarks>
    [PrerequisiteCheck(PrerequisiteType.PublicEventObjective188)]
    public class PrerequisiteCheckPublicEventObjective : IPrerequisiteCheck
    {
        #region Dependency Injection

        private readonly ILogger<PrerequisiteCheckPublicEventObjective> log;

        public PrerequisiteCheckPublicEventObjective(
            ILogger<PrerequisiteCheckPublicEventObjective> log)
        {
            this.log = log;
        }

        #endregion

        public bool Meets(IPlayer player, PrerequisiteComparison comparison, uint value, uint objectId, IPrerequisiteParameters parameters)
        {
            switch (comparison)
            {
                case PrerequisiteComparison.Equal:
                    return GetStatus(player, objectId) == (PublicEventStatus)value;
                case PrerequisiteComparison.NotEqual:
                    return GetStatus(player, objectId) != (PublicEventStatus)value;
                default:
                    log.LogWarning($"Unhandled PrerequisiteComparison {comparison} for {PrerequisiteType.PublicEventObjective188}!");
                    return false;
            }
        }

        private static PublicEventStatus GetStatus(IPlayer player, uint objectiveId)
        {
            if (player.Map == null)
                return PublicEventStatus.Inactive;

            foreach (IPublicEvent publicEvent in player.Map.PublicEventManager.GetEvents())
                foreach (IPublicEventTeam team in publicEvent.GetTeams())
                    foreach (IPublicEventObjective objective in team.GetObjectives())
                        if (objective.Entry.Id == objectiveId)
                            return objective.Status;

            return PublicEventStatus.Inactive;
        }
    }
}
