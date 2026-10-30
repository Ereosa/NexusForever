using System.Numerics;
using NexusForever.Game.Abstract.Combat;
using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Entity.Movement.Command;
using NexusForever.Game.Abstract.Entity.Movement.Command.Position;
using NexusForever.Game.Abstract.Spell;
using NexusForever.Game.Static.Combat.CrowdControl;
using NexusForever.Game.Static.Entity;
using NexusForever.Game.Static.Entity.Movement.Spline;
using NexusForever.Game.Static.Reputation;
using NexusForever.Game.Static.Spell;
using NexusForever.GameTable;
using NexusForever.GameTable.Model;
using NexusForever.Script.Template;
using NexusForever.Script.Template.Filter.Dynamic;
using NexusForever.Shared;
using NexusForever.Shared.Game;

namespace NexusForever.Script.Main.AI
{
    //[ScriptFilterIgnore]
    [ScriptFilterDynamic<IScriptFilterDynamicCombatAI>]
    public class CombatAI : IUnitScript, IOwnedScript<ICreatureEntity>
    {
        protected ICreatureEntity entity;

        private int autoAttackIndex = 0;
        protected List<uint> autoAttacks = [5649, 5652];
        private readonly UpdateTimer autoAttackTimer = new(TimeSpan.FromSeconds(1.5d));

        /// <summary>
        /// Turrets shoot from where they were placed and never chase.
        /// </summary>
        private bool stationary;
        private const uint TurretUnitRaceId = 267;
        private const uint TurretAutoAttackSpell4Id = 23121;

        private float chaseDistance = 5f;
        private readonly UpdateTimer chaseDistanceTimer = new(TimeSpan.FromSeconds(1f));

        /// <summary>
        /// Distance from its spawn a creature will follow a target before giving up.
        /// </summary>
        /// <remarks>
        /// The client data has no leash distance, this is a typical open world value.
        /// </remarks>
        private const float LeashDistance = 40f;

        /// <summary>
        /// While returning home after a leash the creature ignores everything around it.
        /// </summary>
        private bool evading;
        private readonly UpdateTimer evadeTimer = new(TimeSpan.FromSeconds(8d));

        /// <summary>
        /// Where the creature was standing when the fight started, it leashes to and returns to this point.
        /// </summary>
        /// <remarks>
        /// Creatures on a patrol spline can be far from their spawn point, leashing to the spawn made them give up as soon as they were pulled.
        /// </remarks>
        private Vector3? combatHome;
        private Vector3 Home => combatHome ?? entity.LeashPosition;

        #region Dependency Injection

        private readonly IFactory<ISpellParameters> spellParametersFactory;
        private readonly IGameTableManager gameTableManager;

        public CombatAI(
            IFactory<ISpellParameters> spellParametersFactory,
            IGameTableManager gameTableManager)
        {
            this.spellParametersFactory = spellParametersFactory;
            this.gameTableManager = gameTableManager;
        }

        #endregion

        /// <summary>
        /// Invoked when <see cref="IScript"/> is loaded.
        /// </summary>
        public virtual void OnLoad(ICreatureEntity owner)
        {
            entity = owner;
            entity.SetInRangeCheck(15f);

            if (entity.Type == EntityType.AiTurret || entity.CreatureInfo?.Entry?.UnitRaceId == TurretUnitRaceId)
            {
                stationary  = true;
                autoAttacks = [TurretAutoAttackSpell4Id];
            }
        }

        /// <summary>
        /// Invoked each world tick with the delta since the previous tick occurred.
        /// </summary>
        public virtual void Update(double lastTick)
        {
            if (!entity.IsAlive)
                return;

            if (evading)
            {
                evadeTimer.Update(lastTick);
                if (evadeTimer.HasElapsed || entity.Position.GetDistance(Home) < 2f)
                    evading = false;
                return;
            }

            if (!entity.TargetGuid.HasValue)
                return;

            // also covers fights started without going through AggroEntity (eg: pulled by an ally)
            combatHome ??= entity.Position;

            if (CheckLeash())
                return;

            // crowd control: stunned, asleep, knocked down... creatures stop where they are and don't act, rooted ones don't move
            if (!CanMove(entity))
            {
                if (!heldInPlace)
                {
                    heldInPlace = true;
                    entity.MovementManager.Finalise();
                }
            }
            else
                heldInPlace = false;

            if (!CanAct(entity))
                return;

            if (entity.IsCasting())
                return;

            UpdateAI(lastTick);
        }

        private bool heldInPlace;

