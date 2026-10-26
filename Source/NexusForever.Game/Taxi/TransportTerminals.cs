using System.Numerics;
using NexusForever.Database;
using NexusForever.Database.World;
using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Cinematic.Scene;
using NexusForever.Game.Static;
using NexusForever.Game.Static.Reputation;
using NexusForever.Game.Static.Spell;
using NexusForever.GameTable;
using NexusForever.GameTable.Model;
using NLog;

namespace NexusForever.Game.Taxi
{
    /// <summary>
    /// Ships and transports between continents and regions ("Ship to Thayd", "Travel to Wilderrun", ...).
    /// </summary>
    /// <remarks>
    /// The spaceport terminals in Thayd and Illium and their counterparts in each region cast a yes/no spell that only has an
    /// Activate effect, the destination was server script data. The destination is the name in the terminal ("Ship to X") or
    /// its spell ("Travel to X"), "Transport to" objects are quest instance entrances with their own handling, resolved in this order:
    /// 1. the spaceport terminals spawned in a zone named X (or inside it), preferring the one that ships back to where the
    ///    player came from, eg: "Ship to Thayd" in Fool's Hope lands at the "Ship to Wilderrun" terminal in Thayd's spaceport;
    /// 2. a taxi or rapid transport node named X or "Somewhere, X" of the player's faction, lowest level first;
    /// 3. a landing site in the zone named X or its sub zones ("Darkstone's Landing"), else the first location found there.
    /// Terminals whose spell has its own Teleport effect are left to the spell.
    /// </remarks>
    public static class TransportTerminals
    {
        private static readonly string[] namePrefixes = { "Ship to ", "Travel to ", "Transportation to " };

        /// <summary>
        /// Distance from the destination terminal the player arrives at.
        /// </summary>
        private const float ArrivalOffset = 4f;

        private static readonly ILogger log = LogManager.GetCurrentClassLogger();

        private static readonly Lazy<List<(string Destination, uint WorldId, uint ZoneId, Vector3 Position)>> terminals = new(LoadTerminals);

        /// <summary>
        /// Send <see cref="IPlayer"/> to the destination of the supplied transport terminal, returns false if the entity isn't one.
        /// </summary>
        public static bool TryTravel(IWorldEntity terminal, IPlayer player)
        {
            Creature2Entry creature = terminal.CreatureInfo?.Entry;
            if (creature == null || creature.Spell4IdActivate[0] == 0u || !player.CanTeleport())
                return false;

            string destination = GetDestinationName(creature);
            string returnFrom  = destination == null ? GetReturnName(creature) : null;
            if (destination == null && returnFrom == null)
                return false;

            // spells with their own teleport already moved the player
            if (GameTableManager.Instance.Spell4Effects.Entries.Any(e => e.SpellId == creature.Spell4IdActivate[0] && e.EffectType == SpellEffectType.Teleport))
                return false;

            List<string> origin = GetZoneNames(terminal.Zone?.Id ?? 0u);
            if (destination != null && origin.Any(n => string.Equals(n, destination, StringComparison.OrdinalIgnoreCase)))
                return false;

            uint worldId;
            Vector3 position;
            if (returnFrom != null)
            {
                // "Transport from X" (the Star-Comm Basin contacts) goes back to the faction capital's "Transport to X" console
                if (!TryResolveReturn(returnFrom, player, out worldId, out position))
                {
                    log.Warn($"Transport terminal {creature.Id} from {returnFrom} has no way back.");
                    return false;
                }

                destination = GetCapitalName(player);
            }
            else if (!TryResolve(destination, origin, player, out worldId, out position))
            {
                log.Warn($"Transport terminal {creature.Id} to {destination} has no destination.");
                return false;
            }

            // "Transport to" consoles also stand at quest instance entrances, those have their own handling
            if (IsTransportTo(creature) && GameTableManager.Instance.World.GetEntry(worldId)?.Type != 0)
                return false;

            log.Trace($"{player.Name} takes transport terminal {creature.Id} to {destination}, world {worldId} at {position}.");

            // the ship takes off from the terminal's landing site, and lands again on the site at the destination
            bool dominion = player.Faction1 == Faction.Dominion;
            SceneCinematic departure = TransportCinematics.GetDeparture(player.Map.Entry.Id, terminal.Position, dominion);
            SceneCinematic arrival   = TransportCinematics.GetArrival(worldId, position, dominion);

            // a story arrival (the Grimvault crash) replaces the landing and leaves the player at the crash site
            if (TransportCinematics.GetStoryArrival(worldId, position, player) is (SceneCinematic Cinematic, Vector3 Position) story)
            {
                arrival  = story.Cinematic;
                position = story.Position;
            }

            void Travel()
            {
                if (!player.CanTeleport())
                    return;

                if (worldId == player.Map.Entry.Id)
                {
                    player.TeleportToLocal(position, callback: _ =>
                    {
                        if (arrival != null)
                            player.CinematicManager.QueueCinematic(arrival);
                    });
                }
                else
                {
                    if (arrival != null)
                        player.CinematicManager.QueueAfterEnteringWorld(arrival);
                    player.TeleportTo((ushort)worldId, position.X, position.Y, position.Z);
                }
            }

            if (departure != null)
                player.CinematicManager.QueueCinematic(departure, Travel);
            else
                Travel();
            return true;
        }

