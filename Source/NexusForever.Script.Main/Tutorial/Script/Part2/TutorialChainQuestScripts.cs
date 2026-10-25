using NexusForever.Game.Abstract.Quest;
using NexusForever.Script.Template;
using NexusForever.Script.Template.Filter;

namespace NexusForever.Script.Main.Tutorial.Script.Part2
{
    [ScriptFilterOwnerId(10525)]
    public class AClaimToStakeExileQuestScript : TutorialQuestScript
    {
        public override QuestId? NextQuestId => QuestId.LotsInStoreExile;
        // Find Phineas T. Rotostar
        protected override uint? FinalObjectiveId => 21382;
    }

    [ScriptFilterOwnerId(10540)]
    public class LotsInStoreExileQuestScript : TutorialQuestScript, IQuestScript
    {
        private const uint SmartShopperObjectiveId = 21371;
        private const uint SmartShopperItemId      = 86245;

        public override QuestId? NextQuestId => QuestId.ServicingCircuitryExile;
        // Continue to the Mission Simulation, the store objectives are optional
        protected override uint? FinalObjectiveId => 21373;

        /// <summary>
        /// Invoked each world tick with the delta since the previous tick occurred.
        /// </summary>
        /// <remarks>
        /// Purchase the "Title: Smart Shopper" item and redeem it to your character inventory.
        /// </remarks>
        public void Update(double lastTick)
        {
            IQuestObjective objective = owner.GetQuestObjective(SmartShopperObjectiveId);
            if (objective == null || objective.IsComplete())
                return;

            if (owner.GetOwner().Inventory.HasItemCount(SmartShopperItemId, 1u))
                owner.ObjectiveUpdate(SmartShopperObjectiveId, 1u);
        }
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

    [ScriptFilterOwnerId(10528)]
    public class BonVoyageExileQuestScript : TutorialQuestScript
    {
        public override QuestId? NextQuestId => null;
        // Travel to Nexus, the alternative routes are optional
        protected override uint? FinalObjectiveId => 21327;
    }
}
