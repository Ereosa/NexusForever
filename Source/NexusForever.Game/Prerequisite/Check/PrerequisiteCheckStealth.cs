using Microsoft.Extensions.Logging;
using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Prerequisite;
using NexusForever.Game.Static.Entity;
using NexusForever.Game.Static.Prerequisite;

namespace NexusForever.Game.Prerequisite.Check
{
    /// <summary>
    /// Checks if the player is currently stealthed, used by the Stalker Nano Skin toggle to pick between entering and leaving stealth.
    /// </summary>
    [PrerequisiteCheck(PrerequisiteType.Stealth)]
    public class PrerequisiteCheckStealth : IPrerequisiteCheck
    {
        #region Dependency Injection

        private readonly ILogger<PrerequisiteCheckStealth> log;

        public PrerequisiteCheckStealth(
            ILogger<PrerequisiteCheckStealth> log)
        {
            this.log = log;
        }

        #endregion

        public bool Meets(IPlayer player, PrerequisiteComparison comparison, uint value, uint objectId, IPrerequisiteParameters parameters)
        {
            bool stealthed = player.CreateFlags.HasFlag(EntityCreateFlag.IsStealthed);
            switch (comparison)
            {
                case PrerequisiteComparison.Equal:
                    return stealthed;
                case PrerequisiteComparison.NotEqual:
                    return !stealthed;
                default:
                    log.LogWarning($"Unhandled PrerequisiteComparison {comparison} for {PrerequisiteType.Stealth}!");
                    return false;
            }
        }
    }
}
