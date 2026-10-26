using System.Numerics;
using Microsoft.Extensions.Logging;
using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Prerequisite;
using NexusForever.Game.Static.Prerequisite;

namespace NexusForever.Game.Prerequisite.Check
{
    /// <summary>
    /// Value is a trigger volume, compares whether the player is inside it.
    /// </summary>
    /// <remarks>
    /// Trigger volumes are server data and not in the client, the few known ones are reconstructed below from what they gate.
    /// The player is never inside an unknown volume (NotEqual passes, Equal fails).
    /// </remarks>
    [PrerequisiteCheck(PrerequisiteType.InTriggerVolume)]
    public class PrerequisiteCheckInTriggerVolume : IPrerequisiteCheck
    {
        /// <summary>
        /// Reconstructed volumes: world, and a sphere on it (radius 0 for the whole world).
        /// </summary>
        private static readonly Dictionary<uint, (uint WorldId, Vector3 Center, float Radius)> volumes = new()
        {
            // Research Station Tideborn (Knock Knock, Ellevar): Corrigan Doon's "You're not bailing on us now are you?" delivers
            // Tall Tales once the player leaves the station, which is its own map
            { 4097u, (2161u, Vector3.Zero, 0f) }
        };

        private readonly ILogger<PrerequisiteCheckInTriggerVolume> log;

        public PrerequisiteCheckInTriggerVolume(ILogger<PrerequisiteCheckInTriggerVolume> log)
        {
            this.log = log;
        }

        public bool Meets(IPlayer player, PrerequisiteComparison comparison, uint value, uint objectId, IPrerequisiteParameters parameters)
        {
            bool inside = volumes.TryGetValue(value, out var volume)
                && player.Map?.Entry.Id == volume.WorldId
                && (volume.Radius <= 0f || Vector3.Distance(player.Position, volume.Center) <= volume.Radius);

            switch (comparison)
            {
                case PrerequisiteComparison.Equal:
                    return inside;
                case PrerequisiteComparison.NotEqual:
                    return !inside;
                default:
                    log.LogWarning($"Unhandled PrerequisiteComparison {comparison} for {PrerequisiteType.InTriggerVolume}!");
                    return false;
            }
        }
    }
}
