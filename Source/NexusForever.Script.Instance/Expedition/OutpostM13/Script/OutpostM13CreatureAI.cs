using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Spell;
using NexusForever.GameTable;
using NexusForever.Script.Template.Filter;
using NexusForever.Shared;

namespace NexusForever.Script.Instance.Expedition.OutpostM13.Script
{
    // Generated from the archived Jabbithole ability lists of each creature (normal and veteran pages), cooldowns from Spell4.

    /// <summary>
    /// Novaburn Corsair.
    /// </summary>
    [ScriptFilterCreatureId(23453, 69074)]
    public class M13NovaburnCorsairAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [2764];

        public M13NovaburnCorsairAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }
    }

    /// <summary>
    /// Novaburn Plunderer.
    /// </summary>
    [ScriptFilterCreatureId(23454, 69075)]
    public class M13NovaburnPlundererAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [32773, 32774];

        public M13NovaburnPlundererAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }
    }

    /// <summary>
    /// Ransacker Rorgh.
    /// </summary>
    [ScriptFilterCreatureId(23455, 69076)]
    public class M13RansackerRorghAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [32867, 32868];

        public M13RansackerRorghAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }

        public override void OnLoad(ICreatureEntity owner)
        {
            base.OnLoad(owner);
            AddAbility(34131, 0d, 4d); // Hack and Slash
            AddAbility(35727, 0d, 7d); // Onslaught
            AddAbility(49350, 0d, 10d); // Double Strike
            AddAbility(49410, 0d, 13d); // Overpower
            AddAbility(49792, 0d, 16d, veteranOnly: true); // Searing Strikes
            AddAbility(51383, 0d, 19d, veteranOnly: true); // Melt Armor
        }
    }

    /// <summary>
    /// Infected Miner.
    /// </summary>
    [ScriptFilterCreatureId(23500, 69077)]
    public class M13InfectedMinerAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [5649, 5652];

        public M13InfectedMinerAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }

        public override void OnLoad(ICreatureEntity owner)
        {
            base.OnLoad(owner);
            AddAbility(34552, 0d, 4d, veteranOnly: true); // Flourishing Combo
            AddAbility(52448, 0d, 7d, veteranOnly: true); // Haymaker
        }
    }

    /// <summary>
    /// Hive Queen.
    /// </summary>
    [ScriptFilterCreatureId(23513, 69078)]
    public class M13HiveQueenAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [5532, 5536];

        public M13HiveQueenAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }

        public override void OnLoad(ICreatureEntity owner)
        {
            base.OnLoad(owner);
            AddAbility(53424, 0d, 4d); // Royal Offense
            AddAbility(53425, 0d, 7d); // Sticky Spit
            AddAbility(53426, 0d, 10d); // Acid Splash
            AddAbility(53427, 0d, 13d); // One-Ton Bounce
        }
    }

    /// <summary>
    /// Hive Infector.
    /// </summary>
    [ScriptFilterCreatureId(23536, 69080)]
    public class M13HiveInfectorAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [6750, 6751];

        public M13HiveInfectorAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }

        public override void OnLoad(ICreatureEntity owner)
        {
            base.OnLoad(owner);
            AddAbility(35234, 0d, 4d); // Rumble
            AddAbility(39434, 0d, 7d); // Tremors
            AddAbility(52769, 0d, 10d); // Rolling Offensive
        }
    }

    /// <summary>
    /// Space Frizlet.
    /// </summary>
    [ScriptFilterCreatureId(23539, 69081)]
    public class M13SpaceFrizletAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [6909, 6910];

        public M13SpaceFrizletAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }

        public override void OnLoad(ICreatureEntity owner)
        {
            base.OnLoad(owner);
            AddAbility(54381, 0d, 4d); // Flash Flip
            AddAbility(54383, 0d, 7d, veteranOnly: true); // Flux Field
            AddAbility(54563, 0d, 10d); // Forming Dash
        }
    }

    /// <summary>
    /// Novaburn Corsair.
    /// </summary>
    [ScriptFilterCreatureId(23565, 69082)]
    public class M13NovaburnCorsair2AI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [2764];

        public M13NovaburnCorsair2AI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }
    }

    /// <summary>
    /// Space Fraz.
    /// </summary>
    [ScriptFilterCreatureId(23626, 69083)]
    public class M13SpaceFrazAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [56980, 56981];

        public M13SpaceFrazAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }

        public override void OnLoad(ICreatureEntity owner)
        {
            base.OnLoad(owner);
            AddAbility(53985, 0d, 4d); // Ethereal Emission
        }
    }

    /// <summary>
    /// Larval Infector.
    /// </summary>
    [ScriptFilterCreatureId(29612, 69084)]
    public class M13LarvalInfectorAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [6750, 6751];

        public M13LarvalInfectorAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }

        public override void OnLoad(ICreatureEntity owner)
        {
            base.OnLoad(owner);
            AddAbility(35234, 0d, 4d); // Rumble
            AddAbility(39434, 0d, 7d); // Tremors
        }
    }

    /// <summary>
    /// Novaburn Machinegunner.
    /// </summary>
    [ScriptFilterCreatureId(69878, 69881)]
    public class M13NovaburnMachinegunnerAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [2764];

        public M13NovaburnMachinegunnerAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }

        public override void OnLoad(ICreatureEntity owner)
        {
            base.OnLoad(owner);
            AddAbility(51828, 0d, 4d); // Shell Storm
        }
    }

    /// <summary>
    /// Novaburn Smasher.
    /// </summary>
    [ScriptFilterCreatureId(69879, 69882)]
    public class M13NovaburnSmasherAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [32920, 32921];

        public M13NovaburnSmasherAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }

        public override void OnLoad(ICreatureEntity owner)
        {
            base.OnLoad(owner);
            AddAbility(46692, 0d, 4d); // Crushing Flurry
        }
    }

    /// <summary>
    /// Captain Wiko.
    /// </summary>
    [ScriptFilterCreatureId(69898)]
    public class M13CaptainWikoAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [34062];

        public M13CaptainWikoAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }

        public override void OnLoad(ICreatureEntity owner)
        {
            base.OnLoad(owner);
            AddAbility(55371, 0d, 4d); // Bad Weather
            AddAbility(55401, 0d, 7d); // Lightning Zap
            AddAbility(55402, 0d, 10d); // Microburst
            AddAbility(55421, 0d, 13d); // Gust
        }
    }
}
