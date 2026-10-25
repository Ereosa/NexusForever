namespace NexusForever.Game.Static.Spell
{
    /// <summary>
    /// First data value of a <see cref="SpellEffectType.SpellEffectImmunity"/> effect.
    /// </summary>
    /// <remarks>
    /// Names are taken from the descriptions of the spells using each value, e.g. "Boss Immunity - Immune to Damage".
    /// </remarks>
    public enum SpellEffectImmunityType
    {
        SpellCounter = 2,
        ForceMove    = 3,
        CCState      = 4,
        Damage       = 8
    }
}
