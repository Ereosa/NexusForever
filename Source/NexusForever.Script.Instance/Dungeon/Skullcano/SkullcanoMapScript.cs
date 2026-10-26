using System.Numerics;
using NexusForever.Game.Abstract.Cinematic;
using NexusForever.Game.Static.Cinematic;
using NexusForever.GameTable;
using NexusForever.Script.Instance.Expedition;
using NexusForever.Script.Template.Filter;

namespace NexusForever.Script.Instance.Dungeon.Skullcano
{
    /// <summary>
    /// Skullcano (world 1263, public event 148), the Redmoon Marauders' volcano island.
    /// </summary>
    [ScriptFilterOwnerId(1263)]
    public class SkullcanoMapScript : ExpeditionMapScript
    {
        public override uint PublicEventId => 148u;

        protected override CinematicSceneId? ArrivalScene => null;

        // the Travel to Skullcano boat (creature 32444), the way in for both factions
        protected override (uint WorldId, Vector3 Position) ExileReturn => (51, new Vector3(3383.78f, -949.583f, -390.75f));
        protected override (uint WorldId, Vector3 Position) DominionReturn => (51, new Vector3(3383.78f, -949.583f, -390.75f));

        public SkullcanoMapScript(
            IGameTableManager gameTableManager,
            ICinematicFactory cinematicFactory)
            : base(gameTableManager, cinematicFactory)
        {
        }
    }
}
