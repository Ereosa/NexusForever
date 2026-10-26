using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.PublicEvent;
using NexusForever.Game.Abstract.Spell;
using NexusForever.Game.Static.Reputation;
using NexusForever.GameTable;
using NexusForever.Script.Instance.Expedition;
using NexusForever.Script.Template.Filter;
using NexusForever.Shared;

namespace NexusForever.Script.Instance.Dungeon.KelVoreth.Script
{
    /// <summary>
    /// Boss fights of the Ruins of Kel Voreth: abilities from the archived Jabbithole ability lists (the Spell4 descriptions name the boss
    /// and the ability), phases from the archived Dulfy veteran guide (2014). Timings between the guide's events are guesses.
    /// </summary>
    public abstract class KelVorethBossAI : ExpeditionCreatureAI
    {
        private readonly List<float> thresholds;
        private int nextThreshold;
        private Faction? hostileFaction;

        protected KelVorethBossAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager, params float[] thresholds)
            : base(spellParametersFactory, gameTableManager)
        {
            this.thresholds = thresholds.OrderByDescending(t => t).ToList();
        }

        /// <summary>
        /// A fresh pull (full health): the boss' phases start over. Re-entering combat mid fight (after an untargetable phase) keeps them.
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
        /// Phases where the boss can't be attacked: it turns friendly to all, then back.
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

