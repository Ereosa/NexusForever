using NexusForever.Game.Abstract.Entity;

namespace NexusForever.Game.Abstract.Taxi
{
    public interface ICityTourManager
    {
        /// <summary>
        /// Start the tour of the supplied tour vehicle for <see cref="IPlayer"/>, returns false if the vehicle isn't a tour or the player can't take it.
        /// </summary>
        bool TryStart(IPlayer player, IWorldEntity vehicle);
    }
}
