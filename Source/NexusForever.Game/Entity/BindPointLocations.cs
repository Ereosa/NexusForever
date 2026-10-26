using System.Numerics;
using NexusForever.Database;
using NexusForever.Database.World;
using NexusForever.Database.World.Model;
using NexusForever.Game.Abstract.Entity;
using NexusForever.GameTable;
using NexusForever.GameTable.Model;
using NLog;

namespace NexusForever.Game.Entity
{
    /// <summary>
    /// Where players recall to for each bind point: the Transmat Terminal (Creature2 with a BindPointId) spawned for it.
    /// </summary>
    /// <remarks>
    /// "Recall - Transmat" and the other bind point recalls have a Teleport effect without a WorldLocation2, retail filled in the
    /// destination from the character's bind point. No arrival point is in the client data, players arrive next to the terminal.
    /// </remarks>
    public static class BindPointLocations
    {
        /// <summary>
        /// Distance in front of the terminal the player arrives at.
        /// </summary>
        private const float ArrivalOffset = 3f;

        private static readonly ILogger log = LogManager.GetCurrentClassLogger();

        private static readonly Lazy<Dictionary<uint, List<EntityModel>>> terminals = new(LoadTerminals);

        /// <summary>
        /// Teleport <see cref="IPlayer"/> to their bind point, returns false if they have none or it has no spawned terminal.
        /// </summary>
        public static bool TryRecall(IPlayer player)
        {
            if (player.BindPoint == 0 || !TryGetLocation(player.BindPoint, out ushort worldId, out Vector3 position, out float yaw))
                return false;

            if (!player.CanTeleport())
                return true;

            log.Trace($"{player.Name} recalls to bind point {player.BindPoint}, world {worldId} at {position}.");

            player.Rotation = new Vector3(yaw, 0f, 0f);
            if (worldId == player.Map.Entry.Id)
                player.TeleportToLocal(position);
            else
                player.TeleportTo(worldId, position.X, position.Y, position.Z);
            return true;
        }

        /// <summary>
        /// Return the world and position <see cref="IPlayer"/> arrives at when recalling to the supplied bind point.
        /// </summary>
        public static bool TryGetLocation(uint bindPointId, out ushort worldId, out Vector3 position, out float yaw)
        {
            worldId  = 0;
            position = Vector3.Zero;
            yaw      = 0f;

            if (!terminals.Value.TryGetValue(bindPointId, out List<EntityModel> spawns))
                return false;

            EntityModel terminal = spawns[0];
            worldId = terminal.World;
            yaw     = terminal.Rx;
            // step out in front of the terminal so the player doesn't arrive inside it
            position = new Vector3(terminal.X, terminal.Y, terminal.Z)
                + new Vector3(-MathF.Sin(terminal.Rx), 0f, -MathF.Cos(terminal.Rx)) * ArrivalOffset;
            return true;
        }

        private static Dictionary<uint, List<EntityModel>> LoadTerminals()
        {
            Dictionary<uint, uint> bindPointByCreature = GameTableManager.Instance.Creature2.Entries
                .Where(c => c != null && c.BindPointId != 0u)
                .ToDictionary(c => c.Id, c => c.BindPointId);

            var result = new Dictionary<uint, List<EntityModel>>();
            foreach (EntityModel model in DatabaseManager.Instance.GetDatabase<WorldDatabase>().GetEntitiesByCreature(bindPointByCreature.Keys)
                .OrderBy(e => e.Id))
            {
                uint bindPointId = bindPointByCreature[model.Creature];
                if (!result.TryGetValue(bindPointId, out List<EntityModel> spawns))
                    result.Add(bindPointId, spawns = new List<EntityModel>());
                spawns.Add(model);
            }

            log.Info($"Loaded {result.Values.Sum(s => s.Count)} Transmat Terminals for {result.Count} bind points.");
            return result;
        }
    }
}
