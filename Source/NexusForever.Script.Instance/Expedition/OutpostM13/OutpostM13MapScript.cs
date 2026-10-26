using System.Numerics;
using NexusForever.Game.Abstract.Cinematic;
using NexusForever.Game.Static.Cinematic;
using NexusForever.GameTable;
using NexusForever.Script.Template.Filter;

namespace NexusForever.Script.Instance.Expedition.OutpostM13
{
    [ScriptFilterOwnerId(1319)]
    public class OutpostM13MapScript : ExpeditionMapScript
    {
        public override uint PublicEventId => 108u;

        protected override CinematicSceneId? ArrivalScene => CinematicSceneId.OutpostM13Arrival;

        // Transport to Steady Traveler: Algoroc (Exile, creature 36642) and Deradune (Dominion, creature 23765)
        protected override (uint WorldId, Vector3 Position) ExileReturn => (51, new Vector3(4367.96f, -1057.19f, -3978.62f));
        protected override (uint WorldId, Vector3 Position) DominionReturn => (22, new Vector3(-4881.05f, -930.882f, -95.7716f));

        public OutpostM13MapScript(
            IGameTableManager gameTableManager,
            ICinematicFactory cinematicFactory)
            : base(gameTableManager, cinematicFactory)
        {
        }
    }
}
