using Microsoft.Extensions.Logging;
using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Map.Search;
using NexusForever.Game.Abstract.Prerequisite;
using NexusForever.Game.Static.Prerequisite;

namespace NexusForever.Game.Prerequisite.Check
{
    /// <summary>
    /// Object id is a target group, "Equal" while one of its creatures is within value metres of the player, "NotEqual" while none is.
    /// </summary>
    /// <remarks>
    /// Eg: "CreatureWithinDistance, Equal, 15, 5468" (Scout Swifton nearby), pairs of "NotEqual 30" and "Equal 40" make distance bands.
    /// An object id of 0 refers to the creature of the context (unknown here) and counts as met.
    /// </remarks>
    [PrerequisiteCheck(PrerequisiteType.CreatureWithinDistance)]
    public class PrerequisiteCheckCreatureWithinDistance : IPrerequisiteCheck
    {
        private readonly ILogger<PrerequisiteCheckCreatureWithinDistance> log;

        public PrerequisiteCheckCreatureWithinDistance(ILogger<PrerequisiteCheckCreatureWithinDistance> log)
        {
            this.log = log;
        }

        public bool Meets(IPlayer player, PrerequisiteComparison comparison, uint value, uint objectId, IPrerequisiteParameters parameters)
        {
            if (objectId == 0u)
                return true;

            bool near = player.Map != null && player.Map
                .Search(player.Position, value, new SearchCheckTargetGroup(objectId))
                .Any();

            switch (comparison)
            {
                case PrerequisiteComparison.Equal:
                    return near;
                case PrerequisiteComparison.NotEqual:
                    return !near;
                default:
                    log.LogWarning($"Unhandled PrerequisiteComparison {comparison} for {PrerequisiteType.CreatureWithinDistance}!");
                    return false;
            }
        }

        private class SearchCheckTargetGroup : ISearchCheck<IWorldEntity>
        {
            private readonly uint targetGroupId;

            public SearchCheckTargetGroup(uint targetGroupId)
            {
                this.targetGroupId = targetGroupId;
            }

            public bool CheckEntity(IWorldEntity entity)
            {
                return entity is not IPlayer
                    && (entity is not IUnitEntity unit || unit.IsAlive)
                    && AssetManager.Instance.GetTargetGroupsForCreatureId(entity.CreatureId).Contains(targetGroupId);
            }
        }
    }

    /// <summary>
    /// Compares the seat the player has in the vehicle of creature object id (any vehicle for 0) with the value, -1 when not in it.
    /// </summary>
    /// <remarks>
    /// Eg: "Vehicle, Equal, -1, 0" (not in a vehicle), "Vehicle, NotEqual, -1, 24295" (riding a Darkspur Hoverbike),
    /// "Vehicle, Equal, 0, 13054" (riding Lucy the Equivar).
    /// </remarks>
    [PrerequisiteCheck(PrerequisiteType.Vehicle)]
    public class PrerequisiteCheckVehicle : IPrerequisiteCheck
    {
        private readonly ILogger<PrerequisiteCheckVehicle> log;

        public PrerequisiteCheckVehicle(ILogger<PrerequisiteCheckVehicle> log)
        {
            this.log = log;
        }

        public bool Meets(IPlayer player, PrerequisiteComparison comparison, uint value, uint objectId, IPrerequisiteParameters parameters)
        {
            uint seat = uint.MaxValue;
            if (player.PlatformGuid != null
                && player.GetVisible<IGridEntity>(player.PlatformGuid.Value) is IVehicleEntity vehicle
                && (objectId == 0u || vehicle.CreatureId == objectId))
            {
                IVehiclePassenger passenger = vehicle.GetPassenger(player.Guid);
                if (passenger != null)
                    seat = passenger.SeatPosition;
            }

            switch (comparison)
            {
                case PrerequisiteComparison.Equal:
                    return seat == value;
                case PrerequisiteComparison.NotEqual:
                    return seat != value;
                default:
                    log.LogWarning($"Unhandled PrerequisiteComparison {comparison} for {PrerequisiteType.Vehicle}!");
                    return false;
            }
        }
    }
}
