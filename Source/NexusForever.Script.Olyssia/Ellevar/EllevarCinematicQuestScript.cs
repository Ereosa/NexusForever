using NexusForever.Game.Abstract.Cinematic;
using NexusForever.Game.Static.Cinematic;
using NexusForever.Game.Static.Quest;
using NexusForever.Script.Template;
using NexusForever.Script.Template.Filter;

namespace NexusForever.Script.Olyssia.Ellevar
{
    /// <summary>
    /// Cinematics played by Ellevar quests.
    /// </summary>
    /// <remarks>
    /// The quest to cinematic links aren't in the client data, they follow the quest text and its hidden scripted objectives
    /// (type 5 without data), which are completed once the cinematic ends.
    /// </remarks>
    [ScriptFilterOwnerId(6578)]
    public class EllevarCinematicQuestScript : CinematicQuestScript
    {
        private static readonly CinematicQuestTrigger[] triggers =
        [
            // The Wrath of the High Priest: the Stormseeker ritual ends with the High Priest dead (guessed)
            new(6578, CinematicSceneId.EllevarHighShamanRitual) { State = QuestState.Achieved }
        ];

        protected override IReadOnlyList<CinematicQuestTrigger> Triggers => triggers;

        public EllevarCinematicQuestScript(
            ICinematicFactory cinematicFactory)
            : base(cinematicFactory)
        {
        }
    }
}
