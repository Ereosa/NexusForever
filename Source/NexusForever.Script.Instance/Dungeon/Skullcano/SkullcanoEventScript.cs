using System.Numerics;
using NexusForever.Game.Abstract.Cinematic;
using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Entity.Creature;
using NexusForever.Game.Abstract.PublicEvent;
using NexusForever.Game.Abstract.Quest;
using NexusForever.Game.Abstract.Spell;
using NexusForever.GameTable;
using NexusForever.Script.Instance.Expedition;
using NexusForever.Script.Template.Filter;
using NexusForever.Shared;

namespace NexusForever.Script.Instance.Dungeon.Skullcano
{
    /// <summary>
    /// Skullcano (public event 148, world 1263): the Redmoon Marauders' excavation of the volcano.
    /// </summary>
    /// <remarks>
    /// No packet captures exist. The order follows the objective data: defeat Thunderfoot to open the way up to the volcano, find Chief
    /// Kaskalak at the entrance to the Molten Cavern (objective 362/820 location, WorldLocation2 18273) and speak to him, escort him through
    /// the cavern (he walks the client's Spline2 6904, which starts at his spot and ends at the cavern's far side), make your way across the
    /// Lava-Flooded Chasm (turnstile location 18109), defeat Bosun Octog, gather on the Redmoon Hauling Platform (location 18879, where the
    /// platform's Spline2 7037 starts) which carries the party to the summit, fight through the cave to the Eldan Terraformer (location
    /// 18093) and defeat Mordechai Redmoon. The optional objectives count through the generic kill and activation handling of their target
    /// groups, the jailors by their deaths.
    /// Guessed: Kaskalak's walking speed and that he starts over at the cavern entrance when he dies, the chasm counting once a player
    /// is within 60 m of its location, the platform ride (the party is carried to the end of the platform's path after 5 seconds on it,
    /// the client doesn't carry players on a server moved platform), no doors (their positions aren't in the client data).
    /// </remarks>
    [ScriptFilterOwnerId(148)]
    public class SkullcanoEventScript : ExpeditionEventScript
    {
        private enum Phase : uint
        {
            Thunderfoot,
            FindKaskalak,
            TalkKaskalak,
            Escort,
            Octog,
            Platform,
            Cave,
            Mordechai,
            Done
        }

        private static class Objective
        {
            public const uint StewShaman      = 321;
            public const uint KillThunderfoot = 322;
            public const uint KillOctog       = 323;
            public const uint KillMordechai   = 324;
            public const uint Escort          = 328;
            public const uint Chasm           = 329;
            public const uint ShinyObjects    = 333;
            public const uint Marauders       = 334;
            public const uint FreeLopp        = 335;
            public const uint Jailors         = 336;
            public const uint LavaCores       = 338;
            public const uint FindKaskalak    = 362;
            public const uint ClusterMissiles = 365;
            public const uint Platform        = 372;
            public const uint Cave            = 376;
            public const uint Quartermaster   = 440;
            public const uint TalkKaskalak    = 820;
        }

        private static readonly Dictionary<uint, uint> Veteran = new()
        {
            { 24475, 24893 }, { 24486, 24894 }, { 24489, 24895 }, { 24493, 24898 }, { 24490, 24896 }, { 24488, 34551 },
            { 33452, 24788 }, { 34457, 34458 }, { 39137, 39191 }, { 39336, 57126 }, { 24680, 24921 }, { 24499, 24902 },
            { 24498, 24901 }, { 24494, 24899 }, { 33084, 33140 }, { 24495, 24900 }, { 32664, 38592 }, { 51470, 57188 },
            { 33348, 33350 }, { 25564, 25565 }, { 39419, 56779 }, { 24578, 24913 }, { 26236, 26238 }, { 25173, 25226 },
            { 24618, 24920 }, { 24580, 24916 }, { 24512, 24910 }, { 24514, 24911 }, { 24510, 24908 }, { 24581, 24919 },
            { 24507, 24905 }, { 24508, 24906 }, { 24509, 24907 }, { 25561, 25562 }, { 24579, 24914 }, { 37751, 37752 },
            { 33079, 59974 }, { 38426, 56952 }, { 51943, 57189 }
        };

        public const uint Thunderfoot = 24475;
        public const uint Octog       = 24486;
        public const uint Mordechai   = 24489;
        private const uint Kaskalak   = 33452;
        private const uint Jailor     = 24510;
        private const uint HaulingPlatform = 25365;
        private const uint SquirglingOffspring = 38426;
        private const uint RedmoonCrusher = 24618;

