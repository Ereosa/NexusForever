namespace NexusForever.GameTable.Static
{
    public enum ItemStatType
    {
        None            = 0,
        Standard        = 1,
        RandomStatGroup = 2,
        /// <summary>
        /// Property is chosen while circuit crafting, the data is the <see cref="Static.ItemCraftingGroupFlag"/> mask allowed for the socket.
        /// </summary>
        Crafted         = 4
    }
}
