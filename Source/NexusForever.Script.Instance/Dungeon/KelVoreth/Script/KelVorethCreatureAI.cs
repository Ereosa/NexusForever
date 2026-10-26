using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Spell;
using NexusForever.GameTable;
using NexusForever.Script.Instance.Expedition;
using NexusForever.Script.Template.Filter;
using NexusForever.Shared;

namespace NexusForever.Script.Instance.Dungeon.KelVoreth.Script
{
    // Generated from the archived Jabbithole ability lists of each creature (normal and veteran pages), cooldowns from Spell4.

    /// <summary>
    /// Voreth Mechano Slaver.
    /// </summary>
    [ScriptFilterCreatureId(32547, 32549)]
    public class KelVorethVorethMechanoSlaverAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [49911, 49912];

        public KelVorethVorethMechanoSlaverAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }

        public override void OnLoad(ICreatureEntity owner)
        {
            base.OnLoad(owner);
            AddAbility(49913, 0d, 4d); // Mighty Swing
            AddAbility(49915, 0d, 7d); // Hammer Down
            AddAbility(49943, 0d, 10d); // Stomp
        }
    }

    /// <summary>
    /// Voreth Battlesworn.
    /// </summary>
    [ScriptFilterCreatureId(32555, 32556)]
    public class KelVorethVorethBattleswornAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [49911, 49912];

        public KelVorethVorethBattleswornAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }

        public override void OnLoad(ICreatureEntity owner)
        {
            base.OnLoad(owner);
            AddAbility(40692, 0d, 4d); // Hammer of Wrath
        }
    }

    /// <summary>
    /// Voreth Beastmaster.
    /// </summary>
    [ScriptFilterCreatureId(32606, 32607)]
    public class KelVorethVorethBeastmasterAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [49911, 49912];

        public KelVorethVorethBeastmasterAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }

        public override void OnLoad(ICreatureEntity owner)
        {
            base.OnLoad(owner);
            AddAbility(51338, 0d, 4d); // Pulverize
        }
    }

    /// <summary>
    /// Voreth Blinded Slaver.
    /// </summary>
    [ScriptFilterCreatureId(32614, 32613)]
    public class KelVorethVorethBlindedSlaverAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [51967];

        public KelVorethVorethBlindedSlaverAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }

        public override void OnLoad(ICreatureEntity owner)
        {
            base.OnLoad(owner);
            AddAbility(45606, 0d, 4d); // Conflagrate
            AddAbility(51968, 0d, 7d); // Magma Plume
        }
    }

    /// <summary>
    /// Voreth Darkwitch.
    /// </summary>
    [ScriptFilterCreatureId(32618, 32619)]
    public class KelVorethVorethDarkwitchAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [51965];

        public KelVorethVorethDarkwitchAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }

        public override void OnLoad(ICreatureEntity owner)
        {
            base.OnLoad(owner);
            AddAbility(40708, 0d, 4d); // Dread
            AddAbility(50282, 0d, 7d); // Rupture
            AddAbility(50283, 0d, 10d); // Rupture
        }
    }

    /// <summary>
    /// Kel Voreth Slave.
    /// </summary>
    [ScriptFilterCreatureId(32621)]
    public class KelVorethKelVorethSlaveAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [5649, 5652];

        public KelVorethKelVorethSlaveAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }

        public override void OnLoad(ICreatureEntity owner)
        {
            base.OnLoad(owner);
            AddAbility(44220, 0d, 4d); // Break the Chains
        }
    }

    /// <summary>
    /// Voreth Warhound.
    /// </summary>
    [ScriptFilterCreatureId(32727, 32726)]
    public class KelVorethVorethWarhoundAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [71008, 71009];

        public KelVorethVorethWarhoundAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }
    }

    /// <summary>
    /// Darkwitch Gurka.
    /// </summary>
    [ScriptFilterCreatureId(33049, 33050)]
    public class KelVorethDarkwitchGurkaAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [51965, 63505];

        public KelVorethDarkwitchGurkaAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }

        public override void OnLoad(ICreatureEntity owner)
        {
            base.OnLoad(owner);
            AddAbility(44315, 0d, 4d); // Blinding Dark
            AddAbility(63508, 0d, 7d, veteranOnly: true); // Blinding Dark
        }
    }

    /// <summary>
    /// Eldan Phase Blaster.
    /// </summary>
    [ScriptFilterCreatureId(33090)]
    public class KelVorethEldanPhaseBlasterAI : ExpeditionCreatureAI
    {
        public KelVorethEldanPhaseBlasterAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }

        public override void OnLoad(ICreatureEntity owner)
        {
            base.OnLoad(owner);
            AddAbility(36864, 0d, 4d); // Phase Blast
        }
    }

    /// <summary>
    /// Bloodsucker Flea.
    /// </summary>
    [ScriptFilterCreatureId(33837, 56485)]
    public class KelVorethBloodsuckerFleaAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [40361];

        public KelVorethBloodsuckerFleaAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }

        public override void OnLoad(ICreatureEntity owner)
        {
            base.OnLoad(owner);
            AddAbility(63099, 0d, 4d); // Bloodburst
        }
    }

    /// <summary>
    /// Destructo Construct.
    /// </summary>
    [ScriptFilterCreatureId(35424, 56496)]
    public class KelVorethDestructoConstructAI : ExpeditionCreatureAI
    {
        public KelVorethDestructoConstructAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }

        public override void OnLoad(ICreatureEntity owner)
        {
            base.OnLoad(owner);
            AddAbility(39202, 0d, 4d); // Self-Destruct
            AddAbility(39301, 0d, 7d); // Self-Destruct
        }
    }

    /// <summary>
    /// Bombshell Construct.
    /// </summary>
    [ScriptFilterCreatureId(35538, 56497)]
    public class KelVorethBombshellConstructAI : ExpeditionCreatureAI
    {
        public KelVorethBombshellConstructAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }

        public override void OnLoad(ICreatureEntity owner)
        {
            base.OnLoad(owner);
            AddAbility(39303, 0d, 4d); // Detonate
        }
    }

    /// <summary>
    /// Enslaved Protector.
    /// </summary>
    [ScriptFilterCreatureId(37110, 37111)]
    public class KelVorethEnslavedProtectorAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [71002, 71003];

        public KelVorethEnslavedProtectorAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }

        public override void OnLoad(ICreatureEntity owner)
        {
            base.OnLoad(owner);
            AddAbility(40712, 0d, 4d); // Lockdown
            AddAbility(40713, 0d, 7d); // Resupply
        }
    }

    /// <summary>
    /// Enslaved Augmentor.
    /// </summary>
    [ScriptFilterCreatureId(37113, 37114)]
    public class KelVorethEnslavedAugmentorAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [71006, 71007];

        public KelVorethEnslavedAugmentorAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }

        public override void OnLoad(ICreatureEntity owner)
        {
            base.OnLoad(owner);
            AddAbility(44468, 0d, 4d); // Dual Saw
        }
    }

    /// <summary>
    /// Blood Pit Slave.
    /// </summary>
    [ScriptFilterCreatureId(37119)]
    public class KelVorethBloodPitSlaveAI : ExpeditionCreatureAI
    {
        public KelVorethBloodPitSlaveAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }

        public override void OnLoad(ICreatureEntity owner)
        {
            base.OnLoad(owner);
            AddAbility(51345, 0d, 4d); // Punch
            AddAbility(51346, 0d, 7d); // Punch
        }
    }

    /// <summary>
    /// Bloodspawn Flea.
    /// </summary>
    [ScriptFilterCreatureId(37200, 56575)]
    public class KelVorethBloodspawnFleaAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [4012, 4013];

        public KelVorethBloodspawnFleaAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }
    }

    /// <summary>
    /// Enslaved Probe.
    /// </summary>
    [ScriptFilterCreatureId(37791, 37792)]
    public class KelVorethEnslavedProbeAI : ExpeditionCreatureAI
    {
        public KelVorethEnslavedProbeAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }

        public override void OnLoad(ICreatureEntity owner)
        {
            base.OnLoad(owner);
            AddAbility(40718, 0d, 4d); // Self-Destruct
            AddAbility(71004, 0d, 7d); // Barrage
            AddAbility(71005, 0d, 10d); // Barrage
        }
    }

    /// <summary>
    /// Blood Pit Necromancer.
    /// </summary>
    [ScriptFilterCreatureId(50991, 50992)]
    public class KelVorethBloodPitNecromancerAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [51965];

        public KelVorethBloodPitNecromancerAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }

        public override void OnLoad(ICreatureEntity owner)
        {
            base.OnLoad(owner);
            AddAbility(50304, 0d, 4d); // Wail of the Darkwitch
        }
    }

    /// <summary>
    /// Blood Pit Gladiator.
    /// </summary>
    [ScriptFilterCreatureId(51068, 51069)]
    public class KelVorethBloodPitGladiatorAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [49911, 49912];

        public KelVorethBloodPitGladiatorAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }

        public override void OnLoad(ICreatureEntity owner)
        {
            base.OnLoad(owner);
            AddAbility(49913, 0d, 4d); // Mighty Swing
            AddAbility(50318, 0d, 7d); // Gladiator Rampage
        }
    }

    /// <summary>
    /// Fissure.
    /// </summary>
    [ScriptFilterCreatureId(51431, 67360)]
    public class KelVorethFissureAI : ExpeditionCreatureAI
    {
        public KelVorethFissureAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }

        public override void OnLoad(ICreatureEntity owner)
        {
            base.OnLoad(owner);
            AddAbility(51991, 0d, 4d); // Inferno
            AddAbility(52014, 0d, 7d); // Inferno
        }
    }

    /// <summary>
    /// Voreth Necromancer.
    /// </summary>
    [ScriptFilterCreatureId(51432, 51433)]
    public class KelVorethVorethNecromancerAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [51965];

        public KelVorethVorethNecromancerAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }

        public override void OnLoad(ICreatureEntity owner)
        {
            base.OnLoad(owner);
            AddAbility(50304, 0d, 4d); // Wail of the Darkwitch
            AddAbility(51975, 0d, 7d); // Raise Dead
        }
    }

    /// <summary>
    /// Voreth Gladiator.
    /// </summary>
    [ScriptFilterCreatureId(51453, 51454)]
    public class KelVorethVorethGladiatorAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [49911, 49912];

        public KelVorethVorethGladiatorAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }

        public override void OnLoad(ICreatureEntity owner)
        {
            base.OnLoad(owner);
            AddAbility(49913, 0d, 4d); // Mighty Swing
            AddAbility(50318, 0d, 7d); // Gladiator Rampage
        }
    }

    /// <summary>
    /// Flame Monument.
    /// </summary>
    [ScriptFilterCreatureId(52572, 67362)]
    public class KelVorethFlameMonumentAI : ExpeditionCreatureAI
    {
        public KelVorethFlameMonumentAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }

        public override void OnLoad(ICreatureEntity owner)
        {
            base.OnLoad(owner);
            AddAbility(53077, 0d, 4d); // Molten Breath
        }
    }

    /// <summary>
    /// Lava Spout.
    /// </summary>
    [ScriptFilterCreatureId(52573, 67364)]
    public class KelVorethLavaSpoutAI : ExpeditionCreatureAI
    {
        public KelVorethLavaSpoutAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }

        public override void OnLoad(ICreatureEntity owner)
        {
            base.OnLoad(owner);
            AddAbility(54819, 0d, 4d); // Molten Gusher
        }
    }
}
