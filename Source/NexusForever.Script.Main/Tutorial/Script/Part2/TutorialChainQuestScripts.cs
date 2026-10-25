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

    [ScriptFilterOwnerId(10528)]
    public class BonVoyageExileQuestScript : TutorialQuestScript
    {
        public override QuestId? NextQuestId => null;
        // Travel to Nexus, the alternative routes are optional
        protected override uint? FinalObjectiveId => 21327;
    }
}
