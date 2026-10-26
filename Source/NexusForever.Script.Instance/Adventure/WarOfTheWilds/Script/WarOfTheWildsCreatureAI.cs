using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Spell;
using NexusForever.GameTable;
using NexusForever.Script.Template.Filter;
using NexusForever.Script.Instance.Expedition;
using NexusForever.Shared;

namespace NexusForever.Script.Instance.Adventure.WarOfTheWilds.Script
{
    // Generated from the archived Jabbithole ability lists of each creature (normal and veteran pages), cooldowns from Spell4.

    /// <summary>
    /// Rootbrute Deathcap.
    /// </summary>
    [ScriptFilterCreatureId(25961)]
    public class WildsRootbruteDeathcapAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [65958, 65959];

        public WildsRootbruteDeathcapAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }

        public override void OnLoad(ICreatureEntity owner)
        {
            base.OnLoad(owner);
            AddAbility(35323, 0d, 4d); // Grasping Roots
            AddAbility(75121, 0d, 7d); // Root Spike
            AddAbility(75122, 0d, 10d); // Grasping Roots
            AddAbility(75123, 0d, 13d); // Ascending Roots
            AddAbility(75125, 0d, 16d); // Blinding Essence
        }
    }

    /// <summary>
    /// Lord Hoarfrost.
    /// </summary>
    [ScriptFilterCreatureId(25967)]
    public class WildsLordHoarfrostAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [65937, 65938];

        public WildsLordHoarfrostAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }

        public override void OnLoad(ICreatureEntity owner)
        {
            base.OnLoad(owner);
            AddAbility(38484, 0d, 4d); // Unearthing Smash
            AddAbility(66135, 0d, 7d); // Devastation
            AddAbility(66137, 0d, 10d); // Unearthing Smash
            AddAbility(69189, 0d, 13d); // Devastation
            AddAbility(69192, 0d, 16d); // Rock Throw
        }
    }

    /// <summary>
    /// Glaciax.
    /// </summary>
    [ScriptFilterCreatureId(25968)]
    public class WildsGlaciaxAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [65769, 65770];

        public WildsGlaciaxAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }

        public override void OnLoad(ICreatureEntity owner)
        {
            base.OnLoad(owner);
            AddAbility(37317, 0d, 4d); // Ground Slam
            AddAbility(66096, 0d, 7d); // Ground Slam
            AddAbility(66133, 0d, 10d); // Maelstrom
            AddAbility(66167, 0d, 13d); // Heavy Rain
        }
    }
}
