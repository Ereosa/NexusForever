using NexusForever.Game.Abstract.Spell;
using NexusForever.Network.Message;
using NexusForever.Network.World.Message.Model.Spell;
using NexusForever.Shared;

namespace NexusForever.WorldServer.Network.Message.Handler.Spell
{
    public class ClientSetSprintStateHandler : IMessageHandler<IWorldSession, ClientSetSprintState>
    {
        /// <summary>
        /// Sprint - Tier 1, increases movement speed while draining endurance.
        /// </summary>
        private const uint SprintSpell4Id = 1316;

        #region Dependency Injection

        private readonly IFactory<ISpellParameters> spellParametersFactory;

        public ClientSetSprintStateHandler(
            IFactory<ISpellParameters> spellParametersFactory)
        {
            this.spellParametersFactory = spellParametersFactory;
        }

        #endregion

        public void HandleMessage(IWorldSession session, ClientSetSprintState setSprintState)
        {
            bool sprinting = session.Player.HasSpell(SprintSpell4Id, out ISpell spell);
            if (setSprintState.Sprint == sprinting)
                return;

            if (setSprintState.Sprint)
            {
                ISpellParameters parameters = spellParametersFactory.Resolve();
                parameters.UserInitiatedSpellCast = false;
                session.Player.CastSpell(SprintSpell4Id, parameters);
            }
            else
                spell.Finish();
        }
    }
}
