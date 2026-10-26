using NexusForever.Game.Abstract.Cinematic;
using NexusForever.Game.Static.Cinematic;
using NexusForever.Game.Static.Quest;
using NexusForever.Script.Template;
using NexusForever.Script.Template.Filter;

namespace NexusForever.Script.Alizar.EverstarGrove.Quest
{
    /// <summary>
    /// Cinematics played by Everstar Grove quests.
    /// </summary>
    /// <remarks>
    /// The quest to cinematic links aren't in the client data, they follow the quest text and its hidden scripted objectives
    /// (type 5 without data), which are completed once the cinematic ends.
    /// </remarks>
    [ScriptFilterOwnerId(6302, 8784)]
    public class EverstarGroveCinematicQuestScript : CinematicQuestScript
    {
        private static readonly CinematicQuestTrigger[] triggers =
        [
            // Waking Elderoot: the espernetic interface wakes Elderoot
            new(6302, CinematicSceneId.EverstarGroveElderootAwakens) { AfterObjectiveId = 9675, CompleteObjectiveId = 15731 },
            // Everything You Ever Wanted to Know: "Listen to Elderoot's knowledge", then the Dominion burns it
            new(8784, CinematicSceneId.EverstarGroveElderootBurns) { State = QuestState.Accepted, CompleteObjectiveId = 15740 }
        ];

        protected override IReadOnlyList<CinematicQuestTrigger> Triggers => triggers;

        public EverstarGroveCinematicQuestScript(
            ICinematicFactory cinematicFactory)
            : base(cinematicFactory)
        {
        }
    }
}
