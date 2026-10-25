using NexusForever.Game.Static.Spell;
using NexusForever.Network.Message;
using NexusForever.Network.World.Message.Model.Spell;

namespace NexusForever.WorldServer.Network.Message.Handler.Spell
{
    public class ClientGameCommandHandler : IMessageHandler<IWorldSession, ClientGameCommand>
    {
        /// <summary>
        /// Sent when a spell shortcut on an action bar shown by the server is pressed, eg: 41693 Go To Location on the Engineer pet bar.
        /// </summary>
        public void HandleMessage(IWorldSession session, ClientGameCommand gameCommand)
        {
            session.Player.CombatPetManager.HandleActionBarCast((ShortcutSet)gameCommand.LimitedActionSetId, gameCommand.AbilityItemIndex,
                gameCommand.TargetUnitId, gameCommand.TargetPosition.Vector);
        }
    }
}
