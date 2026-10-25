using System.Numerics;
using NexusForever.Game.Abstract.Combat;
using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Spell;
using NexusForever.Game.Static.Quest;
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
    /// Virtual Eldan Defense Construct, the boss of Deconstruct the Construct.
    /// </summary>
    /// <remarks>
    /// The creature is flagged NoAI so it gets no generic combat AI. It stays on its platform, engages players that come up
    /// and cycles the NPE bot boss telegraphs: Ionic Discharge up close, otherwise Vortex Grenades and Plasma Sweep.
    /// </remarks>
    [ScriptFilterCreatureId(73499)]
    public class ConstructEntityScript : IUnitScript, IOwnedScript<ICreatureEntity>
    {
        private const float AggroRange    = 20f;
        private const float LeashDistance = 40f;

        private const uint VortexGrenades = 85523;
        private const uint IonicDischarge = 85535;
        private const uint PlasmaSweep    = 85550;

        private ICreatureEntity owner;
        private bool resetting;
        private int rangedIndex;

        private readonly UpdateTimer abilityTimer = new(TimeSpan.FromSeconds(4d));

        #region Dependency Injection

        private readonly IFactory<ISpellParameters> spellParametersFactory;
        private readonly IGameTableManager gameTableManager;

        public ConstructEntityScript(
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
            owner.SetInRangeCheck(25f);
        }

        /// <summary>
        /// Invoked when <see cref="IGridEntity"/> is added to range check range.
        /// </summary>
        public void OnEnterRange(IGridEntity entity)
        {
            if (entity is not IPlayer player)
                return;

            // Deconstruct the Construct: direction arrow hint and jump up to the platform
            player.QuestManager.ObjectiveUpdate(QuestObjectiveType.EnterArea, 8548, 1);
        }

        /// <summary>
        /// Invoked each world tick with the delta since the previous tick occurred.
        /// </summary>
        public void Update(double lastTick)
        {
            if (!owner.IsAlive || owner.IsCasting())
                return;

            if (!owner.TargetGuid.HasValue)
            {
                // engage the first player that reaches the platform
                IPlayer player = owner.Map.Search(owner.Position, AggroRange, new TurretEntityScript.SearchCheckHostilePlayer(owner)).FirstOrDefault();
                if (player != null)
                    owner.ThreatManager.UpdateThreat(player, 1);
                return;
            }

            IUnitEntity target = owner.Map.GetEntity<IUnitEntity>(owner.TargetGuid.Value);
            if (target == null || !target.IsAlive || Vector3.Distance(owner.LeashPosition, target.Position) > LeashDistance)
            {
                Reset();
                return;
            }

            abilityTimer.Update(lastTick);
            if (!abilityTimer.HasElapsed)
                return;

            abilityTimer.Reset();

            uint spell4Id;
            if (owner.Position.GetDistance(target.Position) <= gameTableManager.Spell4.GetEntry(IonicDischarge).TargetMaxRange)
                spell4Id = IonicDischarge;
            else
            {
                spell4Id = rangedIndex == 0 ? VortexGrenades : PlasmaSweep;
                rangedIndex = (rangedIndex + 1) % 2;
            }

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
            abilityTimer.Reset();
        }
    }
}
