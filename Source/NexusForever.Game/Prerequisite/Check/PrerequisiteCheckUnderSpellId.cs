using Microsoft.Extensions.Logging;
using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Prerequisite;
using NexusForever.Game.Static.Prerequisite;

namespace NexusForever.Game.Prerequisite.Check
{
    /// <summary>
    /// Checks if the player has an active instance of spell value, "You cannot do that now" when it fails.
    /// </summary>
    [PrerequisiteCheck(PrerequisiteType.Unknown50)]
    public class PrerequisiteCheckUnderSpellId : IPrerequisiteCheck
    {
        #region Dependency Injection

        private readonly ILogger<PrerequisiteCheckUnderSpellId> log;

        public PrerequisiteCheckUnderSpellId(
            ILogger<PrerequisiteCheckUnderSpellId> log)
        {
            this.log = log;
        }

        #endregion

        public bool Meets(IPlayer player, PrerequisiteComparison comparison, uint value, uint objectId, IPrerequisiteParameters parameters)
        {
            bool hasSpell = player.HasSpell(s => s.Spell4Id == value && !s.IsFinished, out _);
            switch (comparison)
            {
                case PrerequisiteComparison.Equal:
                    return hasSpell;
                case PrerequisiteComparison.NotEqual:
                    return !hasSpell;
                default:
                    log.LogWarning($"Unhandled PrerequisiteComparison {comparison} for {PrerequisiteType.Unknown50}!");
                    return false;
            }
        }
    }
}
