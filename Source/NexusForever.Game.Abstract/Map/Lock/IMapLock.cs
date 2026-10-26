using NexusForever.Game.Static.Map.Lock;
using NexusForever.Game.Static.Setting;

namespace NexusForever.Game.Abstract.Map.Lock
{
    public interface IMapLock
    {
        Guid InstanceId { get; }
        MapLockType Type { get; }
        uint WorldId { get; }

        /// <summary>
        /// Difficulty the instance was created with (instance portal settings dialog).
        /// </summary>
        WorldDifficulty Difficulty { get; set; }

        /// <summary>
        /// Prime level the instance was created with, only used with <see cref="WorldDifficulty.Veteran"/>.
        /// </summary>
        byte PrimeLevel { get; set; }

        /// <summary>
        /// Returns if an instance for the map lock is currently loaded.
        /// </summary>
        bool IsInstanceLoaded { get; set; }

        /// <summary>
        /// Returns if the content of the instance has been completed, entering again starts a new instance.
        /// </summary>
        bool IsCompleted { get; set; }

        /// <summary>
        /// Initialise new map lock with supplied <see cref="MapLockType"/> and world id.
        /// </summary>
        void Initialise(MapLockType mapLockType, uint worldId);

        /// <summary>
        /// Add a character to the map lock.
        /// </summary>
        void AddCharacer(Identity identity);

        /// <summary>
        /// Remove a character from the map lock.
        /// </summary>
        void RemoveCharacter(Identity identity);
    }
}
