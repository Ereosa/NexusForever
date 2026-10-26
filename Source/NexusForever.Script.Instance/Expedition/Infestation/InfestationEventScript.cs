using System.Numerics;
using NexusForever.Game.Abstract.Cinematic;
using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Entity.Creature;
using NexusForever.Game.Abstract.PublicEvent;
using NexusForever.Game.Abstract.Quest;
using NexusForever.Game.Abstract.Spell;
using NexusForever.Game.Static.Cinematic;
using NexusForever.Game.Static.PublicEvent;
using NexusForever.Game.Static.Spell;
using NexusForever.GameTable;
using NexusForever.Script.Template.Filter;
using NexusForever.Shared;

namespace NexusForever.Script.Instance.Expedition.Infestation
{
    /// <summary>
    /// Infestation (public event 95, world 1232): Captain Tolben's Blue Sparrow docked to the wrecked X-82 cargo ship.
    /// </summary>
    /// <remarks>
    /// No packet captures exist. The order follows Captain Tolben's lines and the objective data: board the X-82 and close the Blue Sparrow's
    /// vents, seal the hull breaches and rescue the shiphands caught in them ("with the hull sealed, I can unlock the doors to the med bay"),
    /// find the medical bay (veteran: defeat the lashing attack on it), find the medical supplies ("those creeps released some kind of toxic
    /// gas"), heal the contaminated shiphands and kill the lumbering parasites in the walls, kill the Cyclopean Parasite, then use the Blue
    /// Sparrow's control panel to depart ("you can activate the Blue Sparrow's control panel whenever you're ready to depart").
    /// Guessed: the vents alongside boarding, the shiphands succumbing three minutes after the breaches open (one beside each breach), six of
    /// the nine hull breach locations, healing by walking up to the contaminated shiphands once the supplies are found (they have no activate
    /// spell), the medbay attack waves, Captain Tolben's spot, the Cyclopean Parasite bursting onto the Blue Sparrow at the end (all its
    /// Jabbithole sightings are on the Blue Sparrow).
    /// </remarks>
    [ScriptFilterOwnerId(95)]
    public class InfestationEventScript : ExpeditionEventScript
    {
        private enum Phase : uint
        {
            Board,
            SealBreaches,
            FindMedbay,
            MedbayAttack,
            MedicalSupplies,
            Purge,
            KillCyclopean,
            Depart,
            Done
        }

        private static class Objective
        {
            public const uint CloseVents         = 228;
            public const uint KillParasites      = 229;
            public const uint MedicalSupplies    = 230;
            public const uint HealShiphands      = 231;
            public const uint Board              = 1005;
            public const uint RescueShiphands    = 1006;
            public const uint SealBreaches       = 1007;
            public const uint FindMedbay         = 1438;
            public const uint KillCyclopean      = 1500;
            public const uint MedbayAttack       = 4698;
            public const uint TagCargo           = 4699;
            public const uint GoldMedalTimer     = 4703;
        }

        private static class Text
        {
            public const uint DoesntLookGood   = 191300; // This doesn't look good, $(player.name).
            public const uint ToxicGas         = 192061; // Those creeps released some kind of toxic gas! ...
            public const uint Bulkheads        = 192063; // Wait - something's moving behind the bulkheads. ...
            public const uint HaveMeds         = 192064; // All right, we have meds! ...
            public const uint ReadyToDepart    = 192065; // Great job, $(player.name)! ... control panel whenever you're ready to depart.
            public const uint HullSealed       = 378348; // Blessed Navigator, that was a close one! With the hull sealed, I can unlock the doors to the med bay. ...
            public const uint TooFarGone       = 378359; // I appreciate you trying to help those men, but they were too far gone. ...
        }

        private static readonly Vector3 BoardingPoint = new(-0.239f, -499.993f, 87.626f);   // WorldLocation2 33174, objective 1005
        private static readonly Vector3 Medbay        = new(-34.459f, -522.662f, 69.110f);  // WorldLocation2 32902
        private static readonly Vector3 MedbayAttack  = new(-66.775f, -524.567f, 26.170f);  // WorldLocation2 16400, objective 4698
        private static readonly Vector3 MedbayVent    = new(-66.5f, -525.677f, 26.337f);    // vent prop in the medbay
        private static readonly Vector3 CyclopeanLair = new(-0.8f, -500f, 44.1f);          // densest Jabbithole sighting, on the Blue Sparrow
        // the densest Jabbithole sightings of the contaminated holdout shiphands and guards
        private static readonly Vector2[] HoldoutPoints = [new(-67.2f, 37.9f), new(-62.9f, 24.7f), new(-66.2f, 31.0f), new(-70.8f, 44.0f), new(-62.3f, 16.6f), new(-72.2f, 35.3f)];

        private static readonly Dictionary<uint, uint> Veteran = new()
        {
            { 22669, 69018 }, { 26833, 69019 }, { 36505, 69020 }, { 36506, 69021 }, { 41722, 69022 }, { 53820, 69023 },
            { 36852, 69089 }, { 72779, 72780 }, { 72781, 72782 }
        };

