using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Quest;
using NexusForever.Game.Static.Reputation;
using NexusForever.Script.Template;
using NexusForever.Script.Template.Filter;

namespace NexusForever.Script.Main.Tutorial.Script.Part1
{
    /// <remarks>
    /// The objectives for the medium and hard mines in The Face of the Enemy don't reference the mine creatures in the game tables
    /// (Exile 21340 has no data and 21318 points at an unrelated creature, Dominion 21314 and 21315 point at other simulation creatures),
    /// so they never update from the generic activate handler. Both factions have the medium mine at objective index 2 and the hard mine at 3.
    /// </remarks>
    [ScriptFilterCreatureId(73667, 73668)]
    public class ExplosiveMineEntityScript : IWorldEntityScript, IOwnedScript<IWorldEntity>
    {
        private const uint MediumMineCreatureId = 73667;
        private const byte MediumMineObjectiveIndex = 2;
        private const byte HardMineObjectiveIndex   = 3;

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
            QuestId questId = activator.Faction2 == Faction.Exile
                ? QuestId.TheFaceOfTheEnemyExile
                : QuestId.TheFaceOfTheEnemyDominion;

            IQuest quest = activator.QuestManager.GetActiveQuest(questId);
            if (quest == null)
                return;

            IQuestObjective objective = quest.GetQuestObjectiveByIndex(owner.CreatureId == MediumMineCreatureId ? MediumMineObjectiveIndex : HardMineObjectiveIndex);
            if (objective != null)
                quest.ObjectiveUpdate(objective.ObjectiveInfo.Id, 1u);
        }
    }
}
