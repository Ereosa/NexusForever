using System.Numerics;
using NexusForever.Game.Abstract.Combat;
using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Entity.Creature;
using NexusForever.Game.Abstract.Entity.Movement;
using NexusForever.Game.Abstract.Entity.Movement.Force;
using NexusForever.Game.Abstract.Entity.Stat;
using NexusForever.Game.Abstract.Map;
using NexusForever.Game.Abstract.Spell;
using NexusForever.Game.Abstract.Spell.Effect.Data;
using NexusForever.Game.Abstract.Spell.Info;
using NexusForever.Game.Map.Search;
using NexusForever.Game.Spell;
using NexusForever.Game.Static.Entity;
using NexusForever.Game.Static.Entity.Movement.Spline;
using NexusForever.Game.Static.Pet;
using NexusForever.Game.Static.Reputation;
using NexusForever.GameTable;
using NexusForever.GameTable.Model;
using NexusForever.Network.World.Entity;
using NexusForever.Network.World.Entity.Model;
using NexusForever.Shared;
using NexusForever.Shared.Game;
using NLog;

namespace NexusForever.Game.Entity
{
    /// <summary>
    /// A pet summoned by the SummonPet spell effect that follows its owner and fights according to its <see cref="PetStance"/>.
    /// </summary>
    public class CombatPetEntity : UnitEntity, ICombatPetEntity
    {
        private static readonly ILogger log = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Distance behind the owner the pet stands when following.
        /// </summary>
        private const float FollowDistance = 3f;

        /// <summary>
        /// Angle either side of directly behind the owner, each pet has its own follow slot so two bots don't stand on each other.
        /// </summary>
        private const float FollowSlotAngle = MathF.PI / 4f;

        /// <summary>
        /// Distance from the follow position before the pet moves again.
        /// </summary>
        private const float FollowTolerance = 2f;

        /// <summary>
        /// Range aggressive pets look for hostile units, the same range creature combat AI aggros at.
        /// </summary>
        private const float AggroRange = 15f;

        /// <summary>
        /// Pets drop their target and return when the owner moves this far away, unless they were told to stay.
        /// </summary>
        private const float LeashRangeFromOwner = 40f;

        /// <summary>
        /// Pets that end up this far from their owner (eg: owner taking a taxi) are moved back to the owner.
        /// </summary>
        private const float TeleportRangeFromOwner = 80f;

        private const float BaseRunSpeed = 8f;

        public override EntityType Type => EntityType.Pet;

        public uint OwnerGuid { get; private set; }
        public uint SummonSpell4Id { get; private set; }
        public PetStance Stance { get; private set; }
        public PetStance ValidStances { get; private set; }

        private ISpellInfo summonSpell;
        private CombatPetAbilityInfo abilityInfo;
        private int followSlot;

        private TimeSpan attackInterval;
        private UpdateTimer attackTimer;
        private uint[] attackSpell4BaseIds = [];
        private readonly List<(CombatPetSpecialAttack Attack, UpdateTimer Timer)> specialAttacks = [];

        /// <summary>
        /// Delay between death and removal so the death is seen.
        /// </summary>
        private UpdateTimer despawnTimer;

        /// <summary>
        /// Stance the pet behaves as, summons without stances (eg: Geist, Stalker clone) assist and defend their owner.
        /// </summary>
        private PetStance BehaviourStance => ValidStances != PetStance.None ? Stance : PetStance.Assist;
        private int attackIndex;
        private float attackRange = 5f;

        private readonly UpdateTimer thinkTimer = new(TimeSpan.FromMilliseconds(250d));
        private Vector3? moveDestination;

        /// <summary>
        /// Position the pet was told to go to and stay at.
        /// </summary>
        private Vector3? holdPosition;

        /// <summary>
        /// Target given by the owner, attacked regardless of stance.
        /// </summary>
        private uint? commandTargetGuid;

        /// <summary>
        /// Pet was told to stop, it won't pick its own targets until the next command or stance change.
        /// </summary>
        private bool stopped;

        /// <summary>
        /// Pending ability spells to cast once a leap has landed or after an ability spell.
        /// </summary>
        private (uint[] Spell4BaseIds, uint TargetGuid, double Delay)? pendingCast;

