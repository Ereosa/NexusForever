namespace NexusForever.Game.Static.Spell
{
    /// <summary>
    /// This appears to be an indicator of the location at which target acquisition begins.
    /// </summary>
    /// <remarks>Unsure exactly how this works at this time. It appears to be an optimisation to allow for quicker, accurate selection. Needs investigation.</remarks>
    public enum SpellTargetMechanicType
    {
        Self            = 0,
        PrimaryTarget   = 1,
        SecondaryTarget = 2,

        /// <summary>
        /// Implicit targets around the primary target, eg: 83018 Bruiserbot Blitz interrupting foes around the Blitz target.
        /// </summary>
        PrimaryTargetArea = 3,

        /// <summary>
        /// A position chosen with the ground placement reticle, eg: 41693 Pet Command - Go To Location.
        /// </summary>
        TargetPosition = 4,

        /// <summary>
        /// Positional aoe placed on the primary target, eg: 34589 Artillerybot Barrage.
        /// </summary>
        PrimaryTargetPosition = 8
    }
}
