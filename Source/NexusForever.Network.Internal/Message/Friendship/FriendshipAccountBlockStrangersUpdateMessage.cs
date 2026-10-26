namespace NexusForever.Network.Internal.Message.Friendship
{
    public class FriendshipAccountBlockStrangersUpdateMessage
    {
        public uint AccountId { get; set; }
        public bool BlockStrangerRequests { get; set; }
    }
}
