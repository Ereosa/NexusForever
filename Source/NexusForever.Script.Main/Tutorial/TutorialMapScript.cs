using System.Numerics;
using NexusForever.Game.Abstract.Cinematic;
using NexusForever.Game.Abstract.Cinematic.Cinematics;
using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Map.Instance;
using NexusForever.Game.Abstract.Quest;
using NexusForever.Game.Abstract.Spell;
using NexusForever.Game.Static.Cinematic;
using NexusForever.Game.Static.Quest;
using NexusForever.Game.Static.Reputation;
using NexusForever.Script.Template;
using NexusForever.Script.Template.Filter;
using NexusForever.Shared;

namespace NexusForever.Script.Main.Tutorial
{
    [ScriptFilterOwnerId(3460)]
    public class TutorialMapScript : IInstancedMapScript, IOwnedScript<ITutorialMapInstance>
    {
        private enum SpellId
        {
            HoloTechVfx = 86985
        }

        /// <summary>
        /// Warfare Woods, the combat simulation.
        /// </summary>
        private const uint CombatSimulationZoneId = 5967;

        /// <summary>
        /// Tutorial quests that are completed by script as soon as they are achieved, and the quest that follows them.
        /// </summary>
        private static readonly (QuestId QuestId, QuestId NextQuestId)[] autoCompleteQuests =
        [
            (QuestId.NavigatingNexusExile,       QuestId.NavigatingNexusPt2Exile),
            (QuestId.NavigatingNexusPt2Exile,    QuestId.TheFaceOfTheEnemyExile),
            (QuestId.NavigatingNexusDominion,    QuestId.NavigatingNexusPt2Dominion),
            (QuestId.NavigatingNexusPt2Dominion, QuestId.TheFaceOfTheEnemyDominion)
        ];

        #region Dependency Injection

        private readonly ICinematicFactory cinematicFactory;
        private readonly IFactory<ISpellParameters> spellParameterFactory;

        public TutorialMapScript(
            ICinematicFactory cinematicFactory,
            IFactory<ISpellParameters> spellParameterFactory)
        {
            this.cinematicFactory      = cinematicFactory;
            this.spellParameterFactory = spellParameterFactory;
        }

        #endregion

        private readonly HashSet<ulong> combatTransitionPlayed = [];

        /// <summary>
        /// Invoked when <see cref="IGridEntity"/> is added to map.
        /// </summary>
        public void OnAddToMap(IGridEntity entity)
        {
            if (entity is not IPlayer player)
                return;

            ISpellParameters parameters = spellParameterFactory.Resolve();
            parameters.UserInitiatedSpellCast = false;
            player.CastSpell(SpellId.HoloTechVfx, parameters);

            CompleteAchievedQuests(player);
            LeaveCryoPod(player);

            QuestId questId = player.Faction2 == Faction.Exile
                ? QuestId.NavigatingNexusExile
                : QuestId.NavigatingNexusDominion;

            if (player.QuestManager.GetQuestState(questId) != null)
                return;

            // the bridge intro only plays when the simulation starts, not when a character logs back in
            player.CinematicManager.QueueCinematic(cinematicFactory.CreateCinematic<INoviceTutorialOnEnter>());
            player.QuestManager.QuestAdd(questId);
        }

        /// <summary>
        /// Invoked when <see cref="IPlayer"/> on the map moves into a different zone.
        /// </summary>
        public void OnPlayerZoneChange(IPlayer player, uint zoneId)
        {
            if (zoneId != CombatSimulationZoneId)
                return;

            // combat transition plays when the player first reaches Warfare Woods, before any enemy is killed
            QuestId questId = player.Faction2 == Faction.Exile
                ? QuestId.TheFaceOfTheEnemyExile
                : QuestId.TheFaceOfTheEnemyDominion;

            IQuest quest = player.QuestManager.GetActiveQuest(questId);
            if (quest == null || quest.State != QuestState.Accepted || quest.Any(o => o.Progress > 0u))
                return;

            if (!combatTransitionPlayed.Add(player.CharacterId))
                return;

            ICinematicBase cinematic = cinematicFactory.CreateCinematic(player.Faction2 == Faction.Exile
                ? CinematicSceneId.NpeCombatExile
                : CinematicSceneId.NpeCombatDominion);
            if (cinematic != null)
                player.CinematicManager.QueueCinematic(cinematic);
        }

        /// <summary>
        /// Dominion characters used to wake from cryosleep inside their pod (WorldLocation2 51657) and couldn't move, put them in front of it (52851).
        /// </summary>
        private static void LeaveCryoPod(IPlayer player)
        {
            var pod   = new Vector3(-3689.684f, -436.79004f, -4356.004f);
            var front = new Vector3(-3680.6614f, -437.85535f, -4355.892f);
            if (Vector3.Distance(player.Position, pod) <= 3f)
                player.TeleportToLocal(front);
        }

        /// <summary>
        /// Hand over any auto completed quest that was achieved without its quest script running, eg a Dominion character from before the Dominion scripts existed.
        /// </summary>
        private static void CompleteAchievedQuests(IPlayer player)
        {
            foreach ((QuestId questId, QuestId nextQuestId) in autoCompleteQuests)
            {
                IQuest quest = player.QuestManager.GetActiveQuest(questId);
                if (quest?.State != QuestState.Achieved)
                    continue;

                quest.RewardQuest(0);
                player.QuestManager.QuestComplete(questId);
                if (player.QuestManager.GetQuestState(nextQuestId) == null)
                    player.QuestManager.QuestAdd(nextQuestId);
            }
        }
    }
}
