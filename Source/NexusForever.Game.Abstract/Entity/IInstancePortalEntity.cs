using NexusForever.GameTable.Model;

namespace NexusForever.Game.Abstract.Entity
{
    public interface IInstancePortalEntity : IWorldEntity
    {
        /// <summary>
        /// Return the world the portal leads to: the matching map with the portal's name (InstancePortal has no world).
        /// </summary>
        WorldEntry GetDestinationWorld();

        /// <summary>
        /// Send the player into the portal's instance at its entrance.
        /// </summary>
        bool Enter(IPlayer player);
    }
}
