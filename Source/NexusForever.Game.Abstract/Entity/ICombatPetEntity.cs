using System.Numerics;
using NexusForever.Game.Abstract.Entity.Creature;
using NexusForever.Game.Abstract.Spell;
using NexusForever.Game.Abstract.Spell.Effect.Data;
using NexusForever.Game.Abstract.Spell.Info;
using NexusForever.Game.Static.Pet;

namespace NexusForever.Game.Abstract.Entity
{
    /// <summary>
    /// A pet summoned by the SummonPet spell effect that follows and fights for its owner, eg: Engineer bots.
    /// </summary>
    public interface ICombatPetEntity : IUnitEntity
    {
        uint OwnerGuid { get; }

        /// <summary>
        /// Spell4 id of the spell that summoned the pet.
        /// </summary>
        uint SummonSpell4Id { get; }

        PetStance Stance { get; }
        PetStance ValidStances { get; }

        /// <summary>
        /// Initialise <see cref="ICombatPetEntity"/> for the supplied owner from the summon spell and effect data.
        /// </summary>
        void Initialise(IPlayer owner, ISpellInfo summonSpell, ISpellEffectSummonPetData data, ICreatureInfo creatureInfo, PetStance stance, int followSlot);

        /// <summary>
        /// Set the current <see cref="PetStance"/>, this is ignored if the stance is not valid for the pet.
        /// </summary>
        bool SetStance(PetStance stance);

        /// <summary>
        /// Attack the supplied <see cref="IUnitEntity"/>, ignoring the current stance.
        /// </summary>
        void CommandAttack(IUnitEntity target);

        /// <summary>
        /// Stop attacking and stay out of combat until the next command or a stance change.
        /// </summary>
        void CommandStop();

        /// <summary>
        /// Stop attacking and return to the owner.
        /// </summary>
        void CommandFollow();

        /// <summary>
        /// Move to the supplied position and stay there.
        /// </summary>
        void CommandGoTo(Vector3 position);

        /// <summary>
        /// Cast a spell for the owner, eg: a bot ability triggered by the PetCastSpell effect.
        /// </summary>
        void CastPetSpell(uint spell4Id, uint primaryTargetId);

        /// <summary>
        /// Leap to supplied target and cast the supplied spells, at the tier of the summon spell, when landing.
        /// </summary>
        void LeapCast(IUnitEntity target, uint[] spell4BaseIds);

        /// <summary>
        /// Invoked when a hostile unit is added to the owner's threat list.
        /// </summary>
        void OnOwnerThreatAdd(IUnitEntity hostile);
    }
}
