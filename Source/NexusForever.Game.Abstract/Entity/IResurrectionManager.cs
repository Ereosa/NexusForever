using NexusForever.Game.Static.Entity;
using NexusForever.Shared;

namespace NexusForever.Game.Abstract.Entity
{
    public interface IResurrectionManager : IUpdate
    {
        /// <summary>
        /// Determines if owner <see cref="IPlayer"/> can resurrect another player.
        /// </summary>
        public bool CanResurrectOtherPlayer { get; set; }

        /// <summary>
        /// Initialise a new <see cref="IResurrectionManager"/> for <see cref="IPlayer"/>.
        /// </summary>
        void Initalise(IPlayer owner);

        /// <summary>
        /// Send initial resurrection packets to owner <see cref="IPlayer"/>.
        /// </summary>
        void SendInitialPackets();

        /// <summary>
        /// Show resurrection options to owner <see cref="IPlayer"/>.
        /// </summary>
        void ShowResurrection(ResurrectionType type, TimeSpan? timeUntilResurrection, TimeSpan? timeUntilForcedResurrection);

        /// <summary>
        /// Resurrect owner <see cref="IPlayer"/> with the specified <see cref="ResurrectionType"/>.
        /// </summary>
        void ResurrectSelf(ResurrectionType rezType);

        /// <summary>
        /// Send resurrection request target <see cref="IPlayer>"/>.
        /// </summary>
        void ResurrectTarget(uint targetUnitId);

        /// <summary>
        /// Recieve resurrection request from <see cref="IPlayer"/>.
        /// </summary>
        void OnResurrectRequest(uint unitId);
    }
}
