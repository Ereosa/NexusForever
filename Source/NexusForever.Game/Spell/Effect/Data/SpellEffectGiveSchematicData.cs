using NexusForever.Game.Abstract.Spell.Effect.Data;
using NexusForever.GameTable.Model;

namespace NexusForever.Game.Spell.Effect.Data
{
    public class SpellEffectGiveSchematicData : ISpellEffectGiveSchematicData
    {
        public Spell4EffectsEntry Entry { get; private set; }
        public uint TradeskillSchematic2Id { get; private set; }

        public void Populate(Spell4EffectsEntry entry)
        {
            Entry                  = entry;
            TradeskillSchematic2Id = entry.DataBits00;
        }
    }
}
