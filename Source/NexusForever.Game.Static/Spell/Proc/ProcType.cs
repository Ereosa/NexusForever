namespace NexusForever.Game.Static.Spell.Proc
{
    /// <remarks>
    /// Names other than BeginMoving, CriticalDamage and StopsMoving are inferred from the tooltips of the AMPs and abilities that use them.
    /// </remarks>
    public enum ProcType
    {
        Kill                 = 1,   // "Killing a foe restores health"
        KillPlayer           = 2,   // "Killing an enemy player restores Spell Power"
        BeginMoving          = 11,
        DamageDealt          = 12,  // "Landing a hit with a Consumer has a chance to apply a Snare to foes"
        DamageTaken          = 16,  // "Taking direct damage grants a stack of My Turn"
        HealDealt            = 20,  // "When healing, you have a chance to summon a Companion"
        HealTaken            = 21,  // "On Taking a Heal grants a Defense"
        CriticalDamageDirect = 49,  // "Critical Hits deal damage every 1s"
        Deflect              = 51,  // "Deflecting an attack grants you Interrupt Armor"
        Deflected            = 52,  // "When your attack is Deflected gain an Empower"
        EnterCombat          = 57,  // "Upon entering combat ... grant an Empower"
        Dash                 = 124, // "Dashing grants a Defense"
        CriticalDamage       = 145,
        CriticalDamageTaken  = 146, // "Taking critical damage grants you an absorb"
        CriticalHealDealt    = 147, // "When you critically heal an ally they gain an Absorption Shield"
        StopsMoving          = 214,
        PhysicalDamageDealt  = 229, // "Dealing direct physical or technology damage grants an Empower based on damage type"
        TechDamageDealt      = 230,
        MagicDamageDealt     = 231,
        PhysicalDamageTaken  = 232, // "Taking damage grants a Defense against that damage type"
        TechDamageTaken      = 233,
        MagicDamageTaken     = 234
    }
}
