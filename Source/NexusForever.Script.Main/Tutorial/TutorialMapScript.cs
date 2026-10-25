using NexusForever.Game.Abstract.Cinematic;
using NexusForever.Game.Abstract.Cinematic.Cinematics;
using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Map.Instance;
using NexusForever.Game.Abstract.Quest;
using NexusForever.Game.Abstract.Spell;
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

        /// <summary>
        /// Invoked when <see cref="IGridEntity"/> is added to map.
        /// </summary>
        public void OnAddToMap(IGridEntity entity)
        {
            if (entity is not IPlayer player)
                return;

            player.CinematicManager.QueueCinematic(cinematicFactory.CreateCinematic<INoviceTutorialOnEnter>());

            ISpellParameters parameters = spellParameterFactory.Resolve();
            parameters.UserInitiatedSpellCast = false;
            player.CastSpell(SpellId.HoloTechVfx, parameters);

            CompleteAchievedQuests(player);

            QuestId questId = player.Faction2 == Faction.Exile
                ? QuestId.NavigatingNexusExile
                : QuestId.NavigatingNexusDominion;

            if (player.QuestManager.GetQuestState(questId) != null)
                return;
            
            player.QuestManager.QuestAdd(questId);
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
