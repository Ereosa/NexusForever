using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Spell;
using NexusForever.GameTable;
using NexusForever.Script.Template.Filter;
using NexusForever.Script.Instance.Expedition;
using NexusForever.Shared;

namespace NexusForever.Script.Instance.Adventure.HycrestInsurrection.Script
{
    // Generated from the archived Jabbithole ability lists of each creature (normal and veteran pages), cooldowns from Spell4.

    /// <summary>
    /// Shatterforce Trooper.
    /// </summary>
    [ScriptFilterCreatureId(17745)]
    public class HycrestShatterforceTrooperAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [65950, 65951];

        public HycrestShatterforceTrooperAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }
    }

    /// <summary>
    /// Dominion Commando.
    /// </summary>
    [ScriptFilterCreatureId(17804)]
    public class HycrestDominionCommandoAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [65717];

        public HycrestDominionCommandoAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }
    }

    /// <summary>
    /// Dominion Recon Specialist.
    /// </summary>
    [ScriptFilterCreatureId(17823)]
    public class HycrestDominionReconSpecialistAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [65794, 65795];

        public HycrestDominionReconSpecialistAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }
    }

    /// <summary>
    /// Dominion Scientist.
    /// </summary>
    [ScriptFilterCreatureId(17824)]
    public class HycrestDominionScientistAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [65717];

        public HycrestDominionScientistAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }

        public override void OnLoad(ICreatureEntity owner)
        {
            base.OnLoad(owner);
            AddAbility(66020, 0d, 4d); // Flametorch
        }
    }

    /// <summary>
    /// Dominion Scout.
    /// </summary>
    [ScriptFilterCreatureId(17856)]
    public class HycrestDominionScoutAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [65717];

        public HycrestDominionScoutAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }
    }

    /// <summary>
    /// Dominion Soldier.
    /// </summary>
    [ScriptFilterCreatureId(17857)]
    public class HycrestDominionSoldierAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [65717];

        public HycrestDominionSoldierAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }
    }

    /// <summary>
    /// Dominion Shocktrooper.
    /// </summary>
    [ScriptFilterCreatureId(17858)]
    public class HycrestDominionShocktrooperAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [65715, 65716];

        public HycrestDominionShocktrooperAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }
    }

    /// <summary>
    /// Dominion Techsmith.
    /// </summary>
    [ScriptFilterCreatureId(17859)]
    public class HycrestDominionTechsmithAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [65952, 65953];

        public HycrestDominionTechsmithAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }
    }

    /// <summary>
    /// Arsenax Severus.
    /// </summary>
    [ScriptFilterCreatureId(17928)]
    public class HycrestArsenaxSeverusAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [65950, 65951];

        public HycrestArsenaxSeverusAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
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
    /// Hycrest Watchman.
    /// </summary>
    [ScriptFilterCreatureId(17929)]
    public class HycrestHycrestWatchmanAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [65794, 65795];

        public HycrestHycrestWatchmanAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }
    }

    /// <summary>
    /// Dominion Engineering Officer.
    /// </summary>
    [ScriptFilterCreatureId(17949)]
    public class HycrestDominionEngineeringOfficerAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [65717];

        public HycrestDominionEngineeringOfficerAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }
    }

    /// <summary>
    /// Shatterforce Suppressionbot.
    /// </summary>
    [ScriptFilterCreatureId(18211)]
    public class HycrestShatterforceSuppressionbotAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [65779, 65780];

        public HycrestShatterforceSuppressionbotAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }
    }

    /// <summary>
    /// Field Intelligence Agent.
    /// </summary>
    [ScriptFilterCreatureId(18445)]
    public class HycrestFieldIntelligenceAgentAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [65950, 65951];

        public HycrestFieldIntelligenceAgentAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }
    }

    /// <summary>
    /// Shatterforce Responsebot.
    /// </summary>
    [ScriptFilterCreatureId(18509)]
    public class HycrestShatterforceResponsebotAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [65779, 65780];

        public HycrestShatterforceResponsebotAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }

        public override void OnLoad(ICreatureEntity owner)
        {
            base.OnLoad(owner);
            AddAbility(38573, 0d, 4d); // Exterminate
        }
    }

    /// <summary>
    /// Dominion Tracker.
    /// </summary>
    [ScriptFilterCreatureId(18516)]
    public class HycrestDominionTrackerAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [65717];

        public HycrestDominionTrackerAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }
    }

    /// <summary>
    /// Dominion Tracking Lizard.
    /// </summary>
    [ScriptFilterCreatureId(18517)]
    public class HycrestDominionTrackingLizardAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [65755, 65756];

        public HycrestDominionTrackingLizardAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }
    }

    /// <summary>
    /// Intelligence Recovery Agent.
    /// </summary>
    [ScriptFilterCreatureId(19115)]
    public class HycrestIntelligenceRecoveryAgentAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [65794, 65795];

        public HycrestIntelligenceRecoveryAgentAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }
    }

    /// <summary>
    /// Twitchy Groo.
    /// </summary>
    [ScriptFilterCreatureId(23503)]
    public class HycrestTwitchyGrooAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [65717];

        public HycrestTwitchyGrooAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }
    }

    /// <summary>
    /// Dominion Gatekeeper.
    /// </summary>
    [ScriptFilterCreatureId(26853)]
    public class HycrestDominionGatekeeperAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [65950, 65951];

        public HycrestDominionGatekeeperAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }
    }

    /// <summary>
    /// ICI Assassin.
    /// </summary>
    [ScriptFilterCreatureId(27148)]
    public class HycrestIciAssassinAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [65794, 65795];

        public HycrestIciAssassinAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }
    }

    /// <summary>
    /// ICI Agent Corkus.
    /// </summary>
    [ScriptFilterCreatureId(27150)]
    public class HycrestIciAgentCorkusAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [65950, 65951];

        public HycrestIciAgentCorkusAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }
    }

    /// <summary>
    /// Dominion Rocketeer.
    /// </summary>
    [ScriptFilterCreatureId(48255)]
    public class HycrestDominionRocketeerAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [65794, 65795];

        public HycrestDominionRocketeerAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }
    }

    /// <summary>
    /// Major Rhadman.
    /// </summary>
    [ScriptFilterCreatureId(48613)]
    public class HycrestMajorRhadmanAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [65794, 65795];

        public HycrestMajorRhadmanAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }
    }

    /// <summary>
    /// Lieutenant Feraakh.
    /// </summary>
    [ScriptFilterCreatureId(48617)]
    public class HycrestLieutenantFeraakhAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [65715, 65716];

        public HycrestLieutenantFeraakhAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }
    }

    /// <summary>
    /// Twilo Pim.
    /// </summary>
    [ScriptFilterCreatureId(48778)]
    public class HycrestTwiloPimAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [65717];

        public HycrestTwiloPimAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }
    }

    /// <summary>
    /// Dominion Guardbot.
    /// </summary>
    [ScriptFilterCreatureId(49118)]
    public class HycrestDominionGuardbotAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [65735];

        public HycrestDominionGuardbotAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
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
    /// Predator Drone.
    /// </summary>
    [ScriptFilterCreatureId(49122)]
    public class HycrestPredatorDroneAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [65945, 65946];

        public HycrestPredatorDroneAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }
    }

    /// <summary>
    /// Rapid Response Commando.
    /// </summary>
    [ScriptFilterCreatureId(50378)]
    public class HycrestRapidResponseCommandoAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [65717];

        public HycrestRapidResponseCommandoAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }

        public override void OnLoad(ICreatureEntity owner)
        {
            base.OnLoad(owner);
            AddAbility(66020, 0d, 4d); // Flametorch
        }
    }

    /// <summary>
    /// Suppressionbot Mk. II.
    /// </summary>
    [ScriptFilterCreatureId(50758)]
    public class HycrestSuppressionbotMkIiAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [65779, 65780];

        public HycrestSuppressionbotMkIiAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }
    }

    /// <summary>
    /// Rapid Response Drone.
    /// </summary>
    [ScriptFilterCreatureId(50834)]
    public class HycrestRapidResponseDroneAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [65945, 65946];

        public HycrestRapidResponseDroneAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }
    }

    /// <summary>
    /// Shatterforce Vanguard.
    /// </summary>
    [ScriptFilterCreatureId(52719)]
    public class HycrestShatterforceVanguardAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [65794, 65795];

        public HycrestShatterforceVanguardAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }
    }
}
