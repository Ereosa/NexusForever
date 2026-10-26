using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.PublicEvent;
using NexusForever.Game.Abstract.Spell;
using NexusForever.GameTable;
using NexusForever.Script.Instance.Expedition;
using NexusForever.Script.Template.Filter;
using NexusForever.Shared;

namespace NexusForever.Script.Instance.Dungeon.KelVoreth.Script
{
    /// <summary>
    /// Abilities come from the archived Jabbithole ability lists of each boss (the Spell4 descriptions name the boss and the ability),
    /// cooldowns and the add timings are guesses.
    /// </summary>
    public abstract class KelVorethBossAI : ExpeditionCreatureAI
    {
        private readonly List<float> thresholds;
        private int nextThreshold;

        protected KelVorethBossAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager, params float[] thresholds)
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

        protected void InvokeEvent(Action<KelVorethEventScript> action)
        {
            IPublicEvent publicEvent = entity.Map?.PublicEventManager.GetEvent(161);
            publicEvent?.InvokeScriptCollection(action);
        }
    }

    /// <summary>
    /// Grond the Corpsemaker: Mutilate, Bellow, Headbutt, Buck; the dead of the Blood Pit rise at 66 and 33%.
    /// </summary>
    [ScriptFilterCreatureId(32534, 32535)]
    public class KelVorethGrondAI : KelVorethBossAI
    {
        protected override uint[] AutoAttacks => [37281, 37282];

        public KelVorethGrondAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager, 0.66f, 0.33f)
        {
        }

        public override void OnLoad(ICreatureEntity owner)
        {
            base.OnLoad(owner);
            AddAbility(37223, 10d, 4d);                     // Mutilate
            AddAbility(40117, 14d, 8d);                     // Headbutt
            AddAbility(40118, 18d, 12d);                    // Buck
            AddAbility(37585, 22d, 16d, normalOnly: true);  // Bellow
            AddAbility(63109, 22d, 16d, veteranOnly: true); // Bellow (veteran)
        }

        protected override void OnThreshold(int index)
        {
            InvokeEvent(s => s.OnGrondAdds(entity));
        }
    }

    /// <summary>
    /// Slavemaster Drokk: Homing Barrage, Enslave, Suppression Field, Lockdown; enslaved constructs at 75, 50 and 25%.
    /// </summary>
    [ScriptFilterCreatureId(32536, 32539)]
    public class KelVorethDrokkAI : KelVorethBossAI
    {
        protected override uint[] AutoAttacks => [39183, 39184];

        public KelVorethDrokkAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager, 0.75f, 0.5f, 0.25f)
        {
        }

        public override void OnLoad(ICreatureEntity owner)
        {
            base.OnLoad(owner);
            AddAbility(39342, 9d, 4d);   // Homing Barrage
            AddAbility(39201, 18d, 9d);  // Enslave
            AddAbility(63131, 20d, 14d); // Suppression Field
            AddAbility(63143, 26d, 20d); // Lockdown
        }

        protected override void OnThreshold(int index)
        {
            InvokeEvent(s => s.OnDrokkAdds(entity));
        }
    }

    /// <summary>
    /// Forgemaster Trogun: Volcanic Strike, Exanite Shards, Forgemaster's Call; Spheres of Primal Fire roll in at 80, 60, 40 and 20%.
    /// </summary>
    [ScriptFilterCreatureId(32531, 32533)]
    public class KelVorethTrogunAI : KelVorethBossAI
    {
        protected override uint[] AutoAttacks => [38793, 38794];

        public KelVorethTrogunAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager, 0.8f, 0.6f, 0.4f, 0.2f)
        {
        }

        public override void OnLoad(ICreatureEntity owner)
        {
            base.OnLoad(owner);
            AddAbility(51970, 12d, 5d, normalOnly: true);   // Volcanic Strike
            AddAbility(63106, 12d, 5d, veteranOnly: true);  // Volcanic Strike (veteran)
            AddAbility(51860, 15d, 9d);                     // Exanite Shards
            AddAbility(52512, 25d, 18d);                    // Forgemaster's Call
        }

        protected override void OnThreshold(int index)
        {
            InvokeEvent(s => s.OnTrogunSpheres(entity));
        }
    }
}
