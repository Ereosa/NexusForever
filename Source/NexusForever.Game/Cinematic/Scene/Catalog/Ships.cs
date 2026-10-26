using System.Numerics;
using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Static.Cinematic;
using NexusForever.Game.Static.Reputation;

namespace NexusForever.Game.Cinematic.Scene
{
    public static partial class CinematicSceneCatalog
    {
        // lengths of the generic ship models (Art\Cinematics\Generic\Exports)
        private const uint ShipArrivalLength            = 10000;
        private const uint DominionTransportDeparture   = 17967;
        private const uint DominionAttackshipDeparture  = 13967;
        private const uint ExileTransportDeparture      = 18000;
        private const uint ExileAttackshipDeparture     = 15667;

        /// <summary>
        /// Transport ship arrivals and departures between the capitals and the zones, generic ship models played on the landing sites
        /// in <see cref="TransportCinematics"/>, the origin given here is the first site.
        /// </summary>
        private static void AddShips()
        {
            // arrivals
            Ship(CinematicSceneId.FarsideArrivalExile,          ShipArrivalLength, 69934, 69935, 5316.263f, -481.969f, -4557.592f);
            Ship(CinematicSceneId.FarsideArrivalDominion,       ShipArrivalLength, 69932, 69933, 4137.365f, -691.562f, -5205.401f);
            Ship(CinematicSceneId.WilderrunArrivalExile,        ShipArrivalLength, 69980, 69981, 2072.977f, -843.098f, -1636.445f);
            Ship(CinematicSceneId.WilderrunArrivalDominion,     ShipArrivalLength, 69977, 69978, 1140.728f, -701.581f, -1987.226f);
            Ship(CinematicSceneId.WhitevaleArrivalExile,        ShipArrivalLength, 69937, 69938, 4483.653f, -948.666f, -680.538f);
            Ship(CinematicSceneId.WhitevaleArrivalDominion,     ShipArrivalLength, 69939, 69940, 1761.559f, -984.167f, -718.598f);
            Ship(CinematicSceneId.MalgraveArrivalExile,         ShipArrivalLength, 69949, 69950, 1692.424f, -941.287f, 4041.123f);
            Ship(CinematicSceneId.MalgraveArrivalDominion,      ShipArrivalLength, 69951, 69952, 1631.582f, -941.290f, 4072.855f);
            Ship(CinematicSceneId.GrimvaultArrivalExile,        ShipArrivalLength, 70081, 70082, 909.730f, -891.224f, 20.205f);
            Ship(CinematicSceneId.GrimvaultArrivalDominion,     ShipArrivalLength, 70083, 70084, 2719.134f, -963.060f, 367.068f);
            Ship(CinematicSceneId.NorthernWildsArrival,         ShipArrivalLength, 70127, 70128, 4548.010f, -700.209f, -5204.670f);

            // capital arrivals: GC147/GC148 (Thayd) and GC143/GC144 (Illium) only have plain ship models, no camera or animation.
            // Reconstructed with the generic arrival models of a transport landing on the spaceport pads, guessed.
            Ship(CinematicSceneId.ThaydArrival,                 ShipArrivalLength, 69934, 69935, 4049.792f, -803.212f, -2310.497f);
            Ship(CinematicSceneId.IlliumArrival,                ShipArrivalLength, 69932, 69933, -3477.411f, -861.846f, -509.268f);

            // Northern Wastes (Darkstone's Landing, WorldLocation2 26372) and Crimson Badlands (Bloodstone Landing, 28317)
            Ship(CinematicSceneId.NorthernWastesArrivalExile,    ShipArrivalLength, 69989, 69990, -28644.627f, -687.863f, -28801.691f);
            Ship(CinematicSceneId.NorthernWastesArrivalDominion, ShipArrivalLength, 69991, 69992, -28644.627f, -687.863f, -28801.691f);
            Ship(CinematicSceneId.CrimsonBadlandsArrivalExile,   ShipArrivalLength, 70115, 70116, -21991.766f, -971.474f, -28335.531f);

            // GC159/GC161: the first transport to Grimvault is torn apart by exanite fragments and crash lands (Edge of Twilight),
            // Exiles at the Alpha Company crash site, Dominion at Radiant Alpha Down. Origins are the crash site WorldLocation2 entries
            // of the 2015 block (49509 matches the exported camera splines 22724-22733), the Dominion one is guessed.
            Add(CinematicSceneId.GrimvaultCrashExile)
                .WithDuration(57200)
                .AtOrigin(689.062f, -938.845f, 846.066f)
                .WithCamera(71509)
                .CutTo(7667, 8).CutTo(12667, 9).CutTo(16667, 10).CutTo(20034, 11).CutTo(27034, 12).CutTo(32167, 13).CutTo(36200, 14)
                .CutTo(41267, 15).CutTo(45267, 16)
                .WithActors(71510, 71511, 71512, 71513, 71514, 71515, 73012);

            Add(CinematicSceneId.GrimvaultCrashDominion)
                .WithDuration(57200)
                .AtOrigin(2711.887f, -884.324f, 1014.727f)
                .WithCamera(71772)
                .CutTo(7667, 8).CutTo(12667, 9).CutTo(16667, 10).CutTo(20034, 11).CutTo(27034, 12).CutTo(32167, 13).CutTo(36200, 14)
                .CutTo(41267, 15).CutTo(48600, 16)
                .WithActors(71773, 71774, 71775, 71776, 71777, 71778);

            // departures
            Ship(CinematicSceneId.IlliumDepartureTransport,     DominionTransportDeparture,  69890, 69889, -3477.411f, -861.846f, -509.268f);
            Ship(CinematicSceneId.IlliumDepartureAttackship,    DominionAttackshipDeparture, 69966, 69967, -3477.411f, -861.846f, -509.268f);
            Ship(CinematicSceneId.ThaydDepartureTransport,      ExileTransportDeparture,     70098, 70099, 4049.792f, -803.212f, -2310.497f);
            Ship(CinematicSceneId.ThaydDepartureAttackship,     ExileAttackshipDeparture,    70085, 70086, 4049.792f, -803.212f, -2310.497f);
            Ship(CinematicSceneId.WhitevaleDepartureExile,      ExileAttackshipDeparture,    70054, 70055, 4483.653f, -948.666f, -680.538f);
            Ship(CinematicSceneId.WhitevaleDepartureDominion,   DominionTransportDeparture,  70057, 70058, 1761.559f, -998.343f, -718.598f);
            Ship(CinematicSceneId.FarsideDepartureExile,        ExileTransportDeparture,     70046, 70047, 5867.553f, -494.915f, -4950.604f);
            Ship(CinematicSceneId.FarsideDepartureDominion,     DominionTransportDeparture,  70048, 70049, 4137.365f, -705.563f, -5205.401f);
            Ship(CinematicSceneId.WilderrunDepartureExile,      ExileAttackshipDeparture,    70059, 70060, 2072.977f, -843.098f, -1636.445f);
            Ship(CinematicSceneId.WilderrunDepartureDominion,   DominionTransportDeparture,  70061, 70062, 1129.361f, -705.722f, -2018.215f);
            Ship(CinematicSceneId.MalgraveDepartureExile,       ExileAttackshipDeparture,    70074, 70075, 1689.177f, -955.016f, 4046.074f);
            Ship(CinematicSceneId.MalgraveDepartureDominion,    DominionAttackshipDeparture, 70076, 70077, 1636.791f, -952.770f, 4071.249f);
            Ship(CinematicSceneId.CrimsonBadlandsDepartureExile, ExileTransportDeparture,    70117, 70118, -21991.766f, -971.474f, -28335.531f);
        }

