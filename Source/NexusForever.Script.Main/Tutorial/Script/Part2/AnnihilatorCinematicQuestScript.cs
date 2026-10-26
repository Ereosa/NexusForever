using NexusForever.Game.Abstract.Cinematic;
using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Quest;
using NexusForever.Game.Static.Cinematic;
using NexusForever.Game.Static.Quest;
using NexusForever.Script.Template;
using NexusForever.Script.Template.Filter;

namespace NexusForever.Script.Main.Tutorial.Script.Part2
{
    /// <summary>
    /// Plays the Annihilator cinematic of the mission simulation once Servicing Circuitry is done.
    /// </summary>
    /// <remarks>
    /// Guessed trigger, nothing in the client data ties the cinematic to a quest.
    /// </remarks>
    [ScriptFilterOwnerId(10519, 10522)]
    public class AnnihilatorCinematicQuestScript : IQuestScript, IOwnedScript<IQuest>
    {
        private IQuest owner;

        #region Dependency Injection

        private readonly ICinematicFactory cinematicFactory;

        public AnnihilatorCinematicQuestScript(
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
        }

        /// <summary>
        /// Invoked when <see cref="QuestState"/> is update for <see cref="IQuest"/>.
        /// </summary>
        public void OnQuestStateChange(QuestState newState, QuestState oldState)
        {
            if (newState != QuestState.Achieved || oldState != QuestState.Accepted)
                return;

            IPlayer player = owner.GetOwner();
            ICinematicBase cinematic = cinematicFactory.CreateCinematic(CinematicSceneId.NpeAnnihilator);
            if (player != null && cinematic != null)
                player.CinematicManager.QueueCinematic(cinematic);
        }
    }
}
