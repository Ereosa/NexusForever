using NexusForever.Game.Abstract.Cinematic;
using NexusForever.Game.Static.Cinematic;
using NexusForever.Shared;

namespace NexusForever.Game.Abstract.Entity
{
    public interface ICinematicManager : IUpdate
    {
        /// <summary>
        /// Returns if a cinematic is currently playing for the <see cref="IPlayer"/>.
        /// </summary>
        bool IsPlaying { get; }

        /// <summary>
        /// Queue a <see cref="ICinematicBase"/> to be played.
        /// </summary>
        /// <remarks>
        /// <paramref name="onFinish"/> is invoked once the client reports the cinematic ended, or was skipped.
        /// </remarks>
        void QueueCinematic(ICinematicBase cinematic, Action onFinish = null);

        /// <summary>
        /// Invoke <paramref name="action"/> once every playing and queued cinematic has finished, immediately if none is playing.
        /// </summary>
        void WhenFinished(Action action);

        /// <summary>
        /// Queue a <see cref="ICinematicBase"/> to be played once the <see cref="IPlayer"/> has entered the world after a teleport.
        /// </summary>
        void QueueAfterEnteringWorld(ICinematicBase cinematic);

        /// <summary>
        /// Invoked when the <see cref="IPlayer"/> has finished loading and entered the world.
        /// </summary>
        void OnEnteredWorld();

        /// <summary>
        /// Handle the <see cref="CinematicState"/> the Client sent back. Only to be called from Client Handlers.
        /// </summary>
        void HandleClientCinematicState(CinematicState cinematicState);
    }
}
