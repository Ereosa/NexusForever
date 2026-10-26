namespace NexusForever.GameTable.Static
{
    /// <summary>
    /// Elements of circuit crafting sockets and the properties that can be chosen for them.
    /// </summary>
    [Flags]
    public enum ItemCraftingGroupFlag
    {
        None   = 0x00,
        Air    = 0x01,
        Water  = 0x02,
        Earth  = 0x04,
        Fire   = 0x08,
        Logic  = 0x10,
        Life   = 0x20,
        /// <summary>
        /// Fusion sockets accept a property of any element without a mismatch penalty.
        /// </summary>
        Fusion = 0x40,
        All    = 0x7F
    }
}
