using NexusForever.Game.Abstract.Cinematic;
using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Static.Cinematic;
using NexusForever.Script.Template;
using NexusForever.Script.Template.Filter;

namespace NexusForever.Script.Alizar.Algoroc.Script
{
    /// <summary>
    /// Ship controls at the Tremor Ridge landing site, the shuttle back from Algoroc to Camp Icefury in Northern Wilds.
    /// </summary>
    /// <remarks>
    /// The "Depart" spell has no teleport data and no quest uses the controls, so neither the quest departure nor the transport
    /// terminal handling sends the player anywhere (report 54). The shuttle takes off from Tremor Ridge and lands at the
    /// Camp Icefury landing site, next to the controls of the shuttle that brought the player to Algoroc. Landing spot guessed.
    /// </remarks>
    [ScriptFilterCreatureId(45102)]
    public class TremorRidgeShipControlsEntityScript : IWorldEntityScript, IOwnedScript<IWorldEntity>
    {
        private const ushort NorthernWildsWorldId = 426;

        #region Dependency Injection

        private readonly ICinematicFactory cinematicFactory;

        public TremorRidgeShipControlsEntityScript(
            ICinematicFactory cinematicFactory)
        {
            this.cinematicFactory = cinematicFactory;
        }

        #endregion

        public void OnLoad(IWorldEntity owner)
        {
        }

        /// <summary>
        /// Invoked when <see cref="IWorldEntity"/> is successfully activated by <see cref="IPlayer"/>.
        /// </summary>
        public void OnActivateSuccess(IPlayer activator)
        {
            if (!activator.CanTeleport())
                return;

            void Depart()
            {
                if (!activator.CanTeleport())
                    return;

                ICinematicBase arrival = cinematicFactory.CreateCinematic(CinematicSceneId.NorthernWildsArrival);
                if (arrival != null)
                    activator.CinematicManager.QueueAfterEnteringWorld(arrival);
                activator.TeleportTo(NorthernWildsWorldId, 4534.84f, -694.278f, -5196.13f);
            }

            ICinematicBase departure = cinematicFactory.CreateCinematic(CinematicSceneId.AlgorocDeparture);
            if (departure != null)
                activator.CinematicManager.QueueCinematic(departure, Depart);
            else
                Depart();
        }
    }
}
