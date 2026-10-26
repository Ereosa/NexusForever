using NexusForever.Game.Abstract.Cinematic;
using NexusForever.Game.Static.Cinematic;
using NexusForever.Game.Static.Quest;
using NexusForever.Script.Template;
using NexusForever.Script.Template.Filter;

namespace NexusForever.Script.Alizar.NorthernWilds.Quest
{
    /// <summary>
    /// Cinematics played by Northern Wilds quests.
    /// </summary>
    /// <remarks>
    /// The quest to cinematic links aren't in the client data, they follow the quest text and its hidden scripted objectives
    /// (type 5 without data), which are completed once the cinematic ends.
    /// </remarks>
    [ScriptFilterOwnerId(3673)]
    public class NorthernWildsCinematicQuestScript : CinematicQuestScript
    {
        private static readonly CinematicQuestTrigger[] triggers =
        [
            // Contact with Thayd: the rescue ship lands at the lit flares and is shot down
            new(3673, CinematicSceneId.NorthernWildsSadieDeath) { AfterObjectiveId = 4748, CompleteObjectiveId = 13391 }
        ];

        protected override IReadOnlyList<CinematicQuestTrigger> Triggers => triggers;

        public NorthernWildsCinematicQuestScript(
            ICinematicFactory cinematicFactory)
            : base(cinematicFactory)
        {
        }
    }
}