        private static string GetDestinationName(Creature2Entry creature)
        {
            TextTable text = GameTableManager.Instance.GetTextTable(Language.English);
            if (text == null)
                return null;

            string name = Strip(text.GetEntry(creature.LocalizedTextIdName));
            if (name != null)
                return name;

            string spellName = GetSpellName(creature);
            return Strip(spellName) ?? (IsTransportTo(creature) ? spellName["Transport to ".Length..].Trim() : null);
        }

        private static string GetSpellName(Creature2Entry creature)
        {
            Spell4Entry spell = GameTableManager.Instance.Spell4.GetEntry(creature.Spell4IdActivate[0]);
            Spell4BaseEntry spellBase = spell != null ? GameTableManager.Instance.Spell4Base.GetEntry(spell.Spell4BaseIdBaseSpell) : null;
            return spellBase != null ? GameTableManager.Instance.GetTextTable(Language.English)?.GetEntry(spellBase.LocalizedTextIdName) : null;
        }

        /// <summary>
        /// Returns if the terminal's spell is a "Transport to X" yes/no (Star-Comm Basin consoles), not the "Depart" or "Teleport"
        /// spells of the tutorial ship controls or the "Activating" spells of quest instance transports.
        /// </summary>
        private static bool IsTransportTo(Creature2Entry creature)
        {
            return GetSpellName(creature)?.StartsWith("Transport to ", StringComparison.OrdinalIgnoreCase) ?? false;
        }

        /// <summary>
        /// Return X of a "Transport from X" terminal spell.
        /// </summary>
        private static string GetReturnName(Creature2Entry creature)
        {
            string spellName = GetSpellName(creature);
            return spellName != null && spellName.StartsWith("Transport from ", StringComparison.OrdinalIgnoreCase)
                ? spellName["Transport from ".Length..].Trim()
                : null;
        }

        private static string GetCapitalName(IPlayer player)
        {
            return player.Faction1 == Faction.Dominion ? "Illium" : "Thayd";
        }

        private static bool TryResolveReturn(string from, IPlayer player, out uint worldId, out Vector3 position)
        {
            string capital = GetCapitalName(player);
            var console = terminals.Value
                .Where(t => string.Equals(t.Destination, from, StringComparison.OrdinalIgnoreCase))
                .OrderByDescending(t => GetZoneNames(t.ZoneId).Any(n => string.Equals(n, capital, StringComparison.OrdinalIgnoreCase)))
                .Select(t => ((uint WorldId, Vector3 Position)?)(t.WorldId, t.Position))
                .FirstOrDefault();
            if (console == null)
            {
                worldId  = 0u;
                position = Vector3.Zero;
                return false;
            }

            worldId  = console.Value.WorldId;
            position = console.Value.Position + new Vector3(ArrivalOffset, 0f, 0f);
            return true;
        }

