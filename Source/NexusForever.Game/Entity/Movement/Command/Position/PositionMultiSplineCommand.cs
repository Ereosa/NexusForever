using System.Numerics;
using NexusForever.Game.Abstract.Entity.Movement.Command.Position;
using NexusForever.Game.Abstract.Entity.Movement.Spline;
using NexusForever.Game.Abstract.Entity.Movement.Spline.Template;
using NexusForever.Game.Static.Entity.Movement.Command;
using NexusForever.Game.Static.Entity.Movement.Spline;
using NexusForever.Network.World.Entity;
using NexusForever.Network.World.Entity.Command;

namespace NexusForever.Game.Entity.Movement.Command.Position
{
    /// <summary>
    /// Several tbl splines ridden one after the other (taxi flights over more than one route).
    /// </summary>
    /// <remarks>
    /// The client rides the splines itself, the server follows the chain of the spline points as one linear path to know where the
    /// entity is (vision, relocation of passengers), which is close enough for the dense taxi splines.
    /// </remarks>
    public class PositionMultiSplineCommand : IPositionCommand
    {
        public EntityCommand Command => EntityCommand.SetPositionMultiSpline;

        /// <summary>
        /// Returns if the command has been finalised.
        /// </summary>
        public bool IsFinalised => spline?.IsFinialised ?? false;

        private List<ushort> splineIds;

        #region Dependency Injection

        private readonly ISplineTemplatePath template;
        private readonly ISpline spline;

        public PositionMultiSplineCommand(
            ISplineTemplatePath template,
            ISpline spline)
        {
            this.template = template;
            this.spline   = spline;
        }

        #endregion

        /// <summary>
        /// Initialise command with the supplied splines, their chained points, <see cref="SplineMode"/> and speed.
        /// </summary>
        public void Initialise(List<ushort> splineIds, List<Vector3> points, SplineMode mode, float speed)
        {
            this.splineIds = splineIds;

            template.Initialise(SplineType.Linear, points);
            spline.Initialise(template, mode, speed);
        }

        /// <summary>
        /// Invoked each world tick with the delta since the previous tick occurred.
        /// </summary>
        public void Update(double lastTick)
        {
            spline.Update(lastTick);
        }

        /// <summary>
        /// Returns the <see cref="INetworkEntityCommand"/> for the entity command.
        /// </summary>
        public INetworkEntityCommand GetNetworkEntityCommand()
        {
            return new NetworkEntityCommand
            {
                Command = Command,
                Model   = new SetPositionMultiSplineCommand
                {
                    SplineIds    = splineIds.Select(s => (uint)s).ToList(),
                    Speed        = spline.Speed,
                    Mode         = spline.Mode.Mode,
                    Offset       = (uint)(spline.Position * 1000),
                    Blend        = false,
                    IsContinuing = false
                }
            };
        }

        /// <summary>
        /// Returns the current <see cref="Vector3"/> position for the entity command.
        /// </summary>
        public Vector3 GetPosition()
        {
            return spline.GetPosition();
        }

        /// <summary>
        /// Returns the current <see cref="Vector3"/> rotation for the entity command.
        /// </summary>
        public Vector3 GetRotation()
        {
            return spline.GetRotation();
        }
    }
}
