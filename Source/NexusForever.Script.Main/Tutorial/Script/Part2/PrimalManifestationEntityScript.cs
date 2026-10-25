using System.Numerics;
using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Entity.Creature;
using NexusForever.Game.Abstract.Quest;
using NexusForever.Game.Abstract.Spell;
using NexusForever.Game.Static.Quest;
using NexusForever.Script.Template;
using NexusForever.Script.Template.Filter;
using NexusForever.Shared;

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

        // Primal Power jump buffs, in the order the rings are reached coming from Dorian (east to west, each higher than the last)
        private static readonly (Vector3 Position, uint Spell4Id)[] jumpBuffs =
        [
            (new Vector3(508.627f,   -429.185f,  293.897f),   85708),
            (new Vector3(482.71597f, -420.95218f, 296.00015f), 86888),
            (new Vector3(464.81094f, -410.57098f, 280.37668f), 86889)
        ];

        // centre of the construct objective area (WorldLocation2 51744)
        private static readonly Vector3 ConstructPosition = new(429.29544f, -388.8825f, 253.77565f);

        #region Dependency Injection

        private readonly ICreatureInfoManager creatureInfoManager;
        private readonly IFactory<ISpellParameters> spellParameterFactory;
        private IWorldEntity owner;

        public PrimalManifestationEntityScript(
            ICreatureInfoManager creatureInfoManager,
            IFactory<ISpellParameters> spellParameterFactory)
        {
            this.creatureInfoManager   = creatureInfoManager;
            this.spellParameterFactory = spellParameterFactory;
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

            // each ring boosts the player's jump so they can reach the next one
            ISpellParameters parameters = spellParameterFactory.Resolve();
            parameters.UserInitiatedSpellCast = false;
            parameters.PrimaryTargetId        = player.Guid;
            player.CastSpell(jumpBuffs.MinBy(b => Vector3.Distance(b.Position, owner.Position)).Spell4Id, parameters);

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
