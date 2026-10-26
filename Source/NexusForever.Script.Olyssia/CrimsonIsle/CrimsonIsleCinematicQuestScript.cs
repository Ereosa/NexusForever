using NexusForever.Game.Abstract.Cinematic;
using NexusForever.Game.Static.Cinematic;
using NexusForever.Game.Static.Quest;
using NexusForever.Script.Template;
using NexusForever.Script.Template.Filter;

namespace NexusForever.Script.Olyssia.CrimsonIsle
{
    /// <summary>
    /// Cinematics played by Crimson Isle quests.
    /// </summary>
    /// <remarks>
    /// The quest to cinematic links aren't in the client data, they follow the quest text and its hidden scripted objectives
    /// (type 5 without data), which are completed once the cinematic ends.
    /// </remarks>
    [ScriptFilterOwnerId(5573, 5575, 5583, 5594)]
    public class CrimsonIsleCinematicQuestScript : CinematicQuestScript
    {
        private static readonly CinematicQuestTrigger[] triggers =
        [
            // Powering Down: the Megatech shield generator goes down
            new(5573, CinematicSceneId.CrimsonIsleShieldGenerator) { AfterObjectiveId = 8229, CompleteObjectiveId = 12870 },
            // Seizing Power: the Megatech shield generator goes down
            new(5575, CinematicSceneId.CrimsonIsleShieldGenerator) { AfterObjectiveId = 8371, CompleteObjectiveId = 12871 },
            // Heavy Armor: the Dominion fleet strikes Megatech Station (guessed)
            new(5583, CinematicSceneId.CrimsonIsleDominionInvasion) { State = QuestState.Accepted },
            // Last Resistance: the powered Eldan Annihilator fires as the transport launches
            new(5594, CinematicSceneId.CrimsonIsleEldanAnnihilator) { AfterObjectiveId = 8365, CompleteObjectiveId = 8366 }
        ];

        protected override IReadOnlyList<CinematicQuestTrigger> Triggers => triggers;

        public CrimsonIsleCinematicQuestScript(
            ICinematicFactory cinematicFactory)
            : base(cinematicFactory)
        {
        }
    }
}
