using System.Numerics;
using NexusForever.Game.Abstract.Cinematic;
using NexusForever.Game.Static.Cinematic;
using NexusForever.GameTable;
using NexusForever.Script.Instance.Expedition;
using NexusForever.Script.Template.Filter;

namespace NexusForever.Script.Instance.Dungeon.SwordmaidenSanctuary
{
    /// <summary>
    /// Sanctuary of the Swordmaiden (world 1271, public event 166), the Torine sanctuary corrupted by the Strain.
    /// </summary>
    [ScriptFilterOwnerId(1271)]
    public class SwordmaidenSanctuaryMapScript : ExpeditionMapScript
    {
        public override uint PublicEventId => 166u;

        protected override CinematicSceneId? ArrivalScene => null;

        // the Enter the Sanctuary of the Swordmaiden portal in Wilderrun (creature 52411), the way in for both factions
        protected override (uint WorldId, Vector3 Position) ExileReturn => (22, new Vector3(549.62f, -520.927f, -3800.95f));
        protected override (uint WorldId, Vector3 Position) DominionReturn => (22, new Vector3(549.62f, -520.927f, -3800.95f));

        public SwordmaidenSanctuaryMapScript(
            IGameTableManager gameTableManager,
            ICinematicFactory cinematicFactory)
            : base(gameTableManager, cinematicFactory)
        {
        }
    }
}