        private static readonly HashSet<CCState> movementPreventing =
        [
            CCState.Stun, CCState.Sleep, CCState.Root, CCState.Polymorph, CCState.Hold, CCState.Knockdown, CCState.Disable, CCState.Tether,
            CCState.Subdue
        ];

        private static readonly HashSet<CCState> actionPreventing =
        [
            CCState.Stun, CCState.Sleep, CCState.Polymorph, CCState.Hold, CCState.Knockdown, CCState.Disable
        ];

        /// <summary>
        /// Returns if no crowd control effect on the unit keeps it from moving (stun, sleep, root, hold, knockdown, tether...).
        /// </summary>
        public static bool CanMove(IUnitEntity unit)
        {
            return unit.CrowdControlManager?.GetCCStates().Any(movementPreventing.Contains) != true;
        }

        /// <summary>
        /// Returns if no crowd control effect on the unit keeps it from attacking and casting (stun, sleep, hold, knockdown...).
        /// </summary>
        public static bool CanAct(IUnitEntity unit)
        {
            return unit.CrowdControlManager?.GetCCStates().Any(actionPreventing.Contains) != true;
        }

        protected virtual void UpdateAI(double lastTick)
        {
            autoAttackTimer.Update(lastTick);
            if (autoAttackTimer.HasElapsed)
            {
                DoAutoAttack();
                autoAttackTimer.Reset();
            }

            chaseDistanceTimer.Update(lastTick);
            if (chaseDistanceTimer.HasElapsed)
            {
                DoChase();
                chaseDistanceTimer.Reset();
            }
        }

        /// <summary>
        /// Replace the auto attacks the creature alternates between (default: unarmed punch and jab).
        /// </summary>
        public void SetAutoAttacks(params uint[] spell4Ids)
        {
            autoAttacks = [.. spell4Ids];
            autoAttackIndex = 0;
            chaseDistance = 5f;
        }

        /// <summary>
        /// Give up the fight when pulled too far from home, the threat list clearing sends the creature back and heals it.
        /// </summary>
        private bool CheckLeash()
        {
            if (entity.Position.GetDistance(Home) <= LeashDistance)
                return false;

            evading = true;
            evadeTimer.Reset();
            entity.ThreatManager.ClearThreatList();
            return true;
        }

        private void DoAutoAttack()
        {
            if (autoAttacks.Count == 0)
                return;

            if (!entity.TargetGuid.HasValue)
                return;

            IUnitEntity target = entity.Map.GetEntity<IUnitEntity>(entity.TargetGuid.Value);
            if (target == null)
                return;

            uint spell4Id = autoAttacks[autoAttackIndex];
            autoAttackIndex = (autoAttackIndex++) % autoAttacks.Count;

            Spell4Entry spell4Entry = gameTableManager.Spell4.GetEntry(spell4Id);
            if (spell4Entry == null)
                return;

            chaseDistance = Math.Min(chaseDistance, spell4Entry.TargetMaxRange);

            if (entity.Position.GetDistance(target.Position) > spell4Entry.TargetMaxRange)
                return;

            ISpellParameters spellParameters = spellParametersFactory.Resolve();
            spellParameters.PrimaryTargetId = entity.TargetGuid.Value;
            entity.CastSpell(spell4Id, spellParameters);
        }

        private void DoChase()
        {
            if (stationary || heldInPlace)
                return;

            if (!entity.TargetGuid.HasValue)
                return;

            IUnitEntity target = entity.Map.GetEntity<IUnitEntity>(entity.TargetGuid.Value);
            if (target == null)
                return;

            if (entity.Position.GetDistance(target.Position) < chaseDistance)
                return;
            
            entity.MovementManager.Chase(target, chaseDistance / 2);
        }

        /// <summary>
        /// Invoked when <see cref="IPositionCommand"/> is finalised.
        /// </summary>
        public void OnEntityCommandFinalise(IEntityCommand command)
        {
            if (command is not IPositionCommand)
                return;

            if (!entity.TargetGuid.HasValue)
                return;

            entity.MovementManager.SetRotationFaceUnit(entity.TargetGuid.Value);
        }

        /// <summary>
        /// Invoked when <see cref="IGridEntity"/> is added to range check range.
        /// </summary>
        public void OnEnterRange(IGridEntity entity)
        {
            if (entity is not IUnitEntity unit || !IsProximityAggroTarget(unit))
                return;

            AggroEntity(entity);
        }

