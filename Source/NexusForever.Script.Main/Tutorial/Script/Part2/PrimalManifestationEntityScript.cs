using System.Numerics;
using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Entity.Creature;
using NexusForever.Game.Abstract.Quest;
using NexusForever.Game.Static.Quest;
using NexusForever.Script.Template;
using NexusForever.Script.Template.Filter;

namespace NexusForever.Script.Main.Tutorial.Script.Part2
{
    /// <summary>
    /// Rings the player jumps through in Deconstruct the Construct.
    /// </summary>
    [ScriptFilterCreatureId(73500)]
    public class PrimalManifestationEntityScript : IWorldEntityScript, IOwnedScript<IWorldEntity>
    {
        private const uint RingObjectiveId   = 21294;
        private const uint RingTargetGroupId = 14425;
        private const uint ConstructCreatureId = 73499;

        // centre of the construct objective area (WorldLocation2 51744)
        private static readonly Vector3 ConstructPosition = new(429.29544f, -388.8825f, 253.77565f);

        #region Dependency Injection

        private readonly ICreatureInfoManager creatureInfoManager;
        private IWorldEntity owner;

        public PrimalManifestationEntityScript(
            ICreatureInfoManager creatureInfoManager)
        {
            this.creatureInfoManager = creatureInfoManager;
        }

        #endregion

        /// <summary>
        /// Invoked when <see cref="IScript"/> is loaded.
        /// </summary>
        public void OnLoad(IWorldEntity owner)
        {
            this.owner = owner;
            owner.SetInRangeCheck(4f);
        }

        /// <summary>
        /// Invoked when <see cref="IGridEntity"/> is added to range check range.
        /// </summary>
        public void OnEnterRange(IGridEntity entity)
        {
            if (entity is not IPlayer player)
                return;

            IQuest quest = player.QuestManager.GetActiveQuest(QuestId.DeconstructTheConstructExile);
            if (quest == null)
                return;

            IQuestObjective objective = quest.GetQuestObjective(RingObjectiveId);
            if (objective == null || objective.IsComplete())
                return;

            // each ring has its own checklist slot
            player.QuestManager.ObjectiveUpdate(QuestObjectiveType.ActivateTargetGroupChecklist, RingTargetGroupId, owner.QuestChecklistMask);
            if (!objective.IsComplete())
                return;

            // the construct isn't in the world database, spawn it on the platform once all rings are done
            ICreatureInfo creatureInfo = creatureInfoManager.GetCreatureInfo(ConstructCreatureId);
            if (creatureInfo == null)
                return;

            IWorldEntity construct = player.SummonFactory.Summon(creatureInfo, ConstructPosition, Vector3.Zero);
            TutorialFaction.MakeHostile(construct);
        }
    }
}
