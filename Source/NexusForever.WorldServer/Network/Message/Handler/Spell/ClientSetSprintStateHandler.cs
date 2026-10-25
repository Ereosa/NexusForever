using NexusForever.Game.Abstract.Spell;
using NexusForever.Network.Message;
using NexusForever.Network.World.Message.Model.Spell;
using NexusForever.Shared;

namespace NexusForever.WorldServer.Network.Message.Handler.Spell
{
    public class ClientSetSprintStateHandler : IMessageHandler<IWorldSession, ClientSetSprintState>
    {
        /// <summary>
        /// Sprint - Tier 1, increases movement speed while draining endurance, used in combat.
        /// </summary>
        private const uint CombatSprintSpell4Id = 1316;

        /// <summary>
        /// Sprint - Tier 2, out of combat sprint that doesn't drain endurance.
        /// </summary>
        /// <remarks>
        /// The client picks between these from game formula 415 (in combat) and 1227 (out of combat).
        /// </remarks>
        private const uint SprintSpell4Id = 80529;

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
            ISpell spell = null;
            bool sprinting = session.Player.HasSpell(SprintSpell4Id, out spell)
                || session.Player.HasSpell(CombatSprintSpell4Id, out spell);
            if (setSprintState.Sprint == sprinting)
                return;

            if (setSprintState.Sprint)
            {
                ISpellParameters parameters = spellParametersFactory.Resolve();
                parameters.UserInitiatedSpellCast = false;
                session.Player.CastSpell(session.Player.InCombat ? CombatSprintSpell4Id : SprintSpell4Id, parameters);
            }
            else
                spell.Finish();
        }
    }
}
