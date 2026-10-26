using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Spell;
using NexusForever.GameTable;
using NexusForever.Script.Template.Filter;
using NexusForever.Script.Instance.Expedition;
using NexusForever.Shared;

namespace NexusForever.Script.Instance.Adventure.SiegeOfTempestRefuge.Script
{
    // Generated from the archived Jabbithole ability lists of each creature (normal and veteran pages), cooldowns from Spell4.

    /// <summary>
    /// Exile Rifleman.
    /// </summary>
    [ScriptFilterCreatureId(21284)]
    public class SiegeExileRiflemanAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [65717];

        public SiegeExileRiflemanAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }
    }

    /// <summary>
    /// Exile Enforcer.
    /// </summary>
    [ScriptFilterCreatureId(21286)]
    public class SiegeExileEnforcerAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [65950, 65951];

        public SiegeExileEnforcerAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
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
    /// Exile Sniper.
    /// </summary>
    [ScriptFilterCreatureId(21287)]
    public class SiegeExileSniperAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [65717];

        public SiegeExileSniperAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }
    }

    /// <summary>
    /// Exile Shocktrooper.
    /// </summary>
    [ScriptFilterCreatureId(21478)]
    public class SiegeExileShocktrooperAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [65950, 65951];

        public SiegeExileShocktrooperAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
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
    /// Exile Warbot.
    /// </summary>
    [ScriptFilterCreatureId(21544)]
    public class SiegeExileWarbotAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [53364, 65779];

        public SiegeExileWarbotAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }

        public override void OnLoad(ICreatureEntity owner)
        {
            base.OnLoad(owner);
            AddAbility(38625, 0d, 4d); // Missile Swarm
            AddAbility(38727, 0d, 7d); // Exterminate
            AddAbility(38738, 0d, 10d); // Repulse Field
        }
    }

    /// <summary>
    /// Esper Alderblade.
    /// </summary>
    [ScriptFilterCreatureId(21545)]
    public class SiegeEsperAlderbladeAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [53338, 53366];

        public SiegeEsperAlderbladeAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }
    }

    /// <summary>
    /// Elementalist Blazewood.
    /// </summary>
    [ScriptFilterCreatureId(21546)]
    public class SiegeElementalistBlazewoodAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [53340, 53368];

        public SiegeElementalistBlazewoodAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }
    }

    /// <summary>
    /// FCON Defense Tank.
    /// </summary>
    [ScriptFilterCreatureId(21547)]
    public class SiegeFconDefenseTankAI : ExpeditionCreatureAI
    {
        public SiegeFconDefenseTankAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }

        public override void OnLoad(ICreatureEntity owner)
        {
            base.OnLoad(owner);
            AddAbility(66074, 0d, 4d); // Shock Field
        }
    }

    /// <summary>
    /// Sergeant Berog.
    /// </summary>
    [ScriptFilterCreatureId(21549)]
    public class SiegeSergeantBerogAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [53343, 53370];

        public SiegeSergeantBerogAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }
    }

    /// <summary>
    /// Agent Blackwatch.
    /// </summary>
    [ScriptFilterCreatureId(21550)]
    public class SiegeAgentBlackwatchAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [53345, 53372];

        public SiegeAgentBlackwatchAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }
    }

    /// <summary>
    /// Dynamite Dax.
    /// </summary>
    [ScriptFilterCreatureId(21551)]
    public class SiegeDynamiteDaxAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [53346, 53347];

        public SiegeDynamiteDaxAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }
    }

    /// <summary>
    /// Sharpshooter Sparks.
    /// </summary>
    [ScriptFilterCreatureId(21552)]
    public class SiegeSharpshooterSparksAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [53348, 53349];

        public SiegeSharpshooterSparksAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }

        public override void OnLoad(ICreatureEntity owner)
        {
            base.OnLoad(owner);
            AddAbility(44435, 0d, 4d); // Sky Blast
        }
    }

    /// <summary>
    /// Medic Springwell.
    /// </summary>
    [ScriptFilterCreatureId(21553)]
    public class SiegeMedicSpringwellAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [53351, 53378];

        public SiegeMedicSpringwellAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }
    }

    /// <summary>
    /// Trooper Bashok.
    /// </summary>
    [ScriptFilterCreatureId(21554)]
    public class SiegeTrooperBashokAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [53352, 53353];

        public SiegeTrooperBashokAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }
    }

    /// <summary>
    /// Elementalist Frostleaf.
    /// </summary>
    [ScriptFilterCreatureId(21555)]
    public class SiegeElementalistFrostleafAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [53354, 53381];

        public SiegeElementalistFrostleafAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }
    }

    /// <summary>
    /// Reaper Ludov.
    /// </summary>
    [ScriptFilterCreatureId(21556)]
    public class SiegeReaperLudovAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [53357, 53383];

        public SiegeReaperLudovAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }
    }

    /// <summary>
    /// Evoker Peale.
    /// </summary>
    [ScriptFilterCreatureId(21557)]
    public class SiegeEvokerPealeAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [53358, 53359];

        public SiegeEvokerPealeAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }
    }

    /// <summary>
    /// Dominion Enforcer.
    /// </summary>
    [ScriptFilterCreatureId(22161)]
    public class SiegeDominionEnforcerAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [65950, 65951];

        public SiegeDominionEnforcerAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
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
    /// Esper Moko.
    /// </summary>
    [ScriptFilterCreatureId(22166)]
    public class SiegeEsperMokoAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [53338, 53366];

        public SiegeEsperMokoAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }
    }

    /// <summary>
    /// Elementalist Vermilius.
    /// </summary>
    [ScriptFilterCreatureId(22167)]
    public class SiegeElementalistVermiliusAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [53340, 53368];

        public SiegeElementalistVermiliusAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }
    }

    /// <summary>
    /// Corporal Vizrek.
    /// </summary>
    [ScriptFilterCreatureId(22168)]
    public class SiegeCorporalVizrekAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [53343, 53370];

        public SiegeCorporalVizrekAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }
    }

    /// <summary>
    /// Agent Razios.
    /// </summary>
    [ScriptFilterCreatureId(22169)]
    public class SiegeAgentRaziosAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [53344, 53372];

        public SiegeAgentRaziosAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }
    }

    /// <summary>
    /// Chief Engineer Vortec.
    /// </summary>
    [ScriptFilterCreatureId(22170)]
    public class SiegeChiefEngineerVortecAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [53346, 53374];

        public SiegeChiefEngineerVortecAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }
    }

    /// <summary>
    /// Marksman Decimus.
    /// </summary>
    [ScriptFilterCreatureId(22171)]
    public class SiegeMarksmanDecimusAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [53348, 53349];

        public SiegeMarksmanDecimusAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }

        public override void OnLoad(ICreatureEntity owner)
        {
            base.OnLoad(owner);
            AddAbility(44435, 0d, 4d); // Sky Blast
        }
    }

    /// <summary>
    /// Physician Astius.
    /// </summary>
    [ScriptFilterCreatureId(22172)]
    public class SiegePhysicianAstiusAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [53351, 53378];

        public SiegePhysicianAstiusAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }
    }

    /// <summary>
    /// Bruiser Shuni.
    /// </summary>
    [ScriptFilterCreatureId(22173)]
    public class SiegeBruiserShuniAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [53352, 53353];

        public SiegeBruiserShuniAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }
    }

    /// <summary>
    /// Bloodshaman Vakaza.
    /// </summary>
    [ScriptFilterCreatureId(22174)]
    public class SiegeBloodshamanVakazaAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [53381];

        public SiegeBloodshamanVakazaAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }
    }

    /// <summary>
    /// Saboteur Arminex.
    /// </summary>
    [ScriptFilterCreatureId(22175)]
    public class SiegeSaboteurArminexAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [53356, 53383];

        public SiegeSaboteurArminexAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }
    }

    /// <summary>
    /// Siren Aria.
    /// </summary>
    [ScriptFilterCreatureId(22176)]
    public class SiegeSirenAriaAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [53358, 53385];

        public SiegeSirenAriaAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }
    }

    /// <summary>
    /// Legion Assault Tank.
    /// </summary>
    [ScriptFilterCreatureId(22199)]
    public class SiegeLegionAssaultTankAI : ExpeditionCreatureAI
    {
        public SiegeLegionAssaultTankAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }

        public override void OnLoad(ICreatureEntity owner)
        {
            base.OnLoad(owner);
            AddAbility(66074, 0d, 4d); // Shock Field
        }
    }

    /// <summary>
    /// Dominion Rifleman.
    /// </summary>
    [ScriptFilterCreatureId(22202)]
    public class SiegeDominionRiflemanAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [65717];

        public SiegeDominionRiflemanAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }
    }

    /// <summary>
    /// Dominion Sniper.
    /// </summary>
    [ScriptFilterCreatureId(22203)]
    public class SiegeDominionSniperAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [65717];

        public SiegeDominionSniperAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }
    }

    /// <summary>
    /// Dominion Shocktrooper.
    /// </summary>
    [ScriptFilterCreatureId(22204)]
    public class SiegeDominionShocktrooperAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [65950, 65951];

        public SiegeDominionShocktrooperAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
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
    /// Advanced Warbot.
    /// </summary>
    [ScriptFilterCreatureId(22244)]
    public class SiegeAdvancedWarbotAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [53336, 53364];

        public SiegeAdvancedWarbotAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }

        public override void OnLoad(ICreatureEntity owner)
        {
            base.OnLoad(owner);
            AddAbility(38573, 0d, 4d); // Exterminate
            AddAbility(38625, 0d, 7d); // Missile Swarm
            AddAbility(38749, 0d, 10d); // Rock 'Em Sock 'Em
            AddAbility(38757, 0d, 13d); // Rock 'Em Sock 'Em
            AddAbility(69203, 0d, 16d); // Exterminate
        }
    }
}
