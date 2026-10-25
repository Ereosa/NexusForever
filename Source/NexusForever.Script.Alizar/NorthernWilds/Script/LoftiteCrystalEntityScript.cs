using NexusForever.Game.Abstract.Entity;
using NexusForever.Script.Template;
using NexusForever.Script.Template.Filter;

namespace NexusForever.Script.Alizar.NorthernWilds.Script
{
    /// <summary>
    /// Loftite Crystals outside Exo-Lab 729, jumping into one collects a Loftite Crystal Fragment.
    /// </summary>
    [ScriptFilterCreatureId(11205)]
    public class LoftiteCrystalEntityScript : IWorldEntityScript, IOwnedScript<IWorldEntity>
    {
        // each crystal can only be collected once per player
        private readonly HashSet<ulong> collected = [];

        /// <summary>
        /// Invoked when <see cref="IScript"/> is loaded.
        /// </summary>
        public void OnLoad(IWorldEntity owner)
        {
            owner.SetInRangeCheck(3f);
        }

        /// <summary>
        /// Invoked when <see cref="IGridEntity"/> is added to range check range.
        /// </summary>
        public void OnEnterRange(IGridEntity entity)
        {
            if (entity is not IPlayer player)
                return;

            if (player.QuestManager.GetActiveQuest(LoftiteCrystalFragment.EmpoweredTowerQuestId) == null)
                return;

            if (collected.Add(player.CharacterId))
                LoftiteCrystalFragment.Grant(player);
        }
    }
}
