using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Quest;
using NexusForever.Game.Static.Reputation;
using NexusForever.Script.Template;

namespace NexusForever.Script.Main.Tutorial.Script.Part1
{
    public abstract class NavigatingNexusSequentialEntityScript : ICanSeeMeScript, IOwnedScript<IWorldEntity>
    {
        protected IWorldEntity owner;

        /// <summary>
        /// Invoked when <see cref="IScript"/> is loaded.
        /// </summary>
        public virtual void OnLoad(IWorldEntity owner)
        {
            this.owner = owner;
        }

        /// <summary>
        /// Determines whether the specified <see cref="IGridEntity"/> can see this entity.
        /// </summary>
        public bool CanSeeMe(IGridEntity entity)
        {
            if (entity is not IPlayer player)
                return false;

            QuestId questId = player.Faction2 == Faction.Exile
                ? QuestId.NavigatingNexusExile
                : QuestId.NavigatingNexusDominion;

            IQuest quest = player.QuestManager.GetActiveQuest(questId);
            if (quest == null)
                return false;

            return owner.QuestChecklistIdx switch
            {
                0 => !IsComplete(quest, 0),
                1 => IsComplete(quest, 0) && !IsComplete(quest, 1),
                2 => IsComplete(quest, 1) && !IsComplete(quest, 3),
                3 => IsComplete(quest, 3),
                _ => false,
            };
        }

        /// <remarks>
        /// The Exile and Dominion quests have different objective ids but the same objective order, so objectives are looked up by index.
        /// </remarks>
        private static bool IsComplete(IQuest quest, byte index)
        {
            return quest.GetQuestObjectiveByIndex(index)?.IsComplete() ?? false;
        }
    }
}
