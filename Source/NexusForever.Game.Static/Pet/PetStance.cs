namespace NexusForever.Game.Static.Pet
{
    /// <summary>
    /// Combat pet stances, sent and stored by the client as a bit mask.
    /// </summary>
    /// <remarks>
    /// The client maps the Lua GameLib.CodeEnumPetStance values (1 Aggressive, 2 Defensive, 3 Passive, 4 Assist, 5 Stay) to these bits before sending ClientPetSetStance,
    /// valid stances for a pet are a combination of these bits.
    /// </remarks>
    [Flags]
    public enum PetStance
    {
        None       = 0x00,
        Assist     = 0x01,
        Stay       = 0x02,
        Passive    = 0x04,
        Defensive  = 0x08,
        Aggressive = 0x10,
        All        = Assist | Stay | Passive | Defensive | Aggressive
    }
}
