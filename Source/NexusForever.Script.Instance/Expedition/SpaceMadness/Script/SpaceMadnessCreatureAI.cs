using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Spell;
using NexusForever.GameTable;
using NexusForever.Script.Template.Filter;
using NexusForever.Shared;

namespace NexusForever.Script.Instance.Expedition.SpaceMadness.Script
{
    // Generated from the archived Jabbithole ability lists of each creature (normal and veteran pages), cooldowns from Spell4.

    /// <summary>
    /// Hallucinating Cubig.
    /// </summary>
    [ScriptFilterCreatureId(45724, 69137)]
    public class SpaceMadnessHallucinatingCubigAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [6891, 6892];

        public SpaceMadnessHallucinatingCubigAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }

        public override void OnLoad(ICreatureEntity owner)
        {
            base.OnLoad(owner);
            AddAbility(39867, 0d, 4d); // Bruising Rush
            AddAbility(39868, 0d, 7d); // Pulverizing Vault
        }
    }

    /// <summary>
    /// Maddened Roanstag.
    /// </summary>
    [ScriptFilterCreatureId(45726, 69138)]
    public class SpaceMadnessMaddenedRoanstagAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [33244, 33245];

        public SpaceMadnessMaddenedRoanstagAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }

        public override void OnLoad(ICreatureEntity owner)
        {
            base.OnLoad(owner);
            AddAbility(34227, 0d, 4d); // Bucking Frenzy
        }
    }

    /// <summary>
    /// Foaming Roandoe.
    /// </summary>
    [ScriptFilterCreatureId(45727, 69171)]
    public class SpaceMadnessFoamingRoandoeAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [33244, 33245];

        public SpaceMadnessFoamingRoandoeAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }

        public override void OnLoad(ICreatureEntity owner)
        {
            base.OnLoad(owner);
            AddAbility(34227, 0d, 4d); // Bucking Frenzy
        }
    }

    /// <summary>
    /// Raging Oxianbull.
    /// </summary>
    [ScriptFilterCreatureId(45728, 69172)]
    public class SpaceMadnessRagingOxianbullAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [32761, 32762];

        public SpaceMadnessRagingOxianbullAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }

        public override void OnLoad(ICreatureEntity owner)
        {
            base.OnLoad(owner);
            AddAbility(53434, 0d, 4d); // Bucking Frenzy
            AddAbility(53435, 0d, 7d); // Overwhelming Bellow
        }
    }

    /// <summary>
    /// Violent Oxiancow.
    /// </summary>
    [ScriptFilterCreatureId(45729, 69139)]
    public class SpaceMadnessViolentOxiancowAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [32761, 32762];

        public SpaceMadnessViolentOxiancowAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }

        public override void OnLoad(ICreatureEntity owner)
        {
            base.OnLoad(owner);
            AddAbility(53434, 0d, 4d); // Bucking Frenzy
            AddAbility(53435, 0d, 7d); // Overwhelming Bellow
        }
    }

    /// <summary>
    /// Rampaging Oxian.
    /// </summary>
    [ScriptFilterCreatureId(45730, 69140)]
    public class SpaceMadnessRampagingOxianAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [32761, 32762];

        public SpaceMadnessRampagingOxianAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }

        public override void OnLoad(ICreatureEntity owner)
        {
            base.OnLoad(owner);
            AddAbility(53416, 0d, 4d); // Resounding Charge
            AddAbility(53434, 0d, 7d); // Bucking Frenzy
            AddAbility(53435, 0d, 10d); // Overwhelming Bellow
            AddAbility(53436, 0d, 13d); // Skull Smash
        }
    }

    /// <summary>
    /// Crazed Guard.
    /// </summary>
    [ScriptFilterCreatureId(45845, 69182)]
    public class SpaceMadnessCrazedGuardAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [2764];

        public SpaceMadnessCrazedGuardAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
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
    /// Berserk Crewman.
    /// </summary>
    [ScriptFilterCreatureId(45847)]
    public class SpaceMadnessBerserkCrewmanAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [5649, 5652];

        public SpaceMadnessBerserkCrewmanAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }

        public override void OnLoad(ICreatureEntity owner)
        {
            base.OnLoad(owner);
            AddAbility(34131, 0d, 4d); // Hack and Slash
            AddAbility(34552, 0d, 7d); // Flourishing Combo
            AddAbility(49350, 0d, 10d); // Double Strike
            AddAbility(52448, 0d, 13d); // Haymaker
        }
    }

    /// <summary>
    /// Floating Eyeballs.
    /// </summary>
    [ScriptFilterCreatureId(45849, 69184)]
    public class SpaceMadnessFloatingEyeballsAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [5649, 5652];

        public SpaceMadnessFloatingEyeballsAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
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
    /// Enraged Lamp.
    /// </summary>
    [ScriptFilterCreatureId(45866, 69185)]
    public class SpaceMadnessEnragedLampAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [5649, 5652];

        public SpaceMadnessEnragedLampAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
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
    /// Sentient Shadow.
    /// </summary>
    [ScriptFilterCreatureId(45879, 69186)]
    public class SpaceMadnessSentientShadowAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [5649, 5652];

        public SpaceMadnessSentientShadowAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
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
    /// Hallucinating Worker.
    /// </summary>
    [ScriptFilterCreatureId(45903, 69141, 45904, 69142)]
    public class SpaceMadnessHallucinatingWorkerAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [5649, 5652];

        public SpaceMadnessHallucinatingWorkerAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
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
    /// Unhinged Lookout.
    /// </summary>
    [ScriptFilterCreatureId(45923, 69187)]
    public class SpaceMadnessUnhingedLookoutAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [5649, 5652];

        public SpaceMadnessUnhingedLookoutAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
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
    /// Writhing Engineer.
    /// </summary>
    [ScriptFilterCreatureId(45936, 69188)]
    public class SpaceMadnessWrithingEngineerAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [5649, 5652];

        public SpaceMadnessWrithingEngineerAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
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
    /// Enflamed Scientist.
    /// </summary>
    [ScriptFilterCreatureId(46080, 69189)]
    public class SpaceMadnessEnflamedScientistAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [34062];

        public SpaceMadnessEnflamedScientistAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }

        public override void OnLoad(ICreatureEntity owner)
        {
            base.OnLoad(owner);
            AddAbility(55848, 0d, 4d); // Firestorm
            AddAbility(55849, 0d, 7d); // Erupting Fissure
        }
    }

    /// <summary>
    /// Infested Porter.
    /// </summary>
    [ScriptFilterCreatureId(46091, 69190)]
    public class SpaceMadnessInfestedPorterAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [5649, 5652];

        public SpaceMadnessInfestedPorterAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
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
    /// Webbed Crewman.
    /// </summary>
    [ScriptFilterCreatureId(46105, 69191)]
    public class SpaceMadnessWebbedCrewmanAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [5649, 5652];

        public SpaceMadnessWebbedCrewmanAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
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
    /// Angry Flowerpot.
    /// </summary>
    [ScriptFilterCreatureId(46108, 69192)]
    public class SpaceMadnessAngryFlowerpotAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [5649, 5652];

        public SpaceMadnessAngryFlowerpotAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
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
    /// Incognito Spider.
    /// </summary>
    [ScriptFilterCreatureId(46120, 69193)]
    public class SpaceMadnessIncognitoSpiderAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [6651, 6652];

        public SpaceMadnessIncognitoSpiderAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }
    }

    /// <summary>
    /// Writhing Nightmare.
    /// </summary>
    [ScriptFilterCreatureId(46123, 69213)]
    public class SpaceMadnessWrithingNightmareAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [34361, 34362];

        public SpaceMadnessWrithingNightmareAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }

        public override void OnLoad(ICreatureEntity owner)
        {
            base.OnLoad(owner);
            AddAbility(39866, 0d, 4d); // Slashing Frenzy
            AddAbility(72400, 0d, 7d); // Tainted Frenzy
            AddAbility(72401, 0d, 10d); // Fetid Gnash
        }
    }

    /// <summary>
    /// Stinging Nightmare.
    /// </summary>
    [ScriptFilterCreatureId(46124, 69214)]
    public class SpaceMadnessStingingNightmareAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [32965, 32966];

        public SpaceMadnessStingingNightmareAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }

        public override void OnLoad(ICreatureEntity owner)
        {
            base.OnLoad(owner);
            AddAbility(35963, 0d, 4d); // Flurry of the Buzzbing
            AddAbility(44638, 0d, 7d); // Ground Slam
        }
    }

    /// <summary>
    /// Shocking Nightmare.
    /// </summary>
    [ScriptFilterCreatureId(46126, 69215)]
    public class SpaceMadnessShockingNightmareAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [6909, 6910];

        public SpaceMadnessShockingNightmareAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }

        public override void OnLoad(ICreatureEntity owner)
        {
            base.OnLoad(owner);
            AddAbility(54381, 0d, 4d); // Flash Flip
            AddAbility(54563, 0d, 7d); // Forming Dash
        }
    }

    /// <summary>
    /// Electrifying Nightmare.
    /// </summary>
    [ScriptFilterCreatureId(46127, 69216)]
    public class SpaceMadnessElectrifyingNightmareAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [56980, 56981];

        public SpaceMadnessElectrifyingNightmareAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
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
    /// Venomous Nightmare.
    /// </summary>
    [ScriptFilterCreatureId(46128, 69217)]
    public class SpaceMadnessVenomousNightmareAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [6651, 6652];

        public SpaceMadnessVenomousNightmareAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }

        public override void OnLoad(ICreatureEntity owner)
        {
            base.OnLoad(owner);
            AddAbility(39582, 0d, 4d); // Pincer Flurry
            AddAbility(39598, 0d, 7d); // Burrowed Ambush
        }
    }

    /// <summary>
    /// Rending Nightmare.
    /// </summary>
    [ScriptFilterCreatureId(46130, 69218)]
    public class SpaceMadnessRendingNightmareAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [42657, 42658];

        public SpaceMadnessRendingNightmareAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }

        public override void OnLoad(ICreatureEntity owner)
        {
            base.OnLoad(owner);
            AddAbility(42637, 0d, 4d); // Spine Mine
        }
    }

    /// <summary>
    /// Verminous Nightmare.
    /// </summary>
    [ScriptFilterCreatureId(46131)]
    public class SpaceMadnessVerminousNightmareAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [33242, 33243];

        public SpaceMadnessVerminousNightmareAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }
    }

    /// <summary>
    /// Horrifying Nightmare.
    /// </summary>
    [ScriptFilterCreatureId(46132, 69219)]
    public class SpaceMadnessHorrifyingNightmareAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [25870, 25871];

        public SpaceMadnessHorrifyingNightmareAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }

        public override void OnLoad(ICreatureEntity owner)
        {
            base.OnLoad(owner);
            AddAbility(55859, 0d, 4d); // Snip, Snip, Snip, Sting
            AddAbility(55860, 0d, 7d); // Poisonous Barrage
            AddAbility(55861, 0d, 10d); // Burrowing Ambush
            AddAbility(55862, 0d, 13d); // Blinding Dust
        }
    }

    /// <summary>
    /// Ravenous Nightmare.
    /// </summary>
    [ScriptFilterCreatureId(46133, 69220)]
    public class SpaceMadnessRavenousNightmareAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [28936, 28937];

        public SpaceMadnessRavenousNightmareAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
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
    /// Looming Nightmare.
    /// </summary>
    [ScriptFilterCreatureId(46135, 69221)]
    public class SpaceMadnessLoomingNightmareAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [42874, 42875];

        public SpaceMadnessLoomingNightmareAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }

        public override void OnLoad(ICreatureEntity owner)
        {
            base.OnLoad(owner);
            AddAbility(57320, 0d, 4d); // Cull
            AddAbility(57321, 0d, 7d); // Swirlwind
            AddAbility(57329, 0d, 10d); // Ethereal Strike
        }
    }

    /// <summary>
    /// Skittering Nightmare.
    /// </summary>
    [ScriptFilterCreatureId(46137)]
    public class SpaceMadnessSkitteringNightmareAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [4495, 4496];

        public SpaceMadnessSkitteringNightmareAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }
    }

    /// <summary>
    /// Spiderling.
    /// </summary>
    [ScriptFilterCreatureId(46138, 69194)]
    public class SpaceMadnessSpiderlingAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [6651, 6652];

        public SpaceMadnessSpiderlingAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }
    }

    /// <summary>
    /// Beverage Swarm.
    /// </summary>
    [ScriptFilterCreatureId(46186, 69195)]
    public class SpaceMadnessBeverageSwarmAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [5649, 5652];

        public SpaceMadnessBeverageSwarmAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
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
    /// Sparkling Manifestation.
    /// </summary>
    [ScriptFilterCreatureId(46476, 69196)]
    public class SpaceMadnessSparklingManifestationAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [5649, 5652];

        public SpaceMadnessSparklingManifestationAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
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
    /// Exact Change 3.0.
    /// </summary>
    [ScriptFilterCreatureId(46483, 69204)]
    public class SpaceMadnessExactChange30AI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [28693, 28694];

        public SpaceMadnessExactChange30AI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }

        public override void OnLoad(ICreatureEntity owner)
        {
            base.OnLoad(owner);
            AddAbility(39430, 0d, 4d); // Acrobatic Dash
            AddAbility(39897, 0d, 7d); // Soda Fountain
            AddAbility(40558, 0d, 10d); // Sickly Soda Slime
        }
    }

    /// <summary>
    /// Needy Websack.
    /// </summary>
    [ScriptFilterCreatureId(46621, 69197)]
    public class SpaceMadnessNeedyWebsackAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [5649, 5652];

        public SpaceMadnessNeedyWebsackAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }
    }

    /// <summary>
    /// Bottles and Cans.
    /// </summary>
    [ScriptFilterCreatureId(46660, 69198)]
    public class SpaceMadnessBottlesAndCansAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [5649, 5652];

        public SpaceMadnessBottlesAndCansAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }
    }

    /// <summary>
    /// Fire Establisher.
    /// </summary>
    [ScriptFilterCreatureId(46661, 69199)]
    public class SpaceMadnessFireEstablisherAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [2764];

        public SpaceMadnessFireEstablisherAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }
    }

    /// <summary>
    /// Crazed Handler.
    /// </summary>
    [ScriptFilterCreatureId(46671, 69143, 46672, 69144)]
    public class SpaceMadnessCrazedHandlerAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [5649, 5652];

        public SpaceMadnessCrazedHandlerAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
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
    /// Blazing Crewman.
    /// </summary>
    [ScriptFilterCreatureId(46714, 69205)]
    public class SpaceMadnessBlazingCrewmanAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [34062];

        public SpaceMadnessBlazingCrewmanAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }

        public override void OnLoad(ICreatureEntity owner)
        {
            base.OnLoad(owner);
            AddAbility(55848, 0d, 4d); // Firestorm
            AddAbility(55849, 0d, 7d); // Erupting Fissure
        }
    }

    /// <summary>
    /// Masked Arachnid.
    /// </summary>
    [ScriptFilterCreatureId(46720, 69206)]
    public class SpaceMadnessMaskedArachnidAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [6651, 6652];

        public SpaceMadnessMaskedArachnidAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }
    }

    /// <summary>
    /// Hookfoot Nightmare.
    /// </summary>
    [ScriptFilterCreatureId(46721, 69207)]
    public class SpaceMadnessHookfootNightmareAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [42657, 42658];

        public SpaceMadnessHookfootNightmareAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }

        public override void OnLoad(ICreatureEntity owner)
        {
            base.OnLoad(owner);
            AddAbility(42637, 0d, 4d); // Spine Mine
        }
    }

    /// <summary>
    /// Smoke Nightmare.
    /// </summary>
    [ScriptFilterCreatureId(46722, 69208)]
    public class SpaceMadnessSmokeNightmareAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [5649, 5652];

        public SpaceMadnessSmokeNightmareAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }
    }

    /// <summary>
    /// Eye Nightmare.
    /// </summary>
    [ScriptFilterCreatureId(46723, 69209)]
    public class SpaceMadnessEyeNightmareAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [5649, 5652];

        public SpaceMadnessEyeNightmareAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
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
    /// Enraged Lamp.
    /// </summary>
    [ScriptFilterCreatureId(46724, 69210)]
    public class SpaceMadnessEnragedLamp2AI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [5649, 5652];

        public SpaceMadnessEnragedLamp2AI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
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
    /// Scorched Mauler.
    /// </summary>
    [ScriptFilterCreatureId(46856, 69145)]
    public class SpaceMadnessScorchedMaulerAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [32773, 32774];

        public SpaceMadnessScorchedMaulerAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }

        public override void OnLoad(ICreatureEntity owner)
        {
            base.OnLoad(owner);
            AddAbility(33919, 0d, 4d); // Clawing Frenzy
            AddAbility(46895, 0d, 7d); // Tail Spin
        }
    }

    /// <summary>
    /// Scorched Struther.
    /// </summary>
    [ScriptFilterCreatureId(46858, 69146)]
    public class SpaceMadnessScorchedStrutherAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [28704, 28705];

        public SpaceMadnessScorchedStrutherAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }

        public override void OnLoad(ICreatureEntity owner)
        {
            base.OnLoad(owner);
            AddAbility(34455, 0d, 4d); // Tooth and Nail
            AddAbility(54839, 0d, 7d); // Headbutt
        }
    }

    /// <summary>
    /// Burnt Affliction.
    /// </summary>
    [ScriptFilterCreatureId(46881, 69147)]
    public class SpaceMadnessBurntAfflictionAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [33247, 33248];

        public SpaceMadnessBurntAfflictionAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }

        public override void OnLoad(ICreatureEntity owner)
        {
            base.OnLoad(owner);
            AddAbility(35304, 0d, 4d); // Root Spike
            AddAbility(35323, 0d, 7d); // Grasping Roots
        }
    }

    /// <summary>
    /// Burnt Brute.
    /// </summary>
    [ScriptFilterCreatureId(46882, 69148)]
    public class SpaceMadnessBurntBruteAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [5649, 5652];

        public SpaceMadnessBurntBruteAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
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
    /// Charred Firefang.
    /// </summary>
    [ScriptFilterCreatureId(46883, 69149)]
    public class SpaceMadnessCharredFirefangAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [32768, 32769];

        public SpaceMadnessCharredFirefangAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }

        public override void OnLoad(ICreatureEntity owner)
        {
            base.OnLoad(owner);
            AddAbility(35192, 0d, 4d); // Pride Pounce
            AddAbility(45494, 0d, 7d); // Razor Sharp
        }
    }

    /// <summary>
    /// Charred Brigand.
    /// </summary>
    [ScriptFilterCreatureId(46884, 69150)]
    public class SpaceMadnessCharredBrigandAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [5649, 5652];

        public SpaceMadnessCharredBrigandAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
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
    /// Blazing Succubus.
    /// </summary>
    [ScriptFilterCreatureId(46885, 69151)]
    public class SpaceMadnessBlazingSuccubusAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [2605, 2607];

        public SpaceMadnessBlazingSuccubusAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
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
    /// Blazing Incubus.
    /// </summary>
    [ScriptFilterCreatureId(46887, 69152)]
    public class SpaceMadnessBlazingIncubusAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [2605, 2607];

        public SpaceMadnessBlazingIncubusAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
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
    /// Inferno Intensity.
    /// </summary>
    [ScriptFilterCreatureId(46889, 69153)]
    public class SpaceMadnessInfernoIntensityAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [8093, 8094];

        public SpaceMadnessInfernoIntensityAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }

        public override void OnLoad(ICreatureEntity owner)
        {
            base.OnLoad(owner);
            AddAbility(38501, 0d, 4d); // Burrowing Assault
            AddAbility(57008, 0d, 7d); // Splattering Slime
            AddAbility(57744, 0d, 10d); // Muck Rake
            AddAbility(57878, 0d, 13d); // Corrosive Filth
        }
    }

    /// <summary>
    /// Sparkriden Offender.
    /// </summary>
    [ScriptFilterCreatureId(48098, 69154)]
    public class SpaceMadnessSparkridenOffenderAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [4537];

        public SpaceMadnessSparkridenOffenderAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
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
    /// Sparkriden Skitterer.
    /// </summary>
    [ScriptFilterCreatureId(48099, 69155)]
    public class SpaceMadnessSparkridenSkittererAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [4616, 4617];

        public SpaceMadnessSparkridenSkittererAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }
    }

    /// <summary>
    /// Shockwing Stormer.
    /// </summary>
    [ScriptFilterCreatureId(48101, 69156)]
    public class SpaceMadnessShockwingStormerAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [34062];

        public SpaceMadnessShockwingStormerAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }

        public override void OnLoad(ICreatureEntity owner)
        {
            base.OnLoad(owner);
            AddAbility(55371, 0d, 4d); // Bad Weather
            AddAbility(55401, 0d, 7d); // Lightning Zap
        }
    }

    /// <summary>
    /// Shockwing Slasher.
    /// </summary>
    [ScriptFilterCreatureId(48102, 69157)]
    public class SpaceMadnessShockwingSlasherAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [5968, 5969];

        public SpaceMadnessShockwingSlasherAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }

        public override void OnLoad(ICreatureEntity owner)
        {
            base.OnLoad(owner);
            AddAbility(55044, 0d, 4d); // Discouraging Word
            AddAbility(55185, 0d, 7d); // Slashing Path
        }
    }

    /// <summary>
    /// Sparkcharged Rager.
    /// </summary>
    [ScriptFilterCreatureId(48103, 69158)]
    public class SpaceMadnessSparkchargedRagerAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [6616, 6617];

        public SpaceMadnessSparkchargedRagerAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }

        public override void OnLoad(ICreatureEntity owner)
        {
            base.OnLoad(owner);
            AddAbility(47944, 0d, 4d); // Polar Pummel
            AddAbility(47952, 0d, 7d); // Polar Shift
        }
    }

    /// <summary>
    /// Sparkcharged Cloudburst.
    /// </summary>
    [ScriptFilterCreatureId(48104, 69159)]
    public class SpaceMadnessSparkchargedCloudburstAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [35474, 35475];

        public SpaceMadnessSparkchargedCloudburstAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }

        public override void OnLoad(ICreatureEntity owner)
        {
            base.OnLoad(owner);
            AddAbility(65456, 0d, 4d); // Shock Ring
            AddAbility(69072, 0d, 7d); // Compression Blasts
        }
    }

    /// <summary>
    /// Voltmarked Sparkman.
    /// </summary>
    [ScriptFilterCreatureId(48105, 69160)]
    public class SpaceMadnessVoltmarkedSparkmanAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [33432, 33433];

        public SpaceMadnessVoltmarkedSparkmanAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }

        public override void OnLoad(ICreatureEntity owner)
        {
            base.OnLoad(owner);
            AddAbility(58710, 0d, 4d); // Blast Burst
            AddAbility(58711, 0d, 7d); // Controlled Overload
        }
    }

    /// <summary>
    /// Voltmarked Sparklass.
    /// </summary>
    [ScriptFilterCreatureId(48106, 69161)]
    public class SpaceMadnessVoltmarkedSparklassAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [33432, 33433];

        public SpaceMadnessVoltmarkedSparklassAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }

        public override void OnLoad(ICreatureEntity owner)
        {
            base.OnLoad(owner);
            AddAbility(58710, 0d, 4d); // Blast Burst
            AddAbility(58711, 0d, 7d); // Controlled Overload
        }
    }

    /// <summary>
    /// Thunder Surger.
    /// </summary>
    [ScriptFilterCreatureId(48107, 69162)]
    public class SpaceMadnessThunderSurgerAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [28693, 28694];

        public SpaceMadnessThunderSurgerAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }

        public override void OnLoad(ICreatureEntity owner)
        {
            base.OnLoad(owner);
            AddAbility(39430, 0d, 4d); // Acrobatic Dash
            AddAbility(46809, 0d, 7d); // Energy Storm
        }
    }

    /// <summary>
    /// Dark Creeper.
    /// </summary>
    [ScriptFilterCreatureId(48345, 69163)]
    public class SpaceMadnessDarkCreeperAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [34361, 34362];

        public SpaceMadnessDarkCreeperAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }

        public override void OnLoad(ICreatureEntity owner)
        {
            base.OnLoad(owner);
            AddAbility(39866, 0d, 4d); // Slashing Frenzy
            AddAbility(72400, 0d, 7d); // Tainted Frenzy
            AddAbility(72401, 0d, 10d); // Fetid Gnash
        }
    }

    /// <summary>
    /// Dark Woodheart.
    /// </summary>
    [ScriptFilterCreatureId(48346, 69164)]
    public class SpaceMadnessDarkWoodheartAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [6916, 6917];

        public SpaceMadnessDarkWoodheartAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }

        public override void OnLoad(ICreatureEntity owner)
        {
            base.OnLoad(owner);
            AddAbility(35400, 0d, 4d); // Root Eruption
            AddAbility(35417, 0d, 7d); // Growing Purge
        }
    }

    /// <summary>
    /// Drifting Horror.
    /// </summary>
    [ScriptFilterCreatureId(48347, 69165)]
    public class SpaceMadnessDriftingHorrorAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [42874, 42875];

        public SpaceMadnessDriftingHorrorAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }

        public override void OnLoad(ICreatureEntity owner)
        {
            base.OnLoad(owner);
            AddAbility(57320, 0d, 4d); // Cull
            AddAbility(57321, 0d, 7d); // Swirlwind
        }
    }

    /// <summary>
    /// Stampeding Horror.
    /// </summary>
    [ScriptFilterCreatureId(48348, 69173)]
    public class SpaceMadnessStampedingHorrorAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [42657, 42658];

        public SpaceMadnessStampedingHorrorAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }

        public override void OnLoad(ICreatureEntity owner)
        {
            base.OnLoad(owner);
            AddAbility(42637, 0d, 4d); // Spine Mine
        }
    }

    /// <summary>
    /// Wretched Queen.
    /// </summary>
    [ScriptFilterCreatureId(48349, 69166)]
    public class SpaceMadnessWretchedQueenAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [34062];

        public SpaceMadnessWretchedQueenAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }

        public override void OnLoad(ICreatureEntity owner)
        {
            base.OnLoad(owner);
            AddAbility(54621, 0d, 4d); // Sudden Magics
            AddAbility(54622, 0d, 7d); // Body Slam
            AddAbility(54626, 0d, 10d); // Creeping Power
            AddAbility(54628, 0d, 13d); // Trick Spell
        }
    }

    /// <summary>
    /// Wretched Servant.
    /// </summary>
    [ScriptFilterCreatureId(48350, 69167)]
    public class SpaceMadnessWretchedServantAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [5649, 5652];

        public SpaceMadnessWretchedServantAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }
    }

    /// <summary>
    /// Smoldering Billow.
    /// </summary>
    [ScriptFilterCreatureId(48351, 69168)]
    public class SpaceMadnessSmolderingBillowAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [2605, 2607];

        public SpaceMadnessSmolderingBillowAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
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
    /// Smoldering Bonebeast.
    /// </summary>
    [ScriptFilterCreatureId(48352, 69169)]
    public class SpaceMadnessSmolderingBonebeastAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [32734, 32735];

        public SpaceMadnessSmolderingBonebeastAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }

        public override void OnLoad(ICreatureEntity owner)
        {
            base.OnLoad(owner);
            AddAbility(34262, 0d, 4d); // Ground Pound
            AddAbility(35556, 0d, 7d); // Rolling Rampage
        }
    }

    /// <summary>
    /// Decrepit Atrocity.
    /// </summary>
    [ScriptFilterCreatureId(48353, 69170)]
    public class SpaceMadnessDecrepitAtrocityAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [4040, 4041];

        public SpaceMadnessDecrepitAtrocityAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }

        public override void OnLoad(ICreatureEntity owner)
        {
            base.OnLoad(owner);
            AddAbility(54822, 0d, 4d); // Shards of Earth
            AddAbility(54823, 0d, 7d); // Shock Wave
        }
    }

    /// <summary>
    /// Gnarled Nightmare.
    /// </summary>
    [ScriptFilterCreatureId(58783, 69223)]
    public class SpaceMadnessGnarledNightmareAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [33247, 33248];

        public SpaceMadnessGnarledNightmareAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }

        public override void OnLoad(ICreatureEntity owner)
        {
            base.OnLoad(owner);
            AddAbility(35304, 0d, 4d); // Root Spike
            AddAbility(35323, 0d, 7d); // Grasping Roots
        }
    }

    /// <summary>
    /// Metallic Nightmare.
    /// </summary>
    [ScriptFilterCreatureId(58798, 69222)]
    public class SpaceMadnessMetallicNightmareAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [4616, 4617];

        public SpaceMadnessMetallicNightmareAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
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
}
