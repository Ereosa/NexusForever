using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Loot.Generate.Condition;
using Path = NexusForever.Game.Static.PlayerPath.Path;

namespace NexusForever.Game.Loot.Generate.Condition
{
    public class PathLootGenerationCondition : ILootGenerationCondition
    {
        /// <summary>
        /// Checks if <see cref="IPlayer"/> meets the <see cref="Path"/> condition, e.g. settler resources only drop for settlers.
        /// </summary>
        public bool Meets(IPlayer player, uint condition)
        {
            return player.Path == (Path)condition;
        }
    }
}
