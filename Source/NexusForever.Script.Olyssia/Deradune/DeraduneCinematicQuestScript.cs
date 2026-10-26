using NexusForever.Game.Abstract.Cinematic;
using NexusForever.Game.Static.Cinematic;
using NexusForever.Game.Static.Quest;
using NexusForever.Script.Template;
using NexusForever.Script.Template.Filter;

namespace NexusForever.Script.Olyssia.Deradune
{
    /// <summary>
    /// Cinematics played by Deradune quests.
    /// </summary>
    /// <remarks>
    /// The quest to cinematic links aren't in the client data, they follow the quest text and its hidden scripted objectives
    /// (type 5 without data), which are completed once the cinematic ends.
    /// </remarks>
    [ScriptFilterOwnerId(3170, 5511)]
    public class DeraduneCinematicQuestScript : CinematicQuestScript
    {
        private static readonly CinematicQuestTrigger[] triggers =
        [
            // Blood for Blood: Zonaga appears once Kezzia is met at its lair
            new(3170, CinematicSceneId.DeraduneZonagaIntro) { AfterObjectiveId = 4324 },
            // Blood for Blood: the Bloodfeast at the altar once the head is placed
            new(3170, CinematicSceneId.DeraduneBloodfeast) { AfterObjectiveId = 4326 },
            // Hostile Negotiations: the Falkrin answer the ceremonial horn
            new(5511, CinematicSceneId.DeraduneFalkrinDiplomacy) { AfterObjectiveId = 8187 }
        ];

        protected override IReadOnlyList<CinematicQuestTrigger> Triggers => triggers;

        public DeraduneCinematicQuestScript(
            ICinematicFactory cinematicFactory)
            : base(cinematicFactory)
        {
        }
    }
}
