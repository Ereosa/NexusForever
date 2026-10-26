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

namespace NexusForever.Script.Instance.Dungeon.KelVoreth
{
    /// <summary>
    /// Ruins of Kel Voreth (public event 161, world 1336): the Osun of Kel Voreth forging an armageddon.
    /// </summary>
    /// <remarks>
    /// No packet captures exist. The order follows the objective data: fight through the Blood Pit, defeat Grond the Corpsemaker, then
    /// Slavemaster Drokk, then Forgemaster Trogun. The optional objectives (Battlesworn and Darkwitches, Mechano Slavers and their
    /// constructs, slaves, war supplies, Darkwitch Gurka, data storages, Exo-Lab defenses, forges) count through the generic kill and
    /// activation handling of their target groups.
    /// Guessed: the Blood Pit being its first three champions killed (gladiators and necromancers, the objective's target group is a
    /// faction group the server doesn't evaluate), the boss adds (Grond's skeletons, Drokk's enslaved constructs, Trogun's Spheres of
    /// Primal Fire) and their timing, no doors.
    /// </remarks>
    [ScriptFilterOwnerId(161)]
    public class KelVorethEventScript : ExpeditionEventScript
    {
        private enum Phase : uint
        {
            BloodPit,
            Grond,
            Drokk,
            Trogun,
            Done
        }

        private static class Objective
        {
            public const uint KillGrond         = 444;
            public const uint BloodPit          = 445;
            public const uint KillDrokk         = 446;
            public const uint Battlesworn       = 447;
            public const uint MechanoSlavers    = 448;
            public const uint KillTrogun        = 449;
            public const uint Slaves            = 450;
            public const uint WarSupplies       = 451;
            public const uint DarkwitchGurka    = 453;
            public const uint DataStorage       = 456;
            public const uint ExoLabDefenses    = 459;
            public const uint Forges            = 461;
        }

        private static readonly Dictionary<uint, uint> Veteran = new()
        {
            { 51068, 51069 }, { 50991, 50992 }, { 37200, 56575 }, { 33837, 56485 }, { 35538, 56497 }, { 35424, 56496 },
            { 37113, 37114 }, { 37791, 37792 }, { 37110, 37111 }, { 35418, 35419 }, { 35535, 35536 }, { 51422, 67360 },
            { 52572, 67362 }, { 52573, 67364 }, { 32555, 32556 }, { 32606, 32607 }, { 32614, 32613 }, { 32618, 32619 },
            { 51453, 51454 }, { 32547, 32549 }, { 51432, 51433 }, { 32727, 32726 }, { 33049, 33050 },
            { 32534, 32535 }, { 32536, 32539 }, { 32531, 32533 }
        };

        public const uint Grond  = 32534;
        public const uint Drokk  = 32536;
        public const uint Trogun = 32531;
        private const uint BloodPitSkeleton  = 50998;
        private const uint VorethNecromancer = 51432;
        private const uint EnslavedProbe     = 37791;
        private const uint EnslavedProtector = 37110;
        private const uint SphereOfPrimalFire = 53854;

        // the Blood Pit's champions, normal and veteran
        private static readonly HashSet<uint> BloodPitChampions = [51068, 51069, 50991, 50992, 51453, 51454];

        protected override uint WorldId => 1336;
        protected override uint InitialPhase => (uint)Phase.BloodPit;
        protected override IReadOnlyDictionary<uint, uint> VeteranCreatures => Veteran;

        public KelVorethEventScript(
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
                case Phase.BloodPit:
                    Activate(Objective.BloodPit);
                    Activate(Objective.Battlesworn);
                    Activate(Objective.MechanoSlavers);
                    Activate(Objective.Slaves);
                    Activate(Objective.WarSupplies);
                    Activate(Objective.DarkwitchGurka);
                    Activate(Objective.DataStorage);
                    Activate(Objective.ExoLabDefenses);
                    Activate(Objective.Forges);
                    break;
                case Phase.Grond:
                    Activate(Objective.KillGrond);
                    break;
                case Phase.Drokk:
                    Activate(Objective.KillDrokk);
                    break;
                case Phase.Trogun:
                    Activate(Objective.KillTrogun);
                    break;
            }
        }

        protected override void OnObjectiveSucceeded(uint objectiveId, IPublicEventTeam team)
        {
            switch (objectiveId)
            {
                case Objective.BloodPit:
                    publicEvent.SetPhase(Phase.Grond);
                    break;
                case Objective.KillGrond:
                    publicEvent.SetPhase(Phase.Drokk);
                    break;
                case Objective.KillDrokk:
                    publicEvent.SetPhase(Phase.Trogun);
                    break;
                case Objective.KillTrogun:
                    publicEvent.SetPhase(Phase.Done);
                    // Forgemaster Trogun (Normal) and (Prime), Immortal: Ruins of Kel Voreth (Prime) without deaths
                    CompleteExpedition([], [], 2246, 2247, 2255, true);
                    break;
            }
        }

        public override void OnEntityDeath(IUnitEntity entity)
        {
            uint creatureId = entity.CreatureId;
            if ((Phase)publicEvent.Phase == Phase.BloodPit && BloodPitChampions.Contains(creatureId))
                Update(Objective.BloodPit);

            if (creatureId == GetCreature(Grond))
                GrantBossAchievement(2242, 2243);
            else if (creatureId == GetCreature(Drokk))
                GrantBossAchievement(2244, 2245);
            else if (creatureId == GetCreature(Trogun) && (Phase)publicEvent.Phase == Phase.Trogun)
                Update(Objective.KillTrogun);
        }

        private void GrantBossAchievement(ushort normal, ushort prime)
        {
            ushort achievementId = IsVeteran ? prime : normal;
            foreach (IPlayer player in Players)
                if (!player.AchievementManager.HasCompletedAchievement(achievementId))
                    player.AchievementManager.GrantAchievement(achievementId);
        }

        /// <summary>
        /// Grond the Corpsemaker raises the dead of the Blood Pit (guessed: skeletons and a necromancer).
        /// </summary>
        public void OnGrondAdds(IWorldEntity grond)
        {
            SpawnAround(grond, [BloodPitSkeleton, BloodPitSkeleton, VorethNecromancer], 12f);
        }

        /// <summary>
        /// Slavemaster Drokk calls his enslaved constructs (guessed).
        /// </summary>
        public void OnDrokkAdds(IWorldEntity drokk)
        {
            SpawnAround(drokk, [EnslavedProbe, EnslavedProbe, EnslavedProtector], 12f);
        }

        /// <summary>
        /// Forgemaster Trogun's forge sends out Spheres of Primal Fire that roll towards him (guessed: three at a time).
        /// </summary>
        public void OnTrogunSpheres(IWorldEntity trogun)
        {
            for (int i = 0; i < 3; i++)
            {
                float angle = MathF.Tau / 3f * i + Random.Shared.NextSingle();
                Vector3 start = trogun.Position + new Vector3(MathF.Cos(angle) * 20f, 0f, MathF.Sin(angle) * 20f);
                Vector3 target = trogun.Position;
                SpawnCreature(SphereOfPrimalFire, start, angle + MathF.PI, (map, guid, vector) =>
                {
                    if (mapInstance.GetEntity<IUnitEntity>(guid) is IUnitEntity unit)
                        Walk(unit, [target], 3f);
                });
            }
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