        #region Dependency Injection

        private readonly IGameTableManager gameTableManager;
        private readonly IForcedMovementGenerator forcedMovementGenerator;
        private readonly ISpellInfoManager spellInfoManager;

        public CombatPetEntity(
            IMovementManager movementManager,
            IEntitySummonFactory entitySummonFactory,
            IStatUpdateManager<IUnitEntity> statUpdateManager,
            ISpellFactory spellFactory,
            IGameTableManager gameTableManager,
            IForcedMovementGenerator forcedMovementGenerator,
            ISpellInfoManager spellInfoManager)
            : base(movementManager, entitySummonFactory, statUpdateManager, spellFactory)
        {
            this.gameTableManager        = gameTableManager;
            this.forcedMovementGenerator = forcedMovementGenerator;
            this.spellInfoManager        = spellInfoManager;

            statUpdateManager.Initialise(this);
        }

        #endregion

        /// <summary>
        /// Initialise <see cref="ICombatPetEntity"/> for the supplied owner from the summon spell and effect data.
        /// </summary>
        public void Initialise(IPlayer owner, ISpellInfo summonSpell, ISpellEffectSummonPetData data, ICreatureInfo creatureInfo, PetStance stance, int followSlot)
        {
            OwnerGuid        = owner.Guid;
            SummonSpell4Id   = summonSpell.Entry.Id;
            this.summonSpell = summonSpell;
            this.followSlot  = followSlot;

            abilityInfo  = CombatPetAbilityInfo.Get(summonSpell.BaseInfo.Entry.Id);
            ValidStances = abilityInfo?.IsBot == true ? PetStance.All : PetStance.None;
            Stance       = (ValidStances & stance) != 0 ? stance : PetStance.None;

            attackInterval = data.AttackInterval > TimeSpan.Zero ? data.AttackInterval : abilityInfo?.AttackInterval ?? TimeSpan.FromSeconds(1.5d);
            attackTimer    = new UpdateTimer(attackInterval);

            if (abilityInfo != null)
            {
                (uint[] attacks, CombatPetSpecialAttack[] specials) = abilityInfo.GetAttacks(creatureInfo.Entry.Id);
                attackSpell4BaseIds = attacks;
                foreach (CombatPetSpecialAttack special in specials)
                    specialAttacks.Add((special, new UpdateTimer(special.Interval)));
            }

            // pets are the owner's level and fight for the owner's faction
            Initialise(creatureInfo);
            SetStat(Static.Entity.Stat.Level, owner.Level);
            CalculateDefaultProperties();

            foreach (ICreatureInfoProperty property in creatureInfo.GetPropertyOverrides())
                SetBaseProperty(property.Property, property.Value);

            Health = MaxHealth;
            Shield = MaxShieldCapacity;

            Faction1 = owner.Faction1;
            Faction2 = owner.Faction1;

            SetDependantStatBalance(true);
            InitialiseAttackRange();
        }

        private void InitialiseAttackRange()
        {
            if (attackSpell4BaseIds.Length == 0)
                return;

            ISpellInfo spellInfo = GetTierSpellInfo(attackSpell4BaseIds[0]);
            if (spellInfo != null && spellInfo.Entry.TargetMaxRange > 0f)
                attackRange = spellInfo.Entry.TargetMaxRange;
        }

        protected override IEntityModel BuildEntityModel()
        {
            return new PetEntityModel
            {
                CreatureId = CreatureId,
                OwnerId    = OwnerGuid,
                Name       = ""
            };
        }

