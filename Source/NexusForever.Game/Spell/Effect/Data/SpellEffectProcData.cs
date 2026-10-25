using NexusForever.Game.Abstract.Spell.Effect.Data;
using NexusForever.Game.Static.Spell.Proc;
using NexusForever.GameTable.Model;

namespace NexusForever.Game.Spell.Effect.Data
{
    public class SpellEffectProcData : ISpellEffectProcData
    {
        public Spell4EffectsEntry Entry { get; private set; }
        public ProcType ProcType { get; private set; }
        public uint SpellId { get; private set; }
        public float Chance { get; private set; }
        public uint Flags { get; private set; }
        public uint TriggerTime { get; private set; }
        public uint TargetPrerequisiteId { get; private set; }
        public uint FilterType { get; private set; }
        public uint FilterValue { get; private set; }
        public uint CasterPrerequisiteId { get; private set; }

        public void Populate(Spell4EffectsEntry entry)
        {
            Entry                = entry;
            ProcType             = (ProcType)entry.DataBits00;
            SpellId              = entry.DataBits01;
            Chance               = entry.DataBits02 != 0u ? BitConverter.UInt32BitsToSingle(entry.DataBits02) : 1f;
            Flags                = entry.DataBits03;
            TriggerTime          = entry.DataBits04;
            TargetPrerequisiteId = entry.DataBits06;
            FilterType           = entry.DataBits07;
            FilterValue          = entry.DataBits08;
            CasterPrerequisiteId = entry.DataBits09;
        }
    }
}
