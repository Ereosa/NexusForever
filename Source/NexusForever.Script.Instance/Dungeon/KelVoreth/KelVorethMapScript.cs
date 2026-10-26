using System.Numerics;
using NexusForever.Game.Abstract.Cinematic;
using NexusForever.Game.Static.Cinematic;
using NexusForever.GameTable;
using NexusForever.Script.Instance.Expedition;
using NexusForever.Script.Template.Filter;

namespace NexusForever.Script.Instance.Dungeon.KelVoreth
{
    /// <summary>
    /// Ruins of Kel Voreth (world 1336, public event 161), the Osun forge city under the Fires of Kel Voreth.
    /// </summary>
    [ScriptFilterOwnerId(1336)]
    public class KelVorethMapScript : ExpeditionMapScript
    {
        public override uint PublicEventId => 161u;

        protected override CinematicSceneId? ArrivalScene => null;

        // the Enter the Ruins of Kel Voreth portal in Auroria (creature 33528), the way in for both factions
        protected override (uint WorldId, Vector3 Position) ExileReturn => (22, new Vector3(-756.65f, -794.981f, -435.662f));
        protected override (uint WorldId, Vector3 Position) DominionReturn => (22, new Vector3(-756.65f, -794.981f, -435.662f));

        public KelVorethMapScript(
            IGameTableManager gameTableManager,
            ICinematicFactory cinematicFactory)
            : base(gameTableManager, cinematicFactory)
        {
        }
    }
}