        /// <summary>
        /// Calculate default property value for supplied <see cref="Property"/>.
        /// </summary>
        /// <remarks>
        /// The client uses unit property defaults for pet entities, combat pets get the creature values of their level like any other creature which are sent with the entity.
        /// </remarks>
        protected override float CalculateDefaultProperty(Property property)
        {
            UnitProperty2Entry entry = gameTableManager.UnitProperty2.GetEntry((uint)property);
            if (entry == null)
                return 0f;

            float value = entry.DefaultValue;
            if ((entry.Flags & GameTable.Static.UnitPropertyFlags.Static) == 0)
            {
                uint level = Level;
                Property levelProperty = property;
                if (property >= Property.MoveSpeedMultiplier)
                {
                    level         = (uint)gameTableManager.CreatureLevel.Entries.Length;
                    levelProperty = (Property)(property - Property.MoveSpeedMultiplier);
                }

                CreatureLevelEntry levelEntry = gameTableManager.CreatureLevel.GetEntry(level);
                if (levelEntry != null)
                    value = levelEntry.UnitPropertyValue[(uint)levelProperty];
            }

            // bot archetype 18 has a BaseHealth multiplier of 0, which would summon every bot dead, a 0 health multiplier doesn't adjust health
            value *= GetMultiplier(CreatureInfo?.ArcheTypeEntry?.UnitPropertyMultiplier, property);
            value *= GetMultiplier(CreatureInfo?.DifficultyEntry?.UnitPropertyMultiplier, property);
            value *= GetMultiplier(CreatureInfo?.TierEntry?.UnitPropertyMultiplier, property);

            return value;
        }

        private static float GetMultiplier(float[] multipliers, Property property)
        {
            if (multipliers == null || (uint)property >= multipliers.Length)
                return 1f;

            float multiplier = multipliers[(uint)property];
            return multiplier == 0f && property == Property.BaseHealth ? 1f : multiplier;
        }

        public override void OnAddToMap(IBaseMap map, uint guid, Vector3 vector)
        {
            base.OnAddToMap(map, guid, vector);

            IPlayer owner = GetOwner();
            if (owner == null)
            {
                log.Error($"Combat pet {Guid} has lost its owner {OwnerGuid}!");
                RemoveFromMap();
                return;
            }

            owner.CombatPetManager.OnPetAddToMap(this);
        }

        public override void OnEnqueueRemoveFromMap()
        {
            base.OnEnqueueRemoveFromMap();
            ThreatManager.ClearThreatList();
        }

        private IPlayer GetOwner()
        {
            return Map?.GetEntity<IPlayer>(OwnerGuid);
        }

        /// <summary>
        /// Return the <see cref="ISpellInfo"/> of supplied base spell at the tier of the summon spell.
        /// </summary>
        private ISpellInfo GetTierSpellInfo(uint spell4BaseId)
        {
            ISpellBaseInfo baseInfo = spellInfoManager.GetSpellBaseInfo(spell4BaseId);
            return baseInfo?.GetSpellInfo((byte)summonSpell.Entry.TierIndex) ?? baseInfo?.GetSpellInfo(1);
        }

        /// <summary>
        /// Set the current <see cref="PetStance"/>, this is ignored if the stance is not valid for the pet.
        /// </summary>
        public bool SetStance(PetStance stance)
        {
            if ((ValidStances & stance) == 0 || (stance & (stance - 1)) != 0)
                return false;

            Stance  = stance;
            stopped = false;

            // a stance change ends a go to command, stay keeps the pet where it is
            holdPosition = stance == PetStance.Stay ? Position : null;

            if (stance == PetStance.Passive)
                DropTarget();

            return true;
        }

        /// <summary>
        /// Attack the supplied <see cref="IUnitEntity"/>, ignoring the current stance.
        /// </summary>
        public void CommandAttack(IUnitEntity target)
        {
            if (!IsAlive || target == null || !CanAttack(target))
                return;

            stopped           = false;
            commandTargetGuid = target.Guid;
            Engage(target);
        }

        /// <summary>
        /// Stop attacking and stay out of combat until the next command or a stance change.
        /// </summary>
        public void CommandStop()
        {
            stopped = true;
            DropTarget();
            StopMoving();
        }

        /// <summary>
        /// Stop attacking and return to the owner.
        /// </summary>
        public void CommandFollow()
        {
            stopped      = false;
            holdPosition = null;
            DropTarget();
            moveDestination = null;
        }

        /// <summary>
        /// Move to the supplied position and stay there.
        /// </summary>
        public void CommandGoTo(Vector3 position)
        {
            stopped      = false;
            holdPosition = position;
            DropTarget();
            MoveTo(position);
        }

