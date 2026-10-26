using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Spell;
using NexusForever.GameTable;
using NexusForever.Script.Template.Filter;
using NexusForever.Script.Instance.Expedition;
using NexusForever.Shared;

namespace NexusForever.Script.Instance.Adventure.MalgraveTrail.Script
{
    // Generated from the archived Jabbithole ability lists of each creature (normal and veteran pages), cooldowns from Spell4.

    /// <summary>
    /// Desert Drone.
    /// </summary>
    [ScriptFilterCreatureId(19296)]
    public class MalgraveDesertDroneAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [65954, 65955];

        public MalgraveDesertDroneAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }

        public override void OnLoad(ICreatureEntity owner)
        {
            base.OnLoad(owner);
            AddAbility(66127, 0d, 4d); // Flurry of the Buzzbing
        }
    }

    /// <summary>
    /// Sand Runner.
    /// </summary>
    [ScriptFilterCreatureId(19298)]
    public class MalgraveSandRunnerAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [65812, 65813];

        public MalgraveSandRunnerAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }
    }

    /// <summary>
    /// Sandyhive Doomsting.
    /// </summary>
    [ScriptFilterCreatureId(19454)]
    public class MalgraveSandyhiveDoomstingAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [65954, 65955];

        public MalgraveSandyhiveDoomstingAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }

        public override void OnLoad(ICreatureEntity owner)
        {
            base.OnLoad(owner);
            AddAbility(66127, 0d, 4d); // Flurry of the Buzzbing
        }
    }

    /// <summary>
    /// Sandyhive Buzzer.
    /// </summary>
    [ScriptFilterCreatureId(19456)]
    public class MalgraveSandyhiveBuzzerAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [65954, 65955];

        public MalgraveSandyhiveBuzzerAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }

        public override void OnLoad(ICreatureEntity owner)
        {
            base.OnLoad(owner);
            AddAbility(66127, 0d, 4d); // Flurry of the Buzzbing
        }
    }

    /// <summary>
    /// Sandyhive Drone.
    /// </summary>
    [ScriptFilterCreatureId(19457)]
    public class MalgraveSandyhiveDroneAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [65954, 65955];

        public MalgraveSandyhiveDroneAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }

        public override void OnLoad(ICreatureEntity owner)
        {
            base.OnLoad(owner);
            AddAbility(40032, 0d, 4d); // Buzzbing Rush
            AddAbility(66127, 0d, 7d); // Flurry of the Buzzbing
        }
    }

    /// <summary>
    /// Sandyhive Bladewing.
    /// </summary>
    [ScriptFilterCreatureId(19470)]
    public class MalgraveSandyhiveBladewingAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [65954, 65955];

        public MalgraveSandyhiveBladewingAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }

        public override void OnLoad(ICreatureEntity owner)
        {
            base.OnLoad(owner);
            AddAbility(40032, 0d, 4d); // Buzzbing Rush
            AddAbility(66127, 0d, 7d); // Flurry of the Buzzbing
        }
    }

    /// <summary>
    /// Sandyhive Swarmer.
    /// </summary>
    [ScriptFilterCreatureId(19472)]
    public class MalgraveSandyhiveSwarmerAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [65954, 65955];

        public MalgraveSandyhiveSwarmerAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }
    }

    /// <summary>
    /// Sunfeather Deathstrike.
    /// </summary>
    [ScriptFilterCreatureId(19473)]
    public class MalgraveSunfeatherDeathstrikeAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [65812, 65813];

        public MalgraveSunfeatherDeathstrikeAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }
    }

    /// <summary>
    /// Sunfeather Shredder.
    /// </summary>
    [ScriptFilterCreatureId(19476)]
    public class MalgraveSunfeatherShredderAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [65812, 65813];

        public MalgraveSunfeatherShredderAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }
    }

    /// <summary>
    /// Sunfeather Screecher.
    /// </summary>
    [ScriptFilterCreatureId(19477)]
    public class MalgraveSunfeatherScreecherAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [65812, 65813];

        public MalgraveSunfeatherScreecherAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }
    }

    /// <summary>
    /// Sunfeather Razortalon.
    /// </summary>
    [ScriptFilterCreatureId(19479)]
    public class MalgraveSunfeatherRazortalonAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [65812, 65813];

        public MalgraveSunfeatherRazortalonAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }

        public override void OnLoad(ICreatureEntity owner)
        {
            base.OnLoad(owner);
            AddAbility(66044, 0d, 4d); // Tooth and Nail
        }
    }

    /// <summary>
    /// Sunfeather Bladeclaw.
    /// </summary>
    [ScriptFilterCreatureId(19480)]
    public class MalgraveSunfeatherBladeclawAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [65812, 65813];

        public MalgraveSunfeatherBladeclawAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }

        public override void OnLoad(ICreatureEntity owner)
        {
            base.OnLoad(owner);
            AddAbility(66044, 0d, 4d); // Tooth and Nail
        }
    }

    /// <summary>
    /// Pricklevine Bloodroot.
    /// </summary>
    [ScriptFilterCreatureId(19494)]
    public class MalgravePricklevineBloodrootAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [65761, 65762];

        public MalgravePricklevineBloodrootAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }

        public override void OnLoad(ICreatureEntity owner)
        {
            base.OnLoad(owner);
            AddAbility(38431, 0d, 4d); // Spore Bomb
        }
    }

    /// <summary>
    /// Pricklevine Fangstem.
    /// </summary>
    [ScriptFilterCreatureId(19495)]
    public class MalgravePricklevineFangstemAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [65761, 65762];

        public MalgravePricklevineFangstemAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }

        public override void OnLoad(ICreatureEntity owner)
        {
            base.OnLoad(owner);
            AddAbility(38431, 0d, 4d); // Spore Bomb
        }
    }

    /// <summary>
    /// Duneweb Stalker.
    /// </summary>
    [ScriptFilterCreatureId(19496)]
    public class MalgraveDunewebStalkerAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [65763, 65764];

        public MalgraveDunewebStalkerAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }

        public override void OnLoad(ICreatureEntity owner)
        {
            base.OnLoad(owner);
            AddAbility(66151, 0d, 4d); // Pincer Flurry
            AddAbility(66155, 0d, 7d); // Burrowed Ambush
        }
    }

    /// <summary>
    /// Duneweb Spinner.
    /// </summary>
    [ScriptFilterCreatureId(19497)]
    public class MalgraveDunewebSpinnerAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [65763, 65764];

        public MalgraveDunewebSpinnerAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }

        public override void OnLoad(ICreatureEntity owner)
        {
            base.OnLoad(owner);
            AddAbility(66151, 0d, 4d); // Pincer Flurry
            AddAbility(66155, 0d, 7d); // Burrowed Ambush
        }
    }

    /// <summary>
    /// Duneweb Skitterer.
    /// </summary>
    [ScriptFilterCreatureId(19498)]
    public class MalgraveDunewebSkittererAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [65763, 65764];

        public MalgraveDunewebSkittererAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }
    }

    /// <summary>
    /// Duneweb Broodmother.
    /// </summary>
    [ScriptFilterCreatureId(19499)]
    public class MalgraveDunewebBroodmotherAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [65763, 65764];

        public MalgraveDunewebBroodmotherAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }

        public override void OnLoad(ICreatureEntity owner)
        {
            base.OnLoad(owner);
            AddAbility(66151, 0d, 4d); // Pincer Flurry
            AddAbility(66155, 0d, 7d); // Burrowed Ambush
        }
    }

    /// <summary>
    /// Blacksun Destructor.
    /// </summary>
    [ScriptFilterCreatureId(19500)]
    public class MalgraveBlacksunDestructorAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [65717];

        public MalgraveBlacksunDestructorAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }
    }

    /// <summary>
    /// Blacksun Destroyer.
    /// </summary>
    [ScriptFilterCreatureId(19501)]
    public class MalgraveBlacksunDestroyerAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [65950, 65951];

        public MalgraveBlacksunDestroyerAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }
    }

    /// <summary>
    /// Blacksun Specialist.
    /// </summary>
    [ScriptFilterCreatureId(19502)]
    public class MalgraveBlacksunSpecialistAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [65794, 65795];

        public MalgraveBlacksunSpecialistAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }

        public override void OnLoad(ICreatureEntity owner)
        {
            base.OnLoad(owner);
            AddAbility(39406, 0d, 4d); // Wild Barrage
        }
    }

    /// <summary>
    /// Blacksun Sorcerer.
    /// </summary>
    [ScriptFilterCreatureId(19507)]
    public class MalgraveBlacksunSorcererAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [65849, 65995];

        public MalgraveBlacksunSorcererAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }
    }

    /// <summary>
    /// Blacksun Wrecker.
    /// </summary>
    [ScriptFilterCreatureId(19508)]
    public class MalgraveBlacksunWreckerAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [65950, 65951];

        public MalgraveBlacksunWreckerAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }
    }

    /// <summary>
    /// Blacksun Evoker.
    /// </summary>
    [ScriptFilterCreatureId(19509)]
    public class MalgraveBlacksunEvokerAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [65995];

        public MalgraveBlacksunEvokerAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }
    }

    /// <summary>
    /// Blacksun Equalizer.
    /// </summary>
    [ScriptFilterCreatureId(19510)]
    public class MalgraveBlacksunEqualizerAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [65717];

        public MalgraveBlacksunEqualizerAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }
    }

    /// <summary>
    /// Blacksun Pounder.
    /// </summary>
    [ScriptFilterCreatureId(19511)]
    public class MalgraveBlacksunPounderAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [65749, 65750];

        public MalgraveBlacksunPounderAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }
    }

    /// <summary>
    /// Blisterbone Skinner.
    /// </summary>
    [ScriptFilterCreatureId(19515)]
    public class MalgraveBlisterboneSkinnerAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [65715, 65716];

        public MalgraveBlisterboneSkinnerAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }

        public override void OnLoad(ICreatureEntity owner)
        {
            base.OnLoad(owner);
            AddAbility(39895, 0d, 4d); // Rend
            AddAbility(66010, 0d, 7d); // Lateral Slice
        }
    }

    /// <summary>
    /// Skywing Conqueror.
    /// </summary>
    [ScriptFilterCreatureId(19522)]
    public class MalgraveSkywingConquerorAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [65751, 65752];

        public MalgraveSkywingConquerorAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }

        public override void OnLoad(ICreatureEntity owner)
        {
            base.OnLoad(owner);
            AddAbility(39898, 0d, 4d); // Blade Vortex
            AddAbility(66113, 0d, 7d); // Savage Blades
            AddAbility(66118, 0d, 10d); // Bladestrike
        }
    }

    /// <summary>
    /// Skywing Juggernaut.
    /// </summary>
    [ScriptFilterCreatureId(19523)]
    public class MalgraveSkywingJuggernautAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [65743, 65744];

        public MalgraveSkywingJuggernautAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }
    }

    /// <summary>
    /// Skywing Stormcaller.
    /// </summary>
    [ScriptFilterCreatureId(19524)]
    public class MalgraveSkywingStormcallerAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [65849];

        public MalgraveSkywingStormcallerAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }
    }

    /// <summary>
    /// Skywing Cloudsinger.
    /// </summary>
    [ScriptFilterCreatureId(19525)]
    public class MalgraveSkywingCloudsingerAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [65849, 65995];

        public MalgraveSkywingCloudsingerAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }
    }

    /// <summary>
    /// Skywing Mauler.
    /// </summary>
    [ScriptFilterCreatureId(19526)]
    public class MalgraveSkywingMaulerAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [65743, 65744];

        public MalgraveSkywingMaulerAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }
    }

    /// <summary>
    /// Skywing Vanquisher.
    /// </summary>
    [ScriptFilterCreatureId(19527)]
    public class MalgraveSkywingVanquisherAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [65751, 65752];

        public MalgraveSkywingVanquisherAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }
    }

    /// <summary>
    /// Skywing Butcher.
    /// </summary>
    [ScriptFilterCreatureId(19528)]
    public class MalgraveSkywingButcherAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [65743, 65744];

        public MalgraveSkywingButcherAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }
    }

    /// <summary>
    /// Stray Oxian.
    /// </summary>
    [ScriptFilterCreatureId(19616)]
    public class MalgraveStrayOxianAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [66140, 66141];

        public MalgraveStrayOxianAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }
    }

    /// <summary>
    /// Ob'mar.
    /// </summary>
    [ScriptFilterCreatureId(19665)]
    public class MalgraveObMarAI : ExpeditionCreatureAI
    {
        public MalgraveObMarAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }

        public override void OnLoad(ICreatureEntity owner)
        {
            base.OnLoad(owner);
            AddAbility(65972, 0d, 4d); // Intimidating Bellow
            AddAbility(65976, 0d, 7d); // Ground Pound
            AddAbility(66145, 0d, 10d); // Pulverize
        }
    }

    /// <summary>
    /// Malgrave Sand Pincher.
    /// </summary>
    [ScriptFilterCreatureId(19687)]
    public class MalgraveMalgraveSandPincherAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [65785, 65786];

        public MalgraveMalgraveSandPincherAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }
    }

    /// <summary>
    /// Scrapsting Pincher.
    /// </summary>
    [ScriptFilterCreatureId(19763)]
    public class MalgraveScrapstingPincherAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [65785, 65786];

        public MalgraveScrapstingPincherAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }
    }

    /// <summary>
    /// Blisterbone Freak.
    /// </summary>
    [ScriptFilterCreatureId(19777)]
    public class MalgraveBlisterboneFreakAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [65849, 65995];

        public MalgraveBlisterboneFreakAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }

        public override void OnLoad(ICreatureEntity owner)
        {
            base.OnLoad(owner);
            AddAbility(65982, 0d, 4d); // Lightning Field
            AddAbility(66080, 0d, 7d); // Eruption
            AddAbility(66089, 0d, 10d); // Stone Rain
        }
    }

    /// <summary>
    /// Blisterbone Dreglord.
    /// </summary>
    [ScriptFilterCreatureId(19818)]
    public class MalgraveBlisterboneDreglordAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [65717];

        public MalgraveBlisterboneDreglordAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }

        public override void OnLoad(ICreatureEntity owner)
        {
            base.OnLoad(owner);
            AddAbility(66100, 0d, 4d); // Mortar
            AddAbility(66104, 0d, 7d); // Longshot
            AddAbility(66112, 0d, 10d); // Shell Storm
        }
    }

    /// <summary>
    /// Sunfeather Bloodbeak.
    /// </summary>
    [ScriptFilterCreatureId(20387)]
    public class MalgraveSunfeatherBloodbeakAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [65812, 65813];

        public MalgraveSunfeatherBloodbeakAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }

        public override void OnLoad(ICreatureEntity owner)
        {
            base.OnLoad(owner);
            AddAbility(66044, 0d, 4d); // Tooth and Nail
        }
    }

    /// <summary>
    /// Thirsty Pricklevine.
    /// </summary>
    [ScriptFilterCreatureId(20413)]
    public class MalgraveThirstyPricklevineAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [65761, 65762];

        public MalgraveThirstyPricklevineAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }

        public override void OnLoad(ICreatureEntity owner)
        {
            base.OnLoad(owner);
            AddAbility(38431, 0d, 4d); // Spore Bomb
        }
    }

    /// <summary>
    /// Sandyhive Combguard.
    /// </summary>
    [ScriptFilterCreatureId(20470)]
    public class MalgraveSandyhiveCombguardAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [65954, 65955];

        public MalgraveSandyhiveCombguardAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }

        public override void OnLoad(ICreatureEntity owner)
        {
            base.OnLoad(owner);
            AddAbility(66127, 0d, 4d); // Flurry of the Buzzbing
        }
    }

    /// <summary>
    /// Bonebreaker Destroyer.
    /// </summary>
    [ScriptFilterCreatureId(20588)]
    public class MalgraveBonebreakerDestroyerAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [65950, 65951];

        public MalgraveBonebreakerDestroyerAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }

        public override void OnLoad(ICreatureEntity owner)
        {
            base.OnLoad(owner);
            AddAbility(65994, 0d, 4d); // Shockwave
        }
    }

    /// <summary>
    /// Bonebreaker Fury.
    /// </summary>
    [ScriptFilterCreatureId(20590)]
    public class MalgraveBonebreakerFuryAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [65715, 65716];

        public MalgraveBonebreakerFuryAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }
    }

    /// <summary>
    /// Blisterbone Pariah.
    /// </summary>
    [ScriptFilterCreatureId(20598)]
    public class MalgraveBlisterbonePariahAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [65849, 65995];

        public MalgraveBlisterbonePariahAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }

        public override void OnLoad(ICreatureEntity owner)
        {
            base.OnLoad(owner);
            AddAbility(65987, 0d, 4d); // Microburst
            AddAbility(66080, 0d, 7d); // Eruption
            AddAbility(66085, 0d, 10d); // Boulder Dash
        }
    }

    /// <summary>
    /// Blisterbone Meatfiend.
    /// </summary>
    [ScriptFilterCreatureId(20599)]
    public class MalgraveBlisterboneMeatfiendAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [65950, 65951];

        public MalgraveBlisterboneMeatfiendAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }

        public override void OnLoad(ICreatureEntity owner)
        {
            base.OnLoad(owner);
            AddAbility(66105, 0d, 4d); // Onslaught
            AddAbility(66164, 0d, 7d); // Furious Swings
        }
    }

    /// <summary>
    /// Blisterbone Junker.
    /// </summary>
    [ScriptFilterCreatureId(20600)]
    public class MalgraveBlisterboneJunkerAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [65963];

        public MalgraveBlisterboneJunkerAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }
    }

    /// <summary>
    /// Murk.
    /// </summary>
    [ScriptFilterCreatureId(20814)]
    public class MalgraveMurkAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [65952, 65953];

        public MalgraveMurkAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }

        public override void OnLoad(ICreatureEntity owner)
        {
            base.OnLoad(owner);
            AddAbility(28657, 0d, 4d); // Sweeping Strike
            AddAbility(77170, 0d, 7d); // Directed Rage
        }
    }

    /// <summary>
    /// Gurk.
    /// </summary>
    [ScriptFilterCreatureId(20828)]
    public class MalgraveGurkAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [65952, 65953];

        public MalgraveGurkAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }

        public override void OnLoad(ICreatureEntity owner)
        {
            base.OnLoad(owner);
            AddAbility(28657, 0d, 4d); // Sweeping Strike
            AddAbility(77170, 0d, 7d); // Directed Rage
        }
    }

    /// <summary>
    /// Sunstorm Blademaster.
    /// </summary>
    [ScriptFilterCreatureId(20918)]
    public class MalgraveSunstormBlademasterAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [65950, 65951];

        public MalgraveSunstormBlademasterAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }

        public override void OnLoad(ICreatureEntity owner)
        {
            base.OnLoad(owner);
            AddAbility(66006, 0d, 4d); // Hack and Slash
        }
    }

    /// <summary>
    /// Sunstorm Savager.
    /// </summary>
    [ScriptFilterCreatureId(20919)]
    public class MalgraveSunstormSavagerAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [65715, 65716];

        public MalgraveSunstormSavagerAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }
    }

    /// <summary>
    /// Blisterbone Sandclaw.
    /// </summary>
    [ScriptFilterCreatureId(32337)]
    public class MalgraveBlisterboneSandclawAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [65785, 65786];

        public MalgraveBlisterboneSandclawAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }
    }

    /// <summary>
    /// Dustback Gnawer.
    /// </summary>
    [ScriptFilterCreatureId(32495)]
    public class MalgraveDustbackGnawerAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [65814, 65815];

        public MalgraveDustbackGnawerAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
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
    /// Spirit of Clanlord Vezrek.
    /// </summary>
    [ScriptFilterCreatureId(32687)]
    public class MalgraveSpiritOfClanlordVezrekAI : ExpeditionCreatureAI
    {
        public MalgraveSpiritOfClanlordVezrekAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }

        public override void OnLoad(ICreatureEntity owner)
        {
            base.OnLoad(owner);
            AddAbility(66105, 0d, 4d); // Onslaught
        }
    }

    /// <summary>
    /// Dustwing Scavenger.
    /// </summary>
    [ScriptFilterCreatureId(32893)]
    public class MalgraveDustwingScavengerAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [65753, 65754];

        public MalgraveDustwingScavengerAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }
    }

    /// <summary>
    /// Warthide.
    /// </summary>
    [ScriptFilterCreatureId(33073)]
    public class MalgraveWarthideAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [65715, 65716];

        public MalgraveWarthideAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }

        public override void OnLoad(ICreatureEntity owner)
        {
            base.OnLoad(owner);
            AddAbility(65980, 0d, 4d); // Rending Wave
            AddAbility(66010, 0d, 7d); // Lateral Slice
            AddAbility(66073, 0d, 10d); // Frenzied Rush
        }
    }

    /// <summary>
    /// Sandmaw Alpha.
    /// </summary>
    [ScriptFilterCreatureId(33095)]
    public class MalgraveSandmawAlphaAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [65709, 65711];

        public MalgraveSandmawAlphaAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }

        public override void OnLoad(ICreatureEntity owner)
        {
            base.OnLoad(owner);
            AddAbility(66057, 0d, 4d); // Gnashing Teeth
        }
    }

    /// <summary>
    /// Sandmaw Padfoot.
    /// </summary>
    [ScriptFilterCreatureId(33096)]
    public class MalgraveSandmawPadfootAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [65709, 65711];

        public MalgraveSandmawPadfootAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }

        public override void OnLoad(ICreatureEntity owner)
        {
            base.OnLoad(owner);
            AddAbility(66057, 0d, 4d); // Gnashing Teeth
        }
    }

    /// <summary>
    /// Duneweb Hatchling.
    /// </summary>
    [ScriptFilterCreatureId(33116)]
    public class MalgraveDunewebHatchlingAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [65785, 65786];

        public MalgraveDunewebHatchlingAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }
    }

    /// <summary>
    /// Wildskitter Sandclaw.
    /// </summary>
    [ScriptFilterCreatureId(33194)]
    public class MalgraveWildskitterSandclawAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [65785, 65786];

        public MalgraveWildskitterSandclawAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }
    }

    /// <summary>
    /// Dustback Varmint.
    /// </summary>
    [ScriptFilterCreatureId(33399)]
    public class MalgraveDustbackVarmintAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [65814, 65815];

        public MalgraveDustbackVarmintAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }

        public override void OnLoad(ICreatureEntity owner)
        {
            base.OnLoad(owner);
            AddAbility(66011, 0d, 4d); // Feeding Frenzy
            AddAbility(66014, 0d, 7d); // Fissure Charge
            AddAbility(66130, 0d, 10d); // Snap Trap
        }
    }

    /// <summary>
    /// Dustback Tunneler.
    /// </summary>
    [ScriptFilterCreatureId(33400)]
    public class MalgraveDustbackTunnelerAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [65814, 65815];

        public MalgraveDustbackTunnelerAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
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
    /// Securitybot.
    /// </summary>
    [ScriptFilterCreatureId(33492)]
    public class MalgraveSecuritybotAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [65735];

        public MalgraveSecuritybotAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
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
    /// Sparrek Cloudborn.
    /// </summary>
    [ScriptFilterCreatureId(34285)]
    public class MalgraveSparrekCloudbornAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [65751, 65752];

        public MalgraveSparrekCloudbornAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }

        public override void OnLoad(ICreatureEntity owner)
        {
            base.OnLoad(owner);
            AddAbility(66118, 0d, 4d); // Bladestrike
            AddAbility(66122, 0d, 7d); // Bladespin
        }
    }

    /// <summary>
    /// Skywing Stormmother.
    /// </summary>
    [ScriptFilterCreatureId(34427)]
    public class MalgraveSkywingStormmotherAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [65995];

        public MalgraveSkywingStormmotherAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }
    }

    /// <summary>
    /// Blisterbone Runt.
    /// </summary>
    [ScriptFilterCreatureId(34583)]
    public class MalgraveBlisterboneRuntAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [65715, 65716];

        public MalgraveBlisterboneRuntAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }

        public override void OnLoad(ICreatureEntity owner)
        {
            base.OnLoad(owner);
            AddAbility(39895, 0d, 4d); // Rend
            AddAbility(65980, 0d, 7d); // Rending Wave
            AddAbility(66010, 0d, 10d); // Lateral Slice
            AddAbility(66073, 0d, 13d); // Frenzied Rush
        }
    }

    /// <summary>
    /// Sunfeather Eviscerator.
    /// </summary>
    [ScriptFilterCreatureId(35468)]
    public class MalgraveSunfeatherEvisceratorAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [65812, 65813];

        public MalgraveSunfeatherEvisceratorAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }

        public override void OnLoad(ICreatureEntity owner)
        {
            base.OnLoad(owner);
            AddAbility(66044, 0d, 4d); // Tooth and Nail
        }
    }

    /// <summary>
    /// Duneweb Lurker.
    /// </summary>
    [ScriptFilterCreatureId(35471)]
    public class MalgraveDunewebLurkerAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [65763, 65764];

        public MalgraveDunewebLurkerAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }
    }

    /// <summary>
    /// Blisterbone Raider Ghost.
    /// </summary>
    [ScriptFilterCreatureId(53212)]
    public class MalgraveBlisterboneRaiderGhostAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [65715, 65716];

        public MalgraveBlisterboneRaiderGhostAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }

        public override void OnLoad(ICreatureEntity owner)
        {
            base.OnLoad(owner);
            AddAbility(65980, 0d, 4d); // Rending Wave
            AddAbility(66010, 0d, 7d); // Lateral Slice
            AddAbility(66073, 0d, 10d); // Frenzied Rush
        }
    }

    /// <summary>
    /// Blisterbone Meatfiend Ghost.
    /// </summary>
    [ScriptFilterCreatureId(53213)]
    public class MalgraveBlisterboneMeatfiendGhostAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [65950, 65951];

        public MalgraveBlisterboneMeatfiendGhostAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
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
    /// Blisterbone Runt Ghost.
    /// </summary>
    [ScriptFilterCreatureId(53214)]
    public class MalgraveBlisterboneRuntGhostAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [65715, 65716];

        public MalgraveBlisterboneRuntGhostAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }

        public override void OnLoad(ICreatureEntity owner)
        {
            base.OnLoad(owner);
            AddAbility(65980, 0d, 4d); // Rending Wave
            AddAbility(66010, 0d, 7d); // Lateral Slice
            AddAbility(66073, 0d, 10d); // Frenzied Rush
        }
    }

    /// <summary>
    /// Pricklevine Firebloomer.
    /// </summary>
    [ScriptFilterCreatureId(53370)]
    public class MalgravePricklevineFirebloomerAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [65761, 65762];

        public MalgravePricklevineFirebloomerAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }

        public override void OnLoad(ICreatureEntity owner)
        {
            base.OnLoad(owner);
            AddAbility(66076, 0d, 4d); // Burrow
        }
    }

    /// <summary>
    /// Awakened Warrior.
    /// </summary>
    [ScriptFilterCreatureId(53414)]
    public class MalgraveAwakenedWarriorAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [65743, 65744];

        public MalgraveAwakenedWarriorAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
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
    /// Thornhide.
    /// </summary>
    [ScriptFilterCreatureId(68183)]
    public class MalgraveThornhideAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [65715, 65716];

        public MalgraveThornhideAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }

        public override void OnLoad(ICreatureEntity owner)
        {
            base.OnLoad(owner);
            AddAbility(65980, 0d, 4d); // Rending Wave
            AddAbility(66010, 0d, 7d); // Lateral Slice
            AddAbility(66073, 0d, 10d); // Frenzied Rush
        }
    }
}
