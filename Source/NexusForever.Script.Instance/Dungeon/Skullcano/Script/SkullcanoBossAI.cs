using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.PublicEvent;
using NexusForever.Game.Abstract.Spell;
using NexusForever.GameTable;
using NexusForever.Script.Instance.Expedition;
using NexusForever.Script.Template.Filter;
using NexusForever.Shared;

namespace NexusForever.Script.Instance.Dungeon.Skullcano.Script
{
    /// <summary>
    /// Abilities come from the archived Jabbithole ability lists of each boss (the Spell4 descriptions name the boss and the ability),
    /// cooldowns and the add timings are guesses.
    /// </summary>
    public abstract class SkullcanoBossAI : ExpeditionCreatureAI
    {
        private readonly List<float> thresholds;
        private int nextThreshold;

        protected SkullcanoBossAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager, params float[] thresholds)
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
        protected virtual void OnThreshold(int index)
        {
        }

        protected void InvokeEvent(Action<SkullcanoEventScript> action)
        {
            IPublicEvent publicEvent = entity.Map?.PublicEventManager.GetEvent(148);
            publicEvent?.InvokeScriptCollection(action);
        }
    }

    /// <summary>
    /// Thunderfoot: Pulverize, Thunder Pound, Seismic Tremor.
    /// </summary>
    [ScriptFilterCreatureId(24475, 24893)]
    public class SkullcanoThunderfootAI : SkullcanoBossAI
    {
        protected override uint[] AutoAttacks => [36309, 36310];

        public SkullcanoThunderfootAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }

        public override void OnLoad(ICreatureEntity owner)
        {
            base.OnLoad(owner);
            AddAbility(36283, 10d, 4d);  // Pulverize
            AddAbility(36341, 16d, 9d);  // Thunder Pound
            AddAbility(36437, 24d, 15d); // Seismic Tremor
        }
    }

    /// <summary>
    /// Bosun Octog: Hookshot, Noxious Ink, Scab Stab, Shred; squirgling offspring at 75, 50 and 25%.
    /// </summary>
    [ScriptFilterCreatureId(24486, 24894)]
    public class SkullcanoOctogAI : SkullcanoBossAI
    {
        protected override uint[] AutoAttacks => [36628, 36650];

        public SkullcanoOctogAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager, 0.75f, 0.5f, 0.25f)
        {
        }

        public override void OnLoad(ICreatureEntity owner)
        {
            base.OnLoad(owner);
            AddAbility(41850, 8d, 4d);   // Shred
            AddAbility(31283, 14d, 7d);  // Hookshot
            AddAbility(36614, 18d, 11d); // Noxious Ink
            AddAbility(41443, 22d, 15d); // Scab Stab
        }

        protected override void OnThreshold(int index)
        {
            InvokeEvent(s => s.OnOctogAdds(entity));
        }
    }

    /// <summary>
    /// Mordechai Redmoon: Pin, Vicious Barrage, Turret Grenade; his crew joins at 50%.
    /// </summary>
    [ScriptFilterCreatureId(24489, 24895)]
    public class SkullcanoMordechaiAI : SkullcanoBossAI
    {
        protected override uint[] AutoAttacks => [38458];

        public SkullcanoMordechaiAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager, 0.5f)
        {
        }

        public override void OnLoad(ICreatureEntity owner)
        {
            base.OnLoad(owner);
            AddAbility(39481, 12d, 5d);  // Pin
            AddAbility(38461, 15d, 9d);  // Vicious Barrage
            AddAbility(39493, 20d, 14d); // Turret Grenade
        }

        protected override void OnThreshold(int index)
        {
            InvokeEvent(s => s.OnMordechaiAdds(entity));
        }
    }
}