        /// <summary>
        /// Cast a spell for the owner, eg: a bot ability triggered by the PetCastSpell effect.
        /// </summary>
        public void CastPetSpell(uint spell4Id, uint primaryTargetId)
        {
            if (!IsAlive)
                return;

            // bot abilities interrupt whatever the bot is doing
            CancelSpellCasts();

            if (primaryTargetId != 0u && primaryTargetId != OwnerGuid)
            {
                IUnitEntity target = GetVisible<IUnitEntity>(primaryTargetId);
                if (target != null && CanAttack(target))
                {
                    stopped           = false;
                    commandTargetGuid = target.Guid;
                    Engage(target);
                    MovementManager.SetRotationFaceUnit(target.Guid);
                }
            }

            CastSpell(spell4Id, new SpellParameters
            {
                PrimaryTargetId        = primaryTargetId,
                UserInitiatedSpellCast = false
            });

            Spell4Entry entry = gameTableManager.Spell4.GetEntry(spell4Id);
            if (entry != null && abilityInfo != null && abilityInfo.FollowUpSpells.TryGetValue(entry.Spell4BaseIdBaseSpell, out var followUp))
            {
                if (followUp.Delay > TimeSpan.Zero)
                    pendingCast = (followUp.Spell4BaseIds, primaryTargetId, followUp.Delay.TotalSeconds);
                else
                    CastPendingSpells(followUp.Spell4BaseIds, primaryTargetId);
            }
        }

        private void CastPendingSpells(uint[] spell4BaseIds, uint targetGuid)
        {
            foreach (uint spell4BaseId in spell4BaseIds)
            {
                ISpellInfo spellInfo = GetTierSpellInfo(spell4BaseId);
                if (spellInfo != null)
                    CastSpell(new SpellParameters
                    {
                        SpellInfo              = spellInfo,
                        PrimaryTargetId        = targetGuid,
                        UserInitiatedSpellCast = false
                    });
            }
        }

        /// <summary>
        /// Leap to supplied target and cast the supplied spells when landing.
        /// </summary>
        public void LeapCast(IUnitEntity target, uint[] spell4BaseIds)
        {
            if (!IsAlive || target == null)
                return;

            CancelSpellCasts();

            stopped           = false;
            commandTargetGuid = target.Guid;
            Engage(target);

            // land just in front of the target facing it
            float angle = MathF.Atan2(Position.Z - target.Position.Z, Position.X - target.Position.X);
            Vector3 destination = target.Position.GetPoint2D(angle, Math.Max(1f, target.HitRadius));

            TimeSpan flightTime = TimeSpan.FromMilliseconds(500d);
            moveDestination = null;
            forcedMovementGenerator.ForceMove(this, destination, new Vector3(-angle - MathF.PI / 2f, 0f, 0f), flightTime, 3f, 0f);

            pendingCast = (spell4BaseIds, target.Guid, flightTime.TotalSeconds);
        }

        /// <summary>
        /// Invoked when a hostile unit is added to the owner's threat list.
        /// </summary>
        public void OnOwnerThreatAdd(IUnitEntity hostile)
        {
            if (!IsAlive || abilityInfo == null || TargetGuid != null || stopped)
                return;

            // defensive pets protect their owner, aggressive pets attack anything, assisting pets only attack the owner's target
            PetStance behaviour = BehaviourStance;
            if (behaviour is PetStance.Defensive or PetStance.Aggressive or PetStance.Stay || ValidStances == PetStance.None)
                TryEngage(hostile);
            else if (behaviour == PetStance.Assist && GetOwner()?.TargetGuid == hostile.Guid)
                TryEngage(hostile);
        }

