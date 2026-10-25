using NexusForever.Game.Abstract.Spell;
using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Spell.Info;
using NexusForever.Game.Static.Spell;
using NexusForever.Game.Static.Spell.Proc;
using NexusForever.Network.World.Combat;

namespace NexusForever.Game.Spell.Proc
{
    /// <summary>
    /// Triggers the procs of the units involved in combat events.
    /// </summary>
    public static class ProcTrigger
    {
        private static void Trigger(IUnitEntity owner, ProcType type, IUnitEntity target, ISpellInfo spellInfo)
        {
            owner.ProcManager.TriggerProc(type, new ProcParameters
            {
                Target    = target,
                SpellInfo = spellInfo
            });
        }

        /// <summary>
        /// Invoked when damage from a spell lands on the victim.
        /// </summary>
        public static void OnDamage(IUnitEntity attacker, IUnitEntity victim, ISpellInfo spellInfo, IDamageDescription damage, bool procTriggered)
        {
            // proc spells don't trigger procs, otherwise "landing a hit deals damage" would trigger itself
            if (procTriggered)
                return;

            bool critical = damage.CombatResult == CombatResult.Critical;

            Trigger(attacker, ProcType.DamageDealt, victim, spellInfo);
            if (critical)
            {
                Trigger(attacker, ProcType.CriticalDamage, victim, spellInfo);
                Trigger(attacker, ProcType.CriticalDamageDirect, victim, spellInfo);
            }

            ProcType? dealt = damage.DamageType switch
            {
                DamageType.Physical => ProcType.PhysicalDamageDealt,
                DamageType.Tech     => ProcType.TechDamageDealt,
                DamageType.Magic    => ProcType.MagicDamageDealt,
                _                   => null
            };
            if (dealt.HasValue)
                Trigger(attacker, dealt.Value, victim, spellInfo);

            Trigger(victim, ProcType.DamageTaken, attacker, spellInfo);
            if (critical)
                Trigger(victim, ProcType.CriticalDamageTaken, attacker, spellInfo);

            ProcType? taken = damage.DamageType switch
            {
                DamageType.Physical => ProcType.PhysicalDamageTaken,
                DamageType.Tech     => ProcType.TechDamageTaken,
                DamageType.Magic    => ProcType.MagicDamageTaken,
                _                   => null
            };
            if (taken.HasValue)
                Trigger(victim, taken.Value, attacker, spellInfo);
        }

        /// <summary>
        /// Invoked when the victim deflects an attack.
        /// </summary>
        public static void OnDeflect(IUnitEntity attacker, IUnitEntity victim, ISpellInfo spellInfo, bool procTriggered)
        {
            if (procTriggered)
                return;

            Trigger(victim, ProcType.Deflect, attacker, spellInfo);
            Trigger(attacker, ProcType.Deflected, victim, spellInfo);
        }

        /// <summary>
        /// Invoked when a heal from a spell lands on the target.
        /// </summary>
        public static void OnHeal(IUnitEntity healer, IUnitEntity target, ISpellInfo spellInfo, IDamageDescription heal, bool procTriggered)
        {
            if (procTriggered)
                return;

            Trigger(healer, ProcType.HealDealt, target, spellInfo);
            if (heal.CombatResult == CombatResult.Critical)
                Trigger(healer, ProcType.CriticalHealDealt, target, spellInfo);

            Trigger(target, ProcType.HealTaken, healer, spellInfo);
        }

        /// <summary>
        /// Invoked when the victim is killed.
        /// </summary>
        public static void OnKill(IUnitEntity killer, IUnitEntity victim)
        {
            if (killer == null || killer == victim)
                return;

            Trigger(killer, victim is IPlayer ? ProcType.KillPlayer : ProcType.Kill, victim, null);
        }

        /// <summary>
        /// Invoked when the unit enters combat.
        /// </summary>
        public static void OnEnterCombat(IUnitEntity unit)
        {
            Trigger(unit, ProcType.EnterCombat, null, null);
        }

        /// <summary>
        /// Invoked when the unit dashes.
        /// </summary>
        public static void OnDash(IUnitEntity unit)
        {
            Trigger(unit, ProcType.Dash, null, null);
        }
    }
}