        private static void Ship(CinematicSceneId id, uint duration, uint cameraCreatureId, uint shipCreatureId, float x, float y, float z)
        {
            Add(id)
                .WithDuration(duration)
                .AtOrigin(x, y, z)
                .WithCamera(cameraCreatureId)
                .WithActors(shipCreatureId);
        }
    }

    /// <summary>
    /// Transport ship cinematics of the spaceport terminals: the departure scene on the landing site of the terminal, the arrival scene
    /// on the landing site the player is sent to.
    /// </summary>
    /// <remarks>
    /// The sites are the unreferenced WorldLocation2 block added with the 2015 ship cinematics (49594-49736): an arrival origin and a
    /// departure origin on each landing site, with the parked "Transport - Zone - Cinematic Spawn" ships on them. Which faction lands
    /// on which site of a zone isn't in the data, the site nearest to the terminal or destination is used. Scenes are rotated by the
    /// facing of their site (pi + yaw), which matched the parked transport at Tremor Ridge. Capital arrivals (GC143/144, GC147/148)
    /// have no camera model and use the generic arrival. Northern Wastes and Crimson Badlands land on the WorldLocation2 entries of
    /// their landings (26372, 28317), the client has no Dominion Crimson Badlands scenes or Northern Wastes departure.
    /// </remarks>
    public static class TransportCinematics
    {
        private const float SiteRange = 600f;