        public override void Update(double lastTick)
        {
            base.Update(lastTick);

            if (Map == null)
                return;

            if (!IsAlive)
            {
                despawnTimer?.Update(lastTick);
                if (despawnTimer?.HasElapsed == true)
                {
                    despawnTimer = null;
                    RemoveFromMap();
                }
                return;
            }

            foreach ((CombatPetSpecialAttack _, UpdateTimer timer) in specialAttacks)
                timer.Update(lastTick);

            if (pendingCast.HasValue)
            {
                var cast = pendingCast.Value;
                cast.Delay -= lastTick;
                if (cast.Delay > 0d)
                {
                    pendingCast = cast;
                    return;
                }

                pendingCast = null;
                CastPendingSpells(cast.Spell4BaseIds, cast.TargetGuid);
            }

            attackTimer.Update(lastTick);

            thinkTimer.Update(lastTick);
            if (!thinkTimer.HasElapsed)
                return;

            thinkTimer.Reset();
            Think();
        }

        private void Think()
        {
            IPlayer owner = GetOwner();
            if (owner == null)
                return;

            float ownerDistance = Vector3.Distance(Position, owner.Position);
            if (ownerDistance > TeleportRangeFromOwner)
            {
                DropTarget();
                holdPosition = null;
                StopMoving();
                MovementManager.SetPosition(GetFollowPosition(owner), false);
                moveDestination = null;
                return;
            }

            IUnitEntity target = GetCurrentTarget();
            if (target != null && holdPosition == null && Stance != PetStance.Stay && commandTargetGuid != target.Guid && ownerDistance > LeashRangeFromOwner)
            {
                // don't wander off chasing things while the owner leaves
                DropTarget();
                target = null;
            }

            // pets without known abilities (eg: other classes' summons) only follow their owner
            if (abilityInfo == null)
                target = null;
            else if (target == null)
            {
                target = SelectTarget(owner);

                // pets holding a position only pick fights they can reach from there
                if (target != null && (BehaviourStance == PetStance.Stay || holdPosition.HasValue || abilityInfo.Stationary)
                    && Vector3.Distance(Position, target.Position) - target.HitRadius > attackRange)
                    target = null;
            }
            if (target != null)
            {
                Fight(target);
                return;
            }

            if (holdPosition.HasValue)
            {
                if (Vector3.Distance(Position, holdPosition.Value) > FollowTolerance)
                    MoveTo(holdPosition.Value);
                return;
            }

            if (BehaviourStance == PetStance.Stay || abilityInfo?.Stationary == true)
                return;

            Vector3 followPosition = GetFollowPosition(owner);
            if (Vector3.Distance(Position, followPosition) > FollowTolerance
                && (moveDestination == null || Vector3.Distance(moveDestination.Value, followPosition) > FollowTolerance))
                MoveTo(followPosition, ownerDistance > 15f ? 1.5f : 1f);
        }

        private Vector3 GetFollowPosition(IPlayer owner)
        {
            // directly behind the owner, offset to one side per follow slot
            float angle = -owner.Rotation.X + MathF.PI / 2f;
            angle += followSlot % 2 == 0 ? FollowSlotAngle : -FollowSlotAngle;
            return owner.Position.GetPoint2D(angle, FollowDistance + (followSlot / 2) * 2f);
        }

        /// <summary>
        /// Returns the current target if it is still valid to attack.
        /// </summary>
        private IUnitEntity GetCurrentTarget()
        {
            if (TargetGuid == null)
                return null;

            IUnitEntity target = GetVisible<IUnitEntity>(TargetGuid.Value);
            if (target == null || !target.IsAlive || !CanAttack(target))
            {
                DropTarget();
                return null;
            }

            return target;
        }

        /// <summary>
        /// Select a new target based on the current <see cref="PetStance"/>.
        /// </summary>
        private IUnitEntity SelectTarget(IPlayer owner)
        {
            PetStance behaviour = BehaviourStance;
            if (stopped || behaviour is PetStance.Passive or PetStance.None)
                return null;

            // anything fighting the pet
            IUnitEntity target = GetTopHostile(this);
            if (target != null)
                return target;

            switch (behaviour)
            {
                case PetStance.Assist:
                {
                    // the owner's target, once the owner is fighting it
                    IUnitEntity ownerTarget = owner.TargetGuid != null && owner.InCombat ? GetVisible<IUnitEntity>(owner.TargetGuid.Value) : null;
                    if (ownerTarget != null && ownerTarget.IsAlive && CanAttack(ownerTarget))
                        return ownerTarget;

                    // summons without stances also defend their owner
                    return ValidStances == PetStance.None ? GetTopHostile(owner) : null;
                }
                case PetStance.Defensive:
                case PetStance.Stay:
                    return GetTopHostile(owner);
                case PetStance.Aggressive:
                {
                    target = GetTopHostile(owner);
                    if (target != null)
                        return target;

                    var check = new SearchCheckRange<IUnitEntity>();
                    check.Initialise(Position, AggroRange);
                    return Map.Search(Position, AggroRange, check)
                        .Where(u => u != this && u.IsAlive && u.Faction1 != 0 && u is not IPlayer && GetDispositionTo(u.Faction1) == Disposition.Hostile && CanAttack(u))
                        .OrderBy(u => Vector3.DistanceSquared(Position, u.Position))
                        .FirstOrDefault();
                }
            }

            return null;
        }

