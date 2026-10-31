namespace NexusForever.WorldServer.Network.Internal
{
    public static class FriendshipIdMappingExtensions
    {
        /// <summary>
        /// Character friendship ids below this are shown by the client as "(temporary)" (bTemporary = nId &lt; 0x1000000000000, client sub_1405DAB80).
        /// </summary>
        private const ulong PermanentFriendshipIdBase = 0x1000000000000ul;

        /// <summary>
        /// Convert a friendship server character friendship id to the id sent to the client.
        /// </summary>
        /// <remarks>
        /// The client never sends a character friendship id back (removes and notes go by identity), so this only goes one way.
        /// </remarks>
        public static ulong ToNetworkFriendshipId(this ulong id)
        {
            return id | PermanentFriendshipIdBase;
        }
    }
}
