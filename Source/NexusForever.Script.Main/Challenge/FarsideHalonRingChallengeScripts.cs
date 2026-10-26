using System.Numerics;
using NexusForever.Game.Abstract.Entity;
using NexusForever.GameTable;
using NexusForever.GameTable.Model;
using NexusForever.Script.Template.Filter;

namespace NexusForever.Script.Main.Challenge
{
    /// <summary>
    /// Hot Feet (Farside): "Stamp out the Fire Motes along the Alpha Complex hallway to Bio-Dome 4", each Fire Mote stamped out counts.
    /// Exile (952) and Dominion (954) versions have their own motes.
    /// </summary>
    [ScriptFilterOwnerId(952, 954)]
    public class HotFeetChallengeScript : ChallengeScript
    {
        private static readonly uint[] fireMoteCreatureIds = [45263, 45271];

        public override void OnEntityActivated(IWorldEntity entity)
        {
            if (fireMoteCreatureIds.Contains(entity.CreatureId))
                AddProgress();
        }
    }

    /// <summary>
    /// Races whose finish is somewhere inside a large indicator area, the finish counts as its centre.
    /// </summary>
    public abstract class RaceToAreaChallengeScript : ChallengeScript
    {
        private const float FinishRange = 25f;

        public override void Update(double lastTick)
        {
            WorldLocation2Entry finish = GameTableManager.Instance.WorldLocation2.GetEntry(Challenge.Entry.WorldLocation2IdIndicator);
            if (finish == null || Player.Map?.Entry.Id != finish.WorldId)
                return;

            if (HorizontalDistance(Player.Position, new Vector3(finish.Position0, finish.Position1, finish.Position2)) <= MathF.Min(finish.Radius, FinishRange))
                Complete();
        }
    }

    /// <summary>
    /// Faster and Fearless (Halon Ring): "Complete the Upper Crust speeder race with time to spare!"
    /// </summary>
    [ScriptFilterOwnerId(671)]
    public class FasterAndFearlessChallengeScript : RaceToAreaChallengeScript
    {
    }

    /// <summary>
    /// Dockside Delivery! (Halon Ring): "Race back to the Port of Profit's docks".
    /// </summary>
    [ScriptFilterOwnerId(691)]
    public class DocksideDeliveryChallengeScript : RaceToAreaChallengeScript
    {
    }
}
