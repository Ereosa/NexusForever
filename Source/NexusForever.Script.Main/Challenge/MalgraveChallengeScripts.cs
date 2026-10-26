using System.Numerics;
using NexusForever.Game.Abstract.Entity;
using NexusForever.GameTable;
using NexusForever.GameTable.Model;
using NexusForever.Script.Template.Filter;

namespace NexusForever.Script.Main.Challenge
{
    /// <summary>
    /// One Small Hop (Malgrave): "Reach the hideout before time runs out", hopping up from the lowest platform of Dead Man's Gulch.
    /// </summary>
    /// <remarks>
    /// The indicator (27806) covers the whole gulch, start platform included, the hideout counts as its centre.
    /// </remarks>
    [ScriptFilterOwnerId(559)]
    public class OneSmallHopChallengeScript : ChallengeScript
    {
        private const float HideoutRange  = 10f;
        private const float HideoutHeight = 5f;

        public override void Update(double lastTick)
        {
            WorldLocation2Entry hideout = GameTableManager.Instance.WorldLocation2.GetEntry(Challenge.Entry.WorldLocation2IdIndicator);
            if (hideout == null)
                return;

            var centre = new Vector3(hideout.Position0, hideout.Position1, hideout.Position2);
            if (HorizontalDistance(Player.Position, centre) <= HideoutRange && MathF.Abs(Player.Position.Y - centre.Y) <= HideoutHeight)
                Complete();
        }
    }

    /// <summary>
    /// Roan Skulls (Malgrave): "Collect Roan Skulls before time runs out" around Flamedancer Village.
    /// </summary>
    /// <remarks>
    /// The challenge target (target group 750) is a Magnos databank placeholder, the Roan Skulls (36293) are what's collected.
    /// </remarks>
    [ScriptFilterOwnerId(576)]
    public class RoanSkullsChallengeScript : ChallengeScript
    {
        private const uint SkullCreatureId = 36293;

        public override void OnEntityActivated(IWorldEntity entity)
        {
            if (entity.CreatureId == SkullCreatureId)
                AddProgress();
        }
    }

    /// <summary>
    /// Every Hellrose Has Its Thorn (Malgrave): "Prune Hellroses before time runs out" in Fiera Grove, each Malgrave Hellrose pruned counts.
    /// </summary>
    [ScriptFilterOwnerId(585)]
    public class EveryHellroseHasItsThornChallengeScript : ChallengeScript
    {
        private const uint HellroseCreatureId = 36571;

        public override void OnEntityActivated(IWorldEntity entity)
        {
            if (entity.CreatureId == HellroseCreatureId)
                AddProgress();
        }
    }
}
