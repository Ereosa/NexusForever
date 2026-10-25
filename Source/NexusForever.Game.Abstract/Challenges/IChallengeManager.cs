using NexusForever.Database.Character;
using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Static.Challenges;

namespace NexusForever.Game.Abstract.Challenges
{
    public interface IChallengeManager : IDatabaseCharacter
    {
        void SendInitialPackets();

        void Update(double lastTick);

        /// <summary>
        /// Invoked when the player enters a new zone.
        /// </summary>
        void OnZoneUpdate();

        /// <summary>
        /// Invoked when the player logs out, running challenges fail and their T-spells are removed.
        /// </summary>
        void OnLogout();

        /// <summary>
        /// Invoked when the player gets kill credit for an entity.
        /// </summary>
        void OnKill(IUnitEntity entity);

        /// <summary>
        /// Invoked when the player successfully activates an entity.
        /// </summary>
        void OnActivateSuccess(IWorldEntity entity);

        /// <summary>
        /// Invoked when items arrive in the player's inventory.
        /// </summary>
        void OnItemAdded(uint itemId, uint count);

        /// <summary>
        /// Invoked when a spell cast by the player lands an effect on a unit.
        /// </summary>
        void OnSpellEffectHit(uint spell4Id, IUnitEntity target);

        /// <summary>
        /// Invoked when the player picks a challenge option in the client (start, abandon, accept or decline a shared challenge).
        /// </summary>
        void HandleChoice(uint challengeId, ChallengeChoice choice);

        /// <summary>
        /// Returns if the supplied spell is a T-spell granted by one of the player's running challenges.
        /// </summary>
        bool CanCastObjectiveAbility(uint spell4Id);

        /// <summary>
        /// Invoked when a group member starts a challenge, the challenge is started or offered depending on the player's preference.
        /// </summary>
        void OnChallengeShared(uint challengeId, IPlayer sharer);

        /// <summary>
        /// Set how challenges shared by group members are handled.
        /// </summary>
        void SetSharedChallengePreference(SharedChallengePreference preference);

        /// <summary>
        /// Unlock the supplied challenge, adding it to the challenge log.
        /// </summary>
        bool Unlock(uint challengeId);

        /// <summary>
        /// Start the supplied challenge, unlocking it if needed.
        /// </summary>
        bool Activate(uint challengeId);

        /// <summary>
        /// Add progress to a running challenge, used by scripts for challenges driven by server side events.
        /// </summary>
        void UpdateProgress(uint challengeId, uint count = 1u);

        /// <summary>
        /// Finish a running challenge at the tier it has reached, used by scripts for challenges with a scripted goal.
        /// </summary>
        void Complete(uint challengeId);

        /// <summary>
        /// Fail a running challenge, optionally showing the supplied localised text as the reason.
        /// </summary>
        void Fail(uint challengeId, uint localizedTextId = 0u);

        bool IsUnlocked(uint challengeId);
        bool IsActive(uint challengeId);
        uint GetCompletionCount(uint challengeId);
        uint GetCurrentTier(uint challengeId);
        uint GetCurrentCount(uint challengeId);

        /// <summary>
        /// Returns if the supplied entity has already been used by a running checklist challenge.
        /// </summary>
        bool IsChecklistObjectUsed(uint challengeId, IWorldEntity entity);
    }
}
