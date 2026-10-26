using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Entity.Creature;
using NexusForever.Script.Template.Filter;

namespace NexusForever.Script.Main.Challenge
{
    /// <summary>
    /// Supply Run: "Return supplies to Camp Dustdevil", from the C47 Supply Crate at the XAS Forward Camp to the camp (challenge indicator).
    /// </summary>
    [ScriptFilterOwnerId(47)]
    public class SupplyRunChallengeScript : ChallengeScript
    {
        public override void Update(double lastTick)
        {
            if (IsAtOrAbove(Challenge.Entry.WorldLocation2IdIndicator, 10f))
                Complete();
        }
    }

    /// <summary>
    /// Whimsical Whimfibers: "Jump through as many flying whimfibers as you can within the time limit".
    /// </summary>
    [ScriptFilterOwnerId(172)]
    public class WhimsicalWhimfibersChallengeScript : TouchChallengeScript
    {
        protected override uint[] CreatureIds { get; } = [9374, 20791, 20870, 20871, 20875];
        protected override double? RecountDelay => 20d;
    }

    /// <summary>
    /// Uplifting Updrafts: "Jump through as many Primal Air Accumulations as you can before time runs out", the jump clouds of quest 4908.
    /// </summary>
    [ScriptFilterOwnerId(191)]
    public class UpliftingUpdraftsChallengeScript : TouchChallengeScript
    {
        protected override uint[] CreatureIds { get; } = [18481];
        protected override double? RecountDelay => 20d;
    }

    /// <summary>
    /// Green Thumb: "Collect healthy vegetables from Clearfield Farm", the Ch185 Galettuce and Galaraddish, the bad ones (21186, 21188) don't count.
    /// </summary>
    [ScriptFilterOwnerId(185)]
    public class GreenThumbChallengeScript : ChallengeScript
    {
        private static readonly uint[] healthyVegetables = [21185, 21187];

        public override void OnEntityActivated(IWorldEntity entity)
        {
            if (healthyVegetables.Contains(entity.CreatureId))
                AddProgress();
        }
    }

    /// <summary>
    /// Mortar Dodger: "Dodge the Crimson Legion mortar cannons for as long as you can", started from the C183 beacon, tiers in seconds.
    /// </summary>
    [ScriptFilterOwnerId(183)]
    public class MortarDodgerChallengeScript : HazardDodgeChallengeScript
    {
        protected override uint MarkerCreatureId => 21251;

        public MortarDodgerChallengeScript(ICreatureInfoManager creatureInfoManager)
            : base(creatureInfoManager)
        {
        }
    }
}
