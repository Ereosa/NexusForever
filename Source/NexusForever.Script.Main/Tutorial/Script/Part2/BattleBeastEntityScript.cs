using System.Numerics;
using NexusForever.Game.Abstract.Combat;
using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Spell;
using NexusForever.Game.Static.Entity;
using NexusForever.Game.Static.Entity.Movement.Spline;
using NexusForever.Game.Static.Spell;
using NexusForever.GameTable;
using NexusForever.GameTable.Model;
using NexusForever.Script.Template;
using NexusForever.Script.Template.Filter;
using NexusForever.Shared;
using NexusForever.Shared.Game;

namespace NexusForever.Script.Main.Tutorial.Script.Part2
{
    /// <summary>
    /// Virtual Battle Beasts and Legionnaires (Exile simulation) and Virtual Defense Daguns (Dominion simulation) in The Face of the Enemy.
    /// </summary>
    /// <remarks>
    /// The creature is flagged NoAI in the game tables so the generic combat AI skips it.
    /// It wanders around its spawn point and fights back when attacked, returning home when combat ends.
    /// </remarks>
    [ScriptFilterCreatureId(73464, 73492, 73567, 73465)]
    public class BattleBeastEntityScript : IUnitScript, IOwnedScript<ICreatureEntity>
    {
        private const float WanderRadius  = 6f;
        private const float MeleeRange    = 3f;
        private const float LeashDistance = 30f;

        private static readonly uint[] autoAttacks = [5649, 5652];

        private ICreatureEntity owner;
        private int autoAttackIndex;
        private bool resetting;

        private readonly UpdateTimer wanderTimer = new(TimeSpan.FromSeconds(Random.Shared.Next(4, 10)));
        private readonly UpdateTimer attackTimer = new(TimeSpan.FromSeconds(1.5d));
        private readonly UpdateTimer chaseTimer  = new(TimeSpan.FromSeconds(1d));

        #region Dependency Injection

        private readonly IFactory<ISpellParameters> spellParametersFactory;
        private readonly IGameTableManager gameTableManager;

        public BattleBeastEntityScript(
            IFactory<ISpellParameters> spellParametersFactory,
            IGameTableManager gameTableManager)
        {
            this.spellParametersFactory = spellParametersFactory;
            this.gameTableManager       = gameTableManager;
        }

        #endregion

        /// <summary>
        /// Invoked when <see cref="IScript"/> is loaded.
        /// </summary>
        public void OnLoad(ICreatureEntity owner)
        {
            this.owner = owner;
        }

        /// <summary>
        /// Invoked each world tick with the delta since the previous tick occurred.
        /// </summary>
        public void Update(double lastTick)
        {
            if (!owner.IsAlive || owner.IsCasting())
                return;

            if (owner.TargetGuid.HasValue)
                UpdateCombat(lastTick);
            else
                UpdateWander(lastTick);
        }

        private void UpdateWander(double lastTick)
        {
            wanderTimer.Update(lastTick);
            if (!wanderTimer.HasElapsed)
                return;

            wanderTimer.Reset();

            float angle    = Random.Shared.NextSingle() * MathF.Tau;
            float distance = Random.Shared.NextSingle() * WanderRadius;
            var destination = owner.LeashPosition + new Vector3(MathF.Cos(angle) * distance, 0f, MathF.Sin(angle) * distance);

            float speed = owner.GetPropertyValue(Property.MoveSpeedMultiplier) * 2f;
            owner.MovementManager.LaunchSpline([owner.Position, destination], SplineType.Linear, SplineMode.OneShot, speed);
        }

        private void UpdateCombat(double lastTick)
        {
            IUnitEntity target = owner.Map.GetEntity<IUnitEntity>(owner.TargetGuid.Value);
            if (target == null || !target.IsAlive || Vector3.Distance(owner.Position, owner.LeashPosition) > LeashDistance)
            {
                Reset();
                return;
            }

            chaseTimer.Update(lastTick);
            if (chaseTimer.HasElapsed)
            {
                chaseTimer.Reset();
                if (owner.Position.GetDistance(target.Position) > MeleeRange)
                    owner.MovementManager.Chase(target, MeleeRange / 2f);
            }

            attackTimer.Update(lastTick);
            if (!attackTimer.HasElapsed)
                return;

            attackTimer.Reset();

            uint spell4Id = autoAttacks[autoAttackIndex];
            autoAttackIndex = (autoAttackIndex + 1) % autoAttacks.Length;

            Spell4Entry entry = gameTableManager.Spell4.GetEntry(spell4Id);
            if (entry == null || owner.Position.GetDistance(target.Position) > entry.TargetMaxRange)
                return;

            owner.MovementManager.SetRotationFaceUnit(target.Guid);

            ISpellParameters parameters = spellParametersFactory.Resolve();
            parameters.PrimaryTargetId = target.Guid;
            owner.CastSpell(spell4Id, parameters);
        }

        /// <summary>
        /// Invoked when health is changed by source <see cref="IUnitEntity"/>.
        /// </summary>
        public void OnHealthChange(IUnitEntity source, uint amount, DamageType? type)
        {
            if (type is DamageType.Heal or null || source == null || !owner.IsAlive)
                return;

            owner.MovementManager.Finalise();
            owner.ThreatManager.UpdateThreat(source, 1);
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

        private void SelectTarget()
        {
            IHostileEntity hostile = owner.ThreatManager.GetTopHostile();
            if (hostile == null)
            {
                Reset();
                return;
            }

            if (owner.TargetGuid != hostile.HatedUnitId)
                owner.SetTarget(hostile.HatedUnitId, hostile.Threat);
        }

        private void Reset()
        {
            // clearing the threat list invokes the threat callbacks again
            if (resetting)
                return;

            resetting = true;
            owner.ThreatManager.ClearThreatList();
            resetting = false;

            owner.SetTarget((IWorldEntity)null);
            owner.ModifyHealth(owner.MaxHealth, DamageType.Heal, null);

            float speed = owner.GetPropertyValue(Property.MoveSpeedMultiplier) * 10f;
            owner.MovementManager.LaunchSpline([owner.Position, owner.LeashPosition], SplineType.Linear, SplineMode.OneShot, speed);
            wanderTimer.Reset();
        }
    }
}
