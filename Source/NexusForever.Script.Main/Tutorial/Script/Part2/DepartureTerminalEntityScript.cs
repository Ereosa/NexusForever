using NexusForever.Game.Abstract.Cinematic;
using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Static.Cinematic;
using NexusForever.Script.Template;
using NexusForever.Script.Template.Filter;

namespace NexusForever.Script.Main.Tutorial.Script.Part2
{
    /// <summary>
    /// Arkship departure terminals at the end of the tutorial (Bon Voyage).
    /// </summary>
    /// <remarks>
    /// The terminal's activate spell is a yes/no confirmation, on success the player leaves the tutorial for the chosen starting zone.
    /// Destinations are the veteran (skip tutorial) starting locations from character creation.
    /// The Northern Wilds terminal plays the Arkship launch (GC071) first, the drop pod lands once the player arrives.
    /// </remarks>
    [ScriptFilterCreatureId(EverstarGroveTerminal, NorthernWildsTerminal, CrimsonIsleTerminal, LevianBayTerminal)]
    public class DepartureTerminalEntityScript : IWorldEntityScript, IOwnedScript<IWorldEntity>
    {
        private const uint EverstarGroveTerminal = 73605;
        private const uint NorthernWildsTerminal = 73606;
        private const uint CrimsonIsleTerminal   = 74772;
        private const uint LevianBayTerminal     = 74773;

        private const ushort NorthernWildsWorldId = 426;
        private const ushort EverstarGroveWorldId = 990;
        private const ushort CrimsonIsleWorldId   = 870;
        private const ushort LevianBayWorldId     = 1387;

        private IWorldEntity owner;

        #region Dependency Injection

        private readonly ICinematicFactory cinematicFactory;

        public DepartureTerminalEntityScript(
            ICinematicFactory cinematicFactory)
        {
            this.cinematicFactory = cinematicFactory;
        }

        #endregion

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
            switch (owner.CreatureId)
            {
                case NorthernWildsTerminal:
                {
                    void Depart() => activator.TeleportTo(NorthernWildsWorldId, 4110.71f, -658.625f, -5145.48f);

                    ICinematicBase launch = cinematicFactory.CreateCinematic(CinematicSceneId.NpeLaunchNorthernWilds);
                    if (launch != null)
                        activator.CinematicManager.QueueCinematic(launch, Depart);
                    else
                        Depart();
                    break;
                }
                case EverstarGroveTerminal:
                    activator.TeleportTo(EverstarGroveWorldId, -771.823f, -904.285f, -2269.56f);
                    break;
                case CrimsonIsleTerminal:
                    activator.TeleportTo(CrimsonIsleWorldId, -8261.4f, -995.471f, -242.365f);
                    break;
                case LevianBayTerminal:
                    activator.TeleportTo(LevianBayWorldId, -3835.34f, -980.217f, -6050.52f);
                    break;
            }
        }
    }
}
