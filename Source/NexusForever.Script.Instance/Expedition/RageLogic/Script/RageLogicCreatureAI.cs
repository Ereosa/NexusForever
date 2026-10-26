using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Spell;
using NexusForever.GameTable;
using NexusForever.Script.Template.Filter;
using NexusForever.Shared;

namespace NexusForever.Script.Instance.Expedition.RageLogic.Script
{
    // Generated from the archived Jabbithole ability lists of each creature (normal and veteran pages), cooldowns from Spell4.

    /// <summary>
    /// Ragebot Swarmer.
    /// </summary>
    [ScriptFilterCreatureId(32302, 69096)]
    public class RageLogicRagebotSwarmerAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [4616, 4617];

        public RageLogicRagebotSwarmerAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }
    }

    /// <summary>
    /// Ragebot Infector.
    /// </summary>
    [ScriptFilterCreatureId(32303, 69098)]
    public class RageLogicRagebotInfectorAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [4616];

        public RageLogicRagebotInfectorAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }

        public override void OnLoad(ICreatureEntity owner)
        {
            base.OnLoad(owner);
            AddAbility(30999, 0d, 4d); // Slasher Dash
            AddAbility(41840, 0d, 7d); // Jump Shot
        }
    }

    /// <summary>
    /// Infected Freebot.
    /// </summary>
    [ScriptFilterCreatureId(32323, 69094)]
    public class RageLogicInfectedFreebotAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [4699, 4701];

        public RageLogicInfectedFreebotAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }

        public override void OnLoad(ICreatureEntity owner)
        {
            base.OnLoad(owner);
            AddAbility(42052, 0d, 4d); // Controlled Spin
            AddAbility(54916, 0d, 7d); // Steel Spikes
            AddAbility(54918, 0d, 10d); // Retrieval Protocol
        }
    }

    /// <summary>
    /// Ragebot Exterminator.
    /// </summary>
    [ScriptFilterCreatureId(32324, 69097)]
    public class RageLogicRagebotExterminatorAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [4537];

        public RageLogicRagebotExterminatorAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }

        public override void OnLoad(ICreatureEntity owner)
        {
            base.OnLoad(owner);
            AddAbility(39474, 0d, 4d); // Chain Gun
            AddAbility(52788, 0d, 7d); // Meltdown Missile
            AddAbility(54212, 0d, 10d); // Default Attack Sequence
        }
    }

    /// <summary>
    /// Ragebot Driller.
    /// </summary>
    [ScriptFilterCreatureId(32325, 69100)]
    public class RageLogicRagebotDrillerAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [24990, 24991];

        public RageLogicRagebotDrillerAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }

        public override void OnLoad(ICreatureEntity owner)
        {
            base.OnLoad(owner);
            AddAbility(34585, 0d, 4d, veteranOnly: true); // Homing Drill
            AddAbility(34982, 0d, 7d); // Drill Grind
            AddAbility(35223, 0d, 10d); // Rapid Drill
        }
    }

    /// <summary>
    /// Axiom.
    /// </summary>
    [ScriptFilterCreatureId(32326, 69099)]
    public class RageLogicAxiomAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [24626, 24627];

        public RageLogicAxiomAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }

        public override void OnLoad(ICreatureEntity owner)
        {
            base.OnLoad(owner);
            AddAbility(38625, 0d, 4d); // Missile Swarm
            AddAbility(38727, 0d, 7d); // Exterminate
            AddAbility(43502, 0d, 10d); // Smash 'n Trash
            AddAbility(58548, 0d, 13d); // Liquefy
            AddAbility(58550, 0d, 16d); // Sock 'Em
        }
    }

    /// <summary>
    /// Ragebot Enforcer.
    /// </summary>
    [ScriptFilterCreatureId(43273, 69103)]
    public class RageLogicRagebotEnforcerAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [4537];

        public RageLogicRagebotEnforcerAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }

        public override void OnLoad(ICreatureEntity owner)
        {
            base.OnLoad(owner);
            AddAbility(39474, 0d, 4d); // Chain Gun
            AddAbility(52788, 0d, 7d); // Meltdown Missile
            AddAbility(54212, 0d, 10d); // Default Attack Sequence
        }
    }

    /// <summary>
    /// Rage Scrapper.
    /// </summary>
    [ScriptFilterCreatureId(43276, 69104)]
    public class RageLogicRageScrapperAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [4699, 4701];

        public RageLogicRageScrapperAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }

        public override void OnLoad(ICreatureEntity owner)
        {
            base.OnLoad(owner);
            AddAbility(54916, 0d, 4d); // Steel Spikes
        }
    }

    /// <summary>
    /// Rage Picker.
    /// </summary>
    [ScriptFilterCreatureId(43284, 69105)]
    public class RageLogicRagePickerAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [4616, 4617];

        public RageLogicRagePickerAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }

        public override void OnLoad(ICreatureEntity owner)
        {
            base.OnLoad(owner);
            AddAbility(40714, 0d, 4d); // Flame Barrier
            AddAbility(72728, 0d, 7d); // Phaser Combo
        }
    }

    /// <summary>
    /// Ragebot Scanner.
    /// </summary>
    [ScriptFilterCreatureId(43827, 69106)]
    public class RageLogicRagebotScannerAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [4537];

        public RageLogicRagebotScannerAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }

        public override void OnLoad(ICreatureEntity owner)
        {
            base.OnLoad(owner);
            AddAbility(39474, 0d, 4d); // Chain Gun
            AddAbility(52788, 0d, 7d); // Meltdown Missile
            AddAbility(54212, 0d, 10d); // Default Attack Sequence
        }
    }

    /// <summary>
    /// Trash Crawler.
    /// </summary>
    [ScriptFilterCreatureId(43879, 69179)]
    public class RageLogicTrashCrawlerAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [4616, 4617];

        public RageLogicTrashCrawlerAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }

        public override void OnLoad(ICreatureEntity owner)
        {
            base.OnLoad(owner);
            AddAbility(40714, 0d, 4d); // Flame Barrier
        }
    }

    /// <summary>
    /// Overseerbot.
    /// </summary>
    [ScriptFilterCreatureId(44232, 69107)]
    public class RageLogicOverseerbotAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [4537];

        public RageLogicOverseerbotAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }

        public override void OnLoad(ICreatureEntity owner)
        {
            base.OnLoad(owner);
            AddAbility(39474, 0d, 4d); // Chain Gun
            AddAbility(52788, 0d, 7d); // Meltdown Missile
            AddAbility(54212, 0d, 10d); // Default Attack Sequence
        }
    }

    /// <summary>
    /// Broadcast Transmitter.
    /// </summary>
    [ScriptFilterCreatureId(69807)]
    public class RageLogicBroadcastTransmitterAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [24626, 24627];

        public RageLogicBroadcastTransmitterAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }

        public override void OnLoad(ICreatureEntity owner)
        {
            base.OnLoad(owner);
            AddAbility(38625, 0d, 4d); // Missile Swarm
            AddAbility(38727, 0d, 7d); // Exterminate
            AddAbility(43502, 0d, 10d); // Smash 'n Trash
        }
    }
}
