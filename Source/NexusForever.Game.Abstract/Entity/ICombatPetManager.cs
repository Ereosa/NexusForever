using System.Numerics;
using NexusForever.Game.Abstract.Spell;
using NexusForever.Game.Abstract.Spell.Effect.Data;
using NexusForever.Game.Abstract.Spell.Info;
using NexusForever.Game.Abstract.Spell.Target;
using NexusForever.Game.Static.Pet;
using NexusForever.Game.Static.Spell;
using NexusForever.Network.World.Message.Model;

namespace NexusForever.Game.Abstract.Entity
{
    /// <summary>
    /// Tracks the <see cref="ICombatPetEntity"/>s summoned by an <see cref="IPlayer"/>.
    /// </summary>
    public interface ICombatPetManager
    {
        /// <summary>
        /// Returns all currently spawned <see cref="ICombatPetEntity"/>.
        /// </summary>
        IEnumerable<ICombatPetEntity> GetPets();

        /// <summary>
        /// Return the spawned <see cref="ICombatPetEntity"/> summoned by the supplied spell4 id.
        /// </summary>
        ICombatPetEntity GetPetBySummonSpell(uint spell4Id);

        /// <summary>
        /// Summon a pet for the SummonPet effect of the supplied <see cref="ISpell"/>.
        /// </summary>
        bool Summon(ISpell spell, ISpellTargetEffectInfo info, ISpellEffectSummonPetData data);

        /// <summary>
        /// Remove the pet summoned by the SummonPet effect, invoked when the effect is removed.
        /// </summary>
        void Unsummon(ISpellTargetEffectInfo info);

        /// <summary>
        /// Invoked when <see cref="ICombatPetEntity"/> is added to the map.
        /// </summary>
        void OnPetAddToMap(ICombatPetEntity pet);

        /// <summary>
        /// Invoked when <see cref="ICombatPetEntity"/> is killed, the summon effect is cancelled which dismisses the pet.
        /// </summary>
        void OnPetDeath(ICombatPetEntity pet);

        /// <summary>
        /// Show the engineer pet bar if bots are summoned and no other bar is shown in its place, hide it otherwise.
        /// </summary>
        void UpdateActionBar();

        /// <summary>
        /// Invoked when the owner is removed from the map, pets are removed and summoned again when the owner is added to a map.
        /// </summary>
        void OnRemoveFromMap();

        /// <summary>
        /// Invoked when the owner is added to the map.
        /// </summary>
        void OnAddToMap();

        /// <summary>
        /// Invoked when the owner is relocated on the same map.
        /// </summary>
        void OnRelocate(Vector3 position);

        /// <summary>
        /// Invoked when the owner dies, all pets are dismissed.
        /// </summary>
        void OnDeath();

        /// <summary>
        /// Invoked when a hostile unit is added to the owner's threat list.
        /// </summary>
        void OnOwnerThreatAdd(IUnitEntity hostile);

        /// <summary>
        /// Returns the bot ability spell to cast instead of the supplied summon spell if its pet is already summoned.
        /// </summary>
        ISpellInfo GetPetSwitchSpell(ISpellInfo spellInfo);

        /// <summary>
        /// Invoked when a bot ability (pet switch) spell executes on a target.
        /// </summary>
        void OnPetAbility(ISpell spell, IUnitEntity target);

        /// <summary>
        /// Set the stance of the supplied pet, or all pets if guid is 0.
        /// </summary>
        void SetStance(uint petGuid, PetStance stance);

        /// <summary>
        /// Handle a pet command pressed on an action bar shown to the owner.
        /// </summary>
        void HandleCommand(ShortcutSet shortcutSet, uint slot, bool associatedUnitOnly);

        /// <summary>
        /// Handle a spell pressed on an action bar shown to the owner (ClientGameCommand).
        /// </summary>
        void HandleActionBarCast(ShortcutSet shortcutSet, uint slot, uint targetUnitId, Vector3 position);

        /// <summary>
        /// Move all pets to the supplied position and make them stay there.
        /// </summary>
        void GoTo(Vector3 position);

        /// <summary>
        /// Add currently summoned pets to <see cref="ServerPlayerCreate"/>.
        /// </summary>
        void AddToPlayerCreate(ServerPlayerCreate playerCreate);
    }
}
