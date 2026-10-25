using Microsoft.Extensions.Logging;
using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Prerequisite;
using NexusForever.Game.Static.Entity;
using NexusForever.Game.Static.Prerequisite;

namespace NexusForever.Game.Prerequisite.Check
{
    [PrerequisiteCheck(PrerequisiteType.ItemOnCharacter)]
    public class PrerequisiteCheckItemOnCharacter : IPrerequisiteCheck
    {
        #region Dependency Injection

        private readonly ILogger<PrerequisiteCheckItemOnCharacter> log;

        public PrerequisiteCheckItemOnCharacter(
            ILogger<PrerequisiteCheckItemOnCharacter> log)
        {
            this.log = log;
        }

        #endregion

        /// <remarks>
        /// Value is an item, counted in the inventory and equipped items.
        /// </remarks>
        public bool Meets(IPlayer player, PrerequisiteComparison comparison, uint value, uint objectId, IPrerequisiteParameters parameters)
        {
            bool? result = PrerequisiteCompare.Compare(comparison, player.Inventory.GetItemCount(value, InventoryLocation.Inventory, InventoryLocation.Equipped) > 0u);
            if (result == null)
                log.LogWarning($"Unhandled PrerequisiteComparison {comparison} for {PrerequisiteType.ItemOnCharacter}!");

            return result ?? false;
        }
    }
}
