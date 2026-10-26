using NexusForever.Game.Abstract.Spell;
using NexusForever.Network;
using NexusForever.Network.Message;
using NexusForever.Network.World.Message.Model.Spell;
using NexusForever.Shared;

namespace NexusForever.WorldServer.Network.Message.Handler.Challenge
{
    /// <summary>
    /// Casts of an objective ability, the temporary abilities (T-spells) granted by Ability challenges.
    /// </summary>
    public class ClientCastObjectiveAbilityHandler : IMessageHandler<IWorldSession, ClientCastObjectiveAbility>
    {
        #region Dependency Injection

        private readonly IFactory<ISpellParameters> spellParametersFactory;

        public ClientCastObjectiveAbilityHandler(
            IFactory<ISpellParameters> spellParametersFactory)
        {
            this.spellParametersFactory = spellParametersFactory;
        }

        #endregion

        public void HandleMessage(IWorldSession session, ClientCastObjectiveAbility castObjectiveAbility)
        {
            if (!session.Player.ChallengeManager.CanCastObjectiveAbility(castObjectiveAbility.Spell4Id))
                throw new InvalidPacketValueException();

            ISpellParameters parameters = spellParametersFactory.Resolve();
            parameters.PrimaryTargetId        = castObjectiveAbility.TargetUnitId;
            parameters.TargetPosition         = castObjectiveAbility.Position;
            parameters.UserInitiatedSpellCast = true;
            session.Player.CastSpell(castObjectiveAbility.Spell4Id, parameters);
        }
    }
}
