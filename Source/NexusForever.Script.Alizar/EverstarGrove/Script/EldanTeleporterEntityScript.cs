using System.Numerics;
using NexusForever.Game.Abstract.Entity;
using NexusForever.Script.Template;
using NexusForever.Script.Template.Filter;

namespace NexusForever.Script.Alizar.EverstarGrove.Script
{
    /// <summary>
    /// Eldan Teleporters linking the Mystwood Forest surface with Exo-Lab 71: The Arboretum below it.
    /// </summary>
    /// <remarks>
    /// Stepping onto a pad moves the player to the other one. The interior arrival point is the zone exit location of
    /// Exo-Lab 71 (WorldLocation2 21763), the surface arrival point is the quest's entrance location (WorldLocation2 21761).
    /// </remarks>
    [ScriptFilterCreatureId(28454)]
    public class EldanTeleporterEntityScript : IWorldEntityScript, IOwnedScript<IWorldEntity>
    {
        private const float SurfaceHeight = -1000f;

        private static readonly Vector3 Interior = new(-241.44675f, -1118.9783f, -3012.0237f);
        private static readonly Vector3 Surface  = new(-210.70447f, -937.5956f, -3011.093f);

        private IWorldEntity owner;

        /// <summary>
        /// Invoked when <see cref="IScript"/> is loaded.
        /// </summary>
        public void OnLoad(IWorldEntity owner)
        {
            this.owner = owner;
            owner.SetInRangeCheck(2f);
        }

        /// <summary>
        /// Invoked when <see cref="IGridEntity"/> is added to range check range.
        /// </summary>
        public void OnEnterRange(IGridEntity entity)
        {
            if (entity is not IPlayer player)
                return;

            player.TeleportToLocal(owner.Position.Y > SurfaceHeight ? Interior : Surface);
        }
    }
}
