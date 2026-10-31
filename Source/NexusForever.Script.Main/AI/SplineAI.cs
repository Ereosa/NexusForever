using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Map;
using NexusForever.Game.Static.Entity;
using NexusForever.Game.Static.Entity.Movement.Command.Mode;
using NexusForever.Game.Static.Entity.Movement.Spline;
using NexusForever.Script.Template;
using NexusForever.Script.Template.Filter;
using NexusForever.Script.Template.Filter.Dynamic;

namespace NexusForever.Script.Main.AI
{
    [ScriptFilterDynamic<IScriptFilterDynamicEntitySpline>]
    //[ScriptFilterIgnore]
    public class SplineAI : IWorldEntityScript, IOwnedScript<IWorldEntity>
    {
        private IWorldEntity owner;

        public void OnLoad(IWorldEntity owner)
        {
            this.owner = owner;
        }

        /// <summary>
        /// Invoked when <see cref="IGridEntity"/> is added to <see cref="IBaseMap"/>.
        /// </summary>
        public void OnAddToMap(IBaseMap map)
        {
            if (owner.Spline == null)
                return;

            // TODO: Rawaho, handle negative spline speed and additional modes...
            // moving platforms (elevators, lifts) are mode 6 with speed -1, the client runs those from the tbl spline (report 119)
            if ((owner.Spline.Mode > SplineMode.CyclicReverse || owner.Spline.Speed == -1) && owner.Type != EntityType.Platform)
                return;

            owner.MovementManager.SetMode(ModeType.Walk);
            owner.MovementManager.LaunchSpline(owner.Spline.SplineId, owner.Spline.Mode, owner.Spline.Speed, false);

            // platforms (lifts, elevators) keep their placed rotation so players can stand on them, other units face along their path
            if (owner.Type == EntityType.Platform)
                owner.MovementManager.SetRotation(owner.Rotation, false);
        }
    }
}
