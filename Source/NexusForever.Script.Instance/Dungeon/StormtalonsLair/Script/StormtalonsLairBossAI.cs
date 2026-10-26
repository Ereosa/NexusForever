using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.PublicEvent;
using NexusForever.Game.Abstract.Spell;
using NexusForever.Game.Static.Reputation;
using NexusForever.GameTable;
using NexusForever.Script.Instance.Expedition;
using NexusForever.Script.Template.Filter;
using NexusForever.Shared;

namespace NexusForever.Script.Instance.Dungeon.StormtalonsLair.Script
{
    /// <summary>
    /// Boss fights of Stormtalon's Lair: abilities from the archived Jabbithole ability lists (the Spell4 descriptions name the boss and
    /// the ability), phases from the archived Dulfy veteran guide (2014). Cooldowns between the guide's events are guesses.
    /// </summary>
    public abstract class StormtalonsLairBossAI : ExpeditionCreatureAI
    {
        private readonly List<float> thresholds;
        private int nextThreshold;
        private Faction? hostileFaction;

        protected StormtalonsLairBossAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager, params float[] thresholds)
            : base(spellParametersFactory, gameTableManager)
        {
            this.thresholds = thresholds.OrderByDescending(t => t).ToList();
        }

        /// <summary>
        /// A fresh pull (full health): the boss' phases start over. Re-entering combat mid fight (after an immune phase) keeps them.
        /// </summary>
        protected bool IsFreshPull => entity.Health >= entity.MaxHealth;

        public override void OnEnterCombat()
        {
            base.OnEnterCombat();
            if (IsFreshPull)
                nextThreshold = 0;
        }

        public override void Update(double lastTick)
        {
            base.Update(lastTick);

            if (!entity.IsAlive || nextThreshold >= thresholds.Count)
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

        /// <summary>
        /// Phases where the boss can't be attacked (immune, gone): it turns friendly to all, then back.
        /// </summary>
        protected void SetAttackable(bool attackable)
        {
            if (!attackable)
            {
                hostileFaction ??= entity.Faction1;
                entity.SetFaction(Faction.FriendlyToAll);
            }
            else if (hostileFaction.HasValue)
            {
                entity.SetFaction(hostileFaction.Value);
                hostileFaction = null;
            }
        }

        protected void InvokeEvent(Action<StormtalonsLairEventScript> action)
        {
            IPublicEvent publicEvent = entity.Map?.PublicEventManager.GetEvent(145);
            publicEvent?.InvokeScriptCollection(action);
        }
    }

    /// <summary>
    /// Blade-Wind the Invoker. Phase 1 (100-55%): Thunder Cross often, Lightning Strike. Phase 2 (55%): immune while the four Thundercall
    /// Channelers in the room channel their Static Barrier, Lightning Strikes dropped on them break it, a Static Wisp spawns as each one
    /// dies. Phase 3 (all Channelers dead): Electrostatic Pulse in groups of three, fewer Lightning Strikes.
    /// </summary>
    [ScriptFilterCreatureId(17160, 33405)]
    public class StormtalonsLairInvokerAI : StormtalonsLairBossAI
    {
        private int phase = 1;

        protected override uint[] AutoAttacks => [38818];

        public StormtalonsLairInvokerAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager, 0.55f)
        {
        }

        public override void OnLoad(ICreatureEntity owner)
        {
            base.OnLoad(owner);
            AddAbility(38786, 7d, 3d, normalOnly: true);   // Thunder Cross
            AddAbility(63231, 7d, 3d, veteranOnly: true);  // Thunder Cross (veteran)
            AddAbility(39935, 12d, 6d);                    // Lightning Strike
        }

        public override void OnEnterCombat()
        {
            base.OnEnterCombat();
            if (IsFreshPull)
                phase = 1;
        }

        protected override void OnThreshold(int index)
        {
            phase = 2;
            SetAttackable(false);
            InvokeEvent(s => s.OnInvokerChannelers(entity));
        }

        /// <summary>
        /// The last Channeler fell: phase 3.
        /// </summary>
        public void OnChannelersDead()
        {
            if (phase != 2)
                return;

            phase = 3;
            SetAttackable(true);
            AddAbility(39074, 18d, 4d); // Electrostatic Pulse
        }

