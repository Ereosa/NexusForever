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
    /// Kaskalak at the Molten Cavern and speak to him, escort him through the cavern and cross the Lava-Flooded Chasm, defeat Bosun Octog
    /// to reach the summit, gather on the Redmoon Hauling Platform, defeat Mordechai Redmoon. The optional objectives count through the
    /// generic kill and activation handling of their target groups, the jailors by their deaths.
    /// Guessed: the escort and the chasm complete once the party reaches Bosun Octog (Kaskalak's path through the cavern isn't in the
    /// data, he walks there on his own), the cavern, platform and summit checks are distances to the creatures there, no doors.
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

        protected override uint WorldId => 1263;
        protected override uint InitialPhase => (uint)Phase.Thunderfoot;
        protected override IReadOnlyDictionary<uint, uint> VeteranCreatures => Veteran;

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
                    break;
                case Phase.TalkKaskalak:
                    Activate(Objective.TalkKaskalak);
                    break;
                case Phase.Escort:
                    Activate(Objective.Escort);
                    Activate(Objective.Chasm);
                    // Chief Kaskalak heads through the Molten Cavern towards the summit
                    if (GetEntities<IUnitEntity>(Kaskalak).FirstOrDefault() is IUnitEntity chief
                        && GetEntities<IUnitEntity>(Octog).FirstOrDefault() is IUnitEntity octog)
                        Walk(chief, [Vector3.Lerp(chief.Position, octog.Position, 0.8f)], 4f);
                    break;
                case Phase.Octog:
                    Activate(Objective.KillOctog);
                    break;
                case Phase.Platform:
                    Activate(Objective.Platform);
                    break;
                case Phase.Mordechai:
                    Activate(Objective.KillMordechai);
                    break;
            }
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
                    if (NearCreature(Kaskalak, 25f))
                        Update(Objective.FindKaskalak);
                    break;
                case Phase.Escort:
                    if (NearCreature(Octog, 60f))
                    {
                        Update(Objective.Escort);
                        Update(Objective.Chasm);
                    }
                    break;
                case Phase.Platform:
                    if (NearCreature(HaulingPlatform, 15f) || NearCreature(Mordechai, 60f))
                        Update(Objective.Platform);
                    break;
            }
        }

        private bool NearCreature(uint creatureId, float range)
        {
            return GetEntities<IWorldEntity>(creatureId).Any(e => AnyPlayerNear(e.Position, range));
        }

        public override void OnEntityDeath(IUnitEntity entity)
        {
            uint creatureId = entity.CreatureId;
            if (creatureId == GetCreature(Jailor))
                Update(Objective.Jailors);

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
