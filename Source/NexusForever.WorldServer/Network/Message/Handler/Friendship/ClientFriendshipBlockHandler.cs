using NexusForever.Game.Static.Entity;
using NexusForever.Network.Message;
using NexusForever.Network.World.Message.Model.Friendship;

namespace NexusForever.WorldServer.Network.Message.Handler.Friendship
{
    public class ClientFriendshipBlockHandler : IMessageHandler<IWorldSession, ClientFriendshipBlock>
    {
        public void HandleMessage(IWorldSession session, ClientFriendshipBlock message)
        {
            // FriendshipLib.SetLock, the client reads the state back from the character flags (FriendshipLib.IsLocked)
            if (message.BlockFriendRequests)
                session.Player.SetFlag(CharacterFlag.FriendBlock);
            else
                session.Player.RemoveFlag(CharacterFlag.FriendBlock);
        }
    }
}
