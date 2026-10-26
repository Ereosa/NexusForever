using System.Numerics;
using NexusForever.Database.Character;
using NexusForever.Database.Character.Model;
using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Entity.Creature;
using NexusForever.Game.Abstract.Taxi;
using NexusForever.Game.Static.Entity;
using NexusForever.Game.Static.Reputation;
using NexusForever.Game.Static.Spell;
using NexusForever.GameTable;
using NexusForever.GameTable.Model;
using NexusForever.Network.World.Message.Model;
using NexusForever.Network.World.Message.Static;
using NexusForever.Shared;
using NLog;

namespace NexusForever.Game.Taxi
{
    /// <summary>
    /// Taxi nodes discovered by a player and buying flights from taxi vendors.
    /// </summary>
    /// <remarks>
    /// Retail behaviour, worked out from the client (TaxiMap addon and the taxi manager in WildStar64.exe):
    /// talking to a taxi vendor discovers its taxi node, the client opens the taxi window by itself and lists the nodes of the
    /// vendor's world from <see cref="TaxiNodeEntry"/>. A node is usable when it is discovered, has flag 0x1 (always unlocked,
    /// eg: capital cities) or has an auto unlock level the player has reached. The client finds the cheapest chain of
    /// <see cref="TaxiRouteEntry"/> through usable nodes and sends the route ids, each route costs the destination node's
    /// content tier squared times GameFormula 952 copper. Nodes with flag 0x4 (settler and local taxi services) can be used
    /// without being discovered. A route to another world is a shuttle and has no flight.
    ///
    /// The flight itself rides a taxi vehicle along the tbl spline that links the two taxi node locations, this link is server
    /// data the client doesn't have so it is matched by the spline end points.
    /// </remarks>
    public class TaxiManager : ITaxiManager
    {
        /// <summary>
        /// GameFormula with the cost multiplier of a taxi route (Datafloat0), the client falls back to 132.75 copper.
        /// </summary>
        private const uint RouteCostFormulaId = 952u;
        private const float DefaultRouteCost  = 132.75f;

        /// <summary>
        /// Generic world taxi speeder, single passenger seat.
        /// </summary>
        private const uint DefaultTaxiCreatureId = 37280u;
        private const uint HalonRingTaxiCreatureId = 46580u;
        private const uint HalonRingWorldId = 1068u;

        /// <summary>
        /// Distance from a taxi vendor the player has to be within to buy a flight.
        /// </summary>
        private const float VendorRange = 20f;

        private static readonly ILogger log = LogManager.GetCurrentClassLogger();

        private readonly IPlayer player;
        private readonly IEntityFactory entityFactory;
        private readonly ICreatureInfoManager creatureInfoManager;

        private readonly HashSet<ushort> nodes = new();
        private readonly HashSet<ushort> pendingSave = new();

        /// <summary>
        /// Create a new <see cref="ITaxiManager"/> from existing <see cref="CharacterModel"/> database model.
        /// </summary>
        public TaxiManager(IPlayer owner, CharacterModel model, IEntityFactory entityFactory, ICreatureInfoManager creatureInfoManager)
        {
            player                   = owner;
            this.entityFactory       = entityFactory;
            this.creatureInfoManager = creatureInfoManager;

            foreach (CharacterTaxiNodeModel nodeModel in model.TaxiNode)
                nodes.Add(nodeModel.TaxiNodeId);
        }

        public void Save(CharacterContext context)
        {
            foreach (ushort taxiNodeId in pendingSave)
            {
                context.Add(new CharacterTaxiNodeModel
                {
                    Id         = player.CharacterId,
                    TaxiNodeId = taxiNodeId
                });
            }

            pendingSave.Clear();
        }

        public void SendInitialPackets()
        {
            player.Session.EnqueueMessageEncrypted(new ServerFlightPathUpdate
            {
                TaxiNodes = nodes.Select(n => (uint)n).ToList()
            });
        }

        /// <summary>
        /// Returns if the supplied taxi node is unlocked, either discovered or unlocked by flags or level.
        /// </summary>
        public bool IsNodeUnlocked(uint taxiNodeId)
        {
            if (nodes.Contains((ushort)taxiNodeId))
                return true;

            TaxiNodeEntry entry = GameTableManager.Instance.TaxiNode.GetEntry(taxiNodeId);
            if (entry == null)
                return false;

            // always unlocked
            if (entry.TaxiNodeTypeEnum <= 1u && (entry.Flags & 0x1u) != 0u)
                return true;

            return entry.AutoUnlockLevel != 0u
                && entry.TaxiNodeTypeEnum <= 1u
                && entry.AutoUnlockLevel <= player.Level;
        }

        /// <summary>
        /// Discover the supplied taxi node, returns false if it is already known or belongs to the other faction.
        /// </summary>
        public bool UnlockNode(uint taxiNodeId)
        {
            TaxiNodeEntry entry = GameTableManager.Instance.TaxiNode.GetEntry(taxiNodeId);
            if (entry == null || !IsFactionNode(entry))
                return false;

            if (!nodes.Add((ushort)taxiNodeId))
                return false;

            pendingSave.Add((ushort)taxiNodeId);

            player.Session.EnqueueMessageEncrypted(new ServerFlightPathAdd
            {
                TaxiNodeId = (ushort)taxiNodeId
            });

            log.Trace($"{player.Name} discovered taxi node {taxiNodeId}.");
            return true;
        }

        private bool IsFactionNode(TaxiNodeEntry entry)
        {
            return entry.TaxiNodeFactionEnum switch
            {
                0u => true,
                1u => player.Faction1 == Faction.Exile,
                2u => player.Faction1 == Faction.Dominion,
                _  => false
            };
        }

        /// <summary>
        /// Invoked when the player opens a taxi vendor or shuttle, the taxi node of the vendor is discovered.
        /// </summary>
        public void OnTaxiVendorInteract(IWorldEntity vendor)
        {
            uint taxiNodeId = vendor?.CreatureInfo?.Entry?.TaxiNodeId ?? 0u;
            if (taxiNodeId != 0u)
                UnlockNode(taxiNodeId);
        }

        /// <summary>
        /// Buy the flight along the supplied taxi routes, sent by the client in order from the vendor's taxi node.
        /// </summary>
        public void Purchase(IReadOnlyList<uint> taxiRouteIds)
        {
            if (taxiRouteIds.Count == 0)
                return;

            // the taxi window can stay open during the flight, further purchases are ignored until landing
            if (IsRidingTaxi())
                return;

            var routes = new List<TaxiRouteEntry>();
            foreach (uint taxiRouteId in taxiRouteIds)
            {
                TaxiRouteEntry route = GameTableManager.Instance.TaxiRoute.GetEntry(taxiRouteId);
                if (route == null)
                    return;

                // each route must continue from the destination of the previous one
                if (routes.Count > 0 && routes[^1].TaxiNodeIdDestination != route.TaxiNodeIdSource)
                    return;

                routes.Add(route);
            }

            TaxiNodeEntry origin = GameTableManager.Instance.TaxiNode.GetEntry(routes[0].TaxiNodeIdSource);
            if (origin == null)
                return;

            if (!IsNearVendor(origin.Id))
            {
                player.SendGenericError(GenericError.EmbarkNotInRange);
                return;
            }

            if (!player.IsAlive || player.InCombat || !player.CanTeleport())
            {
                player.SendGenericError(GenericError.EmbarkPlayerUnableToEmbark);
                return;
            }

            // settler and local taxi services (flag 0x4) don't need their nodes discovered
            bool openService = (origin.Flags & 0x4u) != 0u;
            if (!openService && !IsNodeUnlocked(origin.Id))
                return;

            var path = new List<(TaxiRouteEntry Route, TaxiNodeEntry Source, TaxiNodeEntry Destination, WorldLocation2Entry SourceLocation, WorldLocation2Entry DestinationLocation)>();
            for (int i = 0; i < routes.Count; i++)
            {
                TaxiNodeEntry source      = GameTableManager.Instance.TaxiNode.GetEntry(routes[i].TaxiNodeIdSource);
                TaxiNodeEntry destination = GameTableManager.Instance.TaxiNode.GetEntry(routes[i].TaxiNodeIdDestination);
                if (source == null || destination == null || !IsFactionNode(destination))
                    return;

                WorldLocation2Entry sourceLocation      = GameTableManager.Instance.WorldLocation2.GetEntry(source.WorldLocation2Id);
                WorldLocation2Entry destinationLocation = GameTableManager.Instance.WorldLocation2.GetEntry(destination.WorldLocation2Id);
                if (sourceLocation == null || destinationLocation == null)
                    return;

                // nodes along the way have to be usable, the final destination too unless the vendor is an open service
                // hidden waypoints (flag 0x2) are never shown to the player so can't be discovered
                bool last = i == routes.Count - 1;
                if ((destination.Flags & 0x2u) == 0u && !(last && openService) && !IsNodeUnlocked(destination.Id))
                    return;

                path.Add((routes[i], source, destination, sourceLocation, destinationLocation));
            }

            // a route to another world is a shuttle and is always taken on its own
            bool crossWorld = path.Any(p => p.SourceLocation.WorldId != p.DestinationLocation.WorldId);
            if (crossWorld && path.Count > 1)
                return;

            ulong cost = 0ul;
            foreach (var step in path)
                cost += CalculateRouteCost(step.Destination);

            if (!player.CurrencyManager.CanAfford(CurrencyType.Credits, cost))
            {
                player.SendGenericError(GenericError.VendorNotEnoughCash);
                return;
            }

            WorldLocation2Entry finalLocation = path[^1].DestinationLocation;
            var finalPosition = new Vector3(finalLocation.Position0, finalLocation.Position1, finalLocation.Position2);
            Vector3 finalRotation = new Quaternion(finalLocation.Facing0, finalLocation.Facing1, finalLocation.Facing2, finalLocation.Facing3).ToEuler();

            // a flight needs a spline for every route, routes without one (shuttles, transfers) move the player straight there
            List<ushort> splines = crossWorld ? null : path
                .Select(p => TaxiRouteSplines.GetSpline(p.Route.Id))
                .ToList();
            bool flight = splines != null && splines.All(s => s != 0);

            if (cost > 0ul)
                player.CurrencyManager.CurrencySubtractAmount(CurrencyType.Credits, cost);

            DismountForTaxi();

            log.Trace($"{player.Name} bought taxi routes {string.Join(",", taxiRouteIds)} for {cost} copper, flight: {flight}.");

            if (crossWorld)
            {
                player.Rotation = finalRotation;
                player.TeleportTo((ushort)finalLocation.WorldId, finalPosition.X, finalPosition.Y, finalPosition.Z);
            }
            else if (!flight)
            {
                player.Rotation = finalRotation;
                player.TeleportToLocal(finalPosition);
            }
            else
                StartFlight(path[0].SourceLocation.WorldId, splines, finalPosition, finalRotation);
        }

