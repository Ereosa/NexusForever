using Microsoft.Extensions.Logging;
using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Prerequisite;
using NexusForever.Game.Abstract.Spell;
using NexusForever.Game.Static.Prerequisite;

namespace NexusForever.Game.Prerequisite.Check
{
    /// <summary>
    /// AMP selected in the active action set, eg: 24298 "(Unlocked through AMPs)" for 42810 Repair Bot needs AMP 552.
    /// </summary>
    [PrerequisiteCheck(PrerequisiteType.EldanAugmentation227)]
    public class PrerequisiteCheckEldanAugmentation : IPrerequisiteCheck
    {
        #region Dependency Injection

        private readonly ILogger<PrerequisiteCheckEldanAugmentation> log;

        public PrerequisiteCheckEldanAugmentation(
            ILogger<PrerequisiteCheckEldanAugmentation> log)
        {
            this.log = log;
        }

        #endregion

        public bool Meets(IPlayer player, PrerequisiteComparison comparison, uint value, uint objectId, IPrerequisiteParameters parameters)
        {
            IActionSet actionSet = player.SpellManager.GetActionSet(player.SpellManager.ActiveActionSet);
            bool hasAmp = actionSet?.GetAmp((ushort)objectId) != null;

            switch (comparison)
            {
                case PrerequisiteComparison.Equal:
                    return hasAmp;
                case PrerequisiteComparison.NotEqual:
                    return !hasAmp;
                default:
                    log.LogWarning($"Unhandled PrerequisiteComparison {comparison} for {PrerequisiteType.EldanAugmentation227}!");
                    return false;
            }
        }
    }
}
