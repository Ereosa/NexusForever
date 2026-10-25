using System.Collections.Immutable;
using Microsoft.Extensions.Logging;
using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Prerequisite;
using NexusForever.Game.Static.Reputation;
using NexusForever.Game.Static.Prerequisite;
using NexusForever.Game.Static.Quest;
using NexusForever.GameTable;
using NexusForever.GameTable.Model;

namespace NexusForever.Game.Prerequisite.Check
{
    /// <remarks>
    /// Object id is the episode, value is the episode state (client EpisodeState: 0 unknown, 1 mentioned, 2 active, 3 complete).
    /// The state is derived from the episode's quests the player can take (EpisodeQuest).
    /// </remarks>
    [PrerequisiteCheck(PrerequisiteType.EpisodeState)]
    public class PrerequisiteCheckEpisodeState : IPrerequisiteCheck
    {
        private static ImmutableDictionary<uint, ImmutableList<uint>> episodeQuests;

        #region Dependency Injection

        private readonly ILogger<PrerequisiteCheckEpisodeState> log;
        private readonly IGameTableManager gameTableManager;

        public PrerequisiteCheckEpisodeState(
            ILogger<PrerequisiteCheckEpisodeState> log,
            IGameTableManager gameTableManager)
        {
            this.log              = log;
            this.gameTableManager = gameTableManager;
        }

        #endregion

        public bool Meets(IPlayer player, PrerequisiteComparison comparison, uint value, uint objectId, IPrerequisiteParameters parameters)
        {
            switch (comparison)
            {
                case PrerequisiteComparison.Equal:
                    return GetState(player, objectId) == value;
                case PrerequisiteComparison.NotEqual:
                    return GetState(player, objectId) != value;
                default:
                    log.LogWarning($"Unhandled PrerequisiteComparison {comparison} for {PrerequisiteType.EpisodeState}!");
                    return false;
            }
        }

        private uint GetState(IPlayer player, uint episodeId)
        {
            episodeQuests ??= gameTableManager.EpisodeQuest.Entries
                .Where(e => e != null)
                .GroupBy(e => e.EpisodeId)
                .ToImmutableDictionary(g => g.Key, g => g.OrderBy(e => e.OrderIdx).Select(e => e.QuestId).ToImmutableList());

            if (!episodeQuests.TryGetValue(episodeId, out ImmutableList<uint> questIds))
                return 0u;

            // quests of the other faction can never be done
            var states = questIds
                .Where(q => IsForFaction(gameTableManager.Quest2.GetEntry(q), player.Faction1))
                .Select(q => player.QuestManager.GetQuestState((ushort)q))
                .ToList();
            if (states.Count == 0)
                return 0u;

            if (states.All(s => s == QuestState.Completed))
                return 3u;
            if (states.Any(s => s is QuestState.Accepted or QuestState.Achieved or QuestState.Completed))
                return 2u;
            if (states.Any(s => s == QuestState.Mentioned))
                return 1u;
            return 0u;
        }

        private static bool IsForFaction(Quest2Entry quest, Faction faction)
        {
            if (quest == null)
                return false;

            return quest.QuestPlayerFactionEnum switch
            {
                0u => faction == Faction.Exile,
                1u => faction == Faction.Dominion,
                _  => true
            };
        }
    }
}
