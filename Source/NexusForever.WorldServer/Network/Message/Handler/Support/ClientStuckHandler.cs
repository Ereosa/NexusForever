using System.Collections.Generic;
using System.Numerics;
using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Housing;
using NexusForever.Game.Abstract.Map.Lock;
using NexusForever.Game.Entity;
using NexusForever.Game.Static.Spell;
using NexusForever.Game.Static.Support;
using NexusForever.Network.Message;
using NexusForever.Network.World.Message.Model.Support;

namespace NexusForever.WorldServer.Network.Message.Handler.Support
{
    /// <summary>
    /// The "Help, I'm stuck" options: recall to a safe spot, recall home or die and resurrect.
    /// </summary>
    /// <remarks>
    /// Recall to a safe spot goes to the player's bind point, players that never bound to a Transmat Terminal go to the zone's arrival point.
    /// </remarks>
    public class ClientStuckHandler : IMessageHandler<IWorldSession, ClientStuck>
    {
        // arrival points of worlds players reach by transport, others fall back to where the player entered the map
        private static readonly Dictionary<ushort, Vector3> arrivalPoints = new()
        {
            { 426, new Vector3(4110.71f, -658.625f, -5145.48f) },  // Northern Wilds
            { 990, new Vector3(-771.823f, -904.285f, -2269.56f) }  // Everstar Grove
        };

        #region Dependency Injection

        private readonly IGlobalResidenceManager globalResidenceManager;
        private readonly IMapLockManager mapLockManager;

        public ClientStuckHandler(
            IGlobalResidenceManager globalResidenceManager,
            IMapLockManager mapLockManager)
        {
            this.globalResidenceManager = globalResidenceManager;
            this.mapLockManager         = mapLockManager;
        }

        #endregion

        public void HandleMessage(IWorldSession session, ClientStuck stuck)
        {
            IPlayer player = session.Player;
            if (player == null || !player.CanTeleport())
                return;

            switch (stuck.UnstickingType)
            {
                case UnstickType.RecallTransmat:
                {
                    if (BindPointLocations.TryRecall(player))
                        break;

                    ushort worldId = (ushort)player.Map.Entry.Id;
                    if (!arrivalPoints.TryGetValue(worldId, out Vector3 position))
                        position = player.MapEntryPosition;
                    player.TeleportTo(worldId, position.X, position.Y, position.Z);
                    break;
                }
                case UnstickType.RecallHouse:
                {
                    IResidence residence = globalResidenceManager.GetResidenceByOwner(player.Name)
                        ?? globalResidenceManager.CreateResidence(player);
                    IResidenceEntrance entrance = globalResidenceManager.GetResidenceEntrance(residence.PropertyInfoId);
                    IMapLock mapLock = mapLockManager.GetResidenceLock(residence.Parent ?? residence);
                    player.TeleportTo(entrance.Entry, entrance.Position, mapLock);
                    break;
                }
                case UnstickType.FreeSuicide:
                    if (player.IsAlive)
                        player.ModifyHealth(player.Health, DamageType.Physical, player);
                    break;
            }
        }
    }
}
