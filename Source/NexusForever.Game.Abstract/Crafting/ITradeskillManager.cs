using System.Numerics;
using NexusForever.Database.Character;
using NexusForever.Game.Static.Crafting;

namespace NexusForever.Game.Abstract.Crafting
{
    public interface ITradeskillManager : IDatabaseCharacter, IEnumerable<ITradeskill>
    {
        /// <summary>
        /// Time remaining before the player can swap tradeskills again.
        /// </summary>
        TimeSpan RelearnCooldown { get; }

        void SendInitialPackets();

        /// <summary>
        /// Return <see cref="ITradeskill"/> for supplied <see cref="TradeskillType"/>, if the player has ever learned it.
        /// </summary>
        ITradeskill GetTradeskill(TradeskillType tradeskillId);

        /// <summary>
        /// Returns if the player has the supplied <see cref="TradeskillType"/> learned and active.
        /// </summary>
        bool IsTradeskillActive(TradeskillType tradeskillId);

        /// <summary>
        /// Return progress through the supplied TradeskillTier: 0 when not reached, 1 when it is the current tier and 2 when a higher tier has been reached.
        /// </summary>
        uint GetTierProgress(uint tradeskillTierId);

        /// <summary>
        /// Learn supplied <see cref="TradeskillType"/> at a tradeskill trainer, optionally swapping out an active tradeskill.
        /// </summary>
        void LearnTradeskill(TradeskillType learnTradeskillId, TradeskillType dropTradeskillId);

        /// <summary>
        /// Learn any tradeskills that are granted automatically, such as hobbies unlocked at a level.
        /// </summary>
        void LearnAutoTradeskills();

        /// <summary>
        /// Add tradeskill experience to an active <see cref="TradeskillType"/>, advancing tiers as requirements are met.
        /// </summary>
        void AddXp(TradeskillType tradeskillId, uint xp);

        /// <summary>
        /// Returns if the player knows the supplied schematic.
        /// </summary>
        bool IsSchematicKnown(uint schematicId);

        /// <summary>
        /// Learn supplied schematic.
        /// </summary>
        /// <param name="discoveryCoordinates">Coordinates on the discovery panel, when the schematic was discovered during a craft.</param>
        void LearnSchematic(uint schematicId, Vector2? discoveryCoordinates = null);

        /// <summary>
        /// Invoked when an achievement is completed, granting any tradeskill rewards for it.
        /// </summary>
        void OnAchievementCompleted(uint achievementId);
    }
}