        private const uint EscortSpline   = 6904;
        private const uint PlatformSpline = 7037;
        private const float EscortSpeed   = 5f;

        private static readonly Vector3 CavernEntrance = new(-41.9f, -904.1f, -460.8f);   // WorldLocation2 18273, objectives 362 and 820
        private static readonly Vector3 Chasm          = new(35.4f, -842.3f, -921.4f);    // WorldLocation2 18109, objective 329
        private static readonly Vector3 PlatformStart  = new(-711.8f, -879.6f, -1000.7f); // WorldLocation2 18879, objective 372
        private static readonly Vector3 Terraformer    = new(-786.9f, -692.6f, -457.5f);  // WorldLocation2 18093, objectives 324 and 376

        protected override uint WorldId => 1263;
        protected override uint InitialPhase => (uint)Phase.Thunderfoot;
        protected override IReadOnlyDictionary<uint, uint> VeteranCreatures => Veteran;

        private IUnitEntity kaskalak;
        private List<Vector3> escortPath;
        private double platformTimer = -1d;

        public SkullcanoEventScript(
            IGlobalQuestManager globalQuestManager,
            ICreatureInfoManager creatureInfoManager,
            IEntityFactory entityFactory,
            IFactory<ISpellParameters> spellParametersFactory,
            IGameTableManager gameTableManager,
            ICinematicFactory cinematicFactory)
            : base(globalQuestManager, creatureInfoManager, entityFactory, spellParametersFactory, gameTableManager, cinematicFactory)
        {
        }

        protected override void OnPhase(uint phase)
        {
            switch ((Phase)phase)
            {
                case Phase.Thunderfoot:
                    Activate(Objective.KillThunderfoot);
                    Activate(Objective.StewShaman);
                    Activate(Objective.ShinyObjects);
                    Activate(Objective.Marauders);
                    Activate(Objective.FreeLopp);
                    Activate(Objective.Jailors);
                    Activate(Objective.LavaCores);
                    Activate(Objective.ClusterMissiles);
                    Activate(Objective.Quartermaster);
                    break;
                case Phase.FindKaskalak:
                    Activate(Objective.FindKaskalak);
                    SpawnKaskalak();
                    break;
                case Phase.TalkKaskalak:
                    Activate(Objective.TalkKaskalak);
                    break;
                case Phase.Escort:
                    Activate(Objective.Escort);
                    Activate(Objective.Chasm);
                    StartEscort();
                    break;
                case Phase.Octog:
                    Activate(Objective.KillOctog);
                    break;
                case Phase.Platform:
                    Activate(Objective.Platform);
                    break;
                case Phase.Cave:
                    Activate(Objective.Cave);
                    break;
                case Phase.Mordechai:
                    Activate(Objective.KillMordechai);
                    break;
            }
        }

        private void SpawnKaskalak()
        {
            SpawnCreature(Kaskalak, CavernEntrance, 0f, (map, guid, vector) => kaskalak = mapInstance.GetEntity<IUnitEntity>(guid));
        }

        /// <summary>
        /// Chief Kaskalak leads the way through the Molten Cavern along his path from the client data.
        /// </summary>
        private void StartEscort()
        {
            escortPath ??= GetSplineNodes(EscortSpline);
            if (kaskalak == null || !kaskalak.IsAlive || escortPath.Count == 0)
                return;

            Walk(kaskalak, escortPath, EscortSpeed);
        }

        protected override void OnObjectiveSucceeded(uint objectiveId, IPublicEventTeam team)
        {
            switch (objectiveId)
            {
                case Objective.KillThunderfoot:
                    publicEvent.SetPhase(Phase.FindKaskalak);
                    break;
                case Objective.FindKaskalak:
                    publicEvent.SetPhase(Phase.TalkKaskalak);
                    break;
                case Objective.TalkKaskalak:
                    publicEvent.SetPhase(Phase.Escort);
                    break;
                case Objective.Escort:
                case Objective.Chasm:
                    if (IsSucceeded(team, Objective.Escort, Objective.Chasm))
                        publicEvent.SetPhase(Phase.Octog);
                    break;
                case Objective.KillOctog:
                    publicEvent.SetPhase(Phase.Platform);
                    break;
                case Objective.Platform:
                    publicEvent.SetPhase(Phase.Cave);
                    break;
                case Objective.Cave:
                    publicEvent.SetPhase(Phase.Mordechai);
                    break;
                case Objective.KillMordechai:
                    publicEvent.SetPhase(Phase.Done);
                    // Mordechai Redmoon (Normal) and (Prime), Immortal: Skullcano (Prime) without deaths
                    CompleteExpedition([], [], 2226, 2227, 2239, true);
                    break;
            }
        }

