using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Spell.Info;

namespace NexusForever.Game.Abstract.Spell.Proc
{
    public interface IProcParameters
    {
        public IUnitEntity Target { get; set; }

        /// <summary>
        /// Spell that triggered the proc, procs can be limited to spells of a group list.
        /// </summary>
        public ISpellInfo SpellInfo { get; set; }
    }
}
