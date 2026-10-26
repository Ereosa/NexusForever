using System.Numerics;
using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Map.Search;
using NexusForever.Game.Abstract.Quest;
using NexusForever.Game.Static.Quest;
using NexusForever.Script.Template;
using NexusForever.Script.Template.Filter;

namespace NexusForever.Script.Alizar.Celestion
{
    /// <summary>
    /// Nature's Nightlight: "Collect Fireflies by jumping through them in Celestial Falls".
    /// </summary>
    /// <remarks>
    /// The fireflies are the Fireflies objects (30974, tagged "Q6680 - Fireflies - Quest Target"), each swarm counts once when the player
    /// passes through it. The client sends no event for passing through an object, the server checks the distance.
    /// </remarks>
    [ScriptFilterOwnerId(6680)]
    public class NaturesNightlightQuestScript : IQuestScript, IOwnedScript<IQuest>
    {
        private const uint CollectObjectiveId = 10491u;
        private const uint Fireflies          = 30974u;

        /// <summary>
        /// Distance to a swarm that counts as passing through it.
        /// </summary>
        private const float CollectDistance = 2.5f;

        private IQuest owner;
        private double check;
        private readonly HashSet<uint> collected = [];

        public void OnLoad(IQuest owner)
        {
            this.owner = owner;
        }

        public void Update(double lastTick)
        {
            check -= lastTick;
            if (check > 0d)
                return;
            check = 0.25d;

            IPlayer player = owner.GetOwner();
            IQuestObjective objective = owner.GetQuestObjective(CollectObjectiveId);
            if (player?.Map == null || owner.State != QuestState.Accepted || objective == null || objective.IsComplete())
                return;

            foreach (IWorldEntity swarm in player.Map.Search(player.Position, CollectDistance, new SearchCheckFireflies()))
                if (collected.Add(swarm.Guid))
                    owner.ObjectiveUpdate(CollectObjectiveId, 1u);
        }

        private class SearchCheckFireflies : ISearchCheck<IWorldEntity>
        {
            public bool CheckEntity(IWorldEntity entity)
            {
                return entity.CreatureId == Fireflies;
            }
        }
    }
}
