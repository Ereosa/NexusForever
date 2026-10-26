using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Spell;
using NexusForever.GameTable;
using NexusForever.Script.Instance.Expedition;
using NexusForever.Script.Template.Filter;
using NexusForever.Shared;

namespace NexusForever.Script.Instance.Dungeon.Skullcano.Script
{
    // Generated from the archived Jabbithole ability lists of each creature (normal and veteran pages), cooldowns from Spell4.

    /// <summary>
    /// Quartermaster Gruh'ar.
    /// </summary>
    [ScriptFilterCreatureId(24490, 24896)]
    public class SkullcanoQuartermasterGruhArAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [70210, 70211];

        public SkullcanoQuartermasterGruhArAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }

        public override void OnLoad(ICreatureEntity owner)
        {
            base.OnLoad(owner);
            AddAbility(41899, 0d, 4d); // Dead Eye
            AddAbility(41901, 0d, 7d); // Kneecap
            AddAbility(63663, 0d, 10d, veteranOnly: true); // Dead Eye
            AddAbility(63673, 0d, 13d, veteranOnly: true); // Roundhouse
            AddAbility(63677, 0d, 16d, veteranOnly: true); // Fatal Shot
            AddAbility(70217, 0d, 19d); // Debilitating Barrage
        }
    }

    /// <summary>
    /// Grim-Grim Doomcaller.
    /// </summary>
    [ScriptFilterCreatureId(24494, 24899)]
    public class SkullcanoGrimGrimDoomcallerAI : ExpeditionCreatureAI
    {
        public SkullcanoGrimGrimDoomcallerAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }

        public override void OnLoad(ICreatureEntity owner)
        {
            base.OnLoad(owner);
            AddAbility(36339, 0d, 4d); // Fire Storm
            AddAbility(39542, 0d, 7d); // Conjure Spirits
            AddAbility(41657, 0d, 10d); // Condemnation
            AddAbility(52852, 0d, 13d); // Mystical Bolt
        }
    }

    /// <summary>
    /// Grim-Grim Hexweaver.
    /// </summary>
    [ScriptFilterCreatureId(24495, 24900)]
    public class SkullcanoGrimGrimHexweaverAI : ExpeditionCreatureAI
    {
        public SkullcanoGrimGrimHexweaverAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }

        public override void OnLoad(ICreatureEntity owner)
        {
            base.OnLoad(owner);
            AddAbility(52852, 0d, 4d); // Mystical Bolt
        }
    }

    /// <summary>
    /// Grim-Grim Curseblade.
    /// </summary>
    [ScriptFilterCreatureId(24498, 24901)]
    public class SkullcanoGrimGrimCursebladeAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [70190, 70191];

        public SkullcanoGrimGrimCursebladeAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }
    }

    /// <summary>
    /// Grim-Grim Blightmask.
    /// </summary>
    [ScriptFilterCreatureId(24499, 24902)]
    public class SkullcanoGrimGrimBlightmaskAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [70190, 70191];

        public SkullcanoGrimGrimBlightmaskAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }
    }

    /// <summary>
    /// Redmoon Prisoner.
    /// </summary>
    [ScriptFilterCreatureId(24507, 24905)]
    public class SkullcanoRedmoonPrisonerAI : ExpeditionCreatureAI
    {
        public SkullcanoRedmoonPrisonerAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }

        public override void OnLoad(ICreatureEntity owner)
        {
            base.OnLoad(owner);
            AddAbility(44200, 0d, 4d); // Swipe
            AddAbility(70197, 0d, 7d); // Punch
            AddAbility(70198, 0d, 10d); // Jab
        }
    }

    /// <summary>
    /// Redmoon Jailor.
    /// </summary>
    [ScriptFilterCreatureId(24510, 24908)]
    public class SkullcanoRedmoonJailorAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [70196];

        public SkullcanoRedmoonJailorAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }

        public override void OnLoad(ICreatureEntity owner)
        {
            base.OnLoad(owner);
            AddAbility(38250, 0d, 4d); // Enslave
        }
    }

    /// <summary>
    /// Redmoon Gold Guardian.
    /// </summary>
    [ScriptFilterCreatureId(24512, 24910)]
    public class SkullcanoRedmoonGoldGuardianAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [70194, 70195];

        public SkullcanoRedmoonGoldGuardianAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }

        public override void OnLoad(ICreatureEntity owner)
        {
            base.OnLoad(owner);
            AddAbility(41874, 0d, 4d); // Marauder Cleave
            AddAbility(41885, 0d, 7d); // Molten Whip
        }
    }

    /// <summary>
    /// Redmoon Gold Hoarder.
    /// </summary>
    [ScriptFilterCreatureId(24514, 24911)]
    public class SkullcanoRedmoonGoldHoarderAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [70201, 70203];

        public SkullcanoRedmoonGoldHoarderAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }

        public override void OnLoad(ICreatureEntity owner)
        {
            base.OnLoad(owner);
            AddAbility(41847, 0d, 4d); // Debilitating Barrage
        }
    }

    /// <summary>
    /// Redmoon Cannoneer.
    /// </summary>
    [ScriptFilterCreatureId(24578, 24913)]
    public class SkullcanoRedmoonCannoneerAI : ExpeditionCreatureAI
    {
        public SkullcanoRedmoonCannoneerAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }

        public override void OnLoad(ICreatureEntity owner)
        {
            base.OnLoad(owner);
            AddAbility(70213, 0d, 4d); // Plasma Shot
        }
    }

    /// <summary>
    /// Redmoon Sniper.
    /// </summary>
    [ScriptFilterCreatureId(24579, 24914)]
    public class SkullcanoRedmoonSniperAI : ExpeditionCreatureAI
    {
        public SkullcanoRedmoonSniperAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }

        public override void OnLoad(ICreatureEntity owner)
        {
            base.OnLoad(owner);
            AddAbility(41752, 0d, 4d); // Dead Eye
            AddAbility(70207, 0d, 7d); // Plasma Shot
        }
    }

    /// <summary>
    /// Redmoon Furyspitter.
    /// </summary>
    [ScriptFilterCreatureId(24580, 24916)]
    public class SkullcanoRedmoonFuryspitterAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [52704];

        public SkullcanoRedmoonFuryspitterAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }

        public override void OnLoad(ICreatureEntity owner)
        {
            base.OnLoad(owner);
            AddAbility(52705, 0d, 4d); // Meteor
        }
    }

    /// <summary>
    /// Redmoon Necromancer.
    /// </summary>
    [ScriptFilterCreatureId(24581, 24919)]
    public class SkullcanoRedmoonNecromancerAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [52572];

        public SkullcanoRedmoonNecromancerAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }
    }

    /// <summary>
    /// Redmoon Crusher.
    /// </summary>
    [ScriptFilterCreatureId(24618, 24920)]
    public class SkullcanoRedmoonCrusherAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [70208, 70209];

        public SkullcanoRedmoonCrusherAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }

        public override void OnLoad(ICreatureEntity owner)
        {
            base.OnLoad(owner);
            AddAbility(41753, 0d, 4d); // Concussion Blitz
            AddAbility(52661, 0d, 7d); // Twinkle Toes
        }
    }

    /// <summary>
    /// Redmoon Razortail.
    /// </summary>
    [ScriptFilterCreatureId(25561, 25562)]
    public class SkullcanoRedmoonRazortailAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [70199, 70200];

        public SkullcanoRedmoonRazortailAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }
    }

    /// <summary>
    /// Ravenous Razorbeak.
    /// </summary>
    [ScriptFilterCreatureId(25564, 25565)]
    public class SkullcanoRavenousRazorbeakAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [70192, 70193];

        public SkullcanoRavenousRazorbeakAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }

        public override void OnLoad(ICreatureEntity owner)
        {
            base.OnLoad(owner);
            AddAbility(41378, 0d, 4d); // Ravenous Rush
            AddAbility(53188, 0d, 7d); // Booming Screech
        }
    }

    /// <summary>
    /// Redmoon Chaingun.
    /// </summary>
    [ScriptFilterCreatureId(26236, 26238)]
    public class SkullcanoRedmoonChaingunAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [39564];

        public SkullcanoRedmoonChaingunAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }
    }

    /// <summary>
    /// Magma Totem.
    /// </summary>
    [ScriptFilterCreatureId(32664, 38592)]
    public class SkullcanoMagmaTotemAI : ExpeditionCreatureAI
    {
        public SkullcanoMagmaTotemAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }

        public override void OnLoad(ICreatureEntity owner)
        {
            base.OnLoad(owner);
            AddAbility(63270, 0d, 4d); // Burning
            AddAbility(72508, 0d, 7d); // Burning
        }
    }

    /// <summary>
    /// Stew Pot.
    /// </summary>
    [ScriptFilterCreatureId(32751)]
    public class SkullcanoStewPotAI : ExpeditionCreatureAI
    {
        public SkullcanoStewPotAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }

        public override void OnLoad(ICreatureEntity owner)
        {
            base.OnLoad(owner);
            AddAbility(50009, 0d, 4d); // Granok Stew
        }
    }

    /// <summary>
    /// Colossal Mushroom.
    /// </summary>
    [ScriptFilterCreatureId(33071)]
    public class SkullcanoColossalMushroomAI : ExpeditionCreatureAI
    {
        public SkullcanoColossalMushroomAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }

        public override void OnLoad(ICreatureEntity owner)
        {
            base.OnLoad(owner);
            AddAbility(36386, 0d, 4d); // Antigen Rupture
        }
    }

    /// <summary>
    /// Ammunition Deposit.
    /// </summary>
    [ScriptFilterCreatureId(34457, 34458)]
    public class SkullcanoAmmunitionDepositAI : ExpeditionCreatureAI
    {
        public SkullcanoAmmunitionDepositAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }

        public override void OnLoad(ICreatureEntity owner)
        {
            base.OnLoad(owner);
            AddAbility(38447, 0d, 4d); // Explosive Ammunition
        }
    }

    /// <summary>
    /// Terraformer.
    /// </summary>
    [ScriptFilterCreatureId(34629)]
    public class SkullcanoTerraformerAI : ExpeditionCreatureAI
    {
        public SkullcanoTerraformerAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }

        public override void OnLoad(ICreatureEntity owner)
        {
            base.OnLoad(owner);
            AddAbility(38579, 0d, 4d); // Self Destruct
            AddAbility(39501, 0d, 7d); // Combustion
        }
    }

    /// <summary>
    /// Redmoon Wrestler.
    /// </summary>
    [ScriptFilterCreatureId(37751, 37752)]
    public class SkullcanoRedmoonWrestlerAI : ExpeditionCreatureAI
    {
        public SkullcanoRedmoonWrestlerAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }

        public override void OnLoad(ICreatureEntity owner)
        {
            base.OnLoad(owner);
            AddAbility(41767, 0d, 4d); // Pile Driver
            AddAbility(41770, 0d, 7d); // Trample
            AddAbility(70204, 0d, 10d); // Punch
            AddAbility(70205, 0d, 13d); // Jab
        }
    }

    /// <summary>
    /// Flame Wall.
    /// </summary>
    [ScriptFilterCreatureId(39137, 39191)]
    public class SkullcanoFlameWallAI : ExpeditionCreatureAI
    {
        public SkullcanoFlameWallAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }

        public override void OnLoad(ICreatureEntity owner)
        {
            base.OnLoad(owner);
            AddAbility(41285, 0d, 4d); // Flame Wall
        }
    }

    /// <summary>
    /// Heat Vent.
    /// </summary>
    [ScriptFilterCreatureId(39161)]
    public class SkullcanoHeatVentAI : ExpeditionCreatureAI
    {
        public SkullcanoHeatVentAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }

        public override void OnLoad(ICreatureEntity owner)
        {
            base.OnLoad(owner);
            AddAbility(41293, 0d, 4d); // Heat Vent
            AddAbility(63697, 0d, 7d); // Heat Vent
        }
    }

    /// <summary>
    /// Cave In.
    /// </summary>
    [ScriptFilterCreatureId(39211)]
    public class SkullcanoCaveInAI : ExpeditionCreatureAI
    {
        public SkullcanoCaveInAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }

        public override void OnLoad(ICreatureEntity owner)
        {
            base.OnLoad(owner);
            AddAbility(41339, 0d, 4d); // Collapse
            AddAbility(63698, 0d, 7d); // Collapse
        }
    }

    /// <summary>
    /// Razooki.
    /// </summary>
    [ScriptFilterCreatureId(39419, 56779)]
    public class SkullcanoRazookiAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [70199, 70200];

        public SkullcanoRazookiAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }

        public override void OnLoad(ICreatureEntity owner)
        {
            base.OnLoad(owner);
            AddAbility(52949, 0d, 4d); // Aerial Bombardment
        }
    }
}
