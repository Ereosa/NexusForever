using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Spell;
using NexusForever.GameTable;
using NexusForever.Script.Template.Filter;
using NexusForever.Script.Instance.Expedition;
using NexusForever.Shared;

namespace NexusForever.Script.Instance.Adventure.RiotInTheVoid.Script
{
    // Generated from the archived Jabbithole ability lists of each creature (normal and veteran pages), cooldowns from Spell4.

    /// <summary>
    /// Rioting Prisoner.
    /// </summary>
    [ScriptFilterCreatureId(27253)]
    public class RiotRiotingPrisonerAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [46389, 46390];

        public RiotRiotingPrisonerAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }

        public override void OnLoad(ICreatureEntity owner)
        {
            base.OnLoad(owner);
            AddAbility(46398, 0d, 4d); // Flourishing Combo
        }
    }

    /// <summary>
    /// Jarak.
    /// </summary>
    [ScriptFilterCreatureId(27445)]
    public class RiotJarakAI : ExpeditionCreatureAI
    {
        public RiotJarakAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }

        public override void OnLoad(ICreatureEntity owner)
        {
            base.OnLoad(owner);
            AddAbility(46400, 0d, 4d); // Whirlwind
            AddAbility(65991, 0d, 7d); // Shockwave
        }
    }

    /// <summary>
    /// Professor Goldbough.
    /// </summary>
    [ScriptFilterCreatureId(48517, 48846)]
    public class RiotProfessorGoldboughAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [65717];

        public RiotProfessorGoldboughAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }
    }

    /// <summary>
    /// Reprogrammed Guardbot.
    /// </summary>
    [ScriptFilterCreatureId(48715)]
    public class RiotReprogrammedGuardbotAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [65735];

        public RiotReprogrammedGuardbotAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
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
    /// Exile Mercenary.
    /// </summary>
    [ScriptFilterCreatureId(48940)]
    public class RiotExileMercenaryAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [65717, 65950];

        public RiotExileMercenaryAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }

        public override void OnLoad(ICreatureEntity owner)
        {
            base.OnLoad(owner);
            AddAbility(66003, 0d, 4d); // Hack and Slash
        }
    }
}
