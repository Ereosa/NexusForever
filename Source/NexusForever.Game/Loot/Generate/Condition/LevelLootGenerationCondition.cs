using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Loot.Generate.Condition;

namespace NexusForever.Game.Loot.Generate.Condition
{
    public class LevelLootGenerationCondition : ILootGenerationCondition
    {
        /// <summary>
        /// Checks if <see cref="IPlayer"/> is the condition level.
        /// </summary>
        public bool Meets(IPlayer player, uint condition)
        {
            return player.Level == condition;
        }
    }
}
