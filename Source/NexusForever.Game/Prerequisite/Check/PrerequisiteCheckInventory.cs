using Microsoft.Extensions.Logging;
using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Prerequisite;
using NexusForever.Game.Static.Entity;
using NexusForever.Game.Static.Prerequisite;

namespace NexusForever.Game.Prerequisite.Check
{
    [PrerequisiteCheck(PrerequisiteType.Inventory)]
    public class PrerequisiteCheckInventory : IPrerequisiteCheck
    {
        #region Dependency Injection

        private readonly ILogger<PrerequisiteCheckInventory> log;

        public PrerequisiteCheckInventory(
            ILogger<PrerequisiteCheckInventory> log)
        {
            this.log = log;
        }

        #endregion

        /// <remarks>
        /// Value is an item, counted in the inventory, equipped items and the bank, eg: account items that can't be claimed while the character already has the item.
        /// </remarks>
        public bool Meets(IPlayer player, PrerequisiteComparison comparison, uint value, uint objectId, IPrerequisiteParameters parameters)
        {
            bool? result = PrerequisiteCompare.Compare(comparison, player.Inventory.GetItemCount(value, InventoryLocation.Inventory, InventoryLocation.Equipped, InventoryLocation.PlayerBank) > 0u);
            if (result == null)
                log.LogWarning($"Unhandled PrerequisiteComparison {comparison} for {PrerequisiteType.Inventory}!");

            return result ?? false;
        }
    }
}
