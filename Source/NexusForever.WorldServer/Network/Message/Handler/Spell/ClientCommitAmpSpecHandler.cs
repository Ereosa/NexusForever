using NexusForever.Game.Abstract.Spell;
using NexusForever.Network.Message;
using NexusForever.Network.World.Message.Model.Abilities;

namespace NexusForever.WorldServer.Network.Message.Handler.Spell
{
    public class ClientCommitAmpSpecHandler : IMessageHandler<IWorldSession, ClientCommitAmpSpec>
    {
        /// <summary>
        /// Sent when AMPs are committed in the AMP window, the list contains the existing and newly selected AMPs of the active action set.
        /// </summary>
        public void HandleMessage(IWorldSession session, ClientCommitAmpSpec commitAmpSpec)
        {
            byte actionSetIndex = session.Player.SpellManager.ActiveActionSet;
            session.Player.SpellManager.AddAmps(actionSetIndex, commitAmpSpec.Amps);

            IActionSet actionSet = session.Player.SpellManager.GetActionSet(actionSetIndex);
            session.EnqueueMessageEncrypted(actionSet.BuildServerAmpList());
        }
    }
}
