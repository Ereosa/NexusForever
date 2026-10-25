using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Map.Search;
using NexusForever.Script.Template;
using NexusForever.Script.Template.Filter;
using NexusForever.Shared;
using NexusForever.Shared.Game;

namespace NexusForever.Script.Alizar.NorthernWilds.Script
{
    /// <summary>
    /// Barrier Control Panel in Camp Icefury (Shellshock!), using it drops the fence barrier in front of the Dominion Ultrabot.
    /// </summary>
    /// <remarks>
    /// The game tables don't link the panel to the Icefury Fence Door, they stand a few metres apart so the closest door is opened.
    /// The barrier is shared by everyone in the camp, it closes again after a while so the next player has to use the panel too.
    /// </remarks>
    [ScriptFilterCreatureId(12653)]
    public class BarrierControlPanelEntityScript : IWorldEntityScript, IOwnedScript<IWorldEntity>
    {
        private const uint FenceDoorCreatureId = 16799;
        private const float DoorSearchRange = 20f;

        private IWorldEntity owner;
        private IDoorEntity door;

        private readonly UpdateTimer closeTimer = new(TimeSpan.FromMinutes(2d), false);

        /// <summary>
        /// Invoked when <see cref="IScript"/> is loaded.
        /// </summary>
        public void OnLoad(IWorldEntity owner)
        {
            this.owner = owner;
        }

        /// <summary>
        /// Invoked each world tick with the delta since the previous tick occurred.
        /// </summary>
        public void Update(double lastTick)
        {
            if (!closeTimer.IsTicking)
                return;

            closeTimer.Update(lastTick);
            if (!closeTimer.HasElapsed)
                return;

            closeTimer.Reset(false);
            door?.CloseDoor();
        }

        /// <summary>
        /// Invoked when <see cref="IWorldEntity"/> is successfully activated by <see cref="IPlayer"/>.
        /// </summary>
        public void OnActivateSuccess(IPlayer activator)
        {
            door ??= owner.Map.Search(owner.Position, DoorSearchRange, new SearchCheckFenceDoor())
                .OrderBy(d => d.Position.GetDistance(owner.Position))
                .FirstOrDefault();
            if (door == null)
                return;

            if (!door.IsOpen)
                door.OpenDoor();

            closeTimer.Reset(true);
        }

        private class SearchCheckFenceDoor : ISearchCheck<IDoorEntity>
        {
            public bool CheckEntity(IDoorEntity entity)
            {
                return entity.CreatureId == FenceDoorCreatureId;
            }
        }
    }
}
