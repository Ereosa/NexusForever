using NexusForever.Game.Abstract.Cinematic;
using NexusForever.Game.Abstract.Cinematic.Cinematics;
using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Map;
using NexusForever.Game.Static.Cinematic;
using NexusForever.Game.Static.Entity;
using NexusForever.Script.Template;
using NexusForever.Script.Template.Filter;

namespace NexusForever.Script.Olyssia.LevianBay
{
    [ScriptFilterOwnerId(1387)]
    public class LevianBayMapScript : IMapScript, IOwnedScript<IBaseMap>
    {
        public enum Quest : ushort
        {
            LightingTheWay = 6780
        }

        #region Dependency Injection

        private readonly ICinematicFactory cinematicFactory;

        public LevianBayMapScript(
            ICinematicFactory cinematicFactory)
        {
            this.cinematicFactory = cinematicFactory;
        }

        #endregion

        public void OnAddToMap(IGridEntity entity)
        {
            if (entity is not IPlayer player)
                return;

            if (player.QuestManager.GetQuestState(Quest.LightingTheWay) != null)
                return;

            // the arrival model holds the Cassian (GC028) and the Mechari (GC029) arrival
            ICinematicBase cinematic = player.Race == Race.Mechari
                ? cinematicFactory.CreateCinematic(CinematicSceneId.LevianBayMechariArrival)
                : cinematicFactory.CreateCinematic<ILevianBayOnCreate>();
            player.CinematicManager.QueueCinematic(cinematic);
        }
    }
}
