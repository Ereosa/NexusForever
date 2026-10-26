using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Spell;
using NexusForever.GameTable;
using NexusForever.GameTable.Model;
using NexusForever.Script.Main.AI;
using NexusForever.Script.Template;
using NexusForever.Script.Template.Filter;
using NexusForever.Shared;

namespace NexusForever.Script.Instance.Expedition.FragmentZero.Script
{
    /// <summary>
    /// Abilities of a Fragment Zero creature, cast on its target while it fights alongside the generic <see cref="CombatAI"/>.
    /// </summary>
    /// <remarks>
    /// Ability lists come from the archived Jabbithole pages of each creature (normal and veteran pages differ), cooldowns from Spell4
    /// where the spell has one, otherwise a guess.
    /// </remarks>
    public abstract class FragmentZeroCreatureAI : IUnitScript, IOwnedScript<ICreatureEntity>
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

        protected bool IsVeteran => entity.CreatureInfo?.Entry?.Creature2TierId == 2u;

        /// <summary>
        /// Auto attacks the creature alternates between, null keeps the default unarmed punch and jab.
        /// </summary>
        protected virtual uint[] AutoAttacks => null;

        #region Dependency Injection

        protected readonly IFactory<ISpellParameters> spellParametersFactory;
        protected readonly IGameTableManager gameTableManager;

        public FragmentZeroCreatureAI(
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

            if (entity.IsCasting())
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

    /// <summary>
    /// Space Skeech fighters, berserkers, trappers and strikers.
    /// </summary>
    [ScriptFilterCreatureId(67518, 69630, 69046, 69063, 69623, 67519, 69631, 69064, 69624, 68000, 69682, 69696, 68883, 69687)]
    public class SkeechFighterAI : FragmentZeroCreatureAI
    {
        public SkeechFighterAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }

        public override void OnLoad(ICreatureEntity owner)
        {
            base.OnLoad(owner);
            AddAbility(55844, 9d, normalOnly: true);  // Right Hook
            AddAbility(38918, veteranOnly: true);     // Frenzy
            AddAbility(69792, 12d, veteranOnly: true); // Rock Strike
        }
    }

    /// <summary>
    /// Space Skeech shamans, staff auto attack and water spells.
    /// </summary>
    [ScriptFilterCreatureId(67520, 69632, 69065, 69625)]
    public class SkeechShamanAI : FragmentZeroCreatureAI
    {
        protected override uint[] AutoAttacks => [30756];

        public SkeechShamanAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }

        public override void OnLoad(ICreatureEntity owner)
        {
            base.OnLoad(owner);
            AddAbility(55841, 10d, normalOnly: true);  // Geyser Blast
            AddAbility(56683, veteranOnly: true);      // Cold Snap
            AddAbility(56986, 12d, veteranOnly: true); // Hydro Burst
        }
    }

    /// <summary>
    /// Veteran only skeech: looters and exploders (exploders blow up when they are about to die).
    /// </summary>
    [ScriptFilterCreatureId(69767, 69794)]
    public class SkeechVeteranAI : FragmentZeroCreatureAI
    {
        private bool exploded;

        public SkeechVeteranAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }

        public override void OnLoad(ICreatureEntity owner)
        {
            base.OnLoad(owner);
            AddAbility(38918);      // Frenzy
            AddAbility(54595, 8d);  // Konk
            AddAbility(69792, 12d); // Rock Strike
        }

        public override void Update(double lastTick)
        {
            // Volatile Death: exploders detonate below a fifth of their health
            if (!exploded && entity.CreatureId == 69794u && entity.IsAlive && entity.InCombat && entity.Health < entity.MaxHealth / 5u && !entity.IsCasting())
            {
                exploded = true;
                CastSpell(79027, null);
                return;
            }

            base.Update(lastTick);
        }
    }

    /// <summary>
    /// Deep space crawlers and xenobite hatchlings, bite and chomp.
    /// </summary>
    [ScriptFilterCreatureId(67551, 69667, 69092, 69670, 69134, 69671)]
    public class XenobiteCrawlerAI : FragmentZeroCreatureAI
    {
        protected override uint[] AutoAttacks => [4495, 4496];

        public XenobiteCrawlerAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }
    }

    /// <summary>
    /// Deep space creepers.
    /// </summary>
    [ScriptFilterCreatureId(67553, 69668, 69091, 69669)]
    public class XenobiteCreeperAI : FragmentZeroCreatureAI
    {
        protected override uint[] AutoAttacks => [4495, 4496];

        public XenobiteCreeperAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }

        public override void OnLoad(ICreatureEntity owner)
        {
            base.OnLoad(owner);
            AddAbility(56490, 10d);  // Evil Eye
            AddAbility(56492, 14d, 8d); // Event Horizon
        }
    }

    /// <summary>
    /// The Life-Overseer's probes.
    /// </summary>
    [ScriptFilterCreatureId(67894, 69633)]
    public class OverseerProbeAI : FragmentZeroCreatureAI
    {
        protected override uint[] AutoAttacks => [4616];

        public OverseerProbeAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }
    }

    /// <summary>
    /// The Life-Overseer's detectors.
    /// </summary>
    [ScriptFilterCreatureId(67895, 69634)]
    public class OverseerDetectorAI : FragmentZeroCreatureAI
    {
        protected override uint[] AutoAttacks => [4616];

        public OverseerDetectorAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }

        public override void OnLoad(ICreatureEntity owner)
        {
            base.OnLoad(owner);
            AddAbility(30999, 12d);  // Slasher Dash
            AddAbility(41840, 15d, 7d); // Jump Shot
        }
    }
}
