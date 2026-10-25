using Microsoft.Extensions.Logging;
using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Prerequisite;
using NexusForever.Game.Static.Entity;
using NexusForever.Game.Static.Prerequisite;

namespace NexusForever.Game.Prerequisite.Check
{
    /// <summary>
    /// Compares the percentage of vital object id against value (eg: health below 50%).
    /// </summary>
    [PrerequisiteCheck(PrerequisiteType.Unknown71)]
    public class PrerequisiteCheckVitalPercent : IPrerequisiteCheck
    {
        #region Dependency Injection

        private readonly ILogger<PrerequisiteCheckVitalPercent> log;

        public PrerequisiteCheckVitalPercent(
            ILogger<PrerequisiteCheckVitalPercent> log)
        {
            this.log = log;
        }

        #endregion

        public bool Meets(IPlayer player, PrerequisiteComparison comparison, uint value, uint objectId, IPrerequisiteParameters parameters)
        {
            var vital = (Vital)objectId;
            float max = vital switch
            {
                Vital.Health         => player.MaxHealth,
                Vital.ShieldCapacity => player.GetPropertyValue(Property.ShieldCapacityMax),
                Vital.Endurance      => player.GetPropertyValue(Property.ResourceMax0),
                Vital.Resource1      => player.GetPropertyValue(Property.ResourceMax1),
                Vital.Resource2      => player.GetPropertyValue(Property.ResourceMax2),
                Vital.Resource3      => player.GetPropertyValue(Property.ResourceMax3),
                Vital.Resource4      => player.GetPropertyValue(Property.ResourceMax4),
                _                    => 0f
            };
            if (max <= 0f)
            {
                log.LogWarning($"Unhandled vital {vital} for {PrerequisiteType.Unknown71}!");
                return false;
            }

            float percent = player.GetVitalValue(vital) / max * 100f;
            switch (comparison)
            {
                case PrerequisiteComparison.Equal:
                    return MathF.Round(percent) == value;
                case PrerequisiteComparison.NotEqual:
                    return MathF.Round(percent) != value;
                case PrerequisiteComparison.GreaterThanOrEqual:
                    return percent >= value;
                case PrerequisiteComparison.GreaterThan:
                    return percent > value;
                case PrerequisiteComparison.LessThanOrEqual:
                    return percent <= value;
                case PrerequisiteComparison.LessThan:
                    return percent < value;
                default:
                    log.LogWarning($"Unhandled PrerequisiteComparison {comparison} for {PrerequisiteType.Unknown71}!");
                    return false;
            }
        }
    }
}
