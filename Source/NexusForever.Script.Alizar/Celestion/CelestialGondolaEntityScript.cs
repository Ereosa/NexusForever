using System.Numerics;
using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Entity.Creature;
using NexusForever.Game.Static.Entity;
using NexusForever.Game.Static.Entity.Movement.Spline;
using NexusForever.GameTable;
using NexusForever.GameTable.Model;
using NexusForever.Script.Template;
using NexusForever.Script.Template.Filter;

namespace NexusForever.Script.Alizar.Celestion
{
    /// <summary>
    /// Celestial Gondola on the Celestial River, boarding it rides the river down to the far landing.
    /// </summary>
    /// <remarks>
    /// The gondola is a taxi unit with no route of its own (report 106), the route is the river spline that starts at its mooring (9595).
    /// Every rider gets a gondola of their own like the city tours, the one in the world stays for the next player. Ride speed guessed.
    /// </remarks>
    [ScriptFilterCreatureId(GondolaCreatureId)]
    public class CelestialGondolaEntityScript : IWorldEntityScript, IOwnedScript<IWorldEntity>
    {
        private const uint GondolaCreatureId = 32223;
        private const ushort RiverSplineId   = 9595;
        private const float RideSpeed        = 8f;

        #region Dependency Injection

        private readonly IEntityFactory entityFactory;
        private readonly ICreatureInfoManager creatureInfoManager;

        public CelestialGondolaEntityScript(
            IEntityFactory entityFactory,
            ICreatureInfoManager creatureInfoManager)
        {
            this.entityFactory       = entityFactory;
            this.creatureInfoManager = creatureInfoManager;
        }

        #endregion

        private IWorldEntity owner;

        public void OnLoad(IWorldEntity owner)
        {
            this.owner = owner;
        }

        public void OnBoard(IPlayer player)
        {
            if (player.TaxiManager.FlightDestination != null || player.CinematicManager.IsPlaying || !player.CanTeleport())
                return;

            ICreatureInfo creatureInfo = creatureInfoManager.GetCreatureInfo(GondolaCreatureId);
            if (creatureInfo == null)
                return;

            var points = new List<Vector3>();
            foreach (Spline2NodeEntry node in GameTableManager.Instance.Spline2Node.Entries
                .Where(n => n != null && n.SplineId == RiverSplineId)
                .OrderBy(n => n.Ordinal))
            {
                var position = new Vector3(node.Position0, node.Position1, node.Position2);
                if (points.Count == 0 || Vector3.DistanceSquared(points[^1], position) > 0.01f)
                    points.Add(position);
            }
            if (points.Count < 2)
                return;

            if (player.PlatformGuid != null)
                player.Dismount();

            SplineType type = GameTableManager.Instance.Spline2.GetEntry(RiverSplineId)?.SplineType ?? SplineType.CatmullRom;
            var legs = new List<TaxiLeg> { new(points, type, RideSpeed, SplineId: RiverSplineId) };

            var gondola = entityFactory.CreateEntity<ITaxiEntity>();
            gondola.Initialise(player, creatureInfo, legs, points[^1], owner.Rotation, null);
            gondola.EnqueuePassengerAdd(player, VehicleSeatType.Passenger, 0);
            gondola.AddToMap(player.Map, points[0]);
        }
    }
}
