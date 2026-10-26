using Microsoft.Extensions.Logging;
using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Prerequisite;
using NexusForever.Game.Static.Entity;
using NexusForever.Game.Static.Prerequisite;

namespace NexusForever.Game.Prerequisite.Check
{
    /// <summary>
    /// Level of the player's current path compared to the value (eg: "PathLevel, GreaterThan, 11" and "PathLevel, LessThan, 20").
    /// </summary>
    [PrerequisiteCheck(PrerequisiteType.PathLevel)]
    public class PrerequisiteCheckPathLevel : PrerequisiteCheckCompare
    {
        public PrerequisiteCheckPathLevel(ILogger<PrerequisiteCheckPathLevel> log)
            : base(log, PrerequisiteType.PathLevel)
        {
        }

        protected override long GetValue(IPlayer player, uint objectId, uint value)
        {
            return player.PathManager.GetCurrentLevel(player.Path);
        }
    }

    /// <summary>
    /// Value is an item, "Equal" while the player has it equipped (eg: "ItemEquipped, Equal, 12877").
    /// </summary>
    [PrerequisiteCheck(PrerequisiteType.ItemEquipped)]
    public class PrerequisiteCheckItemEquipped : IPrerequisiteCheck
    {
        private readonly ILogger<PrerequisiteCheckItemEquipped> log;

        public PrerequisiteCheckItemEquipped(ILogger<PrerequisiteCheckItemEquipped> log)
        {
            this.log = log;
        }

        public bool Meets(IPlayer player, PrerequisiteComparison comparison, uint value, uint objectId, IPrerequisiteParameters parameters)
        {
            bool equipped = player.Inventory.GetItemCount(value, InventoryLocation.Equipped) > 0u;
            switch (comparison)
            {
                case PrerequisiteComparison.Equal:
                    return equipped;
                case PrerequisiteComparison.NotEqual:
                    return !equipped;
                default:
                    log.LogWarning($"Unhandled PrerequisiteComparison {comparison} for {PrerequisiteType.ItemEquipped}!");
                    return false;
            }
        }
    }

    /// <summary>
    /// Object id is a soldier holdout event, the value compared is 1 while the player runs it and 0 otherwise
    /// (eg: "PathHoldout, NotEqual, 0, 92" while Gallow's memorial is defended).
    /// </summary>
    [PrerequisiteCheck(PrerequisiteType.PathHoldout)]
    public class PrerequisiteCheckPathHoldout : PrerequisiteCheckCompare
    {
        public PrerequisiteCheckPathHoldout(ILogger<PrerequisiteCheckPathHoldout> log)
            : base(log, PrerequisiteType.PathHoldout)
        {
        }

        protected override long GetValue(IPlayer player, uint objectId, uint value)
        {
            return player.PathSoldierManager.CurrentHoldoutEventId == objectId ? 1 : 0;
        }
    }
}