        // the 2015 veteran additions: the medbay holdout and the valuable cargo
        private static readonly uint[] VeteranOnly = [69870, 69871, 69877];

        private const uint Tolben              = 71764;
        private const uint LumberingParasite   = 22669;
        private const uint LumberingBartlett   = 41722;
        private const uint CyclopeanParasite   = 26833;
        private const uint HullBreach          = 36844;
        private const uint BreachVictim        = 36852;
        private const uint ContaminatedShiphand = 23837;
        private const uint HealedShiphand      = 22490;
        private const uint ControlPanel        = 22662;
        private const uint ExpeditionDoor      = 22494;
        private const uint LashingSpawn        = 69870;
        private const uint LashingFiend        = 69871;
        private const uint HoldoutShiphand     = 69803;
        private const uint HoldoutGuard        = 69873;

        private const double VictimTime = 180d;

        protected override uint WorldId => 1232;
        protected override uint InitialPhase => (uint)Phase.Board;
        protected override IReadOnlyDictionary<uint, uint> VeteranCreatures => Veteran;
        protected override IReadOnlyCollection<uint> VeteranOnlyCreatures => VeteranOnly;

        private readonly HashSet<uint> savedVictims = [];
        private readonly HashSet<uint> healed = [];
        private bool victimsLost;
        private double breachTimer;
        private int attackWaves;
        private readonly List<IWorldEntity> attackers = [];
        private IWorldEntity fiend;

        public InfestationEventScript(
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
                case Phase.Board:
                    Activate(Objective.Board);
                    Activate(Objective.CloseVents);
                    AddTrigger(2742, BoardingPoint, 5f);
                    if (IsVeteran)
                    {
                        Activate(Objective.TagCargo);
                        Activate(Objective.GoldMedalTimer);
                    }
                    Delay(5d, () => Yell(Tolben, Text.DoesntLookGood));
                    break;
                case Phase.SealBreaches:
                    Activate(Objective.SealBreaches);
                    Activate(Objective.RescueShiphands);
                    breachTimer = VictimTime;
                    break;
                case Phase.FindMedbay:
                    Yell(Tolben, Text.HullSealed);
                    // the doors between the cargo bay, the corridor and the medical bay unlock
                    foreach (IDoorEntity door in GetEntities<IDoorEntity>(ExpeditionDoor))
                        door.OpenDoor();
                    Activate(Objective.FindMedbay);
                    AddTrigger(4899, Medbay, 16f);
                    break;
                case Phase.MedbayAttack:
                    Activate(Objective.MedbayAttack);
                    SpawnAttackWave();
                    break;
                case Phase.MedicalSupplies:
                    Yell(Tolben, Text.ToxicGas);
                    Activate(Objective.MedicalSupplies);
                    break;
                case Phase.Purge:
                    Yell(Tolben, Text.HaveMeds);
                    Activate(Objective.HealShiphands, (uint)Math.Max(1, GetEntities<IUnitEntity>(ContaminatedShiphand).Count()));
                    Delay(8d, () =>
                    {
                        Yell(Tolben, Text.Bulkheads);
                        Activate(Objective.KillParasites, (uint)Math.Max(1, Parasites().Count()));
                        if (!Parasites().Any())
                            Update(Objective.KillParasites);
                    });
                    break;
                case Phase.KillCyclopean:
                    Activate(Objective.KillCyclopean);
                    SpawnCreature(CyclopeanParasite, Surface(CyclopeanLair.X, CyclopeanLair.Z, CyclopeanLair.Y), MathF.PI, AggroNearestPlayer);
                    break;
                case Phase.Depart:
                    Yell(Tolben, Text.ReadyToDepart);
                    break;
            }
        }

        private IEnumerable<IUnitEntity> Parasites()
        {
            return GetEntities<IUnitEntity>(LumberingParasite)
                .Concat(GetEntities<IUnitEntity>(LumberingBartlett))
                .Where(u => u.IsAlive);
        }

        /// <summary>
        /// Veteran: contaminated shiphands and guards storm the medbay with lashing spawn from its vent, three waves, the lashing fiend comes
        /// with the last one.
        /// </summary>
        private void SpawnAttackWave()
        {
            attackWaves++;
            uint[] creatures = [HoldoutShiphand, HoldoutGuard, LashingSpawn, HoldoutShiphand, LashingSpawn, HoldoutGuard];
            for (int i = 0; i < creatures.Length; i++)
            {
                Vector2 point = HoldoutPoints[i % HoldoutPoints.Length];
                IWorldEntity spawn = SpawnCreature(creatures[i], creatures[i] == LashingSpawn ? MedbayVent : Surface(point.X, point.Y, MedbayAttack.Y), 0f, AggroNearestPlayer);
                if (spawn != null)
                    attackers.Add(spawn);
            }

            if (attackWaves == 3)
                fiend = SpawnCreature(LashingFiend, MedbayVent, 0f, AggroNearestPlayer);
            else
                Delay(20d, SpawnAttackWave);
        }

