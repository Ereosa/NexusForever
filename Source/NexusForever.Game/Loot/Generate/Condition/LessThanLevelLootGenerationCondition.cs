using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Loot.Generate.Condition;

namespace NexusForever.Game.Loot.Generate.Condition
{
    public class LessThanLevelLootGenerationCondition : ILootGenerationCondition
    {
        /// <summary>
        /// Checks if <see cref="IPlayer"/> is below the condition level, e.g. level banded contents of a survival kit.
        /// </summary>
        public bool Meets(IPlayer player, uint condition)
        {
            return player.Level < condition;
        }
    }
}
