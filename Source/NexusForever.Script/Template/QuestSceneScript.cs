using System.Numerics;
using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Map.Search;
using NexusForever.Game.Abstract.Quest;
using NexusForever.Game.Static.Quest;

namespace NexusForever.Script.Template
{
    /// <summary>
    /// Plays a quest step's scene while it is the current step, for steps retail scripts played out while the player waited
    /// ("Wait for Draxa Graystone to finish drinking", "Wait for Sergeant Kara's arrival").
    /// </summary>
    /// <remarks>
    /// The step itself completes through the server's generic handling (eg: waiting it out), the scene only shows what happens.
    /// The client data has no scene data, each quest script notes what it is based on.
    /// </remarks>
    public abstract class QuestSceneScript : IQuestScript, IOwnedScript<IQuest>
    {
        /// <summary>
        /// Objectives with a scene, in quest order.
        /// </summary>
        protected abstract IReadOnlyList<uint> SceneObjectiveIds { get; }

        protected IQuest Owner { get; private set; }

        private uint? active;
        private readonly List<(double At, Action Action)> scheduled = [];
        private double elapsed;

        /// <summary>
        /// Invoked when <see cref="IQuestScript"/> is loaded.
        /// </summary>
        public void OnLoad(IQuest owner)
        {
            Owner = owner;
        }

        /// <summary>
        /// Invoked each world tick with the delta since the previous tick occurred.
        /// </summary>
        public void Update(double lastTick)
        {
            elapsed += lastTick;
            foreach ((double At, Action Action) entry in scheduled.Where(s => s.At <= elapsed).ToList())
            {
                scheduled.Remove(entry);
                entry.Action();
            }

            IPlayer player = Owner.GetOwner();
            uint? current = null;
            if (player?.Map != null && !player.IsLoading && Owner.State == QuestState.Accepted)
            {
                foreach (uint objectiveId in SceneObjectiveIds)
                {
                    IQuestObjective objective = Owner.GetQuestObjective(objectiveId);
                    if (objective == null || objective.IsComplete())
                        continue;
                    if (Owner.Any(o => o.Index < objective.Index && !o.ObjectiveInfo.IsOptional() && !o.IsComplete()))
                        continue;

                    current = objectiveId;
                    break;
                }
            }

            if (current == active)
                return;

            if (active != null && player?.Map != null)
                OnSceneEnd(active.Value, player);

            active = current;
            if (active != null)
                OnSceneStart(active.Value, player);
        }

        /// <summary>
        /// Invoked when the objective becomes the current step.
        /// </summary>
        protected abstract void OnSceneStart(uint objectiveId, IPlayer player);

        /// <summary>
        /// Invoked when the objective is complete or no longer the current step.
        /// </summary>
        protected virtual void OnSceneEnd(uint objectiveId, IPlayer player)
        {
        }

        /// <summary>
        /// Run the supplied action after some seconds.
        /// </summary>
        protected void After(double seconds, Action action)
        {
            scheduled.Add((elapsed + seconds, action));
        }

        /// <summary>
        /// Return the nearest creature with the supplied id around the player.
        /// </summary>
        protected static IWorldEntity FindCreature(IPlayer player, uint creatureId, float range = 50f)
        {
            return player.Map?.Search(player.Position, range, new SearchCheckCreature(creatureId))
                .OrderBy(e => Vector3.DistanceSquared(e.Position, player.Position))
                .FirstOrDefault();
        }

        private class SearchCheckCreature : ISearchCheck<IWorldEntity>
        {
            private readonly uint creatureId;

            public SearchCheckCreature(uint creatureId)
            {
                this.creatureId = creatureId;
            }

            public bool CheckEntity(IWorldEntity entity)
            {
                return entity.CreatureId == creatureId;
            }
        }
    }
}
