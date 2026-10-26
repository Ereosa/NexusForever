using NexusForever.Game.Abstract.Cinematic;
using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Map;
using NexusForever.Game.Static.Cinematic;
using NexusForever.Game.Static.Quest;
using NexusForever.Script.Template;
using NexusForever.Script.Template.Filter;

namespace NexusForever.Script.Alizar.Algoroc
{
    /// <summary>
    /// Alizar (Algoroc, Celestion, Galeras, Thayd, ...).
    /// </summary>
    [ScriptFilterOwnerId(51)]
    public class AlgorocMapScript : IMapScript, IOwnedScript<IBaseMap>
    {
        public enum Quest : ushort
        {
            // Northern Wilds: borrow a Dominion shuttle at Camp Icefury and fly to Algoroc, turned in at Tremor Ridge
            MoreImportantThanRevenge = 3963
        }

        #region Dependency Injection

        private readonly ICinematicFactory cinematicFactory;

        public AlgorocMapScript(
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

            // arriving from the Northern Wilds in the borrowed shuttle, the quest is handed in at Tremor Ridge
            if (player.QuestManager.GetQuestState(Quest.MoreImportantThanRevenge) != QuestState.Achieved)
                return;

            if (!arrivalPlayed.Add(player.CharacterId))
                return;

            ICinematicBase cinematic = cinematicFactory.CreateCinematic(CinematicSceneId.AlgorocArrival);
            if (cinematic != null)
                player.CinematicManager.QueueCinematic(cinematic);
        }
    }
}