        private record Site(uint WorldId, Vector3 Position, float Angle, CinematicSceneId? Exile, CinematicSceneId? Dominion);

        private static readonly Site[] arrivals =
        [
            // Farside
            new(1421, new Vector3(5316.263f, -481.969f, -4557.592f), MathF.PI, CinematicSceneId.FarsideArrivalExile, CinematicSceneId.FarsideArrivalDominion),
            new(1421, new Vector3(4137.365f, -691.562f, -5205.401f), MathF.PI, CinematicSceneId.FarsideArrivalExile, CinematicSceneId.FarsideArrivalDominion),
            new(1421, new Vector3(5867.553f, -555.600f, -4950.604f), MathF.PI, CinematicSceneId.FarsideArrivalExile, CinematicSceneId.FarsideArrivalDominion),
            new(1421, new Vector3(4364.854f, -709.637f, -5720.442f), MathF.PI, CinematicSceneId.FarsideArrivalExile, CinematicSceneId.FarsideArrivalDominion),
            // Whitevale
            new(51, new Vector3(4483.653f, -948.666f, -680.538f), MathF.PI, CinematicSceneId.WhitevaleArrivalExile, CinematicSceneId.WhitevaleArrivalDominion),
            new(51, new Vector3(1761.559f, -984.167f, -718.598f), MathF.PI, CinematicSceneId.WhitevaleArrivalExile, CinematicSceneId.WhitevaleArrivalDominion),
            // Algoroc, Tremor Ridge
            new(51, new Vector3(3814.840f, -1001.468f, -4486.068f), -1.81479f, CinematicSceneId.AlgorocArrival, CinematicSceneId.AlgorocArrival),
            // Wilderrun
            new(22, new Vector3(1140.728f, -701.581f, -1987.226f), MathF.PI, CinematicSceneId.WilderrunArrivalExile, CinematicSceneId.WilderrunArrivalDominion),
            new(22, new Vector3(2072.977f, -843.098f, -1636.445f), MathF.PI, CinematicSceneId.WilderrunArrivalExile, CinematicSceneId.WilderrunArrivalDominion),
            // Ellevar, Lightreach Mission
            new(22, new Vector3(-2601.710f, -789.645f, -3586.000f), MathF.PI, null, CinematicSceneId.EllevarArrival),
            // Malgrave
            new(1061, new Vector3(1692.424f, -941.287f, 4041.123f), MathF.PI, CinematicSceneId.MalgraveArrivalExile, CinematicSceneId.MalgraveArrivalDominion),
            new(1061, new Vector3(1631.582f, -941.290f, 4072.855f), MathF.PI, CinematicSceneId.MalgraveArrivalExile, CinematicSceneId.MalgraveArrivalDominion),
            // Grimvault
            new(1061, new Vector3(909.730f, -891.224f, 20.205f), MathF.PI, CinematicSceneId.GrimvaultArrivalExile, CinematicSceneId.GrimvaultArrivalDominion),
            new(1061, new Vector3(2719.134f, -963.060f, 367.068f), MathF.PI, CinematicSceneId.GrimvaultArrivalExile, CinematicSceneId.GrimvaultArrivalDominion),
            // Northern Wilds, Camp Icefury
            new(426, new Vector3(4548.010f, -700.209f, -5204.670f), MathF.PI, CinematicSceneId.NorthernWildsArrival, null),
            // Northern Wastes, Darkstone's Landing
            new(1658, new Vector3(-28644.627f, -687.863f, -28801.691f), MathF.PI + 0.89403f, CinematicSceneId.NorthernWastesArrivalExile, CinematicSceneId.NorthernWastesArrivalDominion),
            // Crimson Badlands, Bloodstone Landing
            new(2997, new Vector3(-21991.766f, -971.474f, -28335.531f), MathF.PI, CinematicSceneId.CrimsonBadlandsArrivalExile, null),
            // Thayd, Spaceport Horizon pads (generic arrival, guessed)
            new(51, new Vector3(4049.792f, -803.212f, -2310.497f), MathF.PI, CinematicSceneId.ThaydArrival, null),
            new(51, new Vector3(4076.661f, -803.212f, -2311.580f), MathF.PI, CinematicSceneId.ThaydArrival, null),
            new(51, new Vector3(4025.615f, -803.212f, -2311.466f), MathF.PI, CinematicSceneId.ThaydArrival, null),
            // Illium, Spaceport Alpha pads (generic arrival, guessed)
            new(22, new Vector3(-3477.411f, -861.846f, -509.268f), 1.69128f, null, CinematicSceneId.IlliumArrival),
            new(22, new Vector3(-3643.643f, -854.477f, -816.895f), 5.63375f, null, CinematicSceneId.IlliumArrival),
            new(22, new Vector3(-3647.233f, -854.549f, -590.885f), 7.20454f, null, CinematicSceneId.IlliumArrival)
        ];

