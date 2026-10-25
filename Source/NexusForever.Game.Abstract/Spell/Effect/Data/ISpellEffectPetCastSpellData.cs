namespace NexusForever.Game.Abstract.Spell.Effect.Data
{
    public interface ISpellEffectPetCastSpellData : ISpellEffectData
    {
        /// <summary>
        /// Spell4 id of the spell that summoned the pet which will cast <see cref="Spell4Id"/>.
        /// </summary>
        uint SummonSpell4Id { get; }

        /// <summary>
        /// Spell4 id the pet casts.
        /// </summary>
        uint Spell4Id { get; }
    }
}
