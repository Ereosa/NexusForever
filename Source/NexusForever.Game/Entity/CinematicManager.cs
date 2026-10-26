using NexusForever.Game.Abstract.Cinematic;
using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Static.Cinematic;
using NLog;

namespace NexusForever.Game.Entity
{
    public class CinematicManager : ICinematicManager
    {
        private static readonly ILogger log = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Time after the scheduled end of a cinematic the server stops waiting for the client to report it ended.
        /// </summary>
        /// <remarks>
        /// Guards scripts that continue after a cinematic (teleports, quest credit) against a client that never answers.
        /// </remarks>
        private const double EndTimeout = 15d;

        private IPlayer owner;

        private ICinematicBase currentCinematic;
        private Action currentCallback;
        private double currentTimer;
        private readonly Queue<(ICinematicBase Cinematic, Action Callback)> queuedCinematics = new();

        /// <summary>
        /// Returns if a cinematic is currently playing for the <see cref="IPlayer"/>.
        /// </summary>
        public bool IsPlaying => currentCinematic != null;

        /// <summary>
        /// Initialise a <see cref="ICinematicManager"/> for this <see cref="IPlayer"/>.
        /// </summary>
        public CinematicManager(IPlayer player)
        {
            owner = player;
        }

        /// <summary>
        /// Invoked each world tick with the delta since the previous tick occurred.
        /// </summary>
        public void Update(double lastTick)
        {
            if (currentCinematic == null)
                return;

            currentTimer -= lastTick;
            if (currentTimer > 0d)
                return;

            log.Warn($"Cinematic {currentCinematic.GetType().Name} for {owner.Name} never reported an end, finishing it server side.");
            FinishCurrentCinematic();
        }

        /// <summary>
        /// Queue a <see cref="ICinematicBase"/> to be played.
        /// </summary>
        /// <remarks>
        /// <paramref name="onFinish"/> is invoked once the client reports the cinematic ended, or was skipped.
        /// </remarks>
        public void QueueCinematic(ICinematicBase cinematic, Action onFinish = null)
        {
            queuedCinematics.Enqueue((cinematic, onFinish));
            if (currentCinematic == null && queuedCinematics.Count >= 1u)
                PlayQueuedCinematic();
        }

        /// <summary>
        /// Play the next queued <see cref="ICinematicBase"/>.
        /// </summary>
        private void PlayQueuedCinematic()
        {
            if (queuedCinematics.Count == 0)
                return;

            (ICinematicBase cinematic, Action callback) = queuedCinematics.Dequeue();
            currentCinematic = cinematic;
            currentCallback  = callback;

            try
            {
                currentCinematic.StartPlayback(owner);
            }
            catch (Exception exception)
            {
                // a broken cinematic must not stop the script waiting on it
                log.Error(exception, $"Failed to play cinematic {cinematic.GetType().Name} for {owner.Name}.");
                FinishCurrentCinematic();
                return;
            }

            currentTimer = currentCinematic.Duration / 1000d + EndTimeout;
        }

        /// <summary>
        /// Handle the <see cref="CinematicState"/> the Client sent back. Only to be called from Client Handlers.
        /// </summary>
        public void HandleClientCinematicState(CinematicState cinematicState)
        {
            switch (cinematicState)
            {
                case CinematicState.Initalising:
                    // Cast Spell: Generic - Cinematic Player State - Tier 1 (Spell4 ID: 49887)
                    // Make the Player invisible/immune from aggro
                    break;
                case CinematicState.Finishing:
                    // End Cinematic Player State spell
                    // Make the Player visible/remove immunity
                    break;
                case CinematicState.Ended:
                {
                    if (currentCinematic == null)
                        break;

                    FinishCurrentCinematic();
                    break;
                }
            }
        }

        private void FinishCurrentCinematic()
        {
            ICinematicBase cinematic = currentCinematic;
            Action callback          = currentCallback;
            currentCinematic = null;
            currentCallback  = null;

            // Player is back in the world. Continue any scripts that may've been paused.
            owner.Map?.PublicEventManager.OnCinematicFinish(owner, cinematic.CinematicId);

            try
            {
                callback?.Invoke();
            }
            catch (Exception exception)
            {
                log.Error(exception, $"Cinematic {cinematic.GetType().Name} finish callback failed for {owner.Name}.");
            }

            PlayQueuedCinematic();
        }
    }
}
