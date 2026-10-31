using Microsoft.Extensions.Logging;
using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Prerequisite;
using NexusForever.Game.Static.Map;
using NexusForever.Game.Static.Prerequisite;

namespace NexusForever.Game.Prerequisite.Check
{
    /// <summary>
    /// Compares the type of the map the player is on with the object id (a <see cref="MapType"/>).
    /// </summary>
    /// <remarks>
    /// The failure texts of the prerequisites using it name the map types: "Can't use in a dungeon, raid, or adventure" for "NotEqual 11, 9, 6",
    /// "You are not in an Adventure" for "Equal 6", "You are not in a Dungeon" for "Equal 11", "Can't use in a Warplot" for "NotEqual 3, 7, 8".
    /// A value other than 0 (only seen on "NotEqual 3 (pvp), value 1888") is taken as the world id the map type has to be on.
    /// The settler stations are gated by it, their buff spells have persistence prerequisites that end the cast when the check fails.
    /// </remarks>
    [PrerequisiteCheck(PrerequisiteType.Unknown240)]
    public class PrerequisiteCheckMapType : IPrerequisiteCheck
    {
        private readonly ILogger<PrerequisiteCheckMapType> log;

        public PrerequisiteCheckMapType(ILogger<PrerequisiteCheckMapType> log)
        {
            this.log = log;
        }

        public bool Meets(IPlayer player, PrerequisiteComparison comparison, uint value, uint objectId, IPrerequisiteParameters parameters)
        {
            switch (comparison)
            {
                case PrerequisiteComparison.Equal:
                    return IsOnMapType(player, value, objectId);
                case PrerequisiteComparison.NotEqual:
                    return !IsOnMapType(player, value, objectId);
                default:
                    log.LogWarning($"Unhandled PrerequisiteComparison {comparison} for {PrerequisiteType.Unknown240}!");
                    return false;
            }
        }

        private static bool IsOnMapType(IPlayer player, uint worldId, uint mapType)
        {
            if (player.Map?.Entry == null)
                return false;

            return (uint)player.Map.Entry.Type == mapType
                && (worldId == 0u || player.Map.Entry.Id == worldId);
        }
    }
}
