using System.Numerics;
using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Quest;
using NexusForever.Game.Static.Quest;
using NexusForever.GameTable;
using NexusForever.GameTable.Model;
using NexusForever.Script.Template;
using NexusForever.Script.Template.Filter;

namespace NexusForever.Script.Alizar.Celestion
{
    /// <summary>
    /// Catch and Release: catch Singed critters in Emberwood, then release them on grassy or unburned ground (report 85).
    /// </summary>
    /// <remarks>
    /// Each catch hands over the hidden Saved Critter virtual item (906), which counts the critters carried. The catch objective is a
    /// checklist of the critters' indexes, many critters share one index, so catches after the first often didn't count. The release
    /// is the Releasing Critters ability (34448), which the client never offers on the quest tracker.
    /// The catch checklist now follows the number of critters caught, and carried critters are released when the player reaches the
    /// unburned ground: Greenbough Glen (zone 2518) or the release objective's markers. Releasing on arrival is a stand-in for the ability.
    /// </remarks>
    [ScriptFilterOwnerId(6674)]
    public class CatchAndReleaseQuestScript : IQuestScript, IOwnedScript<IQuest>
    {
        private const uint CatchObjectiveId   = 10345u;
        private const uint ReleaseObjectiveId = 10349u;
        private const uint CarriedObjectiveId = 10350u;

        /// <summary>
        /// Greenbough Glen, the unburned ground next to Emberwood.
        /// </summary>
        private const uint SafeZoneId = 2518u;

        private IQuest owner;
        private double check;
        private List<WorldLocation2Entry> releaseLocations;

        public void OnLoad(IQuest owner)
        {
            this.owner = owner;
        }

        public void Update(double lastTick)
        {
            check -= lastTick;
            if (check > 0d)
                return;
            check = 1d;

            IPlayer player = owner.GetOwner();
            if (player?.Map == null || player.IsLoading || owner.State != QuestState.Accepted)
                return;

            IQuestObjective catchObjective   = owner.GetQuestObjective(CatchObjectiveId);
            IQuestObjective releaseObjective = owner.GetQuestObjective(ReleaseObjectiveId);
            IQuestObjective carried          = owner.GetQuestObjective(CarriedObjectiveId);
            if (catchObjective == null || releaseObjective == null || carried == null)
                return;

            uint caught = carried.Progress;
            SyncCatches(catchObjective, caught);

            if (releaseObjective.IsComplete() || caught <= releaseObjective.Progress || !IsOnSafeGround(player, releaseObjective))
                return;

            uint release = Math.Min(caught, releaseObjective.ObjectiveInfo.Entry.Count) - releaseObjective.Progress;
            if (release > 0u)
                owner.ObjectiveUpdate(ReleaseObjectiveId, release);
        }

        /// <summary>
        /// Set one checklist bit per critter caught, the critters' own indexes repeat.
        /// </summary>
        private void SyncCatches(IQuestObjective catchObjective, uint caught)
        {
            uint needed = Math.Min(caught, catchObjective.ObjectiveInfo.Entry.Count);
            uint progress = catchObjective.Progress;
            uint add = 0u;
            for (int bit = 0; bit < 32 && System.Numerics.BitOperations.PopCount(progress | add) < needed; bit++)
                if ((progress & (1u << bit)) == 0u)
                    add |= 1u << bit;

            if (add != 0u)
                owner.ObjectiveUpdate(CatchObjectiveId, add);
        }

        private bool IsOnSafeGround(IPlayer player, IQuestObjective releaseObjective)
        {
            for (WorldZoneEntry zone = player.Zone; zone != null; zone = zone.ParentZoneId != 0u ? GameTableManager.Instance.WorldZone.GetEntry(zone.ParentZoneId) : null)
                if (zone.Id == SafeZoneId)
                    return true;

            releaseLocations ??= releaseObjective.ObjectiveInfo.GetAreaLocations().ToList();
            return releaseLocations.Any(l => l.WorldId == player.Map.Entry.Id
                && Vector3.Distance(player.Position, new Vector3(l.Position0, l.Position1, l.Position2)) <= MathF.Max(l.Radius, 10f));
        }
    }
}