        private IUnitEntity GetTopHostile(IUnitEntity unit)
        {
            foreach (IHostileEntity hostile in unit.ThreatManager.OrderByDescending(h => h.Threat))
            {
                IUnitEntity entity = GetVisible<IUnitEntity>(hostile.HatedUnitId);
                if (entity != null && entity.IsAlive && CanAttack(entity))
                    return entity;
            }

            return null;
        }

        private void TryEngage(IUnitEntity target)
        {
            if (target == null || !target.IsAlive || !CanAttack(target))
                return;

            Engage(target);
        }

        private void Engage(IUnitEntity target)
        {
            if (TargetGuid != target.Guid)
                SetTarget(target);

            // put the pet in combat with the target straight away, the target will fight back
            if (ThreatManager.GetHostile(target.Guid) == null)
                ThreatManager.UpdateThreat(target, 1);
        }

        private void DropTarget()
        {
            commandTargetGuid = null;
            if (TargetGuid != null)
                SetTarget((IWorldEntity)null);
        }

        private void Fight(IUnitEntity target)
        {
            if (TargetGuid != target.Guid)
                Engage(target);

            if (IsCasting())
                return;

            float distance = Vector3.Distance(Position, target.Position) - target.HitRadius;
            if (distance > attackRange)
            {
                if (TryCastSpecialAttack(target, true))
                    return;

                // stay and go to only fight what is in range
                if (BehaviourStance == PetStance.Stay || holdPosition.HasValue || abilityInfo?.Stationary == true)
                {
                    if (commandTargetGuid != target.Guid)
                        DropTarget();
                    return;
                }

                float approach = Math.Max(1f, attackRange * 0.8f) + target.HitRadius;
                Vector3 destination = target.Position.GetPoint2D(MathF.Atan2(Position.Z - target.Position.Z, Position.X - target.Position.X), approach);
                if (moveDestination == null || Vector3.Distance(moveDestination.Value, destination) > FollowTolerance)
                    MoveTo(destination);
                return;
            }

            if (moveDestination != null)
                StopMoving();

            MovementManager.SetRotationFaceUnit(target.Guid);

            if (TryCastSpecialAttack(target, false))
                return;

            if (!attackTimer.HasElapsed)
                return;

            DoAttack(target);
            attackTimer.Reset();
        }

        /// <summary>
        /// Cast the first ready special attack for the supplied range state.
        /// </summary>
        private bool TryCastSpecialAttack(IUnitEntity target, bool outOfRange)
        {
            foreach ((CombatPetSpecialAttack attack, UpdateTimer timer) in specialAttacks)
            {
                if (attack.OutOfRange != outOfRange || !timer.HasElapsed)
                    continue;

                ISpellInfo spellInfo = GetTierSpellInfo(attack.Spell4BaseId);
                if (spellInfo == null)
                    continue;

                if (outOfRange && spellInfo.Entry.TargetMaxRange > 0f && Vector3.Distance(Position, target.Position) - target.HitRadius > spellInfo.Entry.TargetMaxRange)
                    continue;

                timer.Reset();
                CastSpell(new SpellParameters
                {
                    SpellInfo              = spellInfo,
                    PrimaryTargetId        = target.Guid,
                    UserInitiatedSpellCast = false
                });
                return true;
            }

            return false;
        }

