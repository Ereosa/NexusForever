using NexusForever.Game.Abstract.Cinematic;
using NexusForever.Game.Static.Cinematic;
using NexusForever.Game.Static.Quest;
using NexusForever.Script.Template;
using NexusForever.Script.Template.Filter;

namespace NexusForever.Script.Alizar.Algoroc.Quest
{
    /// <summary>
    /// Cinematics played by Algoroc quests.
    /// </summary>
    /// <remarks>
    /// The quest to cinematic links aren't in the client data, they follow the quest text and its hidden scripted objectives
    /// (type 5 without data), which are completed once the cinematic ends.
    /// </remarks>
    [ScriptFilterOwnerId(3766, 3770, 4784, 4829, 4987)]
    public class AlgorocCinematicQuestScript : CinematicQuestScript
    {
        private static readonly CinematicQuestTrigger[] triggers =
        [
            // They're Back!: "Look to the sky", the Skull's Eye arrives
            new(3766, CinematicSceneId.AlgorocSkullsEyeArrival) { State = QuestState.Accepted, CompleteObjectiveId = 4855 },
            // Finish Him!: the Skull's Eye takes off once the player is aboard (guessed)
            new(3770, CinematicSceneId.AlgorocSkullsEyeTakeoff) { AfterObjectiveId = 7792 },
            // Finish Him!: escape pod flight from the Skull's Eye once Thokov is dead
            new(3770, CinematicSceneId.AlgorocSkullsEyeEscape) { AfterObjectiveId = 4869, CompleteObjectiveId = 4871 },
            // Precision Strike: the Caretaker raises the Eldan weapon
            new(4829, CinematicSceneId.AlgorocCannonSpawn) { State = QuestState.Accepted, CompleteObjectiveId = 13809 },
            // Precision Strike: the Eldan weapon destroys the Annihilator
            new(4829, CinematicSceneId.AlgorocWorldDestroyerDeath) { AfterObjectiveId = 6885, CompleteObjectiveId = 12843 },
            // Spawned Out: augmentors at work in the patrol area
            new(4987, CinematicSceneId.AlgorocAugmentation) { AfterObjectiveId = 7109 },
            // The Justice Assembly: Judge Kain is maimed in Throg's attack (guessed after the defence)
            new(4784, CinematicSceneId.AlgorocJudgeKainLosesAnArm) { AfterObjectiveId = 6634 }
        ];

        protected override IReadOnlyList<CinematicQuestTrigger> Triggers => triggers;

        public AlgorocCinematicQuestScript(
            ICinematicFactory cinematicFactory)
            : base(cinematicFactory)
        {
        }
    }
}
