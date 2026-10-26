using System.Threading.Tasks;
using NexusForever.Game;
using NexusForever.Game.Abstract.Entity;
using NexusForever.GameTable;
using NexusForever.GameTable.Model;
using NexusForever.GameTable.Static;
using NexusForever.Network.Internal.Message.Chat;
using NexusForever.Network.World.Message.Model.Chat;
using Rebus.Handlers;

namespace NexusForever.WorldServer.Network.Internal.Handler.Chat
{
    public class ChatChannelMemberLeftHandler : IHandleMessages<ChatChannelMemberLeftMessage>
    {
        #region Dependency Injection

        private readonly IPlayerManager playerManager;
        private readonly IGameTableManager gameTableManager;

        public ChatChannelMemberLeftHandler(
            IPlayerManager playerManager,
            IGameTableManager gameTableManager)
        {
            this.playerManager    = playerManager;
            this.gameTableManager = gameTableManager;
        }

        #endregion

        public Task Handle(ChatChannelMemberLeftMessage message)
        {
            ChatChannelEntry entry = gameTableManager.ChatChannel.GetEntry((ulong)message.Channel.Type);
            if (entry == null)
                return Task.CompletedTask;

            if ((entry.Flags & ChatChannelEntryFlags.ServerSide) == 0)
                return Task.CompletedTask;

            // tells the client it left (or was kicked from) the channel, otherwise the channel stays in its list
            IPlayer player = playerManager.GetPlayer(message.Member.Identity.ToGameIdentity());
            player?.Session.EnqueueMessageEncrypted(new ServerChatLeave
            {
                Channel = new Channel
                {
                    ChatChannelId = message.Channel.Type,
                    ChatId        = message.Channel.ChatId
                },
                Leave = message.Reason
            });

            return Task.CompletedTask;
        }
    }
}
