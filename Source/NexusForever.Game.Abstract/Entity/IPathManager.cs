using NexusForever.Database.Character;
using NexusForever.Network.World.Message.Static;

using NexusForever.GameTable.Model;

namespace NexusForever.Game.Abstract.Entity
{
    public interface IPathManager : IDatabaseCharacter, IEnumerable<IPathEntry>
    {
        /// <summary>
        /// Checks to see if supplied <see cref="Static.PlayerPath.Path"/> is active.
        /// </summary>
        bool IsPathActive(Static.PlayerPath.Path pathToCheck);

        /// <summary>
        /// Attempts to activate supplied <see cref="Static.PlayerPath.Path"/>. 
        /// </summary>
        void ActivatePath(Static.PlayerPath.Path pathToActivate);

        /// <summary>
        /// Checks if supplied <see cref="Static.PlayerPath.Path"/> is unlocked. 
        /// </summary>
        bool IsPathUnlocked(Static.PlayerPath.Path pathToUnlock);

        /// <summary>
        /// Attemps to unlock supplied <see cref="Static.PlayerPath.Path"/>.
        /// </summary>
        void UnlockPath(Static.PlayerPath.Path pathToUnlock);

        /// <summary>
        /// Add XP to the current <see cref="Static.PlayerPath.Path"/>.
        /// </summary>
        void AddXp(uint xp);

        /// <summary>
        /// Get the current level for the supplied <see cref="Static.PlayerPath.Path"/>.
        /// </summary>
        uint GetCurrentLevel(Static.PlayerPath.Path path);

        /// <summary>
        /// Grant the items, spells, quests or titles from a <see cref="PathRewardEntry"/>.
        /// </summary>
        void GrantPathReward(PathRewardEntry pathRewardEntry);

        void SendInitialPackets();
        void SendSetUnitPathTypePacket();
        void SendServerPathActivateResult(GenericError result = GenericError.Ok);
        void SendServerPathUnlockResult(GenericError result = GenericError.Ok);
    }
}