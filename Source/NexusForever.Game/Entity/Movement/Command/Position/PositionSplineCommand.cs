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
    public class PositionSplineCommand : IPositionCommand
    {
        public EntityCommand Command => EntityCommand.SetPositionSpline;

        /// <summary>
        /// Returns if the command has been finalised.
        /// </summary>
        public bool IsFinalised => spline?.IsFinialised ?? false;

        private ushort splineId;
        private SplineMode networkMode;
        private float networkSpeed;

        /// <summary>
        /// Speed the server follows a spline at whose speed is left to the client (-1, the tbl timing).
        /// </summary>
        private const float ClientTimedSpeed = 4f;

        #region Dependency Injection

        private readonly ISplineTemplateTbl template;
        private readonly ISpline spline;

        public PositionSplineCommand(
            ISplineTemplateTbl template,
            ISpline spline)
        {
            this.template = template;
            this.spline = spline;
        }

        #endregion

        /// <summary>
        /// Initialise command with spline, <see cref="SplineType"/>, <see cref="SplineMode"/> and speed.
        /// </summary>
        public void Initialise(ushort splineId, SplineMode mode, float speed)
        {
            this.splineId = splineId;
            networkMode   = mode;
            networkSpeed  = speed;

            // the world database's moving platforms (elevators, lifts) use mode 6 and speed -1, the client runs those itself from
            // the tbl spline (report 119): they go out as they are, the server follows them back and forth at a guessed speed
            if (mode > SplineMode.CyclicReverse)
                mode = SplineMode.BackAndForth;
            if (speed <= 0f)
                speed = ClientTimedSpeed;

            template.Initialise(splineId);
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
                Model   = new SetPositionSplineCommand
                {
                    SplineId            = splineId,
                    Speed               = networkSpeed,
                    Mode                = networkMode,
                    Position            = spline.Offset,
                    IsContinuing        = false,
                    AdjustSpeedToLength = true,
                    Blend               = false,
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
        /// <remarks>
        /// This is used to calculate the direction of travel for movement commands.
        /// </remarks>
        public Vector3 GetRotation()
        {
            return spline.GetRotation();
        }
    }
}
