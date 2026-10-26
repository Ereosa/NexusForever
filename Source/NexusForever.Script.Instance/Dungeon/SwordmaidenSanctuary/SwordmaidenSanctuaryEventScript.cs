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

namespace NexusForever.Script.Instance.Dungeon.SwordmaidenSanctuary
{
    /// <summary>
    /// Sanctuary of the Swordmaiden (public event 166, world 1271): the Sanctuary Defiled.
    /// </summary>
    /// <remarks>
    /// No packet captures exist. The order follows the objective data: defeat Deadringer Shallaos, then the three wings with Ondu
    /// Lifeweaver, Moldwood Overlord Skash and Rayna Darkspeaker, collect the Torine Spirit-Relics of the wings and place them at the
    /// center, escort Spiritmother Selene into the Sanctum, defeat Spiritmother Selene the Corrupted. Optional objectives count through
    /// the generic kill and activation handling of their target groups.
    /// After the relics, Spiritmother Selene's Echo appears where the relics are placed (WorldLocation2 22127, objectives 489/678 of the
    /// Spiritual Revival, public event 202) and the party defends her while she re-manifests; Spiritmother Selene then walks into the
    /// Sanctum along the client's Spline2 8761 and 8762, which run from there to the Sanctum.
    /// Guessed: each wing's Spirit-Relic is collected when its boss falls and the three are placed together once collected (the relics
    /// are door entities whose activation isn't known), the wing turnstiles complete by reaching the wing's boss, the holdout's attackers
    /// (corrupted Torine and Moldwood of the sanctuary, four waves 20 seconds apart, started by coming near the Echo), Selene's walking
    /// speed, no doors (their positions aren't in the client data).
    /// </remarks>
    [ScriptFilterOwnerId(166)]
    public class SwordmaidenSanctuaryEventScript : ExpeditionEventScript
    {
        private enum Phase : uint
        {
            Shallaos,
            Wings,
            Relics,
            Revival,
            Escort,
            Selene,
            Done
        }

        private static class Objective
        {
            public const uint KillShallaos   = 479;
            public const uint KillOndu       = 480;
            public const uint CollectRelics  = 481;
            public const uint PlaceRelics    = 482;
            public const uint KillSkash      = 483;
            public const uint KillRayna      = 486;
            public const uint KillSelene     = 492;
            public const uint EnterTerrace   = 613;
            public const uint ReachMoldwood  = 614;
            public const uint EnterTemple    = 615;
            public const uint EscortSelene   = 662;

            public static readonly uint[] Optional = [493, 494, 495, 496, 499, 500, 501, 502, 503, 504, 505, 627, 629, 630, 639, 641, 1366];
        }

        private static readonly Dictionary<uint, uint> Veteran = new()
        {
            { 41369, 70969 }, { 43493, 57007 }, { 28995, 28996 }, { 29248, 29249 }, { 28985, 28986 }, { 28992, 28993 }, { 41214, 41215 },
            { 28801, 28829 }, { 28600, 28599 }, { 29198, 29199 }, { 29254, 29255 }, { 40402, 40404 }, { 41219, 41220 }, { 28774, 28775 },
            { 28665, 28666 }, { 28795, 28796 }, { 28780, 28781 }, { 43138, 43139 }, { 40436, 70950 }, { 29804, 29805 }, { 40393, 40396 },
            { 41334, 71130 }, { 28721, 28720 }, { 28660, 28661 }, { 28735, 28736 }, { 43372, 57067 }, { 28580, 28585 }, { 28612, 28613 },
            { 28646, 28647 }, { 41203, 41204 }, { 29303, 29304 }, { 29251, 29252 }, { 29222, 29223 }, { 30208, 30209 }, { 29310, 29311 },
            { 28727, 28728 }, { 28929, 28930 }, { 28878, 28892 }, { 28733, 28732 }, { 41238, 56936 }
        };

