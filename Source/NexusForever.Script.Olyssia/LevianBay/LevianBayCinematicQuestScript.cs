using NexusForever.Game.Abstract.Cinematic;
using NexusForever.Game.Static.Cinematic;
using NexusForever.Game.Static.Quest;
using NexusForever.Script.Template;
using NexusForever.Script.Template.Filter;

namespace NexusForever.Script.Olyssia.LevianBay
{
    /// <summary>
    /// Cinematics played by Levian Bay quests.
    /// </summary>
    /// <remarks>
    /// The quest to cinematic links aren't in the client data, they follow the quest text and its hidden scripted objectives
    /// (type 5 without data), which are completed once the cinematic ends.
    /// </remarks>
    [ScriptFilterOwnerId(5866, 5868, 8826)]
    public class LevianBayCinematicQuestScript : CinematicQuestScript
    {
        private static readonly CinematicQuestTrigger[] triggers =
        [
            // Enter the Station: Star-Comm Station powers up
            new(5868, CinematicSceneId.LevianBayStarCommStation) { AfterObjectiveId = 9205, CompleteObjectiveId = 19084 },
            // The Elder Cube: "Observe Artemis as she unlocks the Elder Cube"
            new(8826, CinematicSceneId.LevianBayElderCube) { State = QuestState.Accepted, CompleteObjectiveId = 15850 },
            // A Daring Escape: Artemis flies the Exile transport out of Greyshore
            new(5866, CinematicSceneId.LevianBayEscape) { AfterObjectiveId = 8999 }
        ];

        protected override IReadOnlyList<CinematicQuestTrigger> Triggers => triggers;

        public LevianBayCinematicQuestScript(
            ICinematicFactory cinematicFactory)
            : base(cinematicFactory)
        {
        }
    }
}
