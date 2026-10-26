using NexusForever.Game.Abstract.Cinematic;
using NexusForever.Game.Abstract.Cinematic.Cinematics;
using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Map;
using NexusForever.Game.Static.Cinematic;
using NexusForever.Game.Static.Entity;
using NexusForever.Script.Template;
using NexusForever.Script.Template.Filter;

namespace NexusForever.Script.Olyssia.CrimsonIsle
{
    [ScriptFilterOwnerId(870)]
    public class CrimsonIsleMapScript : IMapScript, IOwnedScript<IBaseMap>
    {
        public enum Quest : ushort
        {
            MindTheMinesScrapTheScrab = 5593,
            BloodInTheSand            = 5595
        }

        public enum Zones : ushort
        {
            CrashSite = 1611
        }

        #region Dependency Injection

        private readonly ICinematicFactory cinematicFactory;

        public CrimsonIsleMapScript(
            ICinematicFactory cinematicFactory)
        {
            this.cinematicFactory = cinematicFactory;
        }

        #endregion

        public void OnAddToMap(IGridEntity entity)
        {
            if (entity is not IPlayer player)
                return;

            if (player.QuestManager.GetQuestState(Quest.MindTheMinesScrapTheScrab) != null
                || player.QuestManager.GetQuestState(Quest.BloodInTheSand) != null)
                return;

            // Draken arrive on the Savage Coast (GC010), Chua on Mondo's Beachhead (GC011)
            ICinematicBase cinematic = cinematicFactory.CreateCinematic(player.Race == Race.Draken
                ? CinematicSceneId.CrimsonIsleDrakenArrival
                : CinematicSceneId.CrimsonIsleChuaArrival);
            if (cinematic != null)
                player.CinematicManager.QueueCinematic(cinematic);
        }

        // TODO: OnEnterZone for Crash Site objective
    }
}
