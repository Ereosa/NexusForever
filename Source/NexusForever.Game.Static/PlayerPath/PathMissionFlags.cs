namespace NexusForever.Game.Static.PlayerPath
{
    [Flags]
    public enum PathMissionFlags
    {
        None     = 0x00,
        // client IsOptional, optional missions don't count towards episode completion
        Optional = 0x40
    }
}
