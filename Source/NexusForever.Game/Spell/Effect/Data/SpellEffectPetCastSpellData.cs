using NexusForever.Game.Abstract.Spell.Effect.Data;
using NexusForever.GameTable.Model;

namespace NexusForever.Game.Spell.Effect.Data
{
    public class SpellEffectPetCastSpellData : ISpellEffectPetCastSpellData
    {
        public Spell4EffectsEntry Entry { get; private set; }
        public uint SummonSpell4Id { get; private set; }
        public uint Spell4Id { get; private set; }

        public void Populate(Spell4EffectsEntry entry)
        {
            Entry          = entry;
            SummonSpell4Id = entry.DataBits00;
            Spell4Id       = entry.DataBits01;
        }
    }
}
