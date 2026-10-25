using NexusForever.Network.World.Message.Model.Crafting;

namespace NexusForever.Game.Abstract.Crafting
{
    public interface ICraftingManager
    {
        /// <summary>
        /// Start crafting the supplied schematic at a crafting station.
        /// </summary>
        /// <remarks>
        /// Coordinate and circuit schematics start a craft that is completed later, simple schematics are crafted straight away.
        /// </remarks>
        void StartCraft(uint stationUnitId, uint schematicId, uint count, uint catalystItemId);

        /// <summary>
        /// Add an additive to the current coordinate craft, moving the craft marker on the grid.
        /// </summary>
        void AddAdditive(uint stationUnitId, uint additiveItemId);

        /// <summary>
        /// Complete the current coordinate craft.
        /// </summary>
        void CompleteCoordinateCraft(uint stationUnitId, uint schematicId);

        /// <summary>
        /// Complete the current circuit craft.
        /// </summary>
        void CompleteCircuitCraft(ClientCraftingComplexCraft complexCraft);

        /// <summary>
        /// Abandon the current craft, materials already used are lost.
        /// </summary>
        void AbandonCraft();

        /// <summary>
        /// Invoked when the crafting spell finishes casting.
        /// </summary>
        void OnCraftSpell();
    }
}
