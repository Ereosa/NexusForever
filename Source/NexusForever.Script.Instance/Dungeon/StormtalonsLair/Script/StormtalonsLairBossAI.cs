using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.PublicEvent;
using NexusForever.Game.Abstract.Spell;
using NexusForever.GameTable;
using NexusForever.Script.Instance.Expedition;
using NexusForever.Script.Template.Filter;
using NexusForever.Shared;

namespace NexusForever.Script.Instance.Dungeon.StormtalonsLair.Script
{
    /// <summary>
    /// Abilities come from the archived Jabbithole ability lists of each boss (the Spell4 descriptions name the boss and the ability),
    /// cooldowns and the add timings are guesses.
    /// </summary>
    public abstract class StormtalonsLairBossAI : ExpeditionCreatureAI
    {
        private readonly List<float> thresholds;
        private int nextThreshold;

        protected StormtalonsLairBossAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager, params float[] thresholds)
            : base(spellParametersFactory, gameTableManager)
        {
            this.thresholds = thresholds.OrderByDescending(t => t).ToList();
        }

        public override void OnEnterCombat()
        {
            base.OnEnterCombat();
            nextThreshold = 0;
        }

        public override void Update(double lastTick)
        {
            base.Update(lastTick);

            if (!entity.IsAlive || !entity.InCombat || nextThreshold >= thresholds.Count)
                return;
            if (entity.Health > entity.MaxHealth * thresholds[nextThreshold])
                return;

            OnThreshold(nextThreshold++);
        }

        /// <summary>
        /// Invoked when the boss' health drops below the next of its thresholds.
        /// </summary>
        protected abstract void OnThreshold(int index);

        protected void InvokeEvent(Action<StormtalonsLairEventScript> action)
        {
            IPublicEvent publicEvent = entity.Map?.PublicEventManager.GetEvent(145);
            publicEvent?.InvokeScriptCollection(action);
        }
    }

    /// <summary>
    /// Blade-Wind the Invoker: Thunder Cross, Lightning Strike on a player's position, Electrostatic Pulse, Static Wisps at 75, 50 and 25%.
    /// </summary>
    [ScriptFilterCreatureId(17160, 33405)]
    public class StormtalonsLairInvokerAI : StormtalonsLairBossAI
    {
        protected override uint[] AutoAttacks => [38818];

        public StormtalonsLairInvokerAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager, 0.75f, 0.5f, 0.25f)
        {
        }

        public override void OnLoad(ICreatureEntity owner)
        {
            base.OnLoad(owner);
            AddAbility(39935, 10d, 5d);                     // Lightning Strike
            AddAbility(38786, 16d, 9d, normalOnly: true);   // Thunder Cross
            AddAbility(63231, 16d, 9d, veteranOnly: true);  // Thunder Cross (veteran)
            AddAbility(39074, 25d, 18d);                    // Electrostatic Pulse
        }

        protected override void OnThreshold(int index)
        {
            InvokeEvent(s => s.OnInvokerAdds(entity));
        }
    }

    /// <summary>
    /// Aethros: Tempest, Thunderbolt, Gusts of Aethros at 80 and 60%, Gusts and a Twister at 40 and 20%.
    /// </summary>
    [ScriptFilterCreatureId(17166, 32703)]
    public class StormtalonsLairAethrosAI : StormtalonsLairBossAI
    {
        protected override uint[] AutoAttacks => [70404, 70405];

        public StormtalonsLairAethrosAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager, 0.8f, 0.6f, 0.4f, 0.2f)
        {
        }

        public override void OnLoad(ICreatureEntity owner)
        {
            base.OnLoad(owner);
            AddAbility(39730, 9d, 4d);                     // Thunderbolt
            AddAbility(39682, 18d, 10d, normalOnly: true);  // Tempest
            AddAbility(63114, 18d, 10d, veteranOnly: true); // Tempest (veteran)
        }

        protected override void OnThreshold(int index)
        {
            InvokeEvent(s => s.OnAethrosAdds(entity, index >= 2));
        }
    }

    /// <summary>
    /// Stormtalon: Chomp, Lightning Strike, Static Wave, Thunder Call; the strikes come faster below 50% (the tiered Lightning Strike).
    /// </summary>
    [ScriptFilterCreatureId(17163, 33406)]
    public class StormtalonsLairStormtalonAI : StormtalonsLairBossAI
    {
        protected override uint[] AutoAttacks => [39580];

        public StormtalonsLairStormtalonAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager, 0.5f)
        {
        }

        public override void OnLoad(ICreatureEntity owner)
        {
            base.OnLoad(owner);
            AddAbility(39596, 10d, 5d);  // Lightning Strike
            AddAbility(39681, 16d, 9d);  // Static Wave
            AddAbility(39800, 24d, 15d); // Thunder Call
        }

        protected override void OnThreshold(int index)
        {
            // below half health the storm builds: a second, stronger Lightning Strike
            AddAbility(63252, 12d, 2d);
        }
    }
}
