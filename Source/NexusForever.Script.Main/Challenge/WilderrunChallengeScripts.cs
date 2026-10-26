using System.Numerics;
using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Entity.Creature;
using NexusForever.Game.Static.Entity.Movement.Spline;
using NexusForever.GameTable;
using NexusForever.GameTable.Model;
using NexusForever.Script.Template.Filter;

namespace NexusForever.Script.Main.Challenge
{
    /// <summary>
    /// Soul Sanctification (Wilderrun): "Purify Tainted Spirits before they reach the Everpool", started from the Sacred Carving for CH977.
    /// </summary>
    /// <remarks>
    /// The Tainted Torine Spirits (47180, 47234) were event spawns that aren't in the world spawns. They rise at Jabbithole's sightings
    /// of them in the Everpool Approach and drift towards the Everpool (location 21477), a spirit that leaves the challenge area got
    /// there. The T-spell purifies a spirit, which turns into a Purified Torine Spirit (47235, 48045) for a moment. The spawn interval
    /// and drift speed aren't in any data.
    /// </remarks>
    [ScriptFilterOwnerId(977)]
    public class SoulSanctificationChallengeScript : ChallengeScript
    {
        private const uint EverpoolLocationId = 21477;
        private const double SpawnInterval    = 5d;
        private const double PurifiedDuration = 3d;
        private const float DriftSpeed        = 2f;
        private const int MaxSpirits          = 6;

        private static readonly uint[] taintedCreatureIds  = [47180, 47234];
        private static readonly uint[] purifiedCreatureIds = [47235, 48045];

        private static readonly Vector2[] sightings =
        [
            new(2091, -3199), new(2120, -3194), new(2093, -3173), new(2095, -3155), new(2079, -3172), new(2092, -3159),
            new(2085, -3165), new(2090, -3207), new(2091, -3179), new(2099, -3203), new(2079, -3180), new(2124, -3193),
            new(2102, -3199), new(2082, -3217), new(2085, -3189), new(2081, -3213), new(2077, -3154), new(2083, -3197),
            new(2081, -3204), new(2090, -3217), new(2080, -3183), new(2109, -3195), new(2078, -3176), new(2098, -3152),
            new(2083, -3221), new(2097, -3213), new(2142, -3174), new(2138, -3184), new(2082, -3159)
        ];

        private readonly ICreatureInfoManager creatureInfoManager;
        private readonly List<uint> spirits = [];
        private readonly List<(uint Guid, double Expiry)> purified = [];
        private double time;
        private double nextSpawn;

        public SoulSanctificationChallengeScript(ICreatureInfoManager creatureInfoManager)
        {
            this.creatureInfoManager = creatureInfoManager;
        }

        public override void OnActivate()
        {
            time      = 0d;
            nextSpawn = 0d;
        }

        public override void Update(double lastTick)
        {
            time += lastTick;

            WorldLocation2Entry area = GameTableManager.Instance.WorldLocation2.GetEntry(Challenge.Entry.WorldLocation2IdIndicator);
            if (area == null)
                return;

            var centre = new Vector3(area.Position0, area.Position1, area.Position2);

            // spirits that drifted out of the approach reached the Everpool
            foreach (uint guid in spirits.ToList())
            {
                IWorldEntity spirit = Player.GetVisible<IWorldEntity>(guid);
                if (spirit != null && HorizontalDistance(spirit.Position, centre) <= area.Radius + 10f)
                    continue;

                Player.SummonFactory.Unsummon(guid);
                spirits.Remove(guid);
            }

            foreach ((uint guid, double expiry) in purified.Where(p => time >= p.Expiry).ToList())
            {
                Player.SummonFactory.Unsummon(guid);
                purified.Remove((guid, expiry));
            }

            if (time < nextSpawn || spirits.Count >= MaxSpirits)
                return;

            nextSpawn = time + SpawnInterval;
            SpawnSpirit(area);
        }

        private void SpawnSpirit(WorldLocation2Entry area)
        {
            ICreatureInfo creatureInfo = creatureInfoManager.GetCreatureInfo(taintedCreatureIds[Random.Shared.Next(taintedCreatureIds.Length)]);
            WorldLocation2Entry everpool = GameTableManager.Instance.WorldLocation2.GetEntry(EverpoolLocationId);
            if (creatureInfo == null || everpool == null)
                return;

            Vector2 sighting = sightings[Random.Shared.Next(sightings.Length)];
            var start = new Vector3(sighting.X, Height(sighting.X, sighting.Y, area.Position1), sighting.Y);

            Vector3 direction = new Vector3(everpool.Position0, 0f, everpool.Position2) - new Vector3(start.X, 0f, start.Z);
            Vector3 end = start + Vector3.Normalize(direction) * (area.Radius * 2f + 20f);
            end.Y = Height(end.X, end.Z, start.Y);

            Player.SummonFactory.Summon(creatureInfo, start, Vector3.Zero, (map, guid, position) =>
            {
                spirits.Add(guid);
                Player.GetVisible<IWorldEntity>(guid)?.MovementManager.LaunchSpline([start, end], SplineType.Linear, SplineMode.OneShot, DriftSpeed);
            });
        }

