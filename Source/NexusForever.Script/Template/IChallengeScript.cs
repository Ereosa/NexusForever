using NexusForever.Game.Abstract.Challenges;
using NexusForever.Game.Abstract.Entity;
using NexusForever.Shared;

namespace NexusForever.Script.Template
{
    /// <summary>
    /// Script for an <see cref="IChallenge"/>, loaded while the challenge runs.
    /// </summary>
    public interface IChallengeScript : IUpdate
    {
        /// <summary>
        /// Invoked each world tick while the challenge runs with the delta since the previous tick occurred.
        /// </summary>
        void IUpdate.Update(double lastTick)
        {
        }

        /// <summary>
        /// Invoked when the challenge starts.
        /// </summary>
        void OnActivate()
        {
        }

        /// <summary>
        /// Invoked while the challenge runs when its player successfully activates an entity.
        /// </summary>
        void OnEntityActivated(IWorldEntity entity)
        {
        }

        /// <summary>
        /// Invoked while the challenge runs when its player gets kill credit for an entity.
        /// </summary>
        void OnEntityKilled(IUnitEntity entity)
        {
        }

        /// <summary>
        /// Invoked when the challenge stops, <paramref name="completed"/> is set if it was completed.
        /// </summary>
        void OnStop(bool completed)
        {
        }
    }
}
