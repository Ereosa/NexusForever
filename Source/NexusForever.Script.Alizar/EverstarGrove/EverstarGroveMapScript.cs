using NexusForever.Game.Abstract.Cinematic;
using NexusForever.Game.Abstract.Cinematic.Cinematics;
using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Map;
using NexusForever.Game.Static.Cinematic;
using NexusForever.Game.Static.Entity;
using NexusForever.Script.Template;
using NexusForever.Script.Template.Filter;

namespace NexusForever.Script.Alizar.EverstarGrove
{
    [ScriptFilterOwnerId(990)]
    public class EverstarGroveMapScript : IMapScript, IOwnedScript<IBaseMap>
    {
        public enum Quest : ushort
        {
            NaturesUprising = 6296
        }

        #region Dependency Injection

        private readonly ICinematicFactory cinematicFactory;

        public EverstarGroveMapScript(
            ICinematicFactory cinematicFactory)
        {
            this.cinematicFactory = cinematicFactory;
        }

        #endregion

        public void OnAddToMap(IGridEntity entity)
        {
            if (entity is not IPlayer player)
                return;

            if (player.QuestManager.GetQuestState(Quest.NaturesUprising) != null)
                return;

            // Mordesh have their own arrival at the Vitalus Production Facility (GC034), everyone else gets the Aurin arrival
            ICinematicBase cinematic = player.Race == Race.Mordesh
                ? cinematicFactory.CreateCinematic(CinematicSceneId.EverstarGroveMordeshArrival)
                : cinematicFactory.CreateCinematic<IEverstarGroveOnCreate>();
            player.CinematicManager.QueueCinematic(cinematic);
        }
    }
}
