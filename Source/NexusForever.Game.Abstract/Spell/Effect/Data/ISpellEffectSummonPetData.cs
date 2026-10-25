namespace NexusForever.Game.Abstract.Spell.Effect.Data
{
    public interface ISpellEffectSummonPetData : ISpellEffectData
    {
        uint CreatureId { get; }

        /// <summary>
        /// Time between the pet's automatic attacks, eg: Bruiserbot "deals damage twice every $(eff.data03MS)s".
        /// </summary>
        TimeSpan AttackInterval { get; }
    }
}
