using Microsoft.Extensions.Logging;
using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Prerequisite;
using NexusForever.Game.Static.Prerequisite;

namespace NexusForever.Game.Prerequisite.Check
{
    /// <summary>
    /// Compares the number of active instances (stacks) of spell object id against value.
    /// </summary>
    [PrerequisiteCheck(PrerequisiteType.Spell59)]
    public class PrerequisiteCheckSpellStacks : IPrerequisiteCheck
    {
        #region Dependency Injection

        private readonly ILogger<PrerequisiteCheckSpellStacks> log;

        public PrerequisiteCheckSpellStacks(
            ILogger<PrerequisiteCheckSpellStacks> log)
        {
            this.log = log;
        }

        #endregion

        public bool Meets(IPlayer player, PrerequisiteComparison comparison, uint value, uint objectId, IPrerequisiteParameters parameters)
        {
            return Compare(comparison, CountStacks(player, objectId), value);
        }

        private static uint CountStacks(IPlayer player, uint spell4Id)
        {
            // HasSpell visits every active spell when the predicate never matches
            uint count = 0u;
            player.HasSpell(s =>
            {
                if (s.Spell4Id == spell4Id && !s.IsFinished)
                    count++;
                return false;
            }, out _);
            return count;
        }

        private bool Compare(PrerequisiteComparison comparison, uint current, uint value)
        {
            switch (comparison)
            {
                case PrerequisiteComparison.Equal:
                    return current == value;
                case PrerequisiteComparison.NotEqual:
                    return current != value;
                case PrerequisiteComparison.GreaterThanOrEqual:
                    return current >= value;
                case PrerequisiteComparison.GreaterThan:
                    return current > value;
                case PrerequisiteComparison.LessThanOrEqual:
                    return current <= value;
                case PrerequisiteComparison.LessThan:
                    return current < value;
                default:
                    log.LogWarning($"Unhandled PrerequisiteComparison {comparison} for {PrerequisiteType.Spell59}!");
                    return false;
            }
        }
    }
}
