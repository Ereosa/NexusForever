using NexusForever.Game.Abstract.Spell;
using NexusForever.GameTable;
using NexusForever.GameTable.Model;
using NexusForever.Network.Message;
using NexusForever.Network.World.Message.Model.Spell;
using NexusForever.Shared;

namespace NexusForever.WorldServer.Network.Message.Handler.Spell
{
    public class ClientCastDefaultAttackHandler : IMessageHandler<IWorldSession, ClientCastDefaultAttack>
    {
        #region Dependency Injection

        private readonly IGameTableManager gameTableManager;
        private readonly IFactory<ISpellParameters> spellParametersFactory;

        public ClientCastDefaultAttackHandler(
            IGameTableManager gameTableManager,
            IFactory<ISpellParameters> spellParametersFactory)
        {
            this.gameTableManager       = gameTableManager;
            this.spellParametersFactory = spellParametersFactory;
        }

        #endregion

        /// <summary>
        /// Sent when the player presses the default attack key, it casts the class's primary attack at the current target.
        /// </summary>
        public void HandleMessage(IWorldSession session, ClientCastDefaultAttack castDefaultAttack)
        {
            if (!castDefaultAttack.ButtonPressed)
                return;

            ClassEntry classEntry = gameTableManager.Class.GetEntry((uint)session.Player.Class);
            uint spell4Id = classEntry?.Spell4IdAttackPrimary[0] ?? 0u;
            if (spell4Id == 0u)
                return;

            ISpellParameters parameters = spellParametersFactory.Resolve();
            parameters.UserInitiatedSpellCast = true;
            if (session.Player.TargetGuid.HasValue)
                parameters.PrimaryTargetId = session.Player.TargetGuid.Value;

            session.Player.CastSpell(spell4Id, parameters);
        }
    }
}
