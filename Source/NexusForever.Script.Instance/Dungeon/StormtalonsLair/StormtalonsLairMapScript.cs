using System.Numerics;
using NexusForever.Game.Abstract.Cinematic;
using NexusForever.Game.Static.Cinematic;
using NexusForever.GameTable;
using NexusForever.Script.Instance.Expedition;
using NexusForever.Script.Template.Filter;

namespace NexusForever.Script.Instance.Dungeon.StormtalonsLair
{
    /// <summary>
    /// Stormtalon's Lair (world 382, public event 145), the Thundercall Pell's temple under the Galeras skies.
    /// </summary>
    [ScriptFilterOwnerId(382)]
    public class StormtalonsLairMapScript : ExpeditionMapScript
    {
        public override uint PublicEventId => 145u;

        protected override CinematicSceneId? ArrivalScene => null;

        // the Stormtalon's Lair portal in Galeras (creature 23700), the way in for both factions
        protected override (uint WorldId, Vector3 Position) ExileReturn => (51, new Vector3(5597.29f, -997.708f, -1787.27f));
        protected override (uint WorldId, Vector3 Position) DominionReturn => (51, new Vector3(5597.29f, -997.708f, -1787.27f));

        public StormtalonsLairMapScript(
            IGameTableManager gameTableManager,
            ICinematicFactory cinematicFactory)
            : base(gameTableManager, cinematicFactory)
        {
        }
    }
}
