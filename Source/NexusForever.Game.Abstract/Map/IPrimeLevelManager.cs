using NexusForever.Database.Character;

namespace NexusForever.Game.Abstract.Map
{
    public interface IPrimeLevelManager : IDatabaseCharacter
    {
        /// <summary>
        /// Return the highest prime level unlocked for the world, completing a prime level unlocks the next one.
        /// </summary>
        byte GetPrimeLevelUnlocked(uint worldId);

        /// <summary>
        /// Unlock the prime level after the supplied completed prime level for the world, capped at the world's maximum.
        /// </summary>
        void CompletePrimeLevel(uint worldId, byte primeLevel);
    }
}
