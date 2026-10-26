using System.Linq;
using System.Threading.Tasks;
using NexusForever.Game;
using NexusForever.Game.Abstract.Chat.Format;
using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Chat;
using NexusForever.Game.Static.Chat;
using NexusForever.Network.Internal.Message.Chat;
using Rebus.Handlers;

namespace NexusForever.WorldServer.Network.Internal.Handler.Chat
{
    public class ChatWhisperTextHandler : IHandleMessages<ChatWhisperTextMessage>
    {
        #region Dependency Injection

        private readonly IPlayerManager playerManager;
        private readonly IChatFormatManager chatFormatManager;

        public ChatWhisperTextHandler(
            IPlayerManager playerManager,
            IChatFormatManager chatFormatManager)
        {
            this.playerManager     = playerManager;
            this.chatFormatManager = chatFormatManager;
        }

        #endregion

        public Task Handle(ChatWhisperTextMessage message)
        {
            IPlayer player = playerManager.GetPlayer(message.Recipient.ToGameIdentity());
            if (player == null)
                return Task.CompletedTask;

            var builder = new ChatMessageBuilder();
            builder.Type    = message.IsAccountWhisper ? ChatChannelType.AccountWhisper : ChatChannelType.Whisper;
            builder.Text    = message.Text.Text;
            builder.Formats = chatFormatManager.ToNetwork(message.Text.Format).ToList();

            builder.FromCharacterId      = message.Sender.Id;
            builder.FromCharacterRealmId = message.Sender.RealmId;
            builder.FromName             = message.SenderName.Name;
            if (message.Recipient.RealmId != message.Sender.RealmId)
                builder.FromRealm = message.SenderName.RealmName;

            IPlayer sender = playerManager.GetPlayer(message.Sender.ToGameIdentity());
            if (sender != null)
                builder.PresenceState = sender.Presence;

            player.Session.EnqueueMessageEncrypted(builder.Build());

            SendAutoResponse(message, player, sender);

            return Task.CompletedTask;
        }

        /// <summary>
        /// Reply to the sender with the recipient's away or busy auto-response, the client shows it as "(Auto-Response)".
        /// </summary>
        private static void SendAutoResponse(ChatWhisperTextMessage message, IPlayer recipient, IPlayer sender)
        {
            if (sender == null)
                return;

            string text = recipient.Presence switch
            {
                AccountPresenceState.Away => recipient.AwayAutoResponse,
                AccountPresenceState.Busy => recipient.BusyAutoResponse,
                _                         => null
            };
            if (string.IsNullOrWhiteSpace(text))
                return;

            var builder = new ChatMessageBuilder
            {
                Type                 = message.IsAccountWhisper ? ChatChannelType.AccountWhisper : ChatChannelType.Whisper,
                AutoResponse         = true,
                FromCharacterId      = message.Recipient.Id,
                FromCharacterRealmId = message.Recipient.RealmId,
                FromName             = message.RecipientName.Name,
                PresenceState        = recipient.Presence,
                Text                 = text
            };
            if (message.Recipient.RealmId != message.Sender.RealmId)
                builder.FromRealm = message.RecipientName.RealmName;

            sender.Session.EnqueueMessageEncrypted(builder.Build());
        }
    }
}