        private float Height(float x, float z, float fallback)
        {
            float? terrain = Player.Map?.GetTerrainHeight(x, z);
            return terrain != null && MathF.Abs(terrain.Value - fallback) < 8f ? terrain.Value : fallback;
        }

        public override void OnEntitySpellHit(uint spell4Id, IUnitEntity target)
        {
            if (!spirits.Remove(target.Guid))
                return;

            Vector3 position = target.Position;
            Player.SummonFactory.Unsummon(target.Guid);

            ICreatureInfo creatureInfo = creatureInfoManager.GetCreatureInfo(purifiedCreatureIds[Random.Shared.Next(purifiedCreatureIds.Length)]);
            if (creatureInfo != null)
                Player.SummonFactory.Summon(creatureInfo, position, Vector3.Zero, (map, guid, p) => purified.Add((guid, time + PurifiedDuration)));
        }

        public override void OnStop(bool completed)
        {
            foreach (uint guid in spirits)
                Player.SummonFactory.Unsummon(guid);
            foreach ((uint guid, double _) in purified)
                Player.SummonFactory.Unsummon(guid);
            spirits.Clear();
            purified.Clear();
        }
    }

    /// <summary>
    /// Bird Watching (Wilderrun): "Click as many Colored Nagaeg as you can" in the Undergrove.
    /// </summary>
    /// <remarks>
    /// The Colored Nagaeg (47265, 47324, 47325, 49986) were event spawns that aren't in the world spawns and have no activate spell,
    /// clicking one selects it. One bird at a time lands on one of Jabbithole's sightings of them and flies off again after a few
    /// seconds, selecting it before it does counts. How long a bird stays isn't in any data.
    /// </remarks>
    [ScriptFilterOwnerId(980)]
    public class BirdWatchingChallengeScript : ChallengeScript
    {
        private const double LandedDuration = 6d;
        private const double LandingDelay   = 1.5d;

        private static readonly uint[] nagaegCreatureIds = [47265, 47324, 47325, 49986];

        private static readonly Vector2[] sightings =
        [
            new(2268, -2422), new(2216, -2458), new(2303, -2469), new(2291, -2487), new(2227, -2462), new(2253, -2532),
            new(2199, -2496), new(2281, -2505), new(2263, -2497), new(2266, -2416), new(2256, -2487), new(2270, -2455),
            new(2295, -2459), new(2234, -2470), new(2314, -2453), new(2252, -2529), new(2221, -2465), new(2268, -2472),
            new(2213, -2453), new(2311, -2473), new(2281, -2481), new(2286, -2420), new(2288, -2493), new(2307, -2457),
            new(2271, -2531), new(2299, -2444), new(2186, -2497), new(2319, -2495), new(2234, -2526), new(2244, -2423),
            new(2280, -2517), new(2228, -2529), new(2261, -2506), new(2241, -2425)
        ];

        private readonly ICreatureInfoManager creatureInfoManager;
        private uint bird;
        private double time;
        private double flyOff;
        private double nextLanding;

        public BirdWatchingChallengeScript(ICreatureInfoManager creatureInfoManager)
        {
            this.creatureInfoManager = creatureInfoManager;
        }

        public override void OnActivate()
        {
            time        = 0d;
            nextLanding = 0d;
            bird        = 0u;
        }

        public override void Update(double lastTick)
        {
            time += lastTick;

            if (bird != 0u)
            {
                if (Player.TargetGuid == bird)
                {
                    AddProgress();
                    FlyOff();
                }
                else if (time >= flyOff)
                    FlyOff();
                return;
            }

            if (time >= nextLanding)
                Land();
        }

        private void Land()
        {
            WorldLocation2Entry area = GameTableManager.Instance.WorldLocation2.GetEntry(Challenge.Entry.WorldLocation2IdIndicator);
            ICreatureInfo creatureInfo = creatureInfoManager.GetCreatureInfo(nagaegCreatureIds[Random.Shared.Next(nagaegCreatureIds.Length)]);
            if (area == null || creatureInfo == null)
                return;

            Vector2 sighting = sightings[Random.Shared.Next(sightings.Length)];
            float? terrain = Player.Map?.GetTerrainHeight(sighting.X, sighting.Y);
            float y = terrain != null && MathF.Abs(terrain.Value - area.Position1) < 8f ? terrain.Value : area.Position1;

            flyOff      = time + LandedDuration;
            nextLanding = flyOff + LandingDelay;
            float yaw   = (float)(Random.Shared.NextDouble() * Math.PI * 2d - Math.PI);
            Player.SummonFactory.Summon(creatureInfo, new Vector3(sighting.X, y, sighting.Y), new Vector3(yaw, 0f, 0f), (map, guid, position) => bird = guid);
        }

        private void FlyOff()
        {
            Player.SummonFactory.Unsummon(bird);
            bird        = 0u;
            nextLanding = time + LandingDelay;
        }

        public override void OnStop(bool completed)
        {
            if (bird != 0u)
                Player.SummonFactory.Unsummon(bird);
            bird = 0u;
        }
    }
}