        protected void InvokeEvent(Action<KelVorethEventScript> action)
        {
            IPublicEvent publicEvent = entity.Map?.PublicEventManager.GetEvent(161);
            publicEvent?.InvokeScriptCollection(action);
        }
    }

    /// <summary>
    /// Grond the Corpsemaker. Phase 1: frontal cleave and back kick, Thrash (a channel), Bellow (knockback and disarm) followed by
    /// "Grond has a nasty itch!": four Bloodsucker Fleas chase random players. Phase 2 ("Grond goes into a murderous frenzy!", guessed at
    /// 50%, the guide gives no %): Blood Frenzy, Mutilate replaces Thrash.
    /// </summary>
    [ScriptFilterCreatureId(32534, 32535)]
    public class KelVorethGrondAI : KelVorethBossAI
    {
        protected override uint[] AutoAttacks => [37281, 37282];

        public KelVorethGrondAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager, 0.5f)
        {
        }

        public override void OnLoad(ICreatureEntity owner)
        {
            base.OnLoad(owner);
            AddAbility(37315, 14d, 6d, normalOnly: true);   // Thrash
            AddAbility(63121, 14d, 6d, veteranOnly: true);  // Thrash (veteran)
            AddAbility(40117, 11d, 4d);                     // Headbutt
            AddAbility(40118, 13d, 9d);                     // Buck
        }

        private double bellow = 25d;

        public override void OnEnterCombat()
        {
            base.OnEnterCombat();
            if (IsFreshPull)
                bellow = 25d;
        }

        public override void Update(double lastTick)
        {
            base.Update(lastTick);
            if (!entity.IsAlive || !entity.InCombat || entity.IsCasting())
                return;

            bellow -= lastTick;
            if (bellow > 0d)
                return;

            bellow = 30d;
            CastSpell(IsVeteran ? 63109u : 37585u, null); // Bellow
            InvokeEvent(s => s.OnGrondItch(entity));
        }

        protected override void OnThreshold(int index)
        {
            // "Grond goes into a murderous frenzy!"
            CastSpell(IsVeteran ? 63128u : 38051u, null); // Blood Frenzy
            AddAbility(37223, 12d, 3d);                   // Mutilate
        }
    }

    /// <summary>
    /// Slavemaster Drokk. Phase 1: Homing Barrage circles; at 83% he vanishes and Bombshell Constructs rush out (phase 2), he returns
    /// once they are destroyed; phase 3 with Bombshells; at 60% the chase (Tracking Beacon on a player, Destructo Constructs after them,
    /// three beacons); at 40% Lockdown tethers and Deplete Silo shockwaves at his portal; at 20% the last chase and Suppression Wave.
    /// </summary>
    [ScriptFilterCreatureId(32536, 32539)]
    public class KelVorethDrokkAI : KelVorethBossAI
    {
        protected override uint[] AutoAttacks => [39183, 39184];

        public KelVorethDrokkAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager, 0.83f, 0.6f, 0.4f, 0.2f)
        {
        }

        public override void OnLoad(ICreatureEntity owner)
        {
            base.OnLoad(owner);
            AddAbility(39342, 8d, 3d);   // Homing Barrage
        }

        protected override void OnThreshold(int index)
        {
            switch (index)
            {
                case 0:
                    SetAttackable(false);
                    InvokeEvent(s => s.OnDrokkBombshells(entity));
                    break;
                case 1:
                    InvokeEvent(s => s.OnDrokkChase(entity, false));
                    break;
                case 2:
                    AddAbility(63143, 18d, 2d);  // Lockdown
                    AddAbility(39156, 9d, 5d);   // Deplete Silo
                    break;
                case 3:
                    AddAbility(51563, 8d, 2d);   // Suppression Wave
                    InvokeEvent(s => s.OnDrokkChase(entity, true));
                    break;
            }
        }

        /// <summary>
        /// The Bombshell Constructs of phase 2 are destroyed: Drokk returns.
        /// </summary>
        public void OnBombshellsDestroyed()
        {
            SetAttackable(true);
        }
    }

    /// <summary>
    /// Forgemaster Trogun. Untargetable for the first 17 seconds while he channels Exanite Weapon; Volcanic Strike, Exanite Shards;
    /// Forgemaster's Call sends Spheres of Primal Fire towards him (players catch them, the ones that reach him empower him). At 50% he
    /// returns to his forge and buzzsaws (Exanite Shards) roll down the stairs; phase 3 adds the buzzsaws to the orb phases.
    /// </summary>
    [ScriptFilterCreatureId(32531, 32533)]
    public class KelVorethTrogunAI : KelVorethBossAI
    {
        private double forgemasterCall = 30d;
        private double exaniteWeapon = -1d;
        private bool buzzsaws;

        protected override uint[] AutoAttacks => [38793, 38794];

        public KelVorethTrogunAI(IFactory<ISpellParameters> spellParametersFactory, IGameTableManager gameTableManager)
            : base(spellParametersFactory, gameTableManager, 0.5f)
        {
        }

        public override void OnLoad(ICreatureEntity owner)
        {
            base.OnLoad(owner);
            AddAbility(51970, 12d, 18d, normalOnly: true);  // Volcanic Strike
            AddAbility(63106, 12d, 18d, veteranOnly: true); // Volcanic Strike (veteran)
            AddAbility(51860, 15d, 22d);                    // Exanite Shards
        }

        private bool started;

        public override void OnLeaveCombat()
        {
            base.OnLeaveCombat();
            // a real reset, not the untargetable start of the fight
            if (exaniteWeapon <= 0d && IsFreshPull)
                started = false;
        }

        public override void OnEnterCombat()
        {
            base.OnEnterCombat();
            if (started || !IsFreshPull)
                return;

            // Exanite Weapon: untargetable for the first 17 seconds
            started = true;
            buzzsaws = false;
            forgemasterCall = 35d;
            exaniteWeapon = 17d;
            SetAttackable(false);
            CastSpell(52521, null);
        }

        public override void Update(double lastTick)
        {
            base.Update(lastTick);
            if (!entity.IsAlive)
                return;

            if (exaniteWeapon > 0d)
            {
                exaniteWeapon -= lastTick;
                if (exaniteWeapon <= 0d)
                    SetAttackable(true);
                return;
            }

            if (!entity.InCombat || entity.IsCasting())
                return;

            forgemasterCall -= lastTick;
            if (forgemasterCall > 0d)
                return;

            forgemasterCall = 35d;
            CastSpell(52512, null); // Forgemaster's Call
            InvokeEvent(s => s.OnTrogunSpheres(entity, buzzsaws));
        }

        protected override void OnThreshold(int index)
        {
            // back to the forge: buzzsaws roll down the stairs
            buzzsaws = true;
            InvokeEvent(s => s.OnTrogunBuzzsaws(entity));
        }
    }
}