        private const uint GrimvaultWorldId = 1061;
        private const ushort EdgeOfTwilightExile    = 8051;
        private const ushort EdgeOfTwilightDominion = 7661;

        /// <summary>
        /// Return a story arrival that replaces the transport arrival, and where it leaves the player: the crash into Grimvault on the
        /// first trip there (Edge of Twilight not started yet).
        /// </summary>
        public static (SceneCinematic Cinematic, Vector3 Position)? GetStoryArrival(uint worldId, Vector3 position, IPlayer player)
        {
            if (worldId != GrimvaultWorldId)
                return null;

            // only for the Grimvault landing sites, Malgrave shares the world
            bool grimvault = Vector2.Distance(new Vector2(position.X, position.Z), new Vector2(909.730f, 20.205f)) <= SiteRange
                || Vector2.Distance(new Vector2(position.X, position.Z), new Vector2(2719.134f, 367.068f)) <= SiteRange;
            if (!grimvault)
                return null;

            bool dominion = player.Faction1 == Faction.Dominion;
            if (player.QuestManager.GetQuestState(dominion ? EdgeOfTwilightDominion : EdgeOfTwilightExile) != null)
                return null;

            CinematicScene scene = CinematicSceneCatalog.Get(dominion ? CinematicSceneId.GrimvaultCrashDominion : CinematicSceneId.GrimvaultCrashExile);
            return scene != null ? (new SceneCinematic(scene), scene.Origin) : null;
        }

