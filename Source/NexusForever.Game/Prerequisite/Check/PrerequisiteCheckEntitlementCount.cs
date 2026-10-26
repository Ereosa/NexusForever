using Microsoft.Extensions.Logging;
using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Prerequisite;
using NexusForever.Game.Static.Entity;
using NexusForever.Game.Static.Prerequisite;
using NexusForever.GameTable;
using NexusForever.GameTable.Model;

namespace NexusForever.Game.Prerequisite.Check
{
    [PrerequisiteCheck(PrerequisiteType.EntitlementCount)]
    public class PrerequisiteCheckEntitlementCount : IPrerequisiteCheck
    {
        #region Dependency Injection

        private readonly ILogger<PrerequisiteCheckEntitlementCount> log;
        private readonly IGameTableManager gameTableManager;

        public PrerequisiteCheckEntitlementCount(
            ILogger<PrerequisiteCheckEntitlementCount> log,
            IGameTableManager gameTableManager)
        {
            this.log              = log;
            this.gameTableManager = gameTableManager;
        }

        #endregion

        /// <remarks>
        /// Object id is the entitlement, eg: account items that can only be claimed until the entitlement reaches its maximum.
        /// </remarks>
        public bool Meets(IPlayer player, PrerequisiteComparison comparison, uint value, uint objectId, IPrerequisiteParameters parameters)
        {
            EntitlementEntry entry = gameTableManager.Entitlement.GetEntry(objectId);
            if (entry == null)
                return false;

            uint count = ((EntitlementFlags)entry.Flags).HasFlag(EntitlementFlags.Character)
                ? player.EntitlementManager.GetEntitlement((EntitlementType)objectId)?.Amount ?? 0u
                : player.Account.EntitlementManager.GetEntitlement((EntitlementType)objectId)?.Amount ?? 0u;

            switch (comparison)
            {
                case PrerequisiteComparison.Equal:
                    return count == value;
                case PrerequisiteComparison.NotEqual:
                    return count != value;
                case PrerequisiteComparison.GreaterThan:
                    return count > value;
                case PrerequisiteComparison.GreaterThanOrEqual:
                    return count >= value;
                case PrerequisiteComparison.LessThan:
                    return count < value;
                case PrerequisiteComparison.LessThanOrEqual:
                    return count <= value;
                default:
                    log.LogWarning($"Unhandled PrerequisiteComparison {comparison} for {PrerequisiteType.EntitlementCount}!");
                    return false;
            }
        }
    }
}
