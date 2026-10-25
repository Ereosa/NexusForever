using NexusForever.Game.Abstract.Entity;
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

            if (SmartShopper.HasRedeemed(owner.GetOwner()))
                owner.ObjectiveUpdate(SmartShopperObjectiveId, 1u);
        }
    }

    /// <summary>
    /// Learning to Shop, the store tutorial outside of the arkship chain.
    /// </summary>
    [ScriptFilterOwnerId(10510)]
    public class LearningToShopQuestScript : IQuestScript, IOwnedScript<IQuest>
    {
        private const uint SmartShopperObjectiveId = 21267;

        private IQuest owner;

        /// <summary>
        /// Invoked when <see cref="IQuestScript"/> is loaded.
        /// </summary>
        public void OnLoad(IQuest owner)
        {
            this.owner = owner;
        }

        /// <summary>
        /// Invoked each world tick with the delta since the previous tick occurred.
        /// </summary>
        /// <remarks>
        /// Purchase "Title: Smart Shopper" with your OmniBit and redeem it to your character inventory.
        /// </remarks>
        public void Update(double lastTick)
        {
            IQuestObjective objective = owner.GetQuestObjective(SmartShopperObjectiveId);
            if (objective == null || objective.IsComplete())
                return;

            if (SmartShopper.HasRedeemed(owner.GetOwner()))
                owner.ObjectiveUpdate(SmartShopperObjectiveId, 1u);
        }
    }

    /// <summary>
    /// The "Title: Smart Shopper" store item the store tutorials ask for.
    /// </summary>
    public static class SmartShopper
    {
        private const uint ItemId  = 86245;
        private const ushort TitleId = 400;

        /// <summary>
        /// Returns if the player has redeemed the item to their inventory, or already used it for the title.
        /// </summary>
        public static bool HasRedeemed(IPlayer player)
        {
            return player.Inventory.HasItemCount(ItemId, 1u) || player.TitleManager.HasTitle(TitleId);
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
