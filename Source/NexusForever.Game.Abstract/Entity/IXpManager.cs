using NexusForever.Database.Character;
using NexusForever.Network.World.Message.Static;

namespace NexusForever.Game.Abstract.Entity
{
    public interface IXpManager : IDatabaseCharacter
    {
        uint TotalXp { get; }
        uint RestBonusXp { get; }

        /// <summary>
        /// Grants <see cref="IPlayer"/> the supplied experience, handling level up if necessary.
        /// </summary>
        /// <param name="earnedXp">Experience to grant</param>
        /// <param name="reason"><see cref="ExpReason"/> for the experience grant</param>
        void GrantXp(uint earnedXp, ExpReason reason = ExpReason.Cheat);

        /// <summary>
        /// Add the supplied number of levels worth of rest XP to the pool, returns false if the pool is already full.
        /// </summary>
        bool AddRestXp(float levels);

        /// <summary>
        /// Sets <see cref="IPlayer"/> to the supplied level and adjusts XP accordingly. Mainly for use with GM commands.
        /// </summary>
        /// <param name="newLevel">New level to be set</param>
        /// <param name="reason"><see cref="ExpReason"/> for the level grant</param>
        void SetLevel(byte newLevel, ExpReason reason = ExpReason.Cheat);
    }
}