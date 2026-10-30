using System.Numerics;
using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Map.Search;
using NexusForever.Game.Abstract.Quest;
using NexusForever.Game.Static.Entity;
using NexusForever.Game.Static.Quest;
using NexusForever.Script.Template;
using NexusForever.Script.Template.Filter;

namespace NexusForever.Script.Alizar.Algoroc.Quest
{
    /// <summary>
    /// By Leaps and Bounds: "Collect a Pure Loftite Fragment from the Loftite Cliffs by jumping through it", then eight of them.
    /// </summary>
    /// <remarks>
    /// The fragments are the Loftite Crystals (13120, tagged "q3777"; the objective's reward pane target group 7534 lists them, its data
    /// 6952 is a door). The client sends nothing for passing through a crystal, the server checks the distance: each crystal gives one
    /// Pure Loftite Fragment (item 6998) per pass, a crystal can be passed through again after a minute (guessed).
    /// </remarks>
    [ScriptFilterOwnerId(3777)]
    public class ByLeapsAndBoundsQuestScript : IQuestScript, IOwnedScript<IQuest>
    {
        private const uint JumpObjectiveId  = 5076u;
        private const uint CollectObjectiveId = 4859u;
        private const uint LoftiteCrystal  = 13120u;
        private const uint PureLoftiteFragment = 6998u;
        private const float PassDistance   = 3f;
        private const double CrystalCooldown = 60d;

        private IQuest owner;
        private double check;
        private readonly Dictionary<uint, double> passed = [];

        public void OnLoad(IQuest owner)
        {
            this.owner = owner;
        }

        public void Update(double lastTick)
        {
            foreach (uint guid in passed.Keys.ToList())
            {
                passed[guid] -= lastTick;
                if (passed[guid] <= 0d)
                    passed.Remove(guid);
            }

            check -= lastTick;
            if (check > 0d)
                return;
            check = 0.25d;

            IPlayer player = owner.GetOwner();
            if (player?.Map == null || player.IsLoading || owner.State != QuestState.Accepted)
                return;

            IQuestObjective collect = owner.GetQuestObjective(CollectObjectiveId);
            IQuestObjective jump = owner.GetQuestObjective(JumpObjectiveId);
            if ((collect == null || collect.IsComplete()) && (jump == null || jump.IsComplete()))
                return;

            foreach (IWorldEntity crystal in player.Map.Search(player.Position, PassDistance, new SearchCheckCrystal()))
            {
                if (passed.ContainsKey(crystal.Guid))
                    continue;

                passed[crystal.Guid] = CrystalCooldown;
                player.Inventory.ItemCreate(InventoryLocation.Inventory, PureLoftiteFragment, 1u);
                owner.ObjectiveUpdate(JumpObjectiveId, 1u);
            }
        }

        private class SearchCheckCrystal : ISearchCheck<IWorldEntity>
        {
            public bool CheckEntity(IWorldEntity entity)
            {
                return entity.CreatureId == LoftiteCrystal;
            }
        }
    }
}
