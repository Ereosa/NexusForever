using NexusForever.Game.Static.Spell.Proc;

namespace NexusForever.Game.Abstract.Spell.Effect.Data
{
    public interface ISpellEffectProcData : ISpellEffectData
    {
        ProcType ProcType { get; }
        uint SpellId { get; }

        /// <summary>
        /// Chance for the proc to trigger, 1 is always.
        /// </summary>
        float Chance { get; }

        /// <summary>
        /// Flags for the proc spell, 0x04 casts the proc spell on the unit that triggered the proc instead of the owner.
        /// </summary>
        uint Flags { get; }

        /// <summary>
        /// Internal cooldown, "Can only occur every $(eff.data04ms)s".
        /// </summary>
        uint TriggerTime { get; }

        uint TargetPrerequisiteId { get; }

        /// <summary>
        /// 2 limits the proc to spells in the <see cref="FilterValue"/> Spell4GroupList.
        /// </summary>
        uint FilterType { get; }
        uint FilterValue { get; }

        uint CasterPrerequisiteId { get; }
    }
}
