using Microsoft.Extensions.Logging;
using NexusForever.Game.Abstract.Entity;
using NexusForever.Network;
using NexusForever.Network.Message;
using NexusForever.Network.World.Message.Model;
using NexusForever.Network.World.Message.Model.Spell;

namespace NexusForever.WorldServer.Network.Message.Handler.Entity
{
    /// <remarks>
    /// Possibly only used by Bindpoint entities
    /// </remarks>
    public class ClientActivateUnitInteractionHandler : IMessageHandler<IWorldSession, ClientCastBindPointSpell>
    {
        #region Dependency Injection

        private readonly ILogger<ClientActivateUnitInteractionHandler> log;

        public ClientActivateUnitInteractionHandler(
            ILogger<ClientActivateUnitInteractionHandler> log)
        {
            this.log = log;
        }

        #endregion

        public void HandleMessage(IWorldSession session, ClientCastBindPointSpell activateUnitInteraction)
        {
            IWorldEntity entity = session.Player.GetVisible<IWorldEntity>(activateUnitInteraction.UnitId);
            // the client can activate a unit the server has just removed from its vision (a respawn, a phase or grid change),
            // that isn't a bad packet and must not disconnect the player (report #47: the Northern Wilds departure console)
            if (entity == null)
            {
                log.LogDebug($"{session.Player.Name} activated unit {activateUnitInteraction.UnitId} which isn't visible to them.");
                return;
            }

            entity.OnActivateCast(session.Player, activateUnitInteraction.ClientSpellCastUniqueId);

            // "Select the Transmat Terminal" (Touring Tremor Ridge, Touring Woodhaven): the terminals are bind points
            session.Player.QuestManager.ObjectiveUpdateUseCreature(entity.CreatureId);
        }
    }
}