        private double pulseRepeat;
        private int pulses;

        public override void Update(double lastTick)
        {
            base.Update(lastTick);
            if (phase != 3 || !entity.IsAlive || !entity.InCombat)
                return;

            // Electrostatic Pulse comes in groups of three casts
            if (entity.IsCasting())
                return;
            pulseRepeat -= lastTick;
            if (pulses > 0 && pulseRepeat <= 0d)
            {
                pulses--;
                pulseRepeat = 3d;
                CastSpell(39074, null);
            }
            else if (pulses == 0 && pulseRepeat <= -18d)
            {
                pulses = 2;
                pulseRepeat = 0d;
            }
        }
    }

    /// <summary>
    /// Aethros. Phase 1: Torrent under each player, Thunderbolt. Air Shift: after a while he dissipates into three Gusts of Aethros;
    /// once they are dead he knocks everyone back to the start of the room and casts Tempest (15 s) while tornadoes cross the room, an
    /// uninterrupted Tempest wipes the party. Twice, then phase 3: phase 1 with the tornadoes moving all the time.
    /// </summary>
    [ScriptFilterCreatureId(17166, 32703)]
    public class StormtalonsLairAethrosAI : StormtalonsLairBossAI
    {
        private double airShift;
        private int airShifts;
        private bool dissipated;

        protected override uint[] AutoAttacks => [70404, 70405];

        public StormtalonsLairAethrosAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager)
        {
        }

        public override void OnLoad(ICreatureEntity owner)
        {
            base.OnLoad(owner);
            AddAbility(36028, 10d, 5d);  // Torrent
            AddAbility(39730, 8d, 3d);   // Thunderbolt
        }

        public override void OnEnterCombat()
        {
            base.OnEnterCombat();
            if (!IsFreshPull)
                return;
            airShift = 30d;
            airShifts = 0;
            dissipated = false;
        }

        public override void Update(double lastTick)
        {
            base.Update(lastTick);
            if (!entity.IsAlive || !entity.InCombat || dissipated || airShifts >= 2)
                return;

            airShift -= lastTick;
            if (airShift > 0d || entity.IsCasting())
                return;

            // Air Shift: "Aethros starts to dissipate!"
            airShifts++;
            dissipated = true;
            SetAttackable(false);
            InvokeEvent(s => s.OnAethrosAirShift(entity));
        }

        /// <summary>
        /// The Gusts of Aethros are dead: knockback to the start of the room and Tempest, then back to phase 1.
        /// </summary>
        public void OnGustsDead()
        {
            if (!dissipated)
                return;

            dissipated = false;
            SetAttackable(true);
            CastSpell(IsVeteran ? 63114u : 39682u, null); // Tempest
            InvokeEvent(s => s.OnAethrosTempest(entity, airShifts >= 2));
            airShift = 35d;
        }
    }

    /// <summary>
    /// Stormtalon. Frontal cleave (Chomp), Static Charge (lightning circles under the players), Static Wave at 87 and 61% (knockback, an
    /// enclosing wave), Eye of the Storm from 55% and again at 30 and 5% (replaces Static Wave).
    /// </summary>
    [ScriptFilterCreatureId(17163, 33406)]
    public class StormtalonsLairStormtalonAI : StormtalonsLairBossAI
    {
        protected override uint[] AutoAttacks => [39580];

        public StormtalonsLairStormtalonAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager, 0.87f, 0.61f, 0.55f, 0.30f, 0.05f)
        {
        }

        public override void OnLoad(ICreatureEntity owner)
        {
            base.OnLoad(owner);
            AddAbility(39613, 20d, 8d, normalOnly: true);  // Static Charge
            AddAbility(63127, 20d, 8d, veteranOnly: true); // Static Charge (veteran)
            AddAbility(39596, 9d, 4d);                     // Lightning Strike
        }

        protected override void OnThreshold(int index)
        {
            if (index <= 1)
                CastSpell(39681, null); // Static Wave
            else
                CastSpell(39800, null); // Thunder Call: the Eye of the Storm's lightning around the targeted player
        }
    }
}
