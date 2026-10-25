using NexusForever.Game.Abstract.Entity;
using NexusForever.Network.Session;

namespace NexusForever.Game.Abstract.Quest
{
    public interface ICommunicatorMessage
    {
        uint Id { get; }
        ushort QuestId { get; }

        /// <summary>
        /// Checks if <see cref="IPlayer"/> meets the required conditions for this quest to be added to their communicator.
        /// </summary>
        /// <param name="checkLocation">Whether the world and zone the message is sent in are checked, a message already received can be answered anywhere.</param>
        bool Meets(IPlayer player, bool checkLocation = true);

        /// <summary>
        /// Returns if the message has any condition that decides when it is sent.
        /// </summary>
        bool HasConditions();

        /// <summary>
        /// Send communicator message to <see cref="IGameSession"/>.
        /// </summary>
        void Send(IGameSession session);
    }
}