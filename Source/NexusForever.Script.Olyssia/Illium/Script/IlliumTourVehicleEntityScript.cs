using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Taxi;
using NexusForever.Script.Template;
using NexusForever.Script.Template.Filter;

namespace NexusForever.Script.Olyssia.Illium.Script
{
    /// <summary>
    /// Tour hot rods of Aexa, Vector and Crydax (Illium Tour quests), boarding one starts its tour of the city.
    /// </summary>
    [ScriptFilterCreatureId(69791, 69795, 69796)]
    public class IlliumTourVehicleEntityScript : IWorldEntityScript, IOwnedScript<IWorldEntity>
    {
        #region Dependency Injection

        private readonly ICityTourManager cityTourManager;

        public IlliumTourVehicleEntityScript(
            ICityTourManager cityTourManager)
        {
            this.cityTourManager = cityTourManager;
        }

        #endregion

        private IWorldEntity owner;

        public void OnLoad(IWorldEntity owner)
        {
            this.owner = owner;
        }

        public void OnBoard(IPlayer player)
        {
            cityTourManager.TryStart(player, owner);
        }
    }
}
