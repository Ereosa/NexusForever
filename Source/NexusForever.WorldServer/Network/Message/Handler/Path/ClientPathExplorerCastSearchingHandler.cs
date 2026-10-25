using NexusForever.Game.Spell;
using NexusForever.GameTable;
using NexusForever.GameTable.Model;
using NexusForever.Network.Message;
using NexusForever.Network.World.Message.Model.PlayerPath;

namespace NexusForever.WorldServer.Network.Message.Handler.Path
{
    /// <summary>
    /// Sent by the explorer path action near a scavenger hunt dig location, the band is how close the player is to it.
    /// </summary>
    public class ClientPathExplorerCastSearchingHandler : IMessageHandler<IWorldSession, ClientPathExplorerCastSearching>
    {
        #region Dependency Injection

        private readonly IGameTableManager gameTableManager;

        public ClientPathExplorerCastSearchingHandler(
            IGameTableManager gameTableManager)
        {
            this.gameTableManager = gameTableManager;
        }

        #endregion

        public void HandleMessage(IWorldSession session, ClientPathExplorerCastSearching castSearching)
        {
            // searching spells for each band, same lookup as the client
            GameFormulaEntry entry = gameTableManager.GameFormula.GetEntry(1012);
            uint spell4Id = castSearching.SearchRadiusBand switch
            {
                0 => entry?.Dataint0 ?? 27712u,
                1 => entry?.Dataint01 ?? 27713u,
                2 => entry?.Dataint02 ?? 27714u,
                _ => 0u
            };

            if (spell4Id != 0u)
            {
                session.Player.CastSpell(spell4Id, new SpellParameters());
            }

            session.Player.PathMissionManager.OnExplorerSearch(castSearching.PathExplorerScavengerClueId, castSearching.SearchRadiusBand);
        }
    }
}
