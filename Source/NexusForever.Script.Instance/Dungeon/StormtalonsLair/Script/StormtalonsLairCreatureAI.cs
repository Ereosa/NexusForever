using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Spell;
using NexusForever.GameTable;
using NexusForever.Script.Instance.Expedition;
using NexusForever.Script.Template.Filter;
using NexusForever.Shared;

namespace NexusForever.Script.Instance.Dungeon.StormtalonsLair.Script
{
    // Generated from the archived Jabbithole ability lists of each creature (normal and veteran pages), cooldowns from Spell4.

    /// <summary>
    /// Thundercall Acolyte.
    /// </summary>
    [ScriptFilterCreatureId(17153, 34689)]
    public class StormtalonsLairThundercallAcolyteAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [70286, 70287];

        public StormtalonsLairThundercallAcolyteAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }
    }

    /// <summary>
    /// Thundercall Disciple.
    /// </summary>
    [ScriptFilterCreatureId(17154, 34690)]
    public class StormtalonsLairThundercallDiscipleAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [70285];

        public StormtalonsLairThundercallDiscipleAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }
    }

    /// <summary>
    /// Thundercall Channeler.
    /// </summary>
    [ScriptFilterCreatureId(17173, 34702)]
    public class StormtalonsLairThundercallChannelerAI : ExpeditionCreatureAI
    {
        public StormtalonsLairThundercallChannelerAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }

        public override void OnLoad(ICreatureEntity owner)
        {
            base.OnLoad(owner);
            AddAbility(63233, 0d, 4d); // Shockwave
        }
    }

    /// <summary>
    /// Arcanist Breeze-Binder.
    /// </summary>
    [ScriptFilterCreatureId(24474, 34711)]
    public class StormtalonsLairArcanistBreezeBinderAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [74649, 74650];

        public StormtalonsLairArcanistBreezeBinderAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }

        public override void OnLoad(ICreatureEntity owner)
        {
            base.OnLoad(owner);
            AddAbility(39979, 0d, 4d); // Gust Convergence
        }
    }

    /// <summary>
    /// Overseer Drift-Catcher.
    /// </summary>
    [ScriptFilterCreatureId(33361, 33362)]
    public class StormtalonsLairOverseerDriftCatcherAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [70285, 74651];

        public StormtalonsLairOverseerDriftCatcherAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }

        public override void OnLoad(ICreatureEntity owner)
        {
            base.OnLoad(owner);
            AddAbility(39955, 0d, 4d); // Static Paroxysm
            AddAbility(39979, 0d, 7d); // Gust Convergence
            AddAbility(72358, 0d, 10d); // Static Paroxysm
        }
    }

    /// <summary>
    /// Thundercall Shaman.
    /// </summary>
    [ScriptFilterCreatureId(34697, 37145)]
    public class StormtalonsLairThundercallShamanAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [34062, 70285];

        public StormtalonsLairThundercallShamanAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }
    }

    /// <summary>
    /// Thundercall Sentinel.
    /// </summary>
    [ScriptFilterCreatureId(36758, 37140)]
    public class StormtalonsLairThundercallSentinelAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [70286, 70287];

        public StormtalonsLairThundercallSentinelAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }

        public override void OnLoad(ICreatureEntity owner)
        {
            base.OnLoad(owner);
            AddAbility(39993, 0d, 4d); // Shockwave
        }
    }

    /// <summary>
    /// Thundercall Storm-Weaver.
    /// </summary>
    [ScriptFilterCreatureId(36796, 37141)]
    public class StormtalonsLairThundercallStormWeaverAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [34062, 70285];

        public StormtalonsLairThundercallStormWeaverAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }

        public override void OnLoad(ICreatureEntity owner)
        {
            base.OnLoad(owner);
            AddAbility(39955, 0d, 4d); // Static Paroxysm
            AddAbility(72358, 0d, 7d); // Static Paroxysm
        }
    }

    /// <summary>
    /// Skyborn Gust.
    /// </summary>
    [ScriptFilterCreatureId(36854, 59876)]
    public class StormtalonsLairSkybornGustAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [35474, 35475];

        public StormtalonsLairSkybornGustAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }
    }

    /// <summary>
    /// Skyborn Tempest.
    /// </summary>
    [ScriptFilterCreatureId(36859, 37142)]
    public class StormtalonsLairSkybornTempestAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [26623, 70274];

        public StormtalonsLairSkybornTempestAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }

        public override void OnLoad(ICreatureEntity owner)
        {
            base.OnLoad(owner);
            AddAbility(39982, 0d, 4d); // Tornado Alley
        }
    }

    /// <summary>
    /// Thunderbeak Fleshripper.
    /// </summary>
    [ScriptFilterCreatureId(36861, 37143)]
    public class StormtalonsLairThunderbeakFleshripperAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [70296, 70297];

        public StormtalonsLairThunderbeakFleshripperAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }
    }

    /// <summary>
    /// Thundercall Storm-Watcher.
    /// </summary>
    [ScriptFilterCreatureId(36862, 37144)]
    public class StormtalonsLairThundercallStormWatcherAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [5960, 70286];

        public StormtalonsLairThundercallStormWatcherAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }

        public override void OnLoad(ICreatureEntity owner)
        {
            base.OnLoad(owner);
            AddAbility(40006, 0d, 4d); // Rampage
            AddAbility(40007, 0d, 7d); // Hack and Slash
        }
    }
}
