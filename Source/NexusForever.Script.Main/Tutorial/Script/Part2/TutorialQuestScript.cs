using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Quest;
using NexusForever.Game.Static.Quest;
using NexusForever.Script.Template;

namespace NexusForever.Script.Main.Tutorial.Script.Part2
{
    /// <summary>
    /// Tutorial quests complete themselves and hand out the next quest in the chain.
    /// </summary>
    /// <remarks>
    /// Some tutorial quests have objectives the server can't progress yet (opening the store, optional hints),
    /// <see cref="FinalObjectiveId"/> marks the objective that should finish the quest regardless.
    /// </remarks>
    public abstract class TutorialQuestScript : IQuestScript, IOwnedScript<IQuest>
    {
        public abstract QuestId? NextQuestId { get; }
        protected virtual uint? FinalObjectiveId => null;

        protected IQuest owner;
        private bool advanced;

        /// <summary>
        /// Invoked when <see cref="IQuestScript"/> is loaded.
        /// </summary>
        public void OnLoad(IQuest owner)
        {
            this.owner = owner;
        }

        /// <summary>
        /// Invoked when <see cref="QuestState"/> is update for <see cref="IQuest"/>.
        /// </summary>
        public void OnQuestStateChange(QuestState newState, QuestState oldState)
        {
            if (newState != QuestState.Achieved || advanced)
                return;

            advanced = true;

            // auto completed quests still give their rewards (experience, Omnibits, items)
            IPlayer player = owner.GetOwner();
            owner.RewardQuest(0);
            player.QuestManager.QuestComplete(owner.Id);
            if (NextQuestId.HasValue)
                player.QuestManager.QuestAdd(NextQuestId.Value);
        }

        /// <summary>
        /// Invoked when <see cref="IQuestObjective"/> is updated for <see cref="IQuest"/>.
        /// </summary>
        public virtual void OnObjectiveUpdate(IQuestObjective objective)
        {
            if (objective.ObjectiveInfo.Id != FinalObjectiveId || !objective.IsComplete())
                return;

            // the quest moves to achieved once every objective is complete
            foreach (IQuestObjective remaining in owner)
                if (!remaining.IsComplete())
                    remaining.Complete();
        }
    }
}
