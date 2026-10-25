using NexusForever.Game.Abstract.Entity;
using NexusForever.Script.Template;
using NexusForever.Script.Template.Filter;

namespace NexusForever.Script.Alizar.NorthernWilds.Script
{
    /// <summary>
    /// Destroying a Crystal Guardian gives everyone who fought it a Loftite Crystal Fragment.
    /// </summary>
    [ScriptFilterCreatureId(11925)]
    public class CrystalGuardianEntityScript : IUnitScript, IOwnedScript<IUnitEntity>
    {
        private IUnitEntity owner;

        /// <summary>
        /// Invoked when <see cref="IScript"/> is loaded.
        /// </summary>
        public void OnLoad(IUnitEntity owner)
        {
            this.owner = owner;
        }

        /// <summary>
        /// Invoked for each <see cref="IPlayer"/> on the threat list when <see cref="IUnitEntity"/> is killed.
        /// </summary>
        public void OnRewardKiller(IPlayer player)
        {
            LoftiteCrystalFragment.Grant(player);
        }
    }
}