        protected override void OnObjectiveSucceeded(uint objectiveId, IPublicEventTeam team)
        {
            switch (objectiveId)
            {
                case Objective.Board:
                case Objective.CloseVents:
                    if (IsSucceeded(team, Objective.Board, Objective.CloseVents))
                        publicEvent.SetPhase(Phase.SealBreaches);
                    break;
                case Objective.SealBreaches:
                    if (!victimsLost)
                        Update(Objective.RescueShiphands);
                    publicEvent.SetPhase(Phase.FindMedbay);
                    break;
                case Objective.FindMedbay:
                    publicEvent.SetPhase(IsVeteran ? Phase.MedbayAttack : Phase.MedicalSupplies);
                    break;
                case Objective.MedbayAttack:
                    publicEvent.SetPhase(Phase.MedicalSupplies);
                    break;
                case Objective.MedicalSupplies:
                    publicEvent.SetPhase(Phase.Purge);
                    break;
                case Objective.KillParasites:
                    publicEvent.SetPhase(Phase.KillCyclopean);
                    break;
                case Objective.KillCyclopean:
                    publicEvent.SetPhase(Phase.Depart);
                    break;
            }
        }

        public override void OnEntityDeath(IUnitEntity entity)
        {
            switch ((Phase)publicEvent.Phase)
            {
                case Phase.Purge when entity.CreatureId == GetCreature(LumberingParasite) || entity.CreatureId == GetCreature(LumberingBartlett):
                    Update(Objective.KillParasites);
                    break;
                case Phase.KillCyclopean when entity.CreatureId == GetCreature(CyclopeanParasite):
                    Update(Objective.KillCyclopean);
                    break;
            }

            // the lashing fiend is the medbay objective's target group, its death completes it
            if (entity == fiend)
                fiend = null;
        }

        public override void OnEntityActivated(IWorldEntity entity, IPlayer activator)
        {
            if (entity.CreatureId == HullBreach && (Phase)publicEvent.Phase == Phase.SealBreaches)
            {
                // the shiphand caught in this breach is safe
                IUnitEntity victim = GetEntities<IUnitEntity>(BreachVictim)
                    .Where(v => v.IsAlive)
                    .OrderBy(v => Vector3.DistanceSquared(v.Position, entity.Position))
                    .FirstOrDefault();
                if (victim != null && Vector3.Distance(victim.Position, entity.Position) < 15f)
                    savedVictims.Add(victim.Guid);
                return;
            }

            if (entity.CreatureId == ControlPanel && (Phase)publicEvent.Phase == Phase.Depart)
            {
                publicEvent.SetPhase(Phase.Done);
                PlayCinematic(CinematicSceneId.InfestationDeparture);
                Complete();
            }
        }

        private void Complete()
        {
            IPublicEventTeam team = publicEvent.GetTeams().FirstOrDefault(t => t.Team == PublicEventTeam.PublicTeam);
            bool gold = IsVeteran && team != null && !IsFailed(team, Objective.GoldMedalTimer)
                && IsSucceeded(team, Objective.TagCargo, Objective.RescueShiphands);

            // Expedition: Infestation, the prime dailies; Infestation complete, Prime, Gold Medal (Prime)
            CompleteExpedition([19346], [18934, 18935], 3714, 6023, 6007, gold);
        }

        protected override void OnTick(double lastTick)
        {
            if ((Phase)publicEvent.Phase != Phase.SealBreaches || victimsLost || breachTimer <= 0d)
                return;

            breachTimer -= lastTick;
            if (breachTimer > 0d)
                return;

            // the shiphands still caught in open breaches succumb
            foreach (IUnitEntity victim in GetEntities<IUnitEntity>(BreachVictim).Where(v => v.IsAlive && !savedVictims.Contains(v.Guid)).ToList())
            {
                victimsLost = true;
                victim.ModifyHealth(victim.Health, DamageType.Physical, null);
            }

            if (victimsLost)
                Yell(Tolben, Text.TooFarGone);
        }

        protected override void OnCheck()
        {
            if ((Phase)publicEvent.Phase != Phase.Purge)
                return;

            // the medicine is used on the contaminated shiphands the players reach
            foreach (IUnitEntity shiphand in GetEntities<IUnitEntity>(ContaminatedShiphand).ToList())
            {
                if (healed.Contains(shiphand.Guid) || !AnyPlayerNear(shiphand.Position, 4f))
                    continue;

                healed.Add(shiphand.Guid);
                Vector3 position = shiphand.Position;
                float yaw = shiphand.Rotation.X;
                shiphand.RemoveFromMap();
                SpawnCreature(HealedShiphand, position, yaw);
                Update(Objective.HealShiphands);
            }
        }
    }
}
