using System.Numerics;
using NexusForever.Game.Abstract.Cinematic;
using NexusForever.Game.Static.Cinematic;
using NexusForever.GameTable;
using NexusForever.Script.Template.Filter;

namespace NexusForever.Script.Instance.Expedition.Gautlet
{
    [ScriptFilterOwnerId(2183)]
    public class GauntletMapScript : ExpeditionMapScript
    {
        public override uint PublicEventId => 446u;

        // the show's introduction plays when the crew wakes up aboard the Gauntlet, not on arrival in the shuttle
        protected override CinematicSceneId? ArrivalScene => null;

        // Nightflare Ladder in Malgrave (creature 49827), used by both factions
        protected override (uint WorldId, Vector3 Position) ExileReturn => (1061, new Vector3(3263.97f, -855.203f, 4302.42f));
        protected override (uint WorldId, Vector3 Position) DominionReturn => (1061, new Vector3(3263.97f, -855.203f, 4302.42f));

        public GauntletMapScript(
            IGameTableManager gameTableManager,
            ICinematicFactory cinematicFactory)
            : base(gameTableManager, cinematicFactory)
        {
        }
    }
}
