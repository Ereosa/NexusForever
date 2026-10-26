using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Spell;
using NexusForever.GameTable;
using NexusForever.Script.Instance.Expedition;
using NexusForever.Script.Template.Filter;
using NexusForever.Shared;

namespace NexusForever.Script.Instance.Adventure.LevianBay.Script
{
    // Boss abilities from the client's Levian Bay Adventure spells, the patterns from the 2015 QueueTimes walkthrough; timings guessed.

    /// <summary>
    /// Calidor Antevian: Variance Protocol on one player, Point of Division (to be interrupted), Fractured Symmetry, and Siphoning Power from
    /// the room's batteries at half and a quarter of his health.
    /// </summary>
    [ScriptFilterCreatureId(67701, 69135)]
    public class LevianBayCalidorAI : ExpeditionCreatureAI
    {
        private int siphons;

        protected override uint[] AutoAttacks => [78137];

        public LevianBayCalidorAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }

        public override void OnLoad(ICreatureEntity owner)
        {
            base.OnLoad(owner);
            AddAbility(78673, 16d, 6d);  // Variance Protocol
            AddAbility(78677, 28d, 14d); // Point of Division
            AddAbility(78315, 34d, 22d); // Fractured Symmetry
            AddAbility(78229, 12d, 9d);  // Calidor Blast
        }

        public override void Update(double lastTick)
        {
            if (siphons < 2 && entity.IsAlive && entity.InCombat && !entity.IsCasting()
                && entity.Health < entity.MaxHealth * (siphons == 0 ? 0.5f : 0.25f))
            {
                siphons++;
                CastSpell(78141, null); // Siphoning Power
                return;
            }

            base.Update(lastTick);
        }
    }

    /// <summary>
    /// Varmintron: The Canimid Express and Rock Barrage.
    /// </summary>
    [ScriptFilterCreatureId(68206)]
    public class LevianBayVarmintronAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [77624, 77625];

        public LevianBayVarmintronAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }

        public override void OnLoad(ICreatureEntity owner)
        {
            base.OnLoad(owner);
            AddAbility(77621, 18d, 8d);  // The Canimid Express
            AddAbility(77668, 24d, 14d); // Rock Barrage (Eldan Excavation)
        }
    }

    /// <summary>
    /// Gopherzoid: Earthen Wave and Augmented Tempest (the whirlwind that chases a player).
    /// </summary>
    [ScriptFilterCreatureId(68207)]
    public class LevianBayGopherzoidAI : ExpeditionCreatureAI
    {
        protected override uint[] AutoAttacks => [77624, 77625];

        public LevianBayGopherzoidAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }

        public override void OnLoad(ICreatureEntity owner)
        {
            base.OnLoad(owner);
            AddAbility(77707, 16d, 6d);  // Earthen Wave
            AddAbility(77738, 26d, 12d); // Augmented Tempest
        }
    }
}