        protected override void OnCheck()
        {
            switch ((Phase)publicEvent.Phase)
            {
                case Phase.FindKaskalak:
                    if (AnyPlayerNear(CavernEntrance, 25f))
                        Update(Objective.FindKaskalak);
                    break;
                case Phase.Escort:
                    if (escortPath?.Count > 0 && kaskalak != null && kaskalak.IsAlive
                        && Vector3.Distance(kaskalak.Position, escortPath[^1]) < 6f
                        && !IsObjectiveDone(Objective.Escort))
                        Update(Objective.Escort);
                    if (!IsObjectiveDone(Objective.Chasm) && AnyPlayerNear(Chasm, 60f))
                        Update(Objective.Chasm);
                    break;
                case Phase.Platform:
                    if (platformTimer < 0d && AnyPlayerNear(PlatformStart, 15f))
                        platformTimer = 5d;
                    break;
                case Phase.Cave:
                    if (AnyPlayerNear(Terraformer, 60f))
                        Update(Objective.Cave);
                    break;
            }
        }

        private bool IsObjectiveDone(uint objectiveId)
        {
            IPublicEventTeam team = publicEvent.GetTeams().FirstOrDefault();
            return team != null && IsSucceeded(team, objectiveId);
        }

        protected override void OnTick(double lastTick)
        {
            if (platformTimer < 0d || (Phase)publicEvent.Phase != Phase.Platform)
                return;

            platformTimer -= lastTick;
            if (platformTimer > 0d)
                return;

            platformTimer = double.MaxValue;
            // the Redmoon Hauling Platform carries the party up to the summit, the end of its path
            List<Vector3> path = GetSplineNodes(PlatformSpline);
            if (path.Count > 0)
            {
                foreach (IPlayer player in Players.Where(p => Vector3.Distance(p.Position, PlatformStart) < 25f).ToList())
                    player.TeleportToLocal(path[^1]);
                foreach (IUnitEntity platform in GetEntities<IUnitEntity>(HaulingPlatform))
                    Walk(platform, path, 8f);
            }
            Update(Objective.Platform);
        }

        public override void OnEntityDeath(IUnitEntity entity)
        {
            uint creatureId = entity.CreatureId;
            if (creatureId == GetCreature(Jailor))
                Update(Objective.Jailors);

            if (creatureId == GetCreature(Kaskalak) && (Phase)publicEvent.Phase == Phase.Escort)
            {
                // Kaskalak falls: he is back at the cavern entrance after a while and the escort starts over
                kaskalak = null;
                Delay(10d, () =>
                {
                    SpawnKaskalak();
                    Delay(2d, StartEscort);
                });
            }

            if (creatureId == GetCreature(Thunderfoot))
                GrantBossAchievement(2222, 2223);
            else if (creatureId == GetCreature(Octog))
                GrantBossAchievement(2224, 2225);
            else if (creatureId == GetCreature(Mordechai) && (Phase)publicEvent.Phase == Phase.Mordechai)
                Update(Objective.KillMordechai);
        }

        private void GrantBossAchievement(ushort normal, ushort prime)
        {
            ushort achievementId = IsVeteran ? prime : normal;
            foreach (IPlayer player in Players)
                if (!player.AchievementManager.HasCompletedAchievement(achievementId))
                    player.AchievementManager.GrantAchievement(achievementId);
        }

        /// <summary>
        /// Bosun Octog's squirgling offspring hatch around him (guessed: three at a time).
        /// </summary>
        public void OnOctogAdds(IWorldEntity octog)
        {
            SpawnAround(octog, [SquirglingOffspring, SquirglingOffspring, SquirglingOffspring], 10f);
        }

        /// <summary>
        /// Mordechai Redmoon calls his crew (guessed: two Redmoon Crushers).
        /// </summary>
        public void OnMordechaiAdds(IWorldEntity mordechai)
        {
            SpawnAround(mordechai, [RedmoonCrusher, RedmoonCrusher], 14f);
        }

        private void SpawnAround(IWorldEntity source, uint[] creatures, float radius)
        {
            for (int i = 0; i < creatures.Length; i++)
            {
                float angle = MathF.Tau / creatures.Length * i + Random.Shared.NextSingle();
                Vector3 position = source.Position + new Vector3(MathF.Cos(angle) * radius, 0f, MathF.Sin(angle) * radius);
                SpawnCreature(creatures[i], position, angle + MathF.PI, AggroNearestPlayer);
            }
        }
    }
}
