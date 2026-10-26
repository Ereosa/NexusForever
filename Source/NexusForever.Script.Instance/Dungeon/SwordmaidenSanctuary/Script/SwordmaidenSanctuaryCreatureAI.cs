using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Spell;
using NexusForever.GameTable;
using NexusForever.Script.Instance.Expedition;
using NexusForever.Script.Template.Filter;
using NexusForever.Shared;

namespace NexusForever.Script.Instance.Dungeon.SwordmaidenSanctuary.Script
{
    // Generated from the archived Jabbithole ability lists of each creature (normal and veteran pages), cooldowns from Spell4.

    /// <summary>
    /// Zealous Battlemaiden.
    /// </summary>
    [ScriptFilterCreatureId(28580, 28585)]
    public class SwordmaidenSanctuaryZealousBattlemaidenAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [70223, 70224];

        public SwordmaidenSanctuaryZealousBattlemaidenAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }

        public override void OnLoad(ICreatureEntity owner)
        {
            base.OnLoad(owner);
            AddAbility(42929, 0d, 4d); // Sonic Slash
            AddAbility(42944, 0d, 7d); // Supersonic Slash
        }
    }

    /// <summary>
    /// Zealous Clawsister.
    /// </summary>
    [ScriptFilterCreatureId(28612, 28613)]
    public class SwordmaidenSanctuaryZealousClawsisterAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [70219, 70220];

        public SwordmaidenSanctuaryZealousClawsisterAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }

        public override void OnLoad(ICreatureEntity owner)
        {
            base.OnLoad(owner);
            AddAbility(42986, 0d, 4d); // Sundering Surge
        }
    }

    /// <summary>
    /// Zealous Lifesinger.
    /// </summary>
    [ScriptFilterCreatureId(28646, 28647)]
    public class SwordmaidenSanctuaryZealousLifesingerAI : ExpeditionCreatureAI
    {
        public SwordmaidenSanctuaryZealousLifesingerAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }

        public override void OnLoad(ICreatureEntity owner)
        {
            base.OnLoad(owner);
            AddAbility(53450, 0d, 4d); // Lifebinding Bolt
        }
    }

    /// <summary>
    /// Silent Predator.
    /// </summary>
    [ScriptFilterCreatureId(28660, 28661)]
    public class SwordmaidenSanctuarySilentPredatorAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [70221, 70222];

        public SwordmaidenSanctuarySilentPredatorAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }
    }

    /// <summary>
    /// Lifeweaver Initiate.
    /// </summary>
    [ScriptFilterCreatureId(28665, 28666)]
    public class SwordmaidenSanctuaryLifeweaverInitiateAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [70236, 70237];

        public SwordmaidenSanctuaryLifeweaverInitiateAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }

        public override void OnLoad(ICreatureEntity owner)
        {
            base.OnLoad(owner);
            AddAbility(43467, 0d, 4d); // Titan's Fist
        }
    }

    /// <summary>
    /// Lifeweaver Warcaller.
    /// </summary>
    [ScriptFilterCreatureId(28780, 28781)]
    public class SwordmaidenSanctuaryLifeweaverWarcallerAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [70238, 70239];

        public SwordmaidenSanctuaryLifeweaverWarcallerAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }

        public override void OnLoad(ICreatureEntity owner)
        {
            base.OnLoad(owner);
            AddAbility(43480, 0d, 4d); // Debilitating Blast
            AddAbility(43482, 0d, 7d); // Hack and Slash
        }
    }

    /// <summary>
    /// Lifeweaver Soulbinder.
    /// </summary>
    [ScriptFilterCreatureId(28795, 28796)]
    public class SwordmaidenSanctuaryLifeweaverSoulbinderAI : ExpeditionCreatureAI
    {
        public SwordmaidenSanctuaryLifeweaverSoulbinderAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }

        public override void OnLoad(ICreatureEntity owner)
        {
            base.OnLoad(owner);
            AddAbility(53450, 0d, 4d); // Lifebinding Bolt
        }
    }

    /// <summary>
    /// Corrupted Terrorantula.
    /// </summary>
    [ScriptFilterCreatureId(28801, 28829)]
    public class SwordmaidenSanctuaryCorruptedTerrorantulaAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [70234, 70235];

        public SwordmaidenSanctuaryCorruptedTerrorantulaAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }

        public override void OnLoad(ICreatureEntity owner)
        {
            base.OnLoad(owner);
            AddAbility(43618, 0d, 4d); // Burst
        }
    }

    /// <summary>
    /// Moldwood Skurge Tactician.
    /// </summary>
    [ScriptFilterCreatureId(28878, 28892)]
    public class SwordmaidenSanctuaryMoldwoodSkurgeTacticianAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [70223, 70224];

        public SwordmaidenSanctuaryMoldwoodSkurgeTacticianAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }

        public override void OnLoad(ICreatureEntity owner)
        {
            base.OnLoad(owner);
            AddAbility(43082, 0d, 4d); // Rupture
        }
    }

    /// <summary>
    /// Moldwood Skurge Slasher.
    /// </summary>
    [ScriptFilterCreatureId(28929, 28930)]
    public class SwordmaidenSanctuaryMoldwoodSkurgeSlasherAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [70219, 70220];

        public SwordmaidenSanctuaryMoldwoodSkurgeSlasherAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }

        public override void OnLoad(ICreatureEntity owner)
        {
            base.OnLoad(owner);
            AddAbility(42986, 0d, 4d); // Sundering Surge
        }
    }

    /// <summary>
    /// Corrupted Edgesmith Torian.
    /// </summary>
    [ScriptFilterCreatureId(28985, 28986)]
    public class SwordmaidenSanctuaryCorruptedEdgesmithTorianAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [70219, 70220];

        public SwordmaidenSanctuaryCorruptedEdgesmithTorianAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }

        public override void OnLoad(ICreatureEntity owner)
        {
            base.OnLoad(owner);
            AddAbility(42986, 0d, 4d); // Sundering Surge
            AddAbility(55054, 0d, 7d); // Laceration
            AddAbility(63616, 0d, 10d); // Blade Dance
            AddAbility(63680, 0d, 13d, veteranOnly: true); // Sundering Surge
            AddAbility(63761, 0d, 16d); // Bleeding
        }
    }

    /// <summary>
    /// Corrupted Lifecaller Khalee.
    /// </summary>
    [ScriptFilterCreatureId(28992, 28993)]
    public class SwordmaidenSanctuaryCorruptedLifecallerKhaleeAI : ExpeditionCreatureAI
    {
        public SwordmaidenSanctuaryCorruptedLifecallerKhaleeAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }

        public override void OnLoad(ICreatureEntity owner)
        {
            base.OnLoad(owner);
            AddAbility(43236, 0d, 4d); // Tainted Radiance
            AddAbility(53527, 0d, 7d); // Deathbinding Bolt
            AddAbility(63588, 0d, 10d, veteranOnly: true); // Tainted Radiance
        }
    }

    /// <summary>
    /// Corrupted Deathbringer Dareia.
    /// </summary>
    [ScriptFilterCreatureId(28995, 28996)]
    public class SwordmaidenSanctuaryCorruptedDeathbringerDareiaAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [70223, 70224];

        public SwordmaidenSanctuaryCorruptedDeathbringerDareiaAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }

        public override void OnLoad(ICreatureEntity owner)
        {
            base.OnLoad(owner);
            AddAbility(43200, 0d, 4d); // Wicked Slash
            AddAbility(43226, 0d, 7d); // Encore
            AddAbility(43365, 0d, 10d); // Warrior's Charge
        }
    }

    /// <summary>
    /// Elder Moldwood Ravager.
    /// </summary>
    [ScriptFilterCreatureId(29198, 29199)]
    public class SwordmaidenSanctuaryElderMoldwoodRavagerAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [43327, 43328];

        public SwordmaidenSanctuaryElderMoldwoodRavagerAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }

        public override void OnLoad(ICreatureEntity owner)
        {
            base.OnLoad(owner);
            AddAbility(43087, 0d, 4d); // Ravage
            AddAbility(63421, 0d, 7d, veteranOnly: true); // Ravage
            AddAbility(63429, 0d, 10d, veteranOnly: true); // Strain Overgrowth
        }
    }

    /// <summary>
    /// Corrupted Elder Priestess.
    /// </summary>
    [ScriptFilterCreatureId(29222, 29223)]
    public class SwordmaidenSanctuaryCorruptedElderPriestessAI : ExpeditionCreatureAI
    {
        public SwordmaidenSanctuaryCorruptedElderPriestessAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }

        public override void OnLoad(ICreatureEntity owner)
        {
            base.OnLoad(owner);
            AddAbility(43236, 0d, 4d); // Tainted Radiance
            AddAbility(53527, 0d, 7d); // Deathbinding Bolt
        }
    }

    /// <summary>
    /// Corrupted Deathsting Swarmer.
    /// </summary>
    [ScriptFilterCreatureId(29248, 29249)]
    public class SwordmaidenSanctuaryCorruptedDeathstingSwarmerAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [70242, 70243];

        public SwordmaidenSanctuaryCorruptedDeathstingSwarmerAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }

        public override void OnLoad(ICreatureEntity owner)
        {
            base.OnLoad(owner);
            AddAbility(54655, 0d, 4d); // Stinger Charge
        }
    }

    /// <summary>
    /// Corrupted Deathsting Hive Defender.
    /// </summary>
    [ScriptFilterCreatureId(29251, 29252)]
    public class SwordmaidenSanctuaryCorruptedDeathstingHiveDefenderAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [70242, 70243];

        public SwordmaidenSanctuaryCorruptedDeathstingHiveDefenderAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }

        public override void OnLoad(ICreatureEntity owner)
        {
            base.OnLoad(owner);
            AddAbility(43672, 0d, 4d); // Honey Snare
            AddAbility(43675, 0d, 7d); // Death Sting
        }
    }

    /// <summary>
    /// Flame-Crazed Demon.
    /// </summary>
    [ScriptFilterCreatureId(29254, 29255)]
    public class SwordmaidenSanctuaryFlameCrazedDemonAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [70252, 70253];

        public SwordmaidenSanctuaryFlameCrazedDemonAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }

        public override void OnLoad(ICreatureEntity owner)
        {
            base.OnLoad(owner);
            AddAbility(43493, 0d, 4d); // Eruption
            AddAbility(63486, 0d, 7d, veteranOnly: true); // Consuming Flames
        }
    }

    /// <summary>
    /// Corrupted Veteran Swordmaiden.
    /// </summary>
    [ScriptFilterCreatureId(29266)]
    public class SwordmaidenSanctuaryCorruptedVeteranSwordmaidenAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [70223, 70224];

        public SwordmaidenSanctuaryCorruptedVeteranSwordmaidenAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }

        public override void OnLoad(ICreatureEntity owner)
        {
            base.OnLoad(owner);
            AddAbility(43200, 0d, 4d); // Wicked Slash
        }
    }

    /// <summary>
    /// Corrupted Deathbringer.
    /// </summary>
    [ScriptFilterCreatureId(29303, 29304)]
    public class SwordmaidenSanctuaryCorruptedDeathbringerAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [70219, 70220];

        public SwordmaidenSanctuaryCorruptedDeathbringerAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }

        public override void OnLoad(ICreatureEntity owner)
        {
            base.OnLoad(owner);
            AddAbility(42986, 0d, 4d); // Sundering Surge
        }
    }

    /// <summary>
    /// Moldwood Mauler.
    /// </summary>
    [ScriptFilterCreatureId(29310, 29311)]
    public class SwordmaidenSanctuaryMoldwoodMaulerAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [70245, 70246];

        public SwordmaidenSanctuaryMoldwoodMaulerAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }
    }

    /// <summary>
    /// Moldwood Peep.
    /// </summary>
    [ScriptFilterCreatureId(29804, 29805)]
    public class SwordmaidenSanctuaryMoldwoodPeepAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [4495, 4496];

        public SwordmaidenSanctuaryMoldwoodPeepAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }

        public override void OnLoad(ICreatureEntity owner)
        {
            base.OnLoad(owner);
            AddAbility(54224, 0d, 4d); // Corruption Pustule
        }
    }

    /// <summary>
    /// Moldwood Corruptor.
    /// </summary>
    [ScriptFilterCreatureId(30208, 30209)]
    public class SwordmaidenSanctuaryMoldwoodCorruptorAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [70229, 70230];

        public SwordmaidenSanctuaryMoldwoodCorruptorAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }

        public override void OnLoad(ICreatureEntity owner)
        {
            base.OnLoad(owner);
            AddAbility(43040, 0d, 4d); // Vile Spew
        }
    }

    /// <summary>
    /// Moldwood Swarmling.
    /// </summary>
    [ScriptFilterCreatureId(40393, 40396)]
    public class SwordmaidenSanctuaryMoldwoodSwarmlingAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [42053, 42054];

        public SwordmaidenSanctuaryMoldwoodSwarmlingAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }

        public override void OnLoad(ICreatureEntity owner)
        {
            base.OnLoad(owner);
            AddAbility(54224, 0d, 4d); // Corruption Pustule
        }
    }

    /// <summary>
    /// Gaze of the Moldwood.
    /// </summary>
    [ScriptFilterCreatureId(40402, 40404)]
    public class SwordmaidenSanctuaryGazeOfTheMoldwoodAI : ExpeditionCreatureAI
    {
        public SwordmaidenSanctuaryGazeOfTheMoldwoodAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }

        public override void OnLoad(ICreatureEntity owner)
        {
            base.OnLoad(owner);
            AddAbility(42056, 0d, 4d); // Death Gaze
            AddAbility(63466, 0d, 7d); // Death Gaze
        }
    }

    /// <summary>
    /// Blighted Moldwood Crawler.
    /// </summary>
    [ScriptFilterCreatureId(41203, 41204)]
    public class SwordmaidenSanctuaryBlightedMoldwoodCrawlerAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [70227, 70228];

        public SwordmaidenSanctuaryBlightedMoldwoodCrawlerAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }

        public override void OnLoad(ICreatureEntity owner)
        {
            base.OnLoad(owner);
            AddAbility(43066, 0d, 4d); // Deranged Paroxysm
        }
    }

    /// <summary>
    /// Corrupted Predator.
    /// </summary>
    [ScriptFilterCreatureId(41214, 41215)]
    public class SwordmaidenSanctuaryCorruptedPredatorAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [70221, 70222];

        public SwordmaidenSanctuaryCorruptedPredatorAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }
    }

    /// <summary>
    /// Hammerfist Moldjaw.
    /// </summary>
    [ScriptFilterCreatureId(41219, 41220)]
    public class SwordmaidenSanctuaryHammerfistMoldjawAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [70245, 70246];

        public SwordmaidenSanctuaryHammerfistMoldjawAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }

        public override void OnLoad(ICreatureEntity owner)
        {
            base.OnLoad(owner);
            AddAbility(43430, 0d, 4d); // Mesmerize
            AddAbility(63419, 0d, 7d, veteranOnly: true); // Mesmerize
        }
    }

    /// <summary>
    /// Blighted Moldwood Peep.
    /// </summary>
    [ScriptFilterCreatureId(43493, 57007)]
    public class SwordmaidenSanctuaryBlightedMoldwoodPeepAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [4495, 4496];

        public SwordmaidenSanctuaryBlightedMoldwoodPeepAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }

        public override void OnLoad(ICreatureEntity owner)
        {
            base.OnLoad(owner);
            AddAbility(54224, 0d, 4d); // Corruption Pustule
        }
    }

    /// <summary>
    /// Corrupted Spiderling.
    /// </summary>
    [ScriptFilterCreatureId(43946)]
    public class SwordmaidenSanctuaryCorruptedSpiderlingAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [70260, 70261];

        public SwordmaidenSanctuaryCorruptedSpiderlingAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }
    }

    /// <summary>
    /// Chime.
    /// </summary>
    [ScriptFilterCreatureId(52378)]
    public class SwordmaidenSanctuaryChimeAI : ExpeditionCreatureAI
    {
        public SwordmaidenSanctuaryChimeAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }

        public override void OnLoad(ICreatureEntity owner)
        {
            base.OnLoad(owner);
            AddAbility(42039, 0d, 4d); // Percussion
            AddAbility(54613, 0d, 7d); // Percussion
        }
    }
}
