using System.Text;
using NexusForever.Database;
using NexusForever.Game.Abstract.Entity;
using NexusForever.GameTable;
using NexusForever.GameTable.Model;
using NexusForever.Network.Message;
using NexusForever.Network.World.Message.Model.Support;

namespace NexusForever.WorldServer.Network.Message.Handler.Support
{
    /// <summary>
    /// The Report Bug window, filed as a player report on the report website.
    /// </summary>
    public class ClientReportBugHandler : IMessageHandler<IWorldSession, ClientReportBug>
    {
        #region Dependency Injection

        private readonly IDatabaseManager databaseManager;

        public ClientReportBugHandler(
            IDatabaseManager databaseManager)
        {
            this.databaseManager = databaseManager;
        }

        #endregion

        public void HandleMessage(IWorldSession session, ClientReportBug reportBug)
        {
            IPlayer player = session.Player;
            if (player == null)
                return;

            string description = reportBug.Description ?? "";
            if (description.Trim() == "")
            {
                player.SendSystemMessage("Please describe the problem before sending the bug report.");
                return;
            }

            BugSubcategoryEntry subcategory = GameTableManager.Instance.BugSubcategory.GetEntry(reportBug.BugSubcategoryId);
            BugCategoryEntry category = subcategory != null ? GameTableManager.Instance.BugCategory.GetEntry(subcategory.BugCategoryId) : null;
            string categoryName = category != null
                ? $"{PlayerReportSender.GetText(category.LocalizedTextId)} / {PlayerReportSender.GetText(subcategory.LocalizedTextId)}"
                : $"unknown ({reportBug.BugSubcategoryId})";

            var context = new StringBuilder();
            context.AppendLine("--- Sent from the in-game Report Bug window ---");
            context.AppendLine($"Bug category: {categoryName}");

            if (reportBug.Quest2Id != 0u)
            {
                Quest2Entry quest = GameTableManager.Instance.Quest2.GetEntry(reportBug.Quest2Id);
                context.AppendLine($"Quest: {(quest != null ? PlayerReportSender.GetText(quest.LocalizedTextIdTitle) : "unknown")} (id {reportBug.Quest2Id})");
            }

            if (reportBug.SelectedUnitId != 0u)
                context.AppendLine($"Selected target: {GetUnitName(player, reportBug.SelectedUnitId)}");

            context.Append(PlayerReportSender.GetPositionLine(player));

            PlayerReportSender.Send(session, databaseManager, GetReportCategory(subcategory?.BugCategoryId ?? 0u, reportBug.BugSubcategoryId),
                PlayerReportSender.GetSummary(description, $"In-game bug report: {categoryName}"), description, context.ToString(), _ => { });
        }

        private static string GetUnitName(IPlayer player, uint unitId)
        {
            IWorldEntity entity = unitId == player.Guid ? player : player.GetVisible<IWorldEntity>(unitId);
            if (entity == null)
                return $"unit {unitId}, no longer in sight";
            if (entity is IPlayer target)
                return $"player {target.Name}";

            Creature2Entry creature = GameTableManager.Instance.Creature2.GetEntry(entity.CreatureId);
            string name = creature != null ? PlayerReportSender.GetText(creature.LocalizedTextIdName) : "";
            return $"{(name != "" ? name : "unnamed")} (creature {entity.CreatureId}, unit {unitId})";
        }

        /// <summary>
        /// Map a BugCategory and BugSubcategory to the report website's category.
        /// </summary>
        private static string GetReportCategory(uint categoryId, uint subcategoryId)
        {
            return categoryId switch
            {
                7u => "quest",                                         // Quest
                4u => subcategoryId == 12u ? "combat" : "creature",    // Creature/NPC, 12: Combat
                5u => "item",                                          // Economy
                3u => subcategoryId == 8u ? "combat" : "other",        // Character, 8: Skills and abilities
                8u => subcategoryId is 25u or 26u ? "crash" : "other", // Technical Issues, 25: Connection, 26: Crashes
                9u => "ui",                                            // UI
                1u => subcategoryId == 2u ? "ui" : "other",            // Art, 2: Map/Minimap
                _  => "other"
            };
        }
    }
}
