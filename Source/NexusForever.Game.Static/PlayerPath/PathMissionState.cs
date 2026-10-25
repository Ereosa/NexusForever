namespace NexusForever.Game.Static.PlayerPath
{
    public enum PathMissionState
    {
        NoMission            = 0,
        // only valid in ServerPathMissionActivate, plays the communicator with the mission unlock text
        CommunicatorUnlock   = 1,
        Unlocked             = 2,
        Started              = 3,
        Complete             = 4,
        // only valid in ServerPathMissionActivate, plays the communicator with the mission communicator text
        CommunicatorComplete = 6
    };
}
