using Microsoft.Extensions.Logging;
using NexusForever.Game.Abstract;
using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Prerequisite;
using NexusForever.Game.Static.Prerequisite;
using NexusForever.Game.Static.Quest;
using NexusForever.GameTable;
using NexusForever.GameTable.Model;

namespace NexusForever.Game.Prerequisite.Check
{
    [PrerequisiteCheck(PrerequisiteType.InTargetGroup)]
    public class PrerequisiteCheckInTargetGroup : IPrerequisiteCheck
    {
        #region Dependency Injection

        private readonly ILogger<PrerequisiteCheckInTargetGroup> log;
        private readonly IAssetManager assetManager;
        private readonly IGameTableManager gameTableManager;

        public PrerequisiteCheckInTargetGroup(
            ILogger<PrerequisiteCheckInTargetGroup> log,
            IAssetManager assetManager,
            IGameTableManager gameTableManager)
        {
            this.log              = log;
            this.assetManager     = assetManager;
            this.gameTableManager = gameTableManager;
        }

        #endregion

        /// <remarks>
        /// Value is a TargetGroup id the target must belong to, eg: the correct harvesting tool for a node.
        /// </remarks>
        public bool Meets(IPlayer player, PrerequisiteComparison comparison, uint value, uint objectId, IPrerequisiteParameters parameters)
        {
            if (parameters?.Target == null)
                return false;

            bool inTargetGroup = IsInTargetGroup(parameters.Target, value);
            switch (comparison)
            {
                case PrerequisiteComparison.Equal:
                    return inTargetGroup;
                case PrerequisiteComparison.NotEqual:
                    return !inTargetGroup;
                default:
                    log.LogWarning($"Unhandled PrerequisiteComparison {comparison} for {PrerequisiteType.InTargetGroup}!");
                    return false;
            }
        }

        private bool IsInTargetGroup(IUnitEntity target, uint targetGroupId)
        {
            TargetGroupEntry entry = gameTableManager.TargetGroup.GetEntry(targetGroupId);
            if (entry == null)
                return false;

            switch ((TargetGroupType)entry.Type)
            {
                case TargetGroupType.FactionIdGroup:
                    return entry.DataEntries.Any(f => f != 0u && f == (uint)target.Faction1);
                case TargetGroupType.NotFactionIdGroup:
                    return !entry.DataEntries.Any(f => f != 0u && f == (uint)target.Faction1);
                default:
                    return assetManager.GetTargetGroupsForCreatureId(target.CreatureId)?.Contains(targetGroupId) ?? false;
            }
        }
    }
}
