using NexusForever.Game.Abstract.Entity;
using NexusForever.Script.Template;
using NexusForever.Script.Template.Filter;

namespace NexusForever.Script.Main.Tutorial.Script.Part2
{
    /// <summary>
    /// Arkship departure terminals at the end of the Exile tutorial (Bon Voyage).
    /// </summary>
    /// <remarks>
    /// The terminal's activate spell is a yes/no confirmation, on success the player leaves the tutorial for the chosen starting zone.
    /// Destinations are the veteran (skip tutorial) starting locations from character creation.
    /// </remarks>
    [ScriptFilterCreatureId(EverstarGroveTerminal, NorthernWildsTerminal)]
    public class DepartureTerminalEntityScript : IWorldEntityScript, IOwnedScript<IWorldEntity>
    {
        private const uint EverstarGroveTerminal = 73605;
        private const uint NorthernWildsTerminal = 73606;

        private const ushort NorthernWildsWorldId = 426;
        private const ushort EverstarGroveWorldId = 990;

        private IWorldEntity owner;

        /// <summary>
        /// Invoked when <see cref="IScript"/> is loaded.
        /// </summary>
        public void OnLoad(IWorldEntity owner)
        {
            this.owner = owner;
        }

        /// <summary>
        /// Invoked when <see cref="IWorldEntity"/> is successfully activated by <see cref="IPlayer"/>.
        /// </summary>
        public void OnActivateSuccess(IPlayer activator)
        {
            if (owner.CreatureId == NorthernWildsTerminal)
                activator.TeleportTo(NorthernWildsWorldId, 4110.71f, -658.625f, -5145.48f);
            else
                activator.TeleportTo(EverstarGroveWorldId, -771.823f, -904.285f, -2269.56f);
        }
    }
}
