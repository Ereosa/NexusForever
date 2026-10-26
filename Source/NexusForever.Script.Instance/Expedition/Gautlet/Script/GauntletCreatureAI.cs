using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Spell;
using NexusForever.GameTable;
using NexusForever.Script.Template.Filter;
using NexusForever.Shared;

namespace NexusForever.Script.Instance.Expedition.Gautlet.Script
{
    // Generated from the archived Jabbithole ability lists of each creature (normal and veteran pages), cooldowns from Spell4.

    /// <summary>
    /// Bash Brother.
    /// </summary>
    [ScriptFilterCreatureId(48454, 69280)]
    public class GauntletBashBrotherAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [4537];

        public GauntletBashBrotherAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }

        public override void OnLoad(ICreatureEntity owner)
        {
            base.OnLoad(owner);
            AddAbility(39474, 0d, 4d); // Chain Gun
            AddAbility(52788, 0d, 7d, veteranOnly: true); // Meltdown Missile
            AddAbility(54212, 0d, 10d); // Default Attack Sequence
        }
    }

    /// <summary>
    /// Bash Brother.
    /// </summary>
    [ScriptFilterCreatureId(48458, 69281)]
    public class GauntletBashBrother2AI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [4537];

        public GauntletBashBrother2AI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }

        public override void OnLoad(ICreatureEntity owner)
        {
            base.OnLoad(owner);
            AddAbility(39474, 0d, 4d); // Chain Gun
            AddAbility(52788, 0d, 7d, veteranOnly: true); // Meltdown Missile
            AddAbility(54212, 0d, 10d); // Default Attack Sequence
        }
    }

    /// <summary>
    /// Arena-Frenzied Chomper.
    /// </summary>
    [ScriptFilterCreatureId(48461, 69237)]
    public class GauntletArenaFrenziedChomperAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [28936, 28937];

        public GauntletArenaFrenziedChomperAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }
    }

    /// <summary>
    /// Rockstar Yeti.
    /// </summary>
    [ScriptFilterCreatureId(48491, 69254)]
    public class GauntletRockstarYetiAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [33251, 33252];

        public GauntletRockstarYetiAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }

        public override void OnLoad(ICreatureEntity owner)
        {
            base.OnLoad(owner);
            AddAbility(52968, 0d, 4d); // Boom Slap
            AddAbility(53034, 0d, 7d); // Shouting Frenzy
            AddAbility(74985, 0d, 10d); // Cumulative Strikes
            AddAbility(74991, 0d, 13d); // Ionized Exhaust
            AddAbility(74992, 0d, 16d); // Forced Aggression
        }
    }

    /// <summary>
    /// Pyro.
    /// </summary>
    [ScriptFilterCreatureId(48493, 69300)]
    public class GauntletPyroAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [2764];

        public GauntletPyroAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }

        public override void OnLoad(ICreatureEntity owner)
        {
            base.OnLoad(owner);
            AddAbility(34299, 0d, 4d); // Flametorch
        }
    }

    /// <summary>
    /// The Morticianatrix.
    /// </summary>
    [ScriptFilterCreatureId(48499, 69257)]
    public class GauntletTheMorticianatrixAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [34062];

        public GauntletTheMorticianatrixAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }

        public override void OnLoad(ICreatureEntity owner)
        {
            base.OnLoad(owner);
            AddAbility(55595, 0d, 4d); // Ring of Thorns
        }
    }

    /// <summary>
    /// Maniac.
    /// </summary>
    [ScriptFilterCreatureId(48510, 69301)]
    public class GauntletManiacAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [32920, 32921];

        public GauntletManiacAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
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
    /// Professor Doctor Voodoo.
    /// </summary>
    [ScriptFilterCreatureId(48511, 69258)]
    public class GauntletProfessorDoctorVoodooAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [30756];

        public GauntletProfessorDoctorVoodooAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }

        public override void OnLoad(ICreatureEntity owner)
        {
            base.OnLoad(owner);
            AddAbility(55683, 0d, 4d); // Earthquake
            AddAbility(55700, 0d, 7d); // Boulder Dash
        }
    }

    /// <summary>
    /// The Handler.
    /// </summary>
    [ScriptFilterCreatureId(48516, 69302)]
    public class GauntletTheHandlerAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [32867, 32868];

        public GauntletTheHandlerAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }

        public override void OnLoad(ICreatureEntity owner)
        {
            base.OnLoad(owner);
            AddAbility(34131, 0d, 4d); // Hack and Slash
            AddAbility(49350, 0d, 7d); // Double Strike
        }
    }

    /// <summary>
    /// Chompers.
    /// </summary>
    [ScriptFilterCreatureId(48518, 69303)]
    public class GauntletChompersAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [28936, 28937];

        public GauntletChompersAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }

        public override void OnLoad(ICreatureEntity owner)
        {
            base.OnLoad(owner);
            AddAbility(34264, 0d, 4d); // Feeding Frenzy
            AddAbility(36414, 0d, 7d); // Snap Trap
        }
    }

    /// <summary>
    /// Spud.
    /// </summary>
    [ScriptFilterCreatureId(48519, 69304)]
    public class GauntletSpudAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [28936, 28937];

        public GauntletSpudAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }

        public override void OnLoad(ICreatureEntity owner)
        {
            base.OnLoad(owner);
            AddAbility(34264, 0d, 4d); // Feeding Frenzy
            AddAbility(36414, 0d, 7d); // Snap Trap
        }
    }

    /// <summary>
    /// The Championator.
    /// </summary>
    [ScriptFilterCreatureId(48529, 69255)]
    public class GauntletTheChampionatorAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [5649, 5652];

        public GauntletTheChampionatorAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }

        public override void OnLoad(ICreatureEntity owner)
        {
            base.OnLoad(owner);
            AddAbility(34552, 0d, 4d); // Flourishing Combo
            AddAbility(52448, 0d, 7d); // Haymaker
        }
    }

    /// <summary>
    /// Shock King.
    /// </summary>
    [ScriptFilterCreatureId(48554, 69305)]
    public class GauntletShockKingAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [34062];

        public GauntletShockKingAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
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
            AddAbility(74700, 0d, 16d); // Sandstorm
            AddAbility(74724, 0d, 19d); // Ground Support
        }
    }

    /// <summary>
    /// Slice.
    /// </summary>
    [ScriptFilterCreatureId(48557, 69306)]
    public class GauntletSliceAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [32867, 32868];

        public GauntletSliceAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }

        public override void OnLoad(ICreatureEntity owner)
        {
            base.OnLoad(owner);
            AddAbility(50007, 0d, 4d); // Tempest Strike
            AddAbility(50103, 0d, 7d); // Kiss of the Wind
            AddAbility(50289, 0d, 10d); // Biting Wind
            AddAbility(51329, 0d, 13d); // Storm of Vengeance
        }
    }

    /// <summary>
    /// Dice.
    /// </summary>
    [ScriptFilterCreatureId(48558, 69307)]
    public class GauntletDiceAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [32867, 32868];

        public GauntletDiceAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }

        public override void OnLoad(ICreatureEntity owner)
        {
            base.OnLoad(owner);
            AddAbility(50007, 0d, 4d); // Tempest Strike
            AddAbility(50103, 0d, 7d); // Kiss of the Wind
            AddAbility(50289, 0d, 10d); // Biting Wind
            AddAbility(51329, 0d, 13d); // Storm of Vengeance
        }
    }

    /// <summary>
    /// Showtime.
    /// </summary>
    [ScriptFilterCreatureId(48579, 69308)]
    public class GauntletShowtimeAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [32867, 32868];

        public GauntletShowtimeAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
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
        }
    }

    /// <summary>
    /// Bonebreaker Zoragg.
    /// </summary>
    [ScriptFilterCreatureId(48588, 69309)]
    public class GauntletBonebreakerZoraggAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [32867, 32868];

        public GauntletBonebreakerZoraggAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }

        public override void OnLoad(ICreatureEntity owner)
        {
            base.OnLoad(owner);
            AddAbility(35717, 0d, 4d); // Whirling Wind
            AddAbility(50103, 0d, 7d); // Kiss of the Wind
        }
    }

    /// <summary>
    /// Fist.
    /// </summary>
    [ScriptFilterCreatureId(48589, 69310)]
    public class GauntletFistAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [5649, 5652];

        public GauntletFistAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }

        public override void OnLoad(ICreatureEntity owner)
        {
            base.OnLoad(owner);
            AddAbility(34552, 0d, 4d); // Flourishing Combo
            AddAbility(52448, 0d, 7d); // Haymaker
        }
    }

    /// <summary>
    /// Goonbot.
    /// </summary>
    [ScriptFilterCreatureId(48591, 69311)]
    public class GauntletGoonbotAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [4537];

        public GauntletGoonbotAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }

        public override void OnLoad(ICreatureEntity owner)
        {
            base.OnLoad(owner);
            AddAbility(39474, 0d, 4d); // Chain Gun
            AddAbility(54212, 0d, 7d); // Default Attack Sequence
        }
    }

    /// <summary>
    /// Brick Braggor.
    /// </summary>
    [ScriptFilterCreatureId(48618, 69688)]
    public class GauntletBrickBraggorAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [32867, 32868];

        public GauntletBrickBraggorAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }

        public override void OnLoad(ICreatureEntity owner)
        {
            base.OnLoad(owner);
            AddAbility(49792, 0d, 4d, veteranOnly: true); // Searing Strikes
            AddAbility(51383, 0d, 7d, veteranOnly: true); // Melt Armor
        }
    }

    /// <summary>
    /// Dominion Garr.
    /// </summary>
    [ScriptFilterCreatureId(48666, 69282)]
    public class GauntletDominionGarrAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [6275, 6276];

        public GauntletDominionGarrAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }
    }

    /// <summary>
    /// Exile Hound.
    /// </summary>
    [ScriptFilterCreatureId(48667, 69283)]
    public class GauntletExileHoundAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [372, 1616];

        public GauntletExileHoundAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }
    }

    /// <summary>
    /// Houndmaster Hagan.
    /// </summary>
    [ScriptFilterCreatureId(48669, 69284)]
    public class GauntletHoundmasterHaganAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [2764];

        public GauntletHoundmasterHaganAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }

        public override void OnLoad(ICreatureEntity owner)
        {
            base.OnLoad(owner);
            AddAbility(51828, 0d, 4d); // Shell Storm
            AddAbility(51899, 0d, 7d); // Longshot
        }
    }

    /// <summary>
    /// Garrmaster Grazza.
    /// </summary>
    [ScriptFilterCreatureId(48670, 69285)]
    public class GauntletGarrmasterGrazzaAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [2764];

        public GauntletGarrmasterGrazzaAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }

        public override void OnLoad(ICreatureEntity owner)
        {
            base.OnLoad(owner);
            AddAbility(51828, 0d, 4d); // Shell Storm
            AddAbility(51899, 0d, 7d); // Longshot
        }
    }

    /// <summary>
    /// Pistolier Jakra.
    /// </summary>
    [ScriptFilterCreatureId(48689, 69286)]
    public class GauntletPistolierJakraAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [27545, 27546];

        public GauntletPistolierJakraAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }

        public override void OnLoad(ICreatureEntity owner)
        {
            base.OnLoad(owner);
            AddAbility(54827, 0d, 4d); // Wild Barrage
        }
    }

    /// <summary>
    /// Deathblade Vytene.
    /// </summary>
    [ScriptFilterCreatureId(48695, 69287)]
    public class GauntletDeathbladeVyteneAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [32867, 32868];

        public GauntletDeathbladeVyteneAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }

        public override void OnLoad(ICreatureEntity owner)
        {
            base.OnLoad(owner);
            AddAbility(34131, 0d, 4d); // Hack and Slash
            AddAbility(49350, 0d, 7d); // Double Strike
        }
    }

    /// <summary>
    /// Nightstalker Accia.
    /// </summary>
    [ScriptFilterCreatureId(48697, 69288)]
    public class GauntletNightstalkerAcciaAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [2605, 2607];

        public GauntletNightstalkerAcciaAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }

        public override void OnLoad(ICreatureEntity owner)
        {
            base.OnLoad(owner);
            AddAbility(54241, 0d, 4d); // Slashing Strikes
            AddAbility(54299, 0d, 7d); // Feet of Fury
        }
    }

    /// <summary>
    /// Gunslinger Zynri.
    /// </summary>
    [ScriptFilterCreatureId(48721, 69289)]
    public class GauntletGunslingerZynriAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [27545, 27546];

        public GauntletGunslingerZynriAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }

        public override void OnLoad(ICreatureEntity owner)
        {
            base.OnLoad(owner);
            AddAbility(54827, 0d, 4d); // Wild Barrage
        }
    }

    /// <summary>
    /// Skullcrusher Panzox.
    /// </summary>
    [ScriptFilterCreatureId(48723, 69290)]
    public class GauntletSkullcrusherPanzoxAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [32867, 32868];

        public GauntletSkullcrusherPanzoxAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }

        public override void OnLoad(ICreatureEntity owner)
        {
            base.OnLoad(owner);
            AddAbility(34131, 0d, 4d); // Hack and Slash
            AddAbility(49350, 0d, 7d); // Double Strike
        }
    }

    /// <summary>
    /// Shadowblade Ivan.
    /// </summary>
    [ScriptFilterCreatureId(48728, 69291)]
    public class GauntletShadowbladeIvanAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [2605, 2607];

        public GauntletShadowbladeIvanAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }

        public override void OnLoad(ICreatureEntity owner)
        {
            base.OnLoad(owner);
            AddAbility(54241, 0d, 4d); // Slashing Strikes
            AddAbility(54299, 0d, 7d); // Feet of Fury
        }
    }

    /// <summary>
    /// Arena-Frenzied Shocker.
    /// </summary>
    [ScriptFilterCreatureId(48765, 69239)]
    public class GauntletArenaFrenziedShockerAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [6909, 6910];

        public GauntletArenaFrenziedShockerAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }
    }

    /// <summary>
    /// Arena-Frenzied Moodie.
    /// </summary>
    [ScriptFilterCreatureId(48793, 69240)]
    public class GauntletArenaFrenziedMoodieAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [5960, 5961];

        public GauntletArenaFrenziedMoodieAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }
    }

    /// <summary>
    /// Arena-Frenzied Skitterbot.
    /// </summary>
    [ScriptFilterCreatureId(48833, 69241)]
    public class GauntletArenaFrenziedSkitterbotAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [4616, 4617];

        public GauntletArenaFrenziedSkitterbotAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }
    }

    /// <summary>
    /// Arena-Frenzied Scurrier.
    /// </summary>
    [ScriptFilterCreatureId(48835, 69242)]
    public class GauntletArenaFrenziedScurrierAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [33242, 33243];

        public GauntletArenaFrenziedScurrierAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }
    }

    /// <summary>
    /// Arena-Frenzied Razortail.
    /// </summary>
    [ScriptFilterCreatureId(48842, 69243)]
    public class GauntletArenaFrenziedRazortailAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [32773, 32774];

        public GauntletArenaFrenziedRazortailAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }
    }

    /// <summary>
    /// Arena-Frenzied Peeper.
    /// </summary>
    [ScriptFilterCreatureId(48843, 69244)]
    public class GauntletArenaFrenziedPeeperAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [4495, 4496];

        public GauntletArenaFrenziedPeeperAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }
    }

    /// <summary>
    /// Arena-Frenzied Skeech.
    /// </summary>
    [ScriptFilterCreatureId(48844, 69246)]
    public class GauntletArenaFrenziedSkeechAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [5649, 5652];

        public GauntletArenaFrenziedSkeechAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }
    }

    /// <summary>
    /// Arena-Frenzied Arachnid.
    /// </summary>
    [ScriptFilterCreatureId(48863, 69247)]
    public class GauntletArenaFrenziedArachnidAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [6651, 6652];

        public GauntletArenaFrenziedArachnidAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }
    }

    /// <summary>
    /// Arena-Frenzied Spikehorde.
    /// </summary>
    [ScriptFilterCreatureId(48865, 69248)]
    public class GauntletArenaFrenziedSpikehordeAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [4012, 4013];

        public GauntletArenaFrenziedSpikehordeAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }
    }

    /// <summary>
    /// Arena-Frenzied Stumpkin.
    /// </summary>
    [ScriptFilterCreatureId(48866)]
    public class GauntletArenaFrenziedStumpkinAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [6916, 6917];

        public GauntletArenaFrenziedStumpkinAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }
    }

    /// <summary>
    /// Arena-Frenzied Vulcarrion.
    /// </summary>
    [ScriptFilterCreatureId(48867, 69251)]
    public class GauntletArenaFrenziedVulcarrionAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [6146, 6147];

        public GauntletArenaFrenziedVulcarrionAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }
    }

    /// <summary>
    /// Darkspur Bandit.
    /// </summary>
    [ScriptFilterCreatureId(49209, 69274)]
    public class GauntletDarkspurBanditAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [27545, 27546];

        public GauntletDarkspurBanditAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }

        public override void OnLoad(ICreatureEntity owner)
        {
            base.OnLoad(owner);
            AddAbility(54827, 0d, 4d); // Wild Barrage
        }
    }

    /// <summary>
    /// Darkspur Enforcer.
    /// </summary>
    [ScriptFilterCreatureId(49212, 69275)]
    public class GauntletDarkspurEnforcerAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [32867, 32868];

        public GauntletDarkspurEnforcerAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }

        public override void OnLoad(ICreatureEntity owner)
        {
            base.OnLoad(owner);
            AddAbility(49792, 0d, 4d); // Searing Strikes
            AddAbility(49928, 0d, 7d); // Brush Fire
        }
    }

    /// <summary>
    /// Darkspur Commando.
    /// </summary>
    [ScriptFilterCreatureId(49213, 69273)]
    public class GauntletDarkspurCommandoAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [2764];

        public GauntletDarkspurCommandoAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
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
    /// Darkspur Heavy.
    /// </summary>
    [ScriptFilterCreatureId(49214, 69276)]
    public class GauntletDarkspurHeavyAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [32920, 32921];

        public GauntletDarkspurHeavyAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
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
    /// Darkspur Chompacabra.
    /// </summary>
    [ScriptFilterCreatureId(49399, 69277)]
    public class GauntletDarkspurChompacabraAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [28936, 28937];

        public GauntletDarkspurChompacabraAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }

        public override void OnLoad(ICreatureEntity owner)
        {
            base.OnLoad(owner);
            AddAbility(34264, 0d, 4d); // Feeding Frenzy
            AddAbility(36414, 0d, 7d); // Snap Trap
        }
    }

    /// <summary>
    /// Darkspur Enforcer.
    /// </summary>
    [ScriptFilterCreatureId(49401, 69278)]
    public class GauntletDarkspurEnforcer2AI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [32867, 32868];

        public GauntletDarkspurEnforcer2AI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }

        public override void OnLoad(ICreatureEntity owner)
        {
            base.OnLoad(owner);
            AddAbility(49792, 0d, 4d); // Searing Strikes
            AddAbility(49928, 0d, 7d); // Brush Fire
        }
    }
}
