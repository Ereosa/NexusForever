using System.Numerics;
using NexusForever.Game.Abstract.Cinematic;
using NexusForever.Game.Static.Cinematic;
using NexusForever.GameTable;
using NexusForever.Script.Template.Filter;

namespace NexusForever.Script.Instance.Expedition.Infestation
{
    [ScriptFilterOwnerId(1232)]
    public class InfestationMapScript : ExpeditionMapScript
    {
        public override uint PublicEventId => 95u;

        protected override CinematicSceneId? ArrivalScene => CinematicSceneId.InfestationArrival;

        // Transport to Blue Sparrow: Algoroc (Exile, creature 22988) and Deradune (Dominion, creature 36831)
        protected override (uint WorldId, Vector3 Position) ExileReturn => (51, new Vector3(5828.06f, -861.451f, -2291.71f));
        protected override (uint WorldId, Vector3 Position) DominionReturn => (22, new Vector3(-1355.95f, -898.634f, -1159.3f));

        public InfestationMapScript(
            IGameTableManager gameTableManager,
            ICinematicFactory cinematicFactory)
            : base(gameTableManager, cinematicFactory)
        {
        }
    }
}