        private void DoAttack(IUnitEntity target)
        {
            if (attackSpell4BaseIds.Length == 0)
                return;

            uint spell4BaseId = attackSpell4BaseIds[attackIndex];
            attackIndex = (attackIndex + 1) % attackSpell4BaseIds.Length;

            ISpellInfo spellInfo = GetTierSpellInfo(spell4BaseId);
            if (spellInfo == null)
                return;

            CastSpell(new SpellParameters
            {
                SpellInfo              = spellInfo,
                PrimaryTargetId        = target.Guid,
                UserInitiatedSpellCast = false
            });
        }

        private void MoveTo(Vector3 destination, float speedMultiplier = 1f)
        {
            moveDestination = destination;
            float speed = GetPropertyValue(Property.MoveSpeedMultiplier) * BaseRunSpeed * speedMultiplier;
            MovementManager.LaunchSpline([Position, destination], SplineType.Linear, SplineMode.OneShot, speed);
        }

        private void StopMoving()
        {
            moveDestination = null;
            MovementManager.SetPosition(Position, false);
        }

        /// <summary>
        /// Invoked when <see cref="IPositionCommand"/> is finalised.
        /// </summary>
        public override void OnEntityCommandFinalise(Abstract.Entity.Movement.Command.IEntityCommand command)
        {
            base.OnEntityCommandFinalise(command);

            if (command is not Abstract.Entity.Movement.Command.Position.IPositionCommand)
                return;

            moveDestination = null;
            if (TargetGuid.HasValue)
                MovementManager.SetRotationFaceUnit(TargetGuid.Value);
        }

        /// <summary>
        /// Invoked when a new <see cref="IHostileEntity"/> is added to the threat list.
        /// </summary>
        public override void OnThreatAddTarget(IHostileEntity hostile)
        {
            base.OnThreatAddTarget(hostile);

            IUnitEntity unit = GetVisible<IUnitEntity>(hostile.HatedUnitId);
            if (unit == null)
                return;

            // the owner is in combat with anything its pet is fighting
            IPlayer owner = GetOwner();
            if (owner != null && owner.IsAlive && owner.ThreatManager.GetHostile(unit.Guid) == null)
                owner.ThreatManager.UpdateThreat(unit, 1);

            // pets defend themselves unless told otherwise
            if (abilityInfo != null && TargetGuid == null && !stopped && BehaviourStance != PetStance.Passive)
                TryEngage(unit);
        }

        /// <summary>
        /// Invoked when an existing <see cref="IHostileEntity"/> is removed from the threat list.
        /// </summary>
        public override void OnThreatRemoveTarget(IHostileEntity hostile)
        {
            base.OnThreatRemoveTarget(hostile);

            if (TargetGuid == hostile.HatedUnitId)
                DropTarget();
        }

        /// <summary>
        /// Returns whether or not this <see cref="IUnitEntity"/> is an attackable target for supplied <see cref="IUnitEntity"/>.
        /// </summary>
        public override bool IsValidAttackTarget(IUnitEntity attacker)
        {
            return true;
        }

        public override bool CanAttack(IUnitEntity target)
        {
            if (target.Guid == OwnerGuid || target is ICombatPetEntity pet && pet.OwnerGuid == OwnerGuid)
                return false;

            // pets can only attack players their owner could attack
            if (target is IPlayer or ICombatPetEntity)
            {
                IPlayer owner = GetOwner();
                if (owner == null)
                    return false;

                IUnitEntity victim = target is ICombatPetEntity targetPet ? Map.GetEntity<IUnitEntity>(targetPet.OwnerGuid) : target;
                if (victim == null || !owner.CanAttack(victim))
                    return false;
            }

            return base.CanAttack(target);
        }

        protected override void OnDeath(IUnitEntity killer)
        {
            base.OnDeath(killer);

            pendingCast  = null;
            despawnTimer = new UpdateTimer(TimeSpan.FromSeconds(2d));
            GetOwner()?.CombatPetManager.OnPetDeath(this);
        }

        protected override void GenerateRewards(IUnitEntity killer)
        {
            // pets don't reward their killers
        }
    }
}
