using NexusForever.Game.Static.Prerequisite;

namespace NexusForever.Game.Prerequisite
{
    public static class PrerequisiteCompare
    {
        /// <summary>
        /// Compare an actual value against the prerequisite value, returns null for comparisons that don't apply to numbers.
        /// </summary>
        public static bool? Compare(PrerequisiteComparison comparison, ulong actual, ulong value)
        {
            return comparison switch
            {
                PrerequisiteComparison.Equal              => actual == value,
                PrerequisiteComparison.NotEqual           => actual != value,
                PrerequisiteComparison.GreaterThan        => actual > value,
                PrerequisiteComparison.GreaterThanOrEqual => actual >= value,
                PrerequisiteComparison.LessThan           => actual < value,
                PrerequisiteComparison.LessThanOrEqual    => actual <= value,
                _                                         => null
            };
        }

        /// <summary>
        /// Compare a state, Equal is met when the state is true and NotEqual when it is false, returns null for other comparisons.
        /// </summary>
        public static bool? Compare(PrerequisiteComparison comparison, bool state)
        {
            return comparison switch
            {
                PrerequisiteComparison.Equal    => state,
                PrerequisiteComparison.NotEqual => !state,
                _                               => null
            };
        }
    }
}
