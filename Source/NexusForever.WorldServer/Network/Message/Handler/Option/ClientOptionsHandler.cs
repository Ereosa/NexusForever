using NexusForever.Game.Static.Challenges;
using NexusForever.Game.Static.Option;
using NexusForever.Network.Message;
using NexusForever.Network.World.Message.Model.Option;

namespace NexusForever.WorldServer.Network.Message.Handler.Option
{
    public class ClientOptionsHandler : IMessageHandler<IWorldSession, ClientOptions>
    {
        public void HandleMessage(IWorldSession session, ClientOptions options)
        {
            switch (options.Type)
            {
                // the client sends its shared challenge preference on load and whenever it changes, it isn't saved by the server
                case OptionType.SharedChallenge:
                    session.Player.ChallengeManager.SetSharedChallengePreference((SharedChallengePreference)options.NewValue);
                    break;
                // casting options only change client side casting behaviour
                case OptionType.Casting:
                    break;
            }
        }
    }
}
