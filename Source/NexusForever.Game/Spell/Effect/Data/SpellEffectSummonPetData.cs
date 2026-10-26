using NexusForever.Game.Abstract.Spell.Effect.Data;
using NexusForever.GameTable.Model;

namespace NexusForever.Game.Spell.Effect.Data
{
    public class SpellEffectSummonPetData : ISpellEffectSummonPetData
    {
        public Spell4EffectsEntry Entry { get; private set; }
        public uint CreatureId { get; private set; }
        public TimeSpan AttackInterval { get; private set; }
        public TimeSpan DeathCooldown { get; private set; }

        public void Populate(Spell4EffectsEntry entry)
        {
            Entry          = entry;
            CreatureId     = entry.DataBits00;
            AttackInterval = TimeSpan.FromMilliseconds(entry.DataBits03);
            DeathCooldown  = TimeSpan.FromMilliseconds(entry.DataBits06);
        }
    }
}
