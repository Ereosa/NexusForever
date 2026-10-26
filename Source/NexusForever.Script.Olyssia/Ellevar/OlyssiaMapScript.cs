using NexusForever.Game.Abstract.Cinematic;
using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Map;
using NexusForever.Game.Static.Cinematic;
using NexusForever.Game.Static.Quest;
using NexusForever.Game.Static.Reputation;
using NexusForever.Script.Template;
using NexusForever.Script.Template.Filter;

namespace NexusForever.Script.Olyssia.Ellevar
{
    /// <summary>
    /// Olyssia (Ellevar, Deradune, Illium, ...).
    /// </summary>
    [ScriptFilterOwnerId(22)]
    public class OlyssiaMapScript : IMapScript, IOwnedScript<IBaseMap>
    {
        public enum Quest : ushort
        {
            // Levian Bay: escape in Artemis' stolen Exile transport, handed in at Lightreach Mission
            ADaringEscape  = 5866,
            // Crimson Isle: the Exile transport launched from Megatech Station
            LastResistance = 5594,
            // Deradune: first Bloodfire Village quest of the Dominion story
            SanguineSanctity = 6489
        }

        // Bloodfire Village, Western Deradune
        private const uint BloodfireVillageZoneId = 484;

        #region Dependency Injection

        private readonly ICinematicFactory cinematicFactory;

        public OlyssiaMapScript(
            ICinematicFactory cinematicFactory)
        {
            this.cinematicFactory = cinematicFactory;
        }

        #endregion

        private readonly HashSet<ulong> arrivalPlayed = [];
        private readonly HashSet<ulong> deraduneArrivalPlayed = [];

        public void OnAddToMap(IGridEntity entity)
        {
            if (entity is not IPlayer player)
                return;

            // arriving in Ellevar from the Dominion starter zones, the quest is handed in on arrival
            if (player.QuestManager.GetQuestState(Quest.ADaringEscape) != QuestState.Achieved
                && player.QuestManager.GetQuestState(Quest.LastResistance) != QuestState.Achieved)
                return;

            if (!arrivalPlayed.Add(player.CharacterId))
                return;

            ICinematicBase cinematic = cinematicFactory.CreateCinematic(CinematicSceneId.EllevarArrival);
            if (cinematic != null)
                player.CinematicManager.QueueCinematic(cinematic);
        }

        public void OnPlayerZoneChange(IPlayer player, uint zoneId)
        {
            // Deradune arrival (GC012) on the first visit to Bloodfire Village, before the Bloodfire story starts.
            // The retail trigger isn't known, guessed from the scene (arriving at the village).
            if (zoneId != BloodfireVillageZoneId || player.Faction1 != Faction.Dominion || player.CinematicManager.IsPlaying)
                return;

            if (player.QuestManager.GetQuestState(Quest.SanguineSanctity) != null)
                return;

            if (!deraduneArrivalPlayed.Add(player.CharacterId))
                return;

            ICinematicBase cinematic = cinematicFactory.CreateCinematic(CinematicSceneId.DeraduneArrival);
            if (cinematic != null)
                player.CinematicManager.QueueCinematic(cinematic);
        }
    }
}
