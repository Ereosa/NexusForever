using System.Numerics;
using NexusForever.Game.Abstract.Cinematic;
using NexusForever.Game.Static.Cinematic;
using NexusForever.GameTable;
using NexusForever.Script.Template.Filter;

namespace NexusForever.Script.Instance.Expedition.SpaceMadness
{
    [ScriptFilterOwnerId(2149)]
    public class SpaceMadnessMapScript : ExpeditionMapScript
    {
        public override uint PublicEventId => 390u;

        protected override CinematicSceneId? ArrivalScene => CinematicSceneId.SpaceMadnessArrival;

        // Transport to Space Station Venture in Farside (Exile creature 46003, Dominion creature 51705)
        protected override (uint WorldId, Vector3 Position) ExileReturn => (1421, new Vector3(4424.82f, -708.33f, -5587.08f));
        protected override (uint WorldId, Vector3 Position) DominionReturn => (1421, new Vector3(3968.68f, -721.66f, -5125.59f));

        public SpaceMadnessMapScript(
            IGameTableManager gameTableManager,
            ICinematicFactory cinematicFactory)
            : base(gameTableManager, cinematicFactory)
        {
        }
    }
}