        private bool IsRidingTaxi()
        {
            return player.PlatformGuid != null && player.GetVisible<IGridEntity>(player.PlatformGuid.Value) is ITaxiEntity;
        }

        private bool IsNearVendor(uint taxiNodeId)
        {
            return player.GetVisible()
                .OfType<IWorldEntity>()
                .Any(e => e.CreatureInfo?.Entry?.TaxiNodeId == taxiNodeId
                    && Vector3.Distance(e.Position, player.Position) <= VendorRange);
        }

        /// <summary>
        /// Return the credit cost of a single taxi route, the same formula the client shows in the taxi window.
        /// </summary>
        public static ulong CalculateRouteCost(TaxiNodeEntry destination)
        {
            GameFormulaEntry formula = GameTableManager.Instance.GameFormula.GetEntry(RouteCostFormulaId);
            float multiplier = formula?.Datafloat0 ?? DefaultRouteCost;

            float cost = destination.ContentTier * multiplier * destination.ContentTier;
            return cost > 0f ? (ulong)(uint)cost : 0ul;
        }

        private void DismountForTaxi()
        {
            if (player.PlatformGuid == null)
                return;

            foreach (Abstract.Spell.ISpell spell in player.GetSpellsByEffect(SpellEffectType.SummonMount).ToList())
                spell.Finish();

            player.Dismount();
        }

        private void StartFlight(uint worldId, List<ushort> splines, Vector3 destination, Vector3 destinationRotation)
        {
            uint creatureId = worldId == HalonRingWorldId ? HalonRingTaxiCreatureId : DefaultTaxiCreatureId;
            ICreatureInfo creatureInfo = creatureInfoManager.GetCreatureInfo(creatureId);
            if (creatureInfo == null)
            {
                player.SendGenericError(GenericError.EmbarkInvalidVehicleUnit);
                return;
            }

            var taxi = entityFactory.CreateEntity<ITaxiEntity>();
            taxi.Initialise(player, creatureInfo, splines, destination, destinationRotation);
            taxi.EnqueuePassengerAdd(player, VehicleSeatType.Passenger, 0);
            taxi.AddToMap(player.Map, TaxiRouteSplines.GetStart(splines[0]));
        }
    }
}
