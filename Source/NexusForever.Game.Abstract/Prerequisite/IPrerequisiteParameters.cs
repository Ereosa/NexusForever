using NexusForever.Game.Abstract.Entity;
using NexusForever.Network.World.Message.Static;
using NexusForever.Game.Static.Spell;

namespace NexusForever.Game.Abstract.Prerequisite
{
    public interface IPrerequisiteParameters
    {
        public CastResult? CastResult { get; set; }
        public IUnitEntity Target { get; set; }
        public ushort? TaxiNode { get; set; }

        /// <summary>
        /// Prerequisite types without a handler count as met (eg: visibility, hiding an NPC on an unknown condition is worse than showing it).
        /// </summary>
        public bool UnhandledMeets { get; set; }
    }
}
