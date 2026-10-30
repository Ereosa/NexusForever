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
    /// The Submit Ticket window, filed as a player report on the report website.
    /// </summary>
    public class ClientSupportTicketHandler : IMessageHandler<IWorldSession, ClientSupportTicket>
    {
        #region Dependency Injection

        private readonly IDatabaseManager databaseManager;

        public ClientSupportTicketHandler(
            IDatabaseManager databaseManager)
        {
            this.databaseManager = databaseManager;
        }

        #endregion

        public void HandleMessage(IWorldSession session, ClientSupportTicket supportTicket)
        {
            IPlayer player = session.Player;
            if (player == null)
                return;

            string subject = supportTicket.Subject ?? "";
            string body    = supportTicket.Body ?? "";
            if (body.Trim() == "" && subject.Trim() == "")
            {
                session.EnqueueMessageEncrypted(new ServerSupportTicketResult { Success = false });
                return;
            }

            TicketCategoryEntry category = GameTableManager.Instance.TicketCategory.GetEntry(supportTicket.TicketCategoryId);
            TicketSubCategoryEntry subcategory = GameTableManager.Instance.TicketSubCategory.GetEntry(supportTicket.TicketSubCategoryId);
            string categoryName = $"{(category != null ? PlayerReportSender.GetText(category.LocalizedTextId) : supportTicket.TicketCategoryId.ToString())} / "
                + (subcategory != null ? PlayerReportSender.GetText(subcategory.LocalizedTextId) : supportTicket.TicketSubCategoryId.ToString());

            var context = new StringBuilder();
            context.AppendLine("--- Sent from the in-game Submit Ticket window ---");
            context.AppendLine($"Ticket category: {categoryName}");
            context.Append(PlayerReportSender.GetPositionLine(player));

            PlayerReportSender.Send(session, databaseManager, GetReportCategory(supportTicket.TicketCategoryId, supportTicket.TicketSubCategoryId),
                PlayerReportSender.GetSummary(subject, $"In-game ticket: {categoryName}"), body.Trim() != "" ? body : subject, context.ToString(),
                id => session.EnqueueMessageEncrypted(new ServerSupportTicketResult { Success = id != 0u }));
        }

        /// <summary>
        /// Map a TicketCategory and TicketSubCategory to the report website's category.
        /// </summary>
        private static string GetReportCategory(uint categoryId, uint subcategoryId)
        {
            return categoryId switch
            {
                1u => subcategoryId == 2u ? "crash" : "other", // Technical Issues, 2: Crashing
                5u => subcategoryId switch                     // Gameplay
                {
                    15u               => "creature",           // NPCs
                    16u or 17u or 19u => "combat",             // PvE, PvP, Abilities
                    18u               => "item",               // Tradeskills
                    _                 => "other"
                },
                _  => "other"
            };
        }
    }
}
