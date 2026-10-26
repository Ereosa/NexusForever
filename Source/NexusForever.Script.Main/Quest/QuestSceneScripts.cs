using NexusForever.Game.Abstract.Entity;
using NexusForever.Script.Template;
using NexusForever.Script.Template.Filter;

namespace NexusForever.Script.Main.Quest
{
    /// <summary>
    /// Drunk Men Tell Ol' Tales: the drinking contest at a Graylight bar. The rival drinks while the player waits, and after the player's
    /// own mugs staggers and collapses.
    /// </summary>
    /// <remarks>
    /// Scene from the objective texts ("Wait for ... to finish drinking", "Wait for ... to collapse after drinking too much brew"):
    /// the drink emote (5) while drinking, stagger (320) then sleep (32, lying down) to collapse, standing up again a minute later.
    /// No dialogue is known, none is said.
    /// </remarks>
    public abstract class DrunkMenTellOlTalesQuestScript : QuestSceneScript
    {
        private const uint DrinkEmote   = 5u;
        private const uint StaggerEmote = 320u;
        private const uint SleepEmote   = 32u;

        protected abstract uint DrinkerCreatureId { get; }
        protected abstract uint FinishDrinkingObjectiveId { get; }
        protected abstract uint CollapseObjectiveId { get; }

        protected override IReadOnlyList<uint> SceneObjectiveIds => [FinishDrinkingObjectiveId, CollapseObjectiveId];

        protected override void OnSceneStart(uint objectiveId, IPlayer player)
        {
            IWorldEntity drinker = FindCreature(player, DrinkerCreatureId);
            if (drinker == null)
                return;

            if (objectiveId == FinishDrinkingObjectiveId)
            {
                drinker.Emote(DrinkEmote);
                After(4d, () => drinker.Emote(DrinkEmote));
                After(8d, () => drinker.Emote(0u));
            }
            else if (objectiveId == CollapseObjectiveId)
            {
                drinker.Emote(StaggerEmote);
                After(4d, () => drinker.Emote(SleepEmote));
                After(70d, () => drinker.Emote(0u));
            }
        }
    }

    /// <summary>
    /// Drunk Men Tell Ol' Tales with Draxa Graystone at On the Rocks.
    /// </summary>
    [ScriptFilterOwnerId(6251)]
    public class DrunkMenTellOlTalesDraxaQuestScript : DrunkMenTellOlTalesQuestScript
    {
        protected override uint DrinkerCreatureId => 29356u;
        protected override uint FinishDrinkingObjectiveId => 9942u;
        protected override uint CollapseObjectiveId => 9946u;
    }

    /// <summary>
    /// Drunk Men Tell Ol' Tales with Kezok the Guzzler at the High Orbit Lounge.
    /// </summary>
    [ScriptFilterOwnerId(6535)]
    public class DrunkMenTellOlTalesKezokQuestScript : DrunkMenTellOlTalesQuestScript
    {
        protected override uint DrinkerCreatureId => 29743u;
        protected override uint FinishDrinkingObjectiveId => 10135u;
        protected override uint CollapseObjectiveId => 10139u;
    }
}