        /// <summary>
        /// Creatures only pull players (and their pets) by walking near them, fights between creatures are staged with "Combat Flavor" units.
        /// </summary>
        /// <remarks>
        /// Without this every hostile pair of creatures fought on sight, eg: Grove Guardians attacking butterflies.
        /// </remarks>
        private bool IsProximityAggroTarget(IUnitEntity unit)
        {
            // players riding a taxi fly over creatures without pulling them
            if (unit is IPlayer { PlatformGuid: uint platformGuid } && unit.Map?.GetEntity<IWorldEntity>(platformGuid) is ITaxiEntity)
                return false;

            if (unit is IPlayer or ICombatPetEntity)
                return true;

            return IsCombatFlavor(entity) || IsCombatFlavor(unit);
        }

        private static bool IsCombatFlavor(IUnitEntity unit)
        {
            return unit.CreatureInfo?.Entry?.Description?.Contains("Combat Flavor", StringComparison.OrdinalIgnoreCase) ?? false;
        }

        /// <summary>
        /// Invoked when <see cref="IGridEntity"/> is removed from vision range.
        /// </summary>
        public void OnRemoveVisibleEntity(IGridEntity entity)
        {
            if (entity.Guid == this.entity.TargetGuid)
                SelectTarget();
        }

        /// <summary>
        /// Invoked when health is changed by source <see cref="IUnitEntity"/>.
        /// </summary>
        public virtual void OnHealthChange(IUnitEntity source, uint amount, DamageType? type)
        {
            if (type is DamageType.Heal or null)
                return;

            AggroEntity(source);
        }

        private void AggroEntity(IGridEntity source)
        {
            if (!entity.IsAlive)
                return;

            if (entity.InCombat || evading)
                return;

            if (source is not IUnitEntity unit)
                return;

            // units without a faction (eg: scenery units) are never hostile
            if (unit.Faction1 == 0 || entity.GetDispositionTo(unit.Faction1) != Disposition.Hostile)
                return;

            // environmental hazards (security beams, traps) aren't something creatures fight
            if (unit.CreatureInfo?.Entry != null && ScriptFilterDynamicCombatAI.IsEnvironmentalHazard(unit.CreatureInfo.Entry))
                return;

            combatHome ??= entity.Position;

            var spellParameters = spellParametersFactory.Resolve();
            entity.CastSpell(41368, spellParameters);

            entity.MovementManager.Finalise();
            entity.MovementManager.SetRotationFaceUnit(unit.Guid);

            entity.ThreatManager.UpdateThreat(unit, 1);
        }

        /// <summary>
        /// Invoked when <see cref="IUnitEntity"/> enters combat.
        /// </summary>
        public virtual void OnEnterCombat()
        {
            // deliberately empty
        }

        /// <summary>
        /// Invoked when <see cref="IUnitEntity"/> leaves combat.
        /// </summary>
        public virtual void OnLeaveCombat()
        {
            // deliberately empty
        }

        /// <summary>
        /// Invoked when <see cref="IUnitEntity"/> is killed.
        /// </summary>
        public virtual void OnDeath()
        {
            // deliberately empty
        }

        /// <summary>
        /// Invoked when a new <see cref="IHostileEntity"/> is added to the threat list.
        /// </summary>
        public void OnThreatAddTarget(IHostileEntity hostile)
        {
            SelectTarget();
        }

        /// <summary>
        /// Invoked when an existing <see cref="IHostileEntity"/> is removed from the threat list.
        /// </summary>
        public void OnThreatRemoveTarget(IHostileEntity hostile)
        {
            SelectTarget();
        }

        /// <summary>
        /// Invoked when an existing <see cref="IHostileEntity"/> is update on the threat list.
        /// </summary>
        public void OnThreatChange(IHostileEntity hostile)
        {
            SelectTarget();
        }

        protected virtual void SelectTarget()
        {
            IHostileEntity hostile = entity.ThreatManager.GetTopHostile();
            if (hostile == null)
            {
                Reset();
                return;
            }

            if (entity.TargetGuid == hostile.HatedUnitId)
                return;

            entity.SetTarget(hostile.HatedUnitId, hostile.Threat);
        }

        private void Reset()
        {
            entity.SetTarget((IWorldEntity)null);

            // the threat list also empties when the creature dies, healing the corpse made it look alive to the client while it stayed dead
            if (!entity.IsAlive)
            {
                combatHome = null;
                return;
            }

            entity.ModifyHealth(entity.MaxHealth, DamageType.Heal, null);

            // TODO: cancel all pending spells

            float speed = entity.GetPropertyValue(Property.MoveSpeedMultiplier) * 10f;
            entity.MovementManager.LaunchSpline([entity.Position, Home], SplineType.Linear, SplineMode.OneShot, speed);
            combatHome = null;
        }
    }
}
