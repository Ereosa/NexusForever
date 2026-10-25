using Microsoft.Extensions.Logging;
using NexusForever.Game.Abstract;
using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Prerequisite;
using NexusForever.Game.Static.Prerequisite;

namespace NexusForever.Game.Prerequisite.Check
{
    [PrerequisiteCheck(PrerequisiteType.InTargetGroup)]
    public class PrerequisiteCheckInTargetGroup : IPrerequisiteCheck
    {
        #region Dependency Injection

        private readonly ILogger<PrerequisiteCheckInTargetGroup> log;
        private readonly IAssetManager assetManager;

        public PrerequisiteCheckInTargetGroup(
            ILogger<PrerequisiteCheckInTargetGroup> log,
            IAssetManager assetManager)
        {
            this.log          = log;
            this.assetManager = assetManager;
        }

        #endregion

        /// <remarks>
        /// Value is a TargetGroup id the target creature must belong to, eg: the correct harvesting tool for a node.
        /// </remarks>
        public bool Meets(IPlayer player, PrerequisiteComparison comparison, uint value, uint objectId, IPrerequisiteParameters parameters)
        {
            if (parameters?.Target == null)
                return false;

            bool inTargetGroup = assetManager.GetTargetGroupsForCreatureId(parameters.Target.CreatureId)?.Contains(value) ?? false;
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
    }
}
