using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Spell;
using NexusForever.GameTable;
using NexusForever.GameTable.Model;
using NexusForever.Script.Main.AI;
using NexusForever.Script.Template;
using NexusForever.Script.Template.Filter;
using NexusForever.Shared;

namespace NexusForever.Script.Instance.Expedition
{
    /// <summary>
    /// Abilities of an expedition creature, cast on its target while it fights alongside the generic <see cref="CombatAI"/>.
    /// </summary>
    /// <remarks>
    /// Ability lists come from the archived Jabbithole pages of each creature (normal and veteran pages differ), cooldowns from Spell4
    /// where the spell has one, otherwise a guess.
    /// </remarks>
    public abstract class ExpeditionCreatureAI : IUnitScript, IOwnedScript<ICreatureEntity>
    {
        protected class Ability
        {
            public uint SpellId { get; init; }
            public double Cooldown { get; init; }
            public double InitialDelay { get; init; }
            public bool VeteranOnly { get; init; }
            public bool NormalOnly { get; init; }
            public double Timer { get; set; }
        }

        protected ICreatureEntity entity;

        private readonly List<Ability> abilities = [];

        // veteran creatures are tier 2 in expeditions, tier 5 in dungeons
        protected bool IsVeteran => entity.CreatureInfo?.Entry?.Creature2TierId is 2u or 5u;

        /// <summary>
        /// Auto attacks the creature alternates between, null keeps the default unarmed punch and jab.
        /// </summary>
        protected virtual uint[] AutoAttacks => null;

        #region Dependency Injection

        protected readonly IFactory<ISpellParameters> spellParametersFactory;
        protected readonly IGameTableManager gameTableManager;

        public ExpeditionCreatureAI(
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
        public virtual void OnLoad(ICreatureEntity owner)
        {
            entity = owner;
        }

        /// <summary>
        /// Add an ability the creature casts on its target, <paramref name="cooldown"/> 0 uses the spell's own cooldown (or 10 seconds).
        /// </summary>
        protected void AddAbility(uint spellId, double cooldown = 0d, double initialDelay = 4d, bool veteranOnly = false, bool normalOnly = false)
        {
            if (cooldown <= 0d)
            {
                Spell4Entry entry = gameTableManager.Spell4.GetEntry(spellId);
                cooldown = entry?.SpellCoolDown > 0u ? Math.Max(6d, entry.SpellCoolDown / 1000d * 1.5d) : 10d;
            }

            abilities.Add(new Ability
            {
                SpellId      = spellId,
                Cooldown     = cooldown,
                InitialDelay = initialDelay,
                VeteranOnly  = veteranOnly,
                NormalOnly   = normalOnly
            });
        }

        /// <summary>
        /// Invoked when <see cref="IUnitEntity"/> enters combat.
        /// </summary>
        public virtual void OnEnterCombat()
        {
            if (AutoAttacks != null)
                entity.InvokeScriptCollection<CombatAI>(s => s.SetAutoAttacks(AutoAttacks));

            foreach (Ability ability in abilities)
                ability.Timer = ability.InitialDelay + Random.Shared.NextDouble() * 2d;
        }

        /// <summary>
        /// Invoked when <see cref="IUnitEntity"/> leaves combat.
        /// </summary>
        public virtual void OnLeaveCombat()
        {
        }

        /// <summary>
        /// Invoked each world tick with the delta since the previous tick occurred.
        /// </summary>
        public virtual void Update(double lastTick)
        {
            if (!entity.IsAlive || !entity.InCombat || entity.TargetGuid == null)
                return;

            foreach (Ability ability in abilities)
                ability.Timer -= lastTick;

            if (entity.IsCasting() || !CombatAI.CanAct(entity))
                return;

            IUnitEntity target = entity.GetVisible<IUnitEntity>(entity.TargetGuid.Value);
            if (target == null || !target.IsAlive)
                return;

            foreach (Ability ability in abilities)
            {
                if (ability.Timer > 0d)
                    continue;
                if ((ability.VeteranOnly && !IsVeteran) || (ability.NormalOnly && IsVeteran))
                    continue;

                Spell4Entry entry = gameTableManager.Spell4.GetEntry(ability.SpellId);
                if (entry == null)
                    continue;

                if (entry.TargetMaxRange > 0f && entity.Position.GetDistance(target.Position) > entry.TargetMaxRange)
                    continue;

                CastSpell(ability.SpellId, target);
                ability.Timer = ability.Cooldown;
                break;
            }
        }

        protected void CastSpell(uint spellId, IUnitEntity target)
        {
            ISpellParameters parameters = spellParametersFactory.Resolve();
            parameters.PrimaryTargetId = target?.Guid ?? entity.Guid;
            entity.CastSpell(spellId, parameters);
        }
    }
}
