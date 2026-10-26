using NexusForever.Database.Character;
using NexusForever.Game.Abstract.Entity;

namespace NexusForever.Game.Abstract.Taxi
{
    public interface ITaxiManager : IDatabaseCharacter
    {
        void SendInitialPackets();

        /// <summary>
        /// Returns if the supplied taxi node is unlocked, either discovered or unlocked by flags or level.
        /// </summary>
        bool IsNodeUnlocked(uint taxiNodeId);

        /// <summary>
        /// Discover the supplied taxi node, returns false if it is already known or belongs to the other faction.
        /// </summary>
        bool UnlockNode(uint taxiNodeId);

        /// <summary>
        /// Invoked when the player opens a taxi vendor or shuttle, the taxi node of the vendor is discovered.
        /// </summary>
        void OnTaxiVendorInteract(IWorldEntity vendor);

        /// <summary>
        /// Buy the flight along the supplied taxi routes, sent by the client in order from the vendor's taxi node.
        /// </summary>
        void Purchase(IReadOnlyList<uint> taxiRouteIds);
    }
}
