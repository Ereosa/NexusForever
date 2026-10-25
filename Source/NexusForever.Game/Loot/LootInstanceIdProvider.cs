using NexusForever.Game.Abstract.Loot;

namespace NexusForever.Game.Loot
{
    public class LootInstanceIdProvider : ILootInstanceIdProvider
    {
        /// <remarks>
        /// The client treats loot ids as unit ids: on ServerLootGrant it destroys the unit with the loot id (retail loot
        /// drops were units). Ids starting at 1 matched real entity guids, the player's own unit (guid 1) included, which
        /// left the client without a player unit and crashed it on the next item update. Loot ids use a range entity
        /// guids never reach.
        /// </remarks>
        private uint id = 0x40000000u;

        public uint GetId()
        {
            return id++;
        }
    }
}