        public const uint Shallaos = 28600;
        public const uint Ondu     = 28721;
        public const uint Skash    = 28727;
        public const uint Rayna    = 28733;
        public const uint Selene   = 28735;
        private const uint SeleneEcho     = 29562;
        private const uint SpiritmotherSelene = 29479;
        private static readonly uint[] RevivalAttackers = [29303, 29222, 28929, 29310];

        private static readonly Vector3 RelicCenter = new(4164.6f, -787.2f, -3428.1f); // WorldLocation2 22127, Spiritual Revival
        private static readonly uint[] SanctumSplines = [8761, 8762];

        private IUnitEntity echo;
        private IUnitEntity spiritmother;
        private bool revivalStarted;
        private int revivalWaves;
        private readonly List<IUnitEntity> revivalAttackers = [];

        protected override uint WorldId => 1271;
        protected override uint InitialPhase => (uint)Phase.Shallaos;
        protected override IReadOnlyDictionary<uint, uint> VeteranCreatures => Veteran;

        public SwordmaidenSanctuaryEventScript(
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
                case Phase.Shallaos:
                    Activate(Objective.KillShallaos);
                    foreach (uint objectiveId in Objective.Optional)
                        Activate(objectiveId);
                    break;
                case Phase.Wings:
                    Activate(Objective.KillOndu);
                    Activate(Objective.KillSkash);
                    Activate(Objective.KillRayna);
                    Activate(Objective.CollectRelics);
                    Activate(Objective.EnterTerrace);
                    Activate(Objective.ReachMoldwood);
                    Activate(Objective.EnterTemple);
                    break;
                case Phase.Relics:
                    Activate(Objective.PlaceRelics);
                    Delay(5d, () => Update(Objective.PlaceRelics, 3));
                    break;
                case Phase.Revival:
                    SpawnCreature(SeleneEcho, RelicCenter, 0f, (map, guid, vector) => echo = mapInstance.GetEntity<IUnitEntity>(guid));
                    break;
                case Phase.Escort:
                    Activate(Objective.EscortSelene);
                    echo?.RemoveFromMap();
                    SpawnCreature(SpiritmotherSelene, RelicCenter, 0f, (map, guid, vector) =>
                    {
                        spiritmother = mapInstance.GetEntity<IUnitEntity>(guid);
                        List<Vector3> path = SanctumSplines.SelectMany(GetSplineNodes).ToList();
                        if (spiritmother != null && path.Count > 0)
                            Walk(spiritmother, path, 3f);
                    });
                    break;
                case Phase.Selene:
                    Activate(Objective.KillSelene);
                    break;
            }
        }

        protected override void OnObjectiveSucceeded(uint objectiveId, IPublicEventTeam team)
        {
            switch (objectiveId)
            {
                case Objective.KillShallaos:
                    publicEvent.SetPhase(Phase.Wings);
                    break;
                case Objective.KillOndu:
                case Objective.KillSkash:
                case Objective.KillRayna:
                case Objective.CollectRelics:
                    if (IsSucceeded(team, Objective.KillOndu, Objective.KillSkash, Objective.KillRayna, Objective.CollectRelics))
                        publicEvent.SetPhase(Phase.Relics);
                    break;
                case Objective.PlaceRelics:
                    publicEvent.SetPhase(Phase.Revival);
                    break;
                case Objective.EscortSelene:
                    publicEvent.SetPhase(Phase.Selene);
                    break;
                case Objective.KillSelene:
                    publicEvent.SetPhase(Phase.Done);
                    // Spiritmother Selene (Normal) and (Prime), Immortal: Sanctuary of the Swordmaiden (Prime) without deaths
                    CompleteExpedition([], [], 2713, 2714, 2733, true);
                    break;
            }
        }

