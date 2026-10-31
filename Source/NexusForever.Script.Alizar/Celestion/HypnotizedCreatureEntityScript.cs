using NexusForever.Game.Abstract.Quest;
using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Static.Quest;
using NexusForever.Script.Template;
using NexusForever.Script.Template.Filter;

namespace NexusForever.Script.Alizar.Celestion
{
    /// <summary>
    /// Hypnotized Dawngrazers and Jabbits in Pollenberry Grove (Watcher's Warning, "Use the Horn of Heralding to awaken hypnotized
    /// dawngrazers and jabbits").
    /// </summary>
    /// <remarks>
    /// The Horn of Heralding is a virtual item the quest pushes (836), its Watch Horn Bellow (34573) wakes the creatures around through its
    /// proxy (35377: advance objective 0, despawn), but nothing in the client data lets the player blow it and the client sends nothing
    /// for it (report 136). A player on the quest coming up to a hypnotized creature blows the horn: it wakes up and counts, and is back
    /// a minute later. The range and the respawn time are guessed.
    /// </remarks>
    [ScriptFilterCreatureId(31561, 31562)]
    public class HypnotizedCreatureEntityScript : IWorldEntityScript, IOwnedScript<IWorldEntity>
    {
        private const ushort WatchersWarning = 6629;
        private const uint AwakenObjectiveId = 10304u;
        private const float HornRange = 6f;

        private IWorldEntity owner;
        private bool awake;

        public void OnLoad(IWorldEntity owner)
        {
            this.owner = owner;
            owner.SetInRangeCheck(HornRange);
        }

        public void OnEnterRange(IGridEntity entity)
        {
            if (awake || entity is not IPlayer player)
                return;

            IQuestObjective objective = player.QuestManager.GetActiveQuest(WatchersWarning)?.GetQuestObjective(AwakenObjectiveId);
            if (objective == null || objective.IsComplete())
                return;

            awake = true;
            player.QuestManager.GetActiveQuest(WatchersWarning).ObjectiveUpdate(AwakenObjectiveId, 1u);
            owner.DespawnWithRespawn(TimeSpan.FromMinutes(1d));
        }
    }
}
