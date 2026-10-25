using NexusForever.Game.Abstract.Quest;
using NexusForever.Script.Template;
using NexusForever.Script.Template.Filter;

namespace NexusForever.Script.Main.Tutorial.Script.Part2
{
    [ScriptFilterOwnerId(10525)]
    public class AClaimToStakeExileQuestScript : TutorialQuestScript, IQuestScript
    {
        // Talk to the Landscape Architects / Decorations Experts, both need 2 talks
        private static readonly uint[] showcaseObjectiveIds = [21319, 21332];

        public override QuestId? NextQuestId => QuestId.LotsInStoreExile;
        // Find Phineas T. Rotostar
        protected override uint? FinalObjectiveId => 21382;

        /// <remarks>
        /// The second talk comes from a gossip option (showing an example plot or decoration) the server doesn't support yet,
        /// so a single talk completes the objective.
        /// </remarks>
        public override void OnObjectiveUpdate(IQuestObjective objective)
        {
            base.OnObjectiveUpdate(objective);

            if (showcaseObjectiveIds.Contains(objective.ObjectiveInfo.Id) && !objective.IsComplete())
                owner.ObjectiveUpdate(objective.ObjectiveInfo.Id, objective.ObjectiveInfo.Entry.Count);
        }

        /// <summary>
        /// Invoked each world tick with the delta since the previous tick occurred.
        /// </summary>
        /// <remarks>
        /// Also finishes showcase objectives that were partially progressed before this script existed.
        /// </remarks>
        public void Update(double lastTick)
        {
            foreach (uint id in showcaseObjectiveIds)
            {
                IQuestObjective objective = owner.GetQuestObjective(id);
                if (objective != null && objective.Progress > 0 && !objective.IsComplete())
                    owner.ObjectiveUpdate(id, objective.ObjectiveInfo.Entry.Count);
            }
        }
    }

    [ScriptFilterOwnerId(10540)]
    public class LotsInStoreExileQuestScript : TutorialQuestScript
    {
        public override QuestId? NextQuestId => QuestId.ServicingCircuitryExile;
        // Continue to the Mission Simulation, the store objectives can't be progressed yet
        protected override uint? FinalObjectiveId => 21373;
    }

    [ScriptFilterOwnerId(10519)]
    public class ServicingCircuitryExileQuestScript : TutorialQuestScript
    {
        public override QuestId? NextQuestId => QuestId.DeconstructTheConstructExile;
    }

    [ScriptFilterOwnerId(10520)]
    public class DeconstructTheConstructExileQuestScript : TutorialQuestScript
    {
        public override QuestId? NextQuestId => QuestId.BonVoyageExile;
        // Wake from cryosleep
        protected override uint? FinalObjectiveId => 21298;
    }
}
