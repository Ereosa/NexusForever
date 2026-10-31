using System.Numerics;
using NexusForever.Game.Abstract.Cinematic;
using NexusForever.Game.Static.Cinematic;
using NexusForever.GameTable;
using NexusForever.Script.Instance.Expedition;
using NexusForever.Script.Template.Filter;

namespace NexusForever.Script.Instance.Dungeon.ProtogamesAcademy
{
    /// <summary>
    /// Protogames Academy (world 3173, public event 667), the level 10 training dungeon.
    /// </summary>
    [ScriptFilterOwnerId(3173)]
    public class ProtogamesAcademyMapScript : ExpeditionMapScript
    {
        public override uint PublicEventId => 667u;

        protected override CinematicSceneId? ArrivalScene => null;

        // the Enter Protogames Academy portals (creature 67845) in Gallow (Algoroc) and in world 22, used when the way in isn't known
        protected override (uint WorldId, Vector3 Position) ExileReturn => (51, new Vector3(4171.6f, -1051.8f, -4066.2f));
        protected override (uint WorldId, Vector3 Position) DominionReturn => (22, new Vector3(-2515.6f, -802.2f, -3689.2f));

        public ProtogamesAcademyMapScript(
            IGameTableManager gameTableManager,
            ICinematicFactory cinematicFactory)
            : base(gameTableManager, cinematicFactory)
        {
        }
    }
}
