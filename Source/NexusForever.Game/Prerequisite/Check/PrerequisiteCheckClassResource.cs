using Microsoft.Extensions.Logging;
using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Prerequisite;
using NexusForever.Game.Static.Entity;
using NexusForever.Game.Static.Prerequisite;
using NexusForever.GameTable;
using NexusForever.GameTable.Model;

namespace NexusForever.Game.Prerequisite.Check
{
    /// <summary>
    /// Compares one of the player's class resources against a value, the object id is the resource slot (VitalEnumResource00-07 of the class).
    /// </summary>
    /// <remarks>
    /// Spell Surge uses slot 4 which is <see cref="Vital.SpellSurge"/> for Spellslingers, it can be cast with more than 25 surge or while already surging.
    /// </remarks>
    [PrerequisiteCheck(PrerequisiteType.Unknown64)]
    public class PrerequisiteCheckClassResource : IPrerequisiteCheck
    {
        #region Dependency Injection

        private readonly ILogger<PrerequisiteCheckClassResource> log;

        public PrerequisiteCheckClassResource(
            ILogger<PrerequisiteCheckClassResource> log)
        {
            this.log = log;
        }

        #endregion

        public bool Meets(IPlayer player, PrerequisiteComparison comparison, uint value, uint objectId, IPrerequisiteParameters parameters)
        {
            ClassEntry classEntry = GameTableManager.Instance.Class.GetEntry((ulong)player.Class);
            if (classEntry == null)
                return false;

            uint vital = objectId switch
            {
                0 => classEntry.VitalEnumResource00,
                1 => classEntry.VitalEnumResource01,
                2 => classEntry.VitalEnumResource02,
                3 => classEntry.VitalEnumResource03,
                4 => classEntry.VitalEnumResource04,
                5 => classEntry.VitalEnumResource05,
                6 => classEntry.VitalEnumResource06,
                7 => classEntry.VitalEnumResource07,
                _ => 0u
            };
            if (vital == 0u)
                return false;

            float current = player.GetVitalValue((Vital)vital);
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
                    log.LogWarning($"Unhandled PrerequisiteComparison {comparison} for {PrerequisiteType.Unknown64}!");
                    return false;
            }
        }
    }
}