        private static readonly Site[] departures =
        [
            // Thayd, Spaceport Horizon pads
            new(51, new Vector3(4049.792f, -803.212f, -2310.497f), MathF.PI, CinematicSceneId.ThaydDepartureTransport, null),
            new(51, new Vector3(4076.661f, -803.212f, -2311.580f), MathF.PI, CinematicSceneId.ThaydDepartureAttackship, null),
            new(51, new Vector3(4025.615f, -803.212f, -2311.466f), MathF.PI, CinematicSceneId.ThaydDepartureTransport, null),
            // Illium, Spaceport Alpha pads
            new(22, new Vector3(-3477.411f, -861.846f, -509.268f), 1.69128f, null, CinematicSceneId.IlliumDepartureTransport),
            new(22, new Vector3(-3643.643f, -854.477f, -816.895f), 5.63375f, null, CinematicSceneId.IlliumDepartureTransport),
            new(22, new Vector3(-3586.581f, -854.632f, -531.359f), 7.20454f, null, CinematicSceneId.IlliumDepartureAttackship),
            new(22, new Vector3(-3647.233f, -854.549f, -590.885f), 7.20454f, null, CinematicSceneId.IlliumDepartureTransport),
            new(22, new Vector3(-3586.921f, -854.480f, -874.929f), 5.63375f, null, CinematicSceneId.IlliumDepartureAttackship),
            // Algoroc, Tremor Ridge
            new(51, new Vector3(3814.840f, -998.086f, -4486.066f), 1.29275f, CinematicSceneId.AlgorocDeparture, CinematicSceneId.AlgorocDeparture),
            // Whitevale
            new(51, new Vector3(4483.653f, -948.666f, -680.538f), MathF.PI, CinematicSceneId.WhitevaleDepartureExile, CinematicSceneId.WhitevaleDepartureDominion),
            new(51, new Vector3(1761.559f, -998.343f, -718.598f), MathF.PI, CinematicSceneId.WhitevaleDepartureExile, CinematicSceneId.WhitevaleDepartureDominion),
            // Farside
            new(1421, new Vector3(5867.553f, -494.915f, -4950.604f), MathF.PI, CinematicSceneId.FarsideDepartureExile, CinematicSceneId.FarsideDepartureDominion),
            new(1421, new Vector3(5316.263f, -491.684f, -4557.592f), MathF.PI, CinematicSceneId.FarsideDepartureExile, CinematicSceneId.FarsideDepartureDominion),
            new(1421, new Vector3(4428.858f, -712.134f, -5685.467f), MathF.PI, CinematicSceneId.FarsideDepartureExile, CinematicSceneId.FarsideDepartureDominion),
            new(1421, new Vector3(4137.365f, -705.563f, -5205.401f), MathF.PI, CinematicSceneId.FarsideDepartureExile, CinematicSceneId.FarsideDepartureDominion),
            // Wilderrun
            new(22, new Vector3(2072.977f, -843.098f, -1636.445f), MathF.PI, CinematicSceneId.WilderrunDepartureExile, CinematicSceneId.WilderrunDepartureDominion),
            new(22, new Vector3(1129.361f, -705.722f, -2018.215f), MathF.PI, CinematicSceneId.WilderrunDepartureExile, CinematicSceneId.WilderrunDepartureDominion),
            // Malgrave
            new(1061, new Vector3(1689.177f, -955.016f, 4046.074f), MathF.PI, CinematicSceneId.MalgraveDepartureExile, CinematicSceneId.MalgraveDepartureDominion),
            new(1061, new Vector3(1636.791f, -952.770f, 4071.249f), MathF.PI, CinematicSceneId.MalgraveDepartureExile, CinematicSceneId.MalgraveDepartureDominion),
            // Crimson Badlands, Bloodstone Landing
            new(2997, new Vector3(-21991.766f, -971.474f, -28335.531f), MathF.PI, CinematicSceneId.CrimsonBadlandsDepartureExile, null)
        ];

        /// <summary>
        /// Return the departure cinematic for a terminal at <paramref name="position"/>, null if there is none.
        /// </summary>
        public static SceneCinematic GetDeparture(uint worldId, Vector3 position, bool dominion)
        {
            return Get(departures, worldId, position, dominion);
        }

        /// <summary>
        /// Return the arrival cinematic for a player sent to <paramref name="position"/>, null if there is none.
        /// </summary>
        public static SceneCinematic GetArrival(uint worldId, Vector3 position, bool dominion)
        {
            return Get(arrivals, worldId, position, dominion);
        }

        private static SceneCinematic Get(Site[] sites, uint worldId, Vector3 position, bool dominion)
        {
            Site site = sites
                .Where(s => s.WorldId == worldId && (dominion ? s.Dominion : s.Exile) != null)
                .Select(s => (Site: s, Distance: Vector2.Distance(new Vector2(s.Position.X, s.Position.Z), new Vector2(position.X, position.Z))))
                .Where(s => s.Distance <= SiteRange)
                .OrderBy(s => s.Distance)
                .Select(s => s.Site)
                .FirstOrDefault();
            if (site == null)
                return null;

            CinematicScene scene = CinematicSceneCatalog.Get((dominion ? site.Dominion : site.Exile).Value);
            return scene != null ? new SceneCinematic(scene, site.Position, site.Angle) : null;
        }
    }
}
