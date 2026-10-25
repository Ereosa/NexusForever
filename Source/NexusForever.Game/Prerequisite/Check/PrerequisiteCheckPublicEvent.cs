using Microsoft.Extensions.Logging;
using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Prerequisite;
using NexusForever.Game.Abstract.PublicEvent;
using NexusForever.Game.Static.Prerequisite;
using NexusForever.Game.Static.PublicEvent;

namespace NexusForever.Game.Prerequisite.Check
{
    /// <remarks>
    /// Object id is the public event, value is a <see cref="PublicEventStatus"/> (inferred: the data only compares against 0, "event isn't running").
    /// </remarks>
    [PrerequisiteCheck(PrerequisiteType.PublicEvent117)]
    public class PrerequisiteCheckPublicEvent : IPrerequisiteCheck
    {
        #region Dependency Injection

        private readonly ILogger<PrerequisiteCheckPublicEvent> log;

        public PrerequisiteCheckPublicEvent(
            ILogger<PrerequisiteCheckPublicEvent> log)
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
                    log.LogWarning($"Unhandled PrerequisiteComparison {comparison} for {PrerequisiteType.PublicEvent117}!");
                    return false;
            }
        }

        private static PublicEventStatus GetStatus(IPlayer player, uint publicEventId)
        {
            IPublicEvent publicEvent = player.Map?.PublicEventManager.GetEvents().FirstOrDefault(e => e.Id == publicEventId);
            if (publicEvent == null)
                return PublicEventStatus.Inactive;

            return publicEvent.HasFinished ? PublicEventStatus.Succeeded : PublicEventStatus.Active;
        }
    }
}
