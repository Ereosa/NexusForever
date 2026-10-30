using NexusForever.Game.Abstract.Entity;
using NexusForever.Script.Template;
using NexusForever.Script.Template.Filter;

namespace NexusForever.Script.Alizar.EverstarGrove.Script
{
    /// <summary>
    /// Transports between the two Exile starting zones, Northern Wilds and Everstar Grove, standing at each zone's landing site.
    /// </summary>
    /// <remarks>
    /// The transports are instance portals (type 1) with yes/no spells that carry no teleport (Everstar's sends a ravel signal), and
    /// neither zone is a matching map, so the instance settings dialog never opened and nothing happened (report 72). Players arrive
    /// at the other zone's starting point, the spot the Arkship departure terminals send them to, next to the transport back.
    /// </remarks>
    [ScriptFilterCreatureId(ToEverstarGrove, ToEverstarGroveVariant, ToNorthernWilds, ToNorthernWildsVariant)]
    public class StarterZoneTransportEntityScript : IWorldEntityScript, IOwnedScript<IWorldEntity>
    {
        private const uint ToEverstarGrove        = 70172;
        private const uint ToEverstarGroveVariant = 70664;
        private const uint ToNorthernWilds        = 70159;
        private const uint ToNorthernWildsVariant = 70663;

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
            if (!activator.CanTeleport())
                return;

            switch (owner.CreatureId)
            {
                case ToEverstarGrove:
                case ToEverstarGroveVariant:
                    activator.TeleportTo(EverstarGroveWorldId, -771.823f, -904.285f, -2269.56f);
                    break;
                case ToNorthernWilds:
                case ToNorthernWildsVariant:
                    activator.TeleportTo(NorthernWildsWorldId, 4110.71f, -658.625f, -5145.48f);
                    break;
            }
        }
    }
}