        private static string Strip(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return null;

            foreach (string prefix in namePrefixes)
                if (name.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                    return name[prefix.Length..].Trim();

            return null;
        }

        private static bool TryResolve(string destination, List<string> origin, IPlayer player, out uint worldId, out Vector3 position)
        {
            // 1. spaceport terminals at the destination
            var arrival = terminals.Value
                .Where(t => GetZoneNames(t.ZoneId).Any(n => string.Equals(n, destination, StringComparison.OrdinalIgnoreCase)))
                .OrderByDescending(t => origin.Any(n => string.Equals(n, t.Destination, StringComparison.OrdinalIgnoreCase)))
                .Select(t => ((uint WorldId, Vector3 Position)?)(t.WorldId, t.Position))
                .FirstOrDefault();
            if (arrival != null)
            {
                worldId  = arrival.Value.WorldId;
                position = arrival.Value.Position + new Vector3(ArrivalOffset, 0f, 0f);
                return true;
            }

            // 2. taxi and rapid transport nodes
            TextTable text = GameTableManager.Instance.GetTextTable(Language.English);
            WorldLocation2Entry location = GameTableManager.Instance.TaxiNode.Entries
                .Where(n => n != null && IsFactionNode(n, player))
                .Select(n => (Node: n, Name: text?.GetEntry(n.LocalizedTextId)?.Trim() ?? ""))
                .Where(n => string.Equals(n.Name, destination, StringComparison.OrdinalIgnoreCase)
                    || n.Name.EndsWith(", " + destination, StringComparison.OrdinalIgnoreCase))
                .OrderBy(n => n.Node.RecommendedMinLevel)
                .Select(n => GameTableManager.Instance.WorldLocation2.GetEntry(n.Node.WorldLocation2Id))
                .FirstOrDefault(l => l != null);

            // 3. a landing site in the zone (eg: Darkstone's Landing in Northern Wastes, Bloodstone Landing in Crimson Badlands),
            //    else the first location of the zone or one of its sub zones
            location ??= GetZoneLocation(destination, text);

            if (location == null)
            {
                worldId  = 0u;
                position = Vector3.Zero;
                return false;
            }

            worldId  = location.WorldId;
            position = new Vector3(location.Position0, location.Position1, location.Position2);
            return true;
        }

        private static readonly string[] landingNames = { "Landing", "Spaceport", "Dock" };

        private static WorldLocation2Entry GetZoneLocation(string destination, TextTable text)
        {
            if (text == null)
                return null;

            ILookup<uint, WorldZoneEntry> children = GameTableManager.Instance.WorldZone.Entries
                .Where(z => z != null && z.ParentZoneId != 0u)
                .ToLookup(z => z.ParentZoneId);

            // the zone itself first, then its sub zones breadth first, the top zone of a pocket world often has no locations of its own
            var zones = new List<WorldZoneEntry>();
            var queue = new Queue<WorldZoneEntry>(GameTableManager.Instance.WorldZone.Entries
                .Where(z => z != null && string.Equals(text.GetEntry(z.LocalizedTextIdName)?.Trim(), destination, StringComparison.OrdinalIgnoreCase)));
            var seen = new HashSet<uint>();
            while (queue.TryDequeue(out WorldZoneEntry zone) && zones.Count < 512)
            {
                if (!seen.Add(zone.Id))
                    continue;

                zones.Add(zone);
                foreach (WorldZoneEntry child in children[zone.Id])
                    queue.Enqueue(child);
            }

            ILookup<uint, WorldLocation2Entry> locations = GameTableManager.Instance.WorldLocation2.Entries
                .Where(l => l != null && l.WorldZoneId != 0u && seen.Contains(l.WorldZoneId))
                .ToLookup(l => l.WorldZoneId);

            WorldZoneEntry best = zones
                .Where(z => locations[z.Id].Any())
                .OrderByDescending(z => landingNames.Any(n => (text.GetEntry(z.LocalizedTextIdName) ?? "").Contains(n, StringComparison.OrdinalIgnoreCase)))
                .FirstOrDefault();

            // exact spots (radius 5 or less) before area markers
            return best != null ? locations[best.Id].OrderBy(l => l.Radius > 5f).ThenBy(l => l.Id).First() : null;
        }

        private static bool IsFactionNode(TaxiNodeEntry entry, IPlayer player)
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
        /// Return the names of the zone and every parent zone.
        /// </summary>
        private static List<string> GetZoneNames(uint zoneId)
        {
            var names = new List<string>();
            TextTable text = GameTableManager.Instance.GetTextTable(Language.English);
            if (text == null)
                return names;

            for (WorldZoneEntry zone = GameTableManager.Instance.WorldZone.GetEntry(zoneId); zone != null && names.Count < 16;
                zone = zone.ParentZoneId != 0u ? GameTableManager.Instance.WorldZone.GetEntry(zone.ParentZoneId) : null)
            {
                string name = text.GetEntry(zone.LocalizedTextIdName);
                if (!string.IsNullOrWhiteSpace(name))
                    names.Add(name.Trim());
            }

            return names;
        }

        private static List<(string, uint, uint, Vector3)> LoadTerminals()
        {
            TextTable text = GameTableManager.Instance.GetTextTable(Language.English);

            // terminals stand in the open worlds, the ones with taxi or rapid transport nodes
            var worlds = GameTableManager.Instance.TaxiNode.Entries
                .Where(n => n != null)
                .Select(n => GameTableManager.Instance.WorldLocation2.GetEntry(n.WorldLocation2Id))
                .Where(l => l != null)
                .Select(l => (ushort)l.WorldId)
                .ToHashSet();

            var result = new List<(string, uint, uint, Vector3)>();
            foreach (ushort worldId in worlds)
            {
                foreach (var model in DatabaseManager.Instance.GetDatabase<WorldDatabase>().GetEntities(worldId))
                {
                    Creature2Entry creature = GameTableManager.Instance.Creature2.GetEntry(model.Creature);
                    if (creature == null || creature.Spell4IdActivate[0] == 0u)
                        continue;

                    string destination = GetDestinationName(creature);
                    if (destination == null)
                        continue;

                    result.Add((destination, model.World, model.Area, new Vector3(model.X, model.Y, model.Z)));
                }
            }

            log.Info($"Loaded {result.Count} transport terminals.");
            return result;
        }
    }
}
