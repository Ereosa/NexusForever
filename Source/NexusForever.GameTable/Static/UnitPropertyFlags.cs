namespace NexusForever.GameTable.Static
{
    [Flags]
    public enum UnitPropertyFlags
    {
        None      = 0x00,
        Static    = 0x01,
        Unknown04 = 0x04,
        /// <summary>
        /// Property can be chosen for a socket while circuit crafting.
        /// </summary>
        Craftable = 0x200
    }
}
