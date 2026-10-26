namespace NexusForever.Game.Abstract.Spell.Effect.Data
{
    public interface ISpellEffectSummonPetData : ISpellEffectData
    {
        uint CreatureId { get; }

        /// <summary>
        /// Time between the pet's automatic attacks, eg: Bruiserbot "deals damage twice every $(eff.data03MS)s".
        /// </summary>
        TimeSpan AttackInterval { get; }

        /// <summary>
        /// Cooldown on the summon spell when the pet dies, "There is a 15 second cooldown on death for each bot".
        /// </summary>
        TimeSpan DeathCooldown { get; }
    }
}
