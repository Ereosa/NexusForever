using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Quest;
using NexusForever.Script.Template;
using NexusForever.Script.Template.Filter;

namespace NexusForever.Script.Main.Tutorial.Script.Part1
{
    /// <remarks>
    /// The Exile objectives for the medium and hard mines in The Face of the Enemy don't reference the mine creatures in the game tables
    /// (objective 21340 has no data and 21318 points at an unrelated creature), so they never update from the generic activate handler.
    /// </remarks>
    [ScriptFilterCreatureId(73667, 73668)]
    public class ExplosiveMineEntityScript : IWorldEntityScript, IOwnedScript<IWorldEntity>
    {
        private const uint MediumMineCreatureId = 73667;
        private const uint MediumMineObjectiveId = 21340;
        private const uint HardMineObjectiveId   = 21318;

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
            IQuest quest = activator.QuestManager.GetActiveQuest(QuestId.TheFaceOfTheEnemyExile);
            if (quest == null)
                return;

            quest.ObjectiveUpdate(owner.CreatureId == MediumMineCreatureId ? MediumMineObjectiveId : HardMineObjectiveId, 1u);
        }
    }
}
