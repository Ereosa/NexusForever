using NexusForever.Game.Abstract.Cinematic;
using NexusForever.Game.Static.Cinematic;
using NexusForever.Game.Static.Quest;
using NexusForever.Script.Template;
using NexusForever.Script.Template.Filter;

namespace NexusForever.Script.Alizar.Celestion
{
    /// <summary>
    /// Cinematics played by Celestion quests.
    /// </summary>
    /// <remarks>
    /// The quest to cinematic links aren't in the client data, they follow the quest text and its hidden scripted objectives
    /// (type 5 without data), which are completed once the cinematic ends.
    /// </remarks>
    [ScriptFilterOwnerId(6805, 6986)]
    public class CelestionCinematicQuestScript : CinematicQuestScript
    {
        private static readonly CinematicQuestTrigger[] triggers =
        [
            // Journey to Woodhaven: the Greenbough Guardians answer the call at Point Firestorm
            new(6805, CinematicSceneId.CelestionForestFightsBack) { AfterObjectiveId = 11577 },
            // Shoot for the Stars: the Junction Zeta weapon destroys the ICI spy ship
            new(6986, CinematicSceneId.CelestionCannonShootsDownShip) { AfterObjectiveId = 12757, CompleteObjectiveId = 16336 }
        ];

        protected override IReadOnlyList<CinematicQuestTrigger> Triggers => triggers;

        public CelestionCinematicQuestScript(
            ICinematicFactory cinematicFactory)
            : base(cinematicFactory)
        {
        }
    }
}
