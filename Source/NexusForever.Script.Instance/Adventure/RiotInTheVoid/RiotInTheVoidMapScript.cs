using System.Numerics;
using NexusForever.Game.Abstract.Cinematic;
using NexusForever.Game.Static.Cinematic;
using NexusForever.GameTable;
using NexusForever.Script.Instance.Expedition;
using NexusForever.Script.Template.Filter;

namespace NexusForever.Script.Instance.Adventure.RiotInTheVoid
{
    [ScriptFilterOwnerId(1437)]
    public class RiotInTheVoidMapScript : ExpeditionMapScript
    {
        public override uint PublicEventId => 179u;

        protected override CinematicSceneId? ArrivalScene => null;

        // the adventure is entered from the Content Finder, players without a return go to the Thayd or Illium spaceport
        protected override (uint WorldId, Vector3 Position) ExileReturn => (51, new Vector3(4049.792f, -803.212f, -2310.497f));
        protected override (uint WorldId, Vector3 Position) DominionReturn => (22, new Vector3(-3477.411f, -861.846f, -509.268f));

        public RiotInTheVoidMapScript(
            IGameTableManager gameTableManager,
            ICinematicFactory cinematicFactory)
            : base(gameTableManager, cinematicFactory)
        {
        }
    }
}
