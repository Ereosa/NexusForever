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
            if (bombshells.Count > 0 && creatureId == GetCreature(BombshellConstruct) && bombshells.All(b => b == entity || !b.IsAlive))
            {
                bombshells.Clear();
                if (mapInstance.GetEntity<IUnitEntity>(drokkGuid) is IUnitEntity drokk)
                    drokk.InvokeScriptCollection<Script.KelVorethDrokkAI>(s => s.OnBombshellsDestroyed());
            }

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

        private const uint BloodsuckerFlea    = 33837;
        private const uint BombshellConstruct = 35538;
        private const uint DestructoConstruct = 35424;
        private const uint ExaniteShard       = 51370;
        private const uint TrackingBeacon     = 63237;
        private readonly List<IUnitEntity> bombshells = [];
        private uint drokkGuid;

        /// <summary>
        /// "Grond has a nasty itch!": four Bloodsucker Fleas go after random players other than his target; they burst after 20 seconds
        /// (retail: kited into the traps; the timing is a guess).
        /// </summary>
        public void OnGrondItch(IUnitEntity grond)
        {
            List<IPlayer> targets = Players.Where(p => p.IsAlive && p.Guid != grond.TargetGuid).ToList();
            if (targets.Count == 0)
                targets = Players.Where(p => p.IsAlive).ToList();
            if (targets.Count == 0)
                return;

            for (int i = 0; i < 4; i++)
            {
                IPlayer target = targets[Random.Shared.Next(targets.Count)];
                float angle = MathF.Tau / 4f * i;
                Vector3 position = grond.Position + new Vector3(MathF.Cos(angle) * 4f, 0f, MathF.Sin(angle) * 4f);
                SpawnCreature(BloodsuckerFlea, position, angle, (map, guid, vector) =>
                {
                    if (mapInstance.GetEntity<IUnitEntity>(guid) is not IUnitEntity flea)
                        return;
                    flea.ThreatManager.UpdateThreat(target, 1000);
                    Delay(20d, () => flea.RemoveFromMap());
                });
            }
        }

        /// <summary>
        /// Slavemaster Drokk vanishes at 83%: Bombshell Constructs rush out, he returns once they are destroyed.
        /// </summary>
        public void OnDrokkBombshells(IUnitEntity drokk)
        {
            drokkGuid = drokk.Guid;
            bombshells.Clear();
            for (int i = 0; i < 4; i++)
            {
                float angle = MathF.Tau / 4f * i;
                Vector3 position = drokk.Position + new Vector3(MathF.Cos(angle) * 6f, 0f, MathF.Sin(angle) * 6f);
                SpawnCreature(BombshellConstruct, position, angle, (map, guid, vector) =>
                {
                    if (mapInstance.GetEntity<IUnitEntity>(guid) is IUnitEntity bombshell)
                        bombshells.Add(bombshell);
                    AggroNearestPlayer(map, guid, vector);
                });
            }
        }

        /// <summary>
        /// The chase: three times a player gets the Tracking Beacon and Destructo Constructs (with a Bombshell in the last chase) go
        /// after them, 15 seconds apart.
        /// </summary>
        public void OnDrokkChase(IUnitEntity drokk, bool withBombshells)
        {
            for (int beacon = 0; beacon < 3; beacon++)
            {
                Delay(beacon * 15d, () =>
                {
                    if (!drokk.IsAlive)
                        return;
                    List<IPlayer> targets = Players.Where(p => p.IsAlive).ToList();
                    if (targets.Count == 0)
                        return;

                    IPlayer target = targets[Random.Shared.Next(targets.Count)];
                    CastOn(drokk, TrackingBeacon, target);
                    uint[] constructs = withBombshells
                        ? [DestructoConstruct, DestructoConstruct, BombshellConstruct]
                        : [DestructoConstruct, DestructoConstruct, DestructoConstruct];
                    for (int i = 0; i < constructs.Length; i++)
                    {
                        Vector3 position = drokk.Position + new Vector3((i - 1) * 3f, 0f, 3f);
                        SpawnCreature(constructs[i], position, 0f, (map, guid, vector) =>
                        {
                            if (mapInstance.GetEntity<IUnitEntity>(guid) is IUnitEntity construct)
                                construct.ThreatManager.UpdateThreat(target, 1000);
                        });
                    }
                });
            }
        }

        /// <summary>
        /// Forgemaster's Call: Spheres of Primal Fire roll towards Trogun from around the room (three, five with the buzzsaws).
        /// </summary>
        public void OnTrogunSpheres(IWorldEntity trogun, bool buzzsaws)
        {
            int count = buzzsaws ? 5 : 3;
            for (int i = 0; i < count; i++)
            {
                float angle = MathF.Tau / count * i + Random.Shared.NextSingle();
                Vector3 start = trogun.Position + new Vector3(MathF.Cos(angle) * 22f, 0f, MathF.Sin(angle) * 22f);
                Vector3 target = trogun.Position;
                SpawnCreature(SphereOfPrimalFire, start, angle + MathF.PI, (map, guid, vector) =>
                {
                    if (mapInstance.GetEntity<IUnitEntity>(guid) is IUnitEntity unit)
                        Walk(unit, [target], 3f);
                });
            }

            if (buzzsaws)
                OnTrogunBuzzsaws(trogun);
        }

        /// <summary>
        /// Buzzsaws (Exanite Shards) roll from Trogun's forge across the room.
        /// </summary>
        public void OnTrogunBuzzsaws(IWorldEntity trogun)
        {
            for (int i = 0; i < 4; i++)
            {
                Vector3 start = trogun.Position + new Vector3((i - 1.5f) * 8f, 0f, 0f);
                Vector3 end = start + new Vector3(0f, 0f, -40f);
                Delay(i * 2d, () => SpawnCreature(ExaniteShard, start, 0f, (map, guid, vector) =>
                {
                    if (mapInstance.GetEntity<IUnitEntity>(guid) is not IUnitEntity saw)
                        return;
                    Walk(saw, [end], 10f);
                    Delay(6d, () => saw.RemoveFromMap());
                }));
            }
        }

        private void CastOn(IUnitEntity caster, uint spellId, IUnitEntity target)
        {
            ISpellParameters parameters = spellParametersFactory.Resolve();
            parameters.PrimaryTargetId = target.Guid;
            caster.CastSpell(spellId, parameters);
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
