using NexusForever.Game.Abstract.Cinematic;
using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Static.Cinematic;
using NexusForever.Script.Template;
using NexusForever.Script.Template.Filter;

namespace NexusForever.Script.Alizar.NorthernWilds.Script
{
    /// <summary>
    /// Dominion shuttle controls at Camp Icefury (More Important Than Revenge), the shuttle takes off before the player is sent to Algoroc.
    /// </summary>
    /// <remarks>
    /// The departure itself is the generic quest Depart handling, it waits for the cinematic.
    /// </remarks>
    [ScriptFilterCreatureId(27196)]
    public class IcefuryShipControlsEntityScript : IWorldEntityScript, IOwnedScript<IWorldEntity>
    {
        #region Dependency Injection

        private readonly ICinematicFactory cinematicFactory;

        public IcefuryShipControlsEntityScript(
            ICinematicFactory cinematicFactory)
        {
            this.cinematicFactory = cinematicFactory;
        }

        #endregion

        public void OnLoad(IWorldEntity owner)
        {
        }

        public void OnActivateSuccess(IPlayer activator)
        {
            ICinematicBase cinematic = cinematicFactory.CreateCinematic(CinematicSceneId.NorthernWildsDeparture);
            if (cinematic != null)
                activator.CinematicManager.QueueCinematic(cinematic);
        }
    }
}
