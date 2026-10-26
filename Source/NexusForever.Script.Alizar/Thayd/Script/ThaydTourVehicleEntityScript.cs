using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Taxi;
using NexusForever.Script.Template;
using NexusForever.Script.Template.Filter;

namespace NexusForever.Script.Alizar.Thayd.Script
{
    /// <summary>
    /// Tour grinders of Jakke, Beraxa and Darak (Thayd Tour quests), boarding one starts its tour of the city.
    /// </summary>
    [ScriptFilterCreatureId(69653, 69654, 69655)]
    public class ThaydTourVehicleEntityScript : IWorldEntityScript, IOwnedScript<IWorldEntity>
    {
        #region Dependency Injection

        private readonly ICityTourManager cityTourManager;

        public ThaydTourVehicleEntityScript(
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
