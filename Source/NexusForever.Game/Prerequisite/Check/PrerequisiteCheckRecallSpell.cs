using Microsoft.Extensions.Logging;
using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Prerequisite;
using NexusForever.Game.Static.Prerequisite;

namespace NexusForever.Game.Prerequisite.Check
{
    /// <remarks>
    /// Checks the player has the recall spell (Spell4Base in the object), only used by 30314 "You don't have a bind point set" for 7575 Recall - Transmat.
    /// Players learn "Recall - Transmat" when they first bind to a Transmat Terminal, a bind point is required as well in case the spell was granted otherwise.
    /// </remarks>
    [PrerequisiteCheck(PrerequisiteType.RecallSpell)]
    public class PrerequisiteCheckRecallSpell : IPrerequisiteCheck
    {
        private const uint RecallTransmatSpell4BaseId = 7575u;

        #region Dependency Injection

        private readonly ILogger<PrerequisiteCheckRecallSpell> log;

        public PrerequisiteCheckRecallSpell(
            ILogger<PrerequisiteCheckRecallSpell> log)
        {
            this.log = log;
        }

        #endregion

        public bool Meets(IPlayer player, PrerequisiteComparison comparison, uint value, uint objectId, IPrerequisiteParameters parameters)
        {
            bool hasRecall = player.SpellManager.GetSpell(objectId) != null
                && (objectId != RecallTransmatSpell4BaseId || player.BindPoint != 0);

            switch (comparison)
            {
                case PrerequisiteComparison.Equal:
                    return hasRecall;
                case PrerequisiteComparison.NotEqual:
                    return !hasRecall;
                default:
                    log.LogWarning($"Unhandled PrerequisiteComparison {comparison} for {PrerequisiteType.RecallSpell}!");
                    return false;
            }
        }
    }
}
