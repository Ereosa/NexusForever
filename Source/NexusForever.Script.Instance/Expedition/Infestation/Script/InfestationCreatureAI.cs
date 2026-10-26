using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Spell;
using NexusForever.GameTable;
using NexusForever.Script.Template.Filter;
using NexusForever.Shared;

namespace NexusForever.Script.Instance.Expedition.Infestation.Script
{
    // Generated from the archived Jabbithole ability lists of each creature (normal and veteran pages), cooldowns from Spell4.

    /// <summary>
    /// Lumbering Parasite.
    /// </summary>
    [ScriptFilterCreatureId(22669, 69018, 41722, 69022)]
    public class InfestationLumberingParasiteAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [8093, 8094];

        public InfestationLumberingParasiteAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }

        public override void OnLoad(ICreatureEntity owner)
        {
            base.OnLoad(owner);
            AddAbility(56630, 0d, 4d, veteranOnly: true); // Sludge Expulsion
            AddAbility(56818, 0d, 7d); // Whirling Sludge
            AddAbility(57711, 0d, 10d, veteranOnly: true); // Gurgling Splash
            AddAbility(57744, 0d, 13d, veteranOnly: true); // Muck Rake
            AddAbility(57878, 0d, 16d); // Corrosive Filth
            AddAbility(58263, 0d, 19d); // Ichor Nipped
            AddAbility(74922, 0d, 22d, veteranOnly: true); // Hard Water
        }
    }

    /// <summary>
    /// Cyclopean Parasite.
    /// </summary>
    [ScriptFilterCreatureId(26833, 69019)]
    public class InfestationCyclopeanParasiteAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [8093, 8094];

        public InfestationCyclopeanParasiteAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
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
    /// Scuttling Shredder.
    /// </summary>
    [ScriptFilterCreatureId(36505, 69020, 72781, 72782)]
    public class InfestationScuttlingShredderAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [4012, 4013];

        public InfestationScuttlingShredderAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }

        public override void OnLoad(ICreatureEntity owner)
        {
            base.OnLoad(owner);
            AddAbility(57287, 0d, 4d); // Careful Coating
        }
    }

    /// <summary>
    /// Lashing Horror.
    /// </summary>
    [ScriptFilterCreatureId(36506, 69021, 72779, 72780)]
    public class InfestationLashingHorrorAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [34361, 34362];

        public InfestationLashingHorrorAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
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
    /// Lashing Terror.
    /// </summary>
    [ScriptFilterCreatureId(53820, 69023)]
    public class InfestationLashingTerrorAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [34361, 34362];

        public InfestationLashingTerrorAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
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
    /// Deceased Shiphand.
    /// </summary>
    [ScriptFilterCreatureId(69803)]
    public class InfestationDeceasedShiphandAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [5649, 5652];

        public InfestationDeceasedShiphandAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }
    }

    /// <summary>
    /// Lashing Spawn.
    /// </summary>
    [ScriptFilterCreatureId(69870)]
    public class InfestationLashingSpawnAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [34361, 34362];

        public InfestationLashingSpawnAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }

        public override void OnLoad(ICreatureEntity owner)
        {
            base.OnLoad(owner);
            AddAbility(72400, 0d, 4d); // Tainted Frenzy
            AddAbility(72401, 0d, 7d); // Fetid Gnash
        }
    }

    /// <summary>
    /// Lashing Fiend.
    /// </summary>
    [ScriptFilterCreatureId(69871)]
    public class InfestationLashingFiendAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [34361, 34362];

        public InfestationLashingFiendAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }

        public override void OnLoad(ICreatureEntity owner)
        {
            base.OnLoad(owner);
            AddAbility(72400, 0d, 4d); // Tainted Frenzy
            AddAbility(72401, 0d, 7d); // Fetid Gnash
        }
    }

    /// <summary>
    /// Deceased Guard.
    /// </summary>
    [ScriptFilterCreatureId(69873)]
    public class InfestationDeceasedGuardAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [2764];

        public InfestationDeceasedGuardAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }

        public override void OnLoad(ICreatureEntity owner)
        {
            base.OnLoad(owner);
            AddAbility(51828, 0d, 4d); // Shell Storm
        }
    }
}
