using NexusForever.Game.Abstract.Cinematic;
using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Map;
using NexusForever.Game.Static.Cinematic;
using NexusForever.Game.Static.Quest;
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
            LastResistance = 5594
        }

        #region Dependency Injection

        private readonly ICinematicFactory cinematicFactory;

        public OlyssiaMapScript(
            ICinematicFactory cinematicFactory)
        {
            this.cinematicFactory = cinematicFactory;
        }

        #endregion

        private readonly HashSet<ulong> arrivalPlayed = [];

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
    }
}
