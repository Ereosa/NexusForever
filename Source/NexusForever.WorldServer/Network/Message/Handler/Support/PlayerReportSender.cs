using System;
using System.Collections.Generic;
using NexusForever.Database;
using NexusForever.Database.Auth;
using NexusForever.Database.Auth.Model;
using NexusForever.Game.Abstract.Entity;
using NexusForever.GameTable;
using NexusForever.Shared.Game.Events;

namespace NexusForever.WorldServer.Network.Message.Handler.Support
{
    /// <summary>
    /// Files in-game bug reports and support tickets into the report website's player_report table (game.eongaming.org/report).
    /// </summary>
    /// <remarks>
    /// Summary and description follow the website form limits (120 and 5000 characters) and categories are the REPORT_CATEGORIES keys in report_lib.php.
    /// </remarks>
    public static class PlayerReportSender
    {
        public const int SummaryMaxLength     = 120;
        public const int DescriptionMaxLength = 5000;

        // same limit as REPORT_SUBMIT_MAX on the website, counted separately
        private const int ReportsPerHour = 6;

        private static readonly Dictionary<uint, List<DateTime>> recentReports = new();

        /// <summary>
        /// Save a report for the session's player, <paramref name="callback"/> is invoked with the new report id or 0 if it was not saved.
        /// </summary>
        public static void Send(IWorldSession session, IDatabaseManager databaseManager, string category, string summary, string description, string context, Action<uint> callback)
        {
            IPlayer player = session.Player;
            if (player == null)
                return;

            if (!TryUseRateLimit(session.Account.Id))
            {
                player.SendSystemMessage("You have sent a lot of reports in the last hour. Please wait a bit before sending more.");
                callback(0u);
                return;
            }

            string footer = $"\n\n{context}";
            description = Truncate(description.Trim().Replace("\r\n", "\n"), DescriptionMaxLength - footer.Length) + footer;

            var report = new PlayerReportModel
            {
                Category      = category,
                Summary       = Truncate(summary.Trim(), SummaryMaxLength),
                Description   = description,
                Zone          = GetZoneName(player),
                AccountId     = session.Account.Id,
                Email         = session.Account.Email,
                CharacterId   = player.CharacterId,
                CharacterName = player.Name,
                Level         = player.Level,
                Class         = (byte)player.Class,
                WorldId       = (ushort)(player.Map?.Entry.Id ?? 0u),
                WorldZoneId   = (ushort)(player.Zone?.Id ?? 0u),
                LocationX     = player.Position.X,
                LocationY     = player.Position.Y,
                LocationZ     = player.Position.Z,
                Ip            = session.RemoteAddress
            };

            session.Events.EnqueueEvent(new TaskGenericEvent<uint>(databaseManager.GetDatabase<AuthDatabase>().AddPlayerReportAsync(report),
                id =>
            {
                if (id != 0u)
                    session.Player?.SendSystemMessage($"Thanks! Your report #{id} was saved. You can follow it or add more information at game.eongaming.org/report");
                else
                    session.Player?.SendSystemMessage("Sorry, your report could not be saved. Please use game.eongaming.org/report instead.");
                callback(id);
            }));
        }

        /// <summary>
        /// Return a summary from the first line of <paramref name="text"/>, or <paramref name="fallback"/> if that line is too short to mean anything.
        /// </summary>
        public static string GetSummary(string text, string fallback)
        {
            string line = text.Trim().Split('\n')[0].Trim();
            return line.Length >= 5 ? Truncate(line, SummaryMaxLength) : fallback;
        }

        public static string GetText(uint localizedTextId)
        {
            return localizedTextId != 0u ? GameTableManager.Instance.TextEnglish?.GetEntry(localizedTextId) ?? "" : "";
        }

        /// <summary>
        /// Return a line describing where the player is, for the report's description.
        /// </summary>
        public static string GetPositionLine(IPlayer player)
        {
            return $"Position: {player.Position.X:0.0}, {player.Position.Y:0.0}, {player.Position.Z:0.0} (world {player.Map?.Entry.Id ?? 0u}, zone {player.Zone?.Id ?? 0u})";
        }

        private static string GetZoneName(IPlayer player)
        {
            string name = player.Zone != null ? GetText(player.Zone.LocalizedTextIdName) : "";
            if (name == "" && player.Map != null)
                name = GetText(player.Map.Entry.LocalizedTextIdName);
            return Truncate(name, 100);
        }

        private static bool TryUseRateLimit(uint accountId)
        {
            lock (recentReports)
            {
                DateTime now = DateTime.UtcNow;
                if (!recentReports.TryGetValue(accountId, out List<DateTime> times))
                    recentReports.Add(accountId, times = new List<DateTime>());

                times.RemoveAll(t => now - t > TimeSpan.FromHours(1));
                if (times.Count >= ReportsPerHour)
                    return false;

                times.Add(now);
                return true;
            }
        }

        private static string Truncate(string text, int length)
        {
            return text.Length <= length ? text : text[..length];
        }
    }
}