        protected override void OnCheck()
        {
            switch ((Phase)publicEvent.Phase)
            {
                case Phase.Wings:
                    // the wings: Lifeweaver Terrace (Ondu), the Moldwood Corruption (Skash), the Temple of the Life-Speaker (Rayna)
                    CheckTurnstile(Objective.EnterTerrace, Ondu);
                    CheckTurnstile(Objective.ReachMoldwood, Skash);
                    CheckTurnstile(Objective.EnterTemple, Rayna);
                    break;
                case Phase.Revival:
                    if (!revivalStarted && echo != null && AnyPlayerNear(echo.Position, 10f))
                    {
                        revivalStarted = true;
                        SpawnRevivalWave();
                    }
                    else if (revivalStarted && revivalWaves >= 4 && revivalAttackers.All(a => !a.IsAlive || a.Map == null))
                        publicEvent.SetPhase(Phase.Escort);
                    break;
                case Phase.Escort:
                    List<Vector3> end = GetSplineNodes(SanctumSplines[^1]);
                    if (spiritmother != null && end.Count > 0 && Vector3.Distance(spiritmother.Position, end[^1]) < 6f)
                    {
                        spiritmother.RemoveFromMap();
                        spiritmother = null;
                        Update(Objective.EscortSelene);
                    }
                    break;
            }
        }

        /// <summary>
        /// The Strain's corrupted come for Selene's Echo while she re-manifests: four waves 20 seconds apart.
        /// </summary>
        private void SpawnRevivalWave()
        {
            revivalWaves++;
            for (int i = 0; i < RevivalAttackers.Length; i++)
            {
                float angle = MathF.Tau / RevivalAttackers.Length * i + revivalWaves;
                Vector3 position = Surface(RelicCenter.X + MathF.Cos(angle) * 22f, RelicCenter.Z + MathF.Sin(angle) * 22f, RelicCenter.Y);
                SpawnCreature(RevivalAttackers[i], position, angle + MathF.PI, (map, guid, vector) =>
                {
                    if (mapInstance.GetEntity<IUnitEntity>(guid) is IUnitEntity unit)
                        revivalAttackers.Add(unit);
                    AggroNearestPlayer(map, guid, vector);
                });
            }

            if (revivalWaves < 4)
                Delay(20d, SpawnRevivalWave);
        }

        private readonly HashSet<uint> turnstiles = [];

        private void CheckTurnstile(uint objectiveId, uint bossId)
        {
            if (turnstiles.Contains(objectiveId))
                return;
            if (!GetEntities<IWorldEntity>(bossId).Any(e => AnyPlayerNear(e.Position, 60f)))
                return;

            turnstiles.Add(objectiveId);
            Update(objectiveId);
        }

        public override void OnEntityDeath(IUnitEntity entity)
        {
            uint creatureId = entity.CreatureId;
            if (creatureId == GetCreature(Shallaos))
                GrantBossAchievement(2717, 2718);
            else if (creatureId == GetCreature(Ondu))
                OnWingBoss(2715, 2716);
            else if (creatureId == GetCreature(Skash))
                OnWingBoss(2719, 2720);
            else if (creatureId == GetCreature(Rayna))
                OnWingBoss(2711, 2712);
            else if (creatureId == GetCreature(Selene) && (Phase)publicEvent.Phase == Phase.Selene)
                Update(Objective.KillSelene);
        }

        /// <summary>
        /// A wing's boss fell: its Spirit-Relic is collected.
        /// </summary>
        private void OnWingBoss(ushort normal, ushort prime)
        {
            GrantBossAchievement(normal, prime);
            if ((Phase)publicEvent.Phase == Phase.Wings)
                Update(Objective.CollectRelics);
        }

        private void GrantBossAchievement(ushort normal, ushort prime)
        {
            ushort achievementId = IsVeteran ? prime : normal;
            foreach (IPlayer player in Players)
                if (!player.AchievementManager.HasCompletedAchievement(achievementId))
                    player.AchievementManager.GrantAchievement(achievementId);
        }
    }
}
