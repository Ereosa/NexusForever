using NexusForever.Game.Abstract.Entity;
using NexusForever.Script.Template;
using NexusForever.Script.Template.Filter;

namespace NexusForever.Script.Alizar.NorthernWilds.Script
{
    /// <summary>
    /// Destroying the Dominion Ultrabot guarding the ships in Camp Icefury grants Warbot Wrangler.
    /// </summary>
    /// <remarks>
    /// The achievement (type 12, granted by retail server scripts) gates the Dominion ship controls (Creature2 27196 anim state prerequisite 12979),
    /// without it the controls never offer the trip to Algoroc for More Important Than Revenge.
    /// </remarks>
    [ScriptFilterCreatureId(12526)]
    public class DominionUltrabotEntityScript : IUnitScript, IOwnedScript<IUnitEntity>
    {
        private const ushort WarbotWranglerAchievementId = 1296;

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
            if (!player.AchievementManager.HasCompletedAchievement(WarbotWranglerAchievementId))
                player.AchievementManager.GrantAchievement(WarbotWranglerAchievementId);
        }
    }
}
