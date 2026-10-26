using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Spell;
using NexusForever.GameTable;
using NexusForever.Script.Template.Filter;
using NexusForever.Script.Instance.Expedition;
using NexusForever.Shared;

namespace NexusForever.Script.Instance.Adventure.CrimelordsOfWhitevale.Script
{
    // Generated from the archived Jabbithole ability lists of each creature (normal and veteran pages), cooldowns from Spell4.

    /// <summary>
    /// Darkspur Gunman.
    /// </summary>
    [ScriptFilterCreatureId(23805)]
    public class CrimelordsDarkspurGunmanAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [65717];

        public CrimelordsDarkspurGunmanAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }
    }

    /// <summary>
    /// Darkspur Enforcer.
    /// </summary>
    [ScriptFilterCreatureId(23807)]
    public class CrimelordsDarkspurEnforcerAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [65950, 65951];

        public CrimelordsDarkspurEnforcerAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }

        public override void OnLoad(ICreatureEntity owner)
        {
            base.OnLoad(owner);
            AddAbility(66003, 0d, 4d); // Hack and Slash
        }
    }

    /// <summary>
    /// Darkspur Goon.
    /// </summary>
    [ScriptFilterCreatureId(23808)]
    public class CrimelordsDarkspurGoonAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [65743, 65744];

        public CrimelordsDarkspurGoonAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }

        public override void OnLoad(ICreatureEntity owner)
        {
            base.OnLoad(owner);
            AddAbility(66053, 0d, 4d); // Flourishing Combo
        }
    }

    /// <summary>
    /// Protostar Drillbot.
    /// </summary>
    [ScriptFilterCreatureId(23812)]
    public class CrimelordsProtostarDrillbotAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [65781, 65782];

        public CrimelordsProtostarDrillbotAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }

        public override void OnLoad(ICreatureEntity owner)
        {
            base.OnLoad(owner);
            AddAbility(66065, 0d, 4d); // Homing Drill
            AddAbility(66081, 0d, 7d); // Rapid Drill
        }
    }

    /// <summary>
    /// Protostar Battlebot.
    /// </summary>
    [ScriptFilterCreatureId(23816)]
    public class CrimelordsProtostarBattlebotAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [65735];

        public CrimelordsProtostarBattlebotAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }

        public override void OnLoad(ICreatureEntity owner)
        {
            base.OnLoad(owner);
            AddAbility(66148, 0d, 4d); // Chain Gun
        }
    }

    /// <summary>
    /// Protostar Hammerbot.
    /// </summary>
    [ScriptFilterCreatureId(23818)]
    public class CrimelordsProtostarHammerbotAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [65738, 65739];

        public CrimelordsProtostarHammerbotAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }

        public override void OnLoad(ICreatureEntity owner)
        {
            base.OnLoad(owner);
            AddAbility(66176, 0d, 4d); // Controlled Spin
        }
    }

    /// <summary>
    /// Redmoon Buccaneer.
    /// </summary>
    [ScriptFilterCreatureId(23838)]
    public class CrimelordsRedmoonBuccaneerAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [65717];

        public CrimelordsRedmoonBuccaneerAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }
    }

    /// <summary>
    /// Redmoon Pirate.
    /// </summary>
    [ScriptFilterCreatureId(23839)]
    public class CrimelordsRedmoonPirateAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [65717];

        public CrimelordsRedmoonPirateAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }
    }

    /// <summary>
    /// Redmoon Privateer.
    /// </summary>
    [ScriptFilterCreatureId(23840)]
    public class CrimelordsRedmoonPrivateerAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [65950, 65951];

        public CrimelordsRedmoonPrivateerAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }

        public override void OnLoad(ICreatureEntity owner)
        {
            base.OnLoad(owner);
            AddAbility(66003, 0d, 4d); // Hack and Slash
        }
    }

    /// <summary>
    /// Redmoon Scallywag.
    /// </summary>
    [ScriptFilterCreatureId(23841)]
    public class CrimelordsRedmoonScallywagAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [65950, 65951];

        public CrimelordsRedmoonScallywagAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }

        public override void OnLoad(ICreatureEntity owner)
        {
            base.OnLoad(owner);
            AddAbility(66003, 0d, 4d); // Hack and Slash
        }
    }

    /// <summary>
    /// Redmoon Deckhand.
    /// </summary>
    [ScriptFilterCreatureId(23842)]
    public class CrimelordsRedmoonDeckhandAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [65794, 65795];

        public CrimelordsRedmoonDeckhandAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }
    }

    /// <summary>
    /// Grinder Smashbot.
    /// </summary>
    [ScriptFilterCreatureId(23845)]
    public class CrimelordsGrinderSmashbotAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [65738, 65739];

        public CrimelordsGrinderSmashbotAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }

        public override void OnLoad(ICreatureEntity owner)
        {
            base.OnLoad(owner);
            AddAbility(66176, 0d, 4d); // Controlled Spin
        }
    }

    /// <summary>
    /// Thrasher Thug.
    /// </summary>
    [ScriptFilterCreatureId(23847)]
    public class CrimelordsThrasherThugAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [65743, 65744];

        public CrimelordsThrasherThugAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }

        public override void OnLoad(ICreatureEntity owner)
        {
            base.OnLoad(owner);
            AddAbility(66053, 0d, 4d); // Flourishing Combo
        }
    }

    /// <summary>
    /// Roller Ruffian.
    /// </summary>
    [ScriptFilterCreatureId(23848)]
    public class CrimelordsRollerRuffianAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [65950, 65951];

        public CrimelordsRollerRuffianAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }

        public override void OnLoad(ICreatureEntity owner)
        {
            base.OnLoad(owner);
            AddAbility(66003, 0d, 4d); // Hack and Slash
        }
    }

    /// <summary>
    /// Ravok Goon.
    /// </summary>
    [ScriptFilterCreatureId(23849)]
    public class CrimelordsRavokGoonAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [65715, 65716];

        public CrimelordsRavokGoonAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }
    }

    /// <summary>
    /// Darkspur Assassin.
    /// </summary>
    [ScriptFilterCreatureId(24705)]
    public class CrimelordsDarkspurAssassinAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [65794, 65795];

        public CrimelordsDarkspurAssassinAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }
    }

    /// <summary>
    /// Darkspur Plunderer.
    /// </summary>
    [ScriptFilterCreatureId(24715)]
    public class CrimelordsDarkspurPlundererAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [65717];

        public CrimelordsDarkspurPlundererAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }
    }

    /// <summary>
    /// Protostar Hackerbot.
    /// </summary>
    [ScriptFilterCreatureId(24737)]
    public class CrimelordsProtostarHackerbotAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [65765, 65766];

        public CrimelordsProtostarHackerbotAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }

        public override void OnLoad(ICreatureEntity owner)
        {
            base.OnLoad(owner);
            AddAbility(66176, 0d, 4d); // Controlled Spin
        }
    }

    /// <summary>
    /// Protostar Bot-Hunter.
    /// </summary>
    [ScriptFilterCreatureId(24757)]
    public class CrimelordsProtostarBotHunterAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [65738];

        public CrimelordsProtostarBotHunterAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }
    }

    /// <summary>
    /// Redmoon Torcher.
    /// </summary>
    [ScriptFilterCreatureId(24774)]
    public class CrimelordsRedmoonTorcherAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [65794, 65795];

        public CrimelordsRedmoonTorcherAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }
    }

    /// <summary>
    /// Redmoon Thug.
    /// </summary>
    [ScriptFilterCreatureId(25379)]
    public class CrimelordsRedmoonThugAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [65743, 65744];

        public CrimelordsRedmoonThugAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }
    }

    /// <summary>
    /// Biker.
    /// </summary>
    [ScriptFilterCreatureId(38050)]
    public class CrimelordsBikerAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [65717];

        public CrimelordsBikerAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }
    }

    /// <summary>
    /// Redmoon Negotiator.
    /// </summary>
    [ScriptFilterCreatureId(38415)]
    public class CrimelordsRedmoonNegotiatorAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [65717];

        public CrimelordsRedmoonNegotiatorAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }
    }

    /// <summary>
    /// Thermock Hater.
    /// </summary>
    [ScriptFilterCreatureId(38584)]
    public class CrimelordsThermockHaterAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [65743, 65744];

        public CrimelordsThermockHaterAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }
    }

    /// <summary>
    /// Darkspur Anklebiter.
    /// </summary>
    [ScriptFilterCreatureId(39129)]
    public class CrimelordsDarkspurAnklebiterAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [65814, 65815];

        public CrimelordsDarkspurAnklebiterAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }

        public override void OnLoad(ICreatureEntity owner)
        {
            base.OnLoad(owner);
            AddAbility(66011, 0d, 4d); // Feeding Frenzy
            AddAbility(66130, 0d, 7d); // Snap Trap
        }
    }

    /// <summary>
    /// Redmoon Mastclimber.
    /// </summary>
    [ScriptFilterCreatureId(39131)]
    public class CrimelordsRedmoonMastclimberAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [65947, 65948];

        public CrimelordsRedmoonMastclimberAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }

        public override void OnLoad(ICreatureEntity owner)
        {
            base.OnLoad(owner);
            AddAbility(65967, 0d, 4d); // Clawing Frenzy
        }
    }

    /// <summary>
    /// Redmoon Zookeeper.
    /// </summary>
    [ScriptFilterCreatureId(39132)]
    public class CrimelordsRedmoonZookeeperAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [65995];

        public CrimelordsRedmoonZookeeperAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }
    }

    /// <summary>
    /// Darkspur Packmaster.
    /// </summary>
    [ScriptFilterCreatureId(39133)]
    public class CrimelordsDarkspurPackmasterAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [65717];

        public CrimelordsDarkspurPackmasterAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }
    }

    /// <summary>
    /// Ravok Aggressor.
    /// </summary>
    [ScriptFilterCreatureId(39233)]
    public class CrimelordsRavokAggressorAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [65794, 65795];

        public CrimelordsRavokAggressorAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }
    }

    /// <summary>
    /// Roller Sharpshot.
    /// </summary>
    [ScriptFilterCreatureId(39234)]
    public class CrimelordsRollerSharpshotAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [65717];

        public CrimelordsRollerSharpshotAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }
    }

    /// <summary>
    /// Thrasher Blademaster.
    /// </summary>
    [ScriptFilterCreatureId(39358)]
    public class CrimelordsThrasherBlademasterAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [65950, 65951];

        public CrimelordsThrasherBlademasterAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }

        public override void OnLoad(ICreatureEntity owner)
        {
            base.OnLoad(owner);
            AddAbility(66003, 0d, 4d); // Hack and Slash
        }
    }

    /// <summary>
    /// Pa Thrasher.
    /// </summary>
    [ScriptFilterCreatureId(39362)]
    public class CrimelordsPaThrasherAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [65715, 65716];

        public CrimelordsPaThrasherAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }
    }

    /// <summary>
    /// Ma Thrasher.
    /// </summary>
    [ScriptFilterCreatureId(39373)]
    public class CrimelordsMaThrasherAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [65743, 65744];

        public CrimelordsMaThrasherAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }
    }

    /// <summary>
    /// Negotiation Assistant.
    /// </summary>
    [ScriptFilterCreatureId(39601)]
    public class CrimelordsNegotiationAssistantAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [65735, 65781];

        public CrimelordsNegotiationAssistantAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }

        public override void OnLoad(ICreatureEntity owner)
        {
            base.OnLoad(owner);
            AddAbility(66065, 0d, 4d); // Homing Drill
            AddAbility(66075, 0d, 7d); // Drill Grind
            AddAbility(66081, 0d, 10d); // Rapid Drill
            AddAbility(66148, 0d, 13d); // Chain Gun
        }
    }

    /// <summary>
    /// Redmoon Bruiser.
    /// </summary>
    [ScriptFilterCreatureId(39887)]
    public class CrimelordsRedmoonBruiserAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [65743, 65744];

        public CrimelordsRedmoonBruiserAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }

        public override void OnLoad(ICreatureEntity owner)
        {
            base.OnLoad(owner);
            AddAbility(66053, 0d, 4d); // Flourishing Combo
        }
    }

    /// <summary>
    /// Scars Bragg.
    /// </summary>
    [ScriptFilterCreatureId(40047)]
    public class CrimelordsScarsBraggAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [65950, 65951];

        public CrimelordsScarsBraggAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }

        public override void OnLoad(ICreatureEntity owner)
        {
            base.OnLoad(owner);
            AddAbility(66003, 0d, 4d); // Hack and Slash
            AddAbility(66105, 0d, 7d); // Onslaught
        }
    }

    /// <summary>
    /// Bigtime Zorgg.
    /// </summary>
    [ScriptFilterCreatureId(40051)]
    public class CrimelordsBigtimeZorggAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [65717];

        public CrimelordsBigtimeZorggAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }
    }

    /// <summary>
    /// Double Tap Vincenzok.
    /// </summary>
    [ScriptFilterCreatureId(40052)]
    public class CrimelordsDoubleTapVincenzokAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [65794, 65795];

        public CrimelordsDoubleTapVincenzokAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }
    }

    /// <summary>
    /// Hot Stuff Grazati.
    /// </summary>
    [ScriptFilterCreatureId(40054)]
    public class CrimelordsHotStuffGrazatiAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [65995];

        public CrimelordsHotStuffGrazatiAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }
    }

    /// <summary>
    /// Cap'n Barugh.
    /// </summary>
    [ScriptFilterCreatureId(40311)]
    public class CrimelordsCapNBarughAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [65950, 65951];

        public CrimelordsCapNBarughAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }
    }

    /// <summary>
    /// Haggar the Hammer.
    /// </summary>
    [ScriptFilterCreatureId(40406)]
    public class CrimelordsHaggarTheHammerAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [65952, 65953];

        public CrimelordsHaggarTheHammerAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }
    }

    /// <summary>
    /// Escaped Prisoner.
    /// </summary>
    [ScriptFilterCreatureId(40952)]
    public class CrimelordsEscapedPrisonerAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [65717, 65743];

        public CrimelordsEscapedPrisonerAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }
    }

    /// <summary>
    /// Protostar Picobot.
    /// </summary>
    [ScriptFilterCreatureId(47607)]
    public class CrimelordsProtostarPicobotAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [65736, 65737];

        public CrimelordsProtostarPicobotAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }

        public override void OnLoad(ICreatureEntity owner)
        {
            base.OnLoad(owner);
            AddAbility(66168, 0d, 4d); // Flame Barrier
        }
    }

    /// <summary>
    /// Darkspur Guard.
    /// </summary>
    [ScriptFilterCreatureId(52063)]
    public class CrimelordsDarkspurGuardAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [65717, 65950];

        public CrimelordsDarkspurGuardAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }

        public override void OnLoad(ICreatureEntity owner)
        {
            base.OnLoad(owner);
            AddAbility(66003, 0d, 4d); // Hack and Slash
        }
    }

    /// <summary>
    /// Redmoon Guard.
    /// </summary>
    [ScriptFilterCreatureId(52068)]
    public class CrimelordsRedmoonGuardAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [65717, 65950];

        public CrimelordsRedmoonGuardAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }

        public override void OnLoad(ICreatureEntity owner)
        {
            base.OnLoad(owner);
            AddAbility(66003, 0d, 4d); // Hack and Slash
        }
    }

    /// <summary>
    /// Protostar Guard.
    /// </summary>
    [ScriptFilterCreatureId(52074)]
    public class CrimelordsProtostarGuardAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [65738, 65739];

        public CrimelordsProtostarGuardAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }

        public override void OnLoad(ICreatureEntity owner)
        {
            base.OnLoad(owner);
            AddAbility(66176, 0d, 4d); // Controlled Spin
        }
    }

    /// <summary>
    /// Darkspur VIP.
    /// </summary>
    [ScriptFilterCreatureId(52075)]
    public class CrimelordsDarkspurVipAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [65950, 65951];

        public CrimelordsDarkspurVipAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }

        public override void OnLoad(ICreatureEntity owner)
        {
            base.OnLoad(owner);
            AddAbility(66003, 0d, 4d); // Hack and Slash
        }
    }

    /// <summary>
    /// Redmoon VIP.
    /// </summary>
    [ScriptFilterCreatureId(52076)]
    public class CrimelordsRedmoonVipAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [65950, 65951];

        public CrimelordsRedmoonVipAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }

        public override void OnLoad(ICreatureEntity owner)
        {
            base.OnLoad(owner);
            AddAbility(66003, 0d, 4d); // Hack and Slash
        }
    }

    /// <summary>
    /// Protostar VIB.
    /// </summary>
    [ScriptFilterCreatureId(52077)]
    public class CrimelordsProtostarVibAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [65781, 65782];

        public CrimelordsProtostarVibAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }

        public override void OnLoad(ICreatureEntity owner)
        {
            base.OnLoad(owner);
            AddAbility(66065, 0d, 4d); // Homing Drill
            AddAbility(66081, 0d, 7d); // Rapid Drill
        }
    }

    /// <summary>
    /// Overbot.
    /// </summary>
    [ScriptFilterCreatureId(52097)]
    public class CrimelordsOverbotAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [65733, 65734];

        public CrimelordsOverbotAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }
    }

    /// <summary>
    /// Darkspur Hitman.
    /// </summary>
    [ScriptFilterCreatureId(52173)]
    public class CrimelordsDarkspurHitmanAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [65794, 65795];

        public CrimelordsDarkspurHitmanAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }
    }

    /// <summary>
    /// Redmoon Hitman.
    /// </summary>
    [ScriptFilterCreatureId(52175)]
    public class CrimelordsRedmoonHitmanAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [65717];

        public CrimelordsRedmoonHitmanAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }
    }

    /// <summary>
    /// Onghr Supporter.
    /// </summary>
    [ScriptFilterCreatureId(52388)]
    public class CrimelordsOnghrSupporterAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [65717];

        public CrimelordsOnghrSupporterAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }
    }
}
