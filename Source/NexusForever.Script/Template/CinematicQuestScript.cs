using NexusForever.Game.Abstract.Cinematic;
using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Quest;
using NexusForever.Game.Static.Cinematic;
using NexusForever.Game.Static.Quest;

namespace NexusForever.Script.Template
{
    /// <summary>
    /// When a quest plays a cinematic.
    /// </summary>
    /// <remarks>
    /// The client data has no link between quests and cinematics, retail ran them from server scripts.
    /// </remarks>
    public record CinematicQuestTrigger(ushort QuestId, CinematicSceneId Scene)
    {
        /// <summary>
        /// Play when the quest moves to this state.
        /// </summary>
        public QuestState? State { get; init; }

        /// <summary>
        /// Play when this objective is completed.
        /// </summary>
        public uint? AfterObjectiveId { get; init; }

        /// <summary>
        /// Objective completed once the cinematic ends, for the scripted "watch" objectives (type 5, no data) the cinematic stands in for.
        /// </summary>
        public uint? CompleteObjectiveId { get; init; }
    }

    /// <summary>
    /// Plays cinematics for the quests of a zone.
    /// </summary>
    public abstract class CinematicQuestScript : IQuestScript, IOwnedScript<IQuest>
    {
        protected abstract IReadOnlyList<CinematicQuestTrigger> Triggers { get; }

        private IQuest owner;
        private readonly HashSet<CinematicQuestTrigger> played = [];

        #region Dependency Injection

        private readonly ICinematicFactory cinematicFactory;

        protected CinematicQuestScript(
            ICinematicFactory cinematicFactory)
        {
            this.cinematicFactory = cinematicFactory;
        }

        #endregion

        /// <summary>
        /// Invoked when <see cref="IQuestScript"/> is loaded.
        /// </summary>
        public void OnLoad(IQuest owner)
        {
            this.owner = owner;

            // scripts are attached after a new quest is already accepted, so accept triggers are handled here
            // a quest loaded with the character (player not in the world yet) doesn't replay its cinematic
            IPlayer player = owner.GetOwner();
            if (owner.State != QuestState.Accepted || player?.Map == null || player.IsLoading)
                return;

            foreach (CinematicQuestTrigger trigger in Triggers)
            {
                if (trigger.QuestId != owner.Id || trigger.State != QuestState.Accepted)
                    continue;

                // a re-accepted quest only replays its cinematic while the objective it stands in for is open
                if (trigger.CompleteObjectiveId is uint objectiveId && owner.GetQuestObjective(objectiveId)?.IsComplete() == true)
                    continue;

                Play(trigger);
            }
        }

        /// <summary>
        /// Invoked when <see cref="QuestState"/> is update for <see cref="IQuest"/>.
        /// </summary>
        public void OnQuestStateChange(QuestState newState, QuestState oldState)
        {
            foreach (CinematicQuestTrigger trigger in Triggers)
                if (trigger.QuestId == owner.Id && trigger.State == newState && trigger.State != oldState)
                    Play(trigger);
        }

        /// <summary>
        /// Invoked when <see cref="IQuestObjective"/> is updated for <see cref="IQuest"/>.
        /// </summary>
        public void OnObjectiveUpdate(IQuestObjective objective)
        {
            if (!objective.IsComplete())
                return;

            foreach (CinematicQuestTrigger trigger in Triggers)
                if (trigger.QuestId == owner.Id && trigger.AfterObjectiveId == objective.ObjectiveInfo.Id)
                    Play(trigger);
        }

        private void Play(CinematicQuestTrigger trigger)
        {
            if (!played.Add(trigger))
                return;

            IPlayer player = owner.GetOwner();
            if (player == null)
                return;

            ICinematicBase cinematic = cinematicFactory.CreateCinematic(trigger.Scene);
            if (cinematic == null)
            {
                CompleteObjective(trigger);
                return;
            }

            player.CinematicManager.QueueCinematic(cinematic, () => CompleteObjective(trigger));
        }

        private void CompleteObjective(CinematicQuestTrigger trigger)
        {
            if (trigger.CompleteObjectiveId is not uint objectiveId)
                return;

            IQuestObjective objective = owner.GetQuestObjective(objectiveId);
            if (objective == null || objective.IsComplete())
                return;

            owner.ObjectiveUpdate(objectiveId, objective.ObjectiveInfo.Entry.Count);
        }
    }
}
