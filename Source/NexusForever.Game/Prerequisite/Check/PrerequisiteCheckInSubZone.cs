using Microsoft.Extensions.Logging;
using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Prerequisite;
using NexusForever.Game.Static.Prerequisite;
using NexusForever.GameTable;
using NexusForever.GameTable.Model;

namespace NexusForever.Game.Prerequisite.Check
{
    [PrerequisiteCheck(PrerequisiteType.InSubZone)]
    public class PrerequisiteCheckInSubZone : IPrerequisiteCheck
    {
        #region Dependency Injection

        private readonly ILogger<PrerequisiteCheckInSubZone> log;
        private readonly IGameTableManager gameTableManager;

        public PrerequisiteCheckInSubZone(
            ILogger<PrerequisiteCheckInSubZone> log,
            IGameTableManager gameTableManager)
        {
            this.log              = log;
            this.gameTableManager = gameTableManager;
        }

        #endregion

        public bool Meets(IPlayer player, PrerequisiteComparison comparison, uint value, uint objectId, IPrerequisiteParameters parameters)
        {
            switch (comparison)
            {
                case PrerequisiteComparison.Equal:
                    return IsInZone(player, value);
                case PrerequisiteComparison.NotEqual:
                    return !IsInZone(player, value);
                default:
                    log.LogWarning($"Unhandled PrerequisiteComparison {comparison} for {PrerequisiteType.InSubZone}!");
                    return false;
            }
        }

        /// <summary>
        /// Returns if the player's current zone is the supplied zone or one of its sub zones.
        /// </summary>
        private bool IsInZone(IPlayer player, uint zoneId)
        {
            for (WorldZoneEntry zone = player.Zone; zone != null; zone = zone.ParentZoneId != 0u ? gameTableManager.WorldZone.GetEntry(zone.ParentZoneId) : null)
                if (zone.Id == zoneId)
                    return true;

            return false;
        }
    }
}
