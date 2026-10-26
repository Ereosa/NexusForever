using System.Collections.Immutable;
using NexusForever.Game.Static.Quest;

namespace NexusForever.Game.Quest
{
    public enum QuestSupportLevel
    {
        Unsupported,
        Partial,
        Supported
    }

    /// <summary>
    /// Which <see cref="QuestObjectiveType"/>'s the server has an event hook for.
    /// </summary>
    /// <remarks>
    /// Keep this in sync with the <see cref="Abstract.Entity.IQuestManager.ObjectiveUpdate(QuestObjectiveType, uint, uint)"/> callers,
    /// it is used for the runtime warnings on quest accept and by the QuestAudit tool.
    /// </remarks>
    public static class QuestObjectiveSupport
    {
        public static ImmutableDictionary<QuestObjectiveType, (QuestSupportLevel Level, string Hook)> Types { get; } =
            new Dictionary<QuestObjectiveType, (QuestSupportLevel, string)>
            {
                [QuestObjectiveType.KillCreature]                 = (QuestSupportLevel.Supported,   "UnitEntity.RewardKiller"),
                [QuestObjectiveType.TalkTo]                       = (QuestSupportLevel.Supported,   "ClientEntityInteractionHandler"),
                [QuestObjectiveType.CollectItem]                  = (QuestSupportLevel.Supported,   "Inventory item add, existing items on accept"),
                [QuestObjectiveType.ActivateEntity]               = (QuestSupportLevel.Supported,   "WorldEntity.OnActivateSuccess"),
                [QuestObjectiveType.KillTargetGroups]             = (QuestSupportLevel.Supported,   "UnitEntity.RewardKiller (target groups)"),
                [QuestObjectiveType.ActivateEntity2]              = (QuestSupportLevel.Supported,   "WorldEntity.OnActivateSuccess"),
                [QuestObjectiveType.Unknown10]                    = (QuestSupportLevel.Partial,     "WorldEntity.OnActivateSuccess (target group activations only)"),
                [QuestObjectiveType.SpellSuccess]                 = (QuestSupportLevel.Supported,   "Spell.UpdateQuestObjectives (cast)"),
                [QuestObjectiveType.SucceedCSI]                   = (QuestSupportLevel.Supported,   "WorldEntity.OnActivateSuccess"),
                [QuestObjectiveType.SpellSuccess2]                = (QuestSupportLevel.Supported,   "Spell.UpdateQuestObjectives (per target)"),
                [QuestObjectiveType.ActivateTargetGroupChecklist] = (QuestSupportLevel.Supported,   "WorldEntity.OnActivateSuccess (checklist)"),
                [QuestObjectiveType.KillTargetGroup]              = (QuestSupportLevel.Supported,   "UnitEntity.RewardKiller (target groups)"),
                [QuestObjectiveType.EnterZone]                    = (QuestSupportLevel.Supported,   "Player zone update"),
                [QuestObjectiveType.TalkToTargetGroup]            = (QuestSupportLevel.Supported,   "ClientEntityInteractionHandler (checklist)"),
                [QuestObjectiveType.Unknown20]                    = (QuestSupportLevel.Partial,     "CraftingManager (data compared to schematic id)"),
                [QuestObjectiveType.GatheResource]                = (QuestSupportLevel.Supported,   "HarvestUnitEntity"),
                [QuestObjectiveType.EnterArea]                    = (QuestSupportLevel.Supported,   "QuestManager.OnRelocate (objective indicator locations, trigger volumes aren't in the client data)"),
                [QuestObjectiveType.ActivateTargetGroup]          = (QuestSupportLevel.Supported,   "WorldEntity.OnActivateSuccess (checklist)"),
                [QuestObjectiveType.CompleteQuest]                = (QuestSupportLevel.Supported,   "QuestManager.QuestComplete"),
                [QuestObjectiveType.CompleteEvent]                = (QuestSupportLevel.Partial,     "PathSoldierManager.StartQuestHoldout (data = soldier holdout event, spawns from the world database), PublicEvent finish, wave scripts"),
                [QuestObjectiveType.Unknown28]                    = (QuestSupportLevel.Supported,   "WorldEntity.OnActivateSuccess (Creature2 QuestAnimStateId)"),
                [QuestObjectiveType.VirtualCollect]               = (QuestSupportLevel.Supported,   "loot, kills and activations (TargetGroupIdRewardPane)"),
                [QuestObjectiveType.CraftSchematic]               = (QuestSupportLevel.Supported,   "CraftingManager"),
                [QuestObjectiveType.SpellSuccess4]                = (QuestSupportLevel.Supported,   "Spell.UpdateQuestObjectives (per target)"),
                [QuestObjectiveType.LearnTradeskill]              = (QuestSupportLevel.Supported,   "TradeskillManager.LearnTradeskill"),
                [QuestObjectiveType.ObtainSchematic]              = (QuestSupportLevel.Supported,   "TradeskillManager schematic learn"),
                [QuestObjectiveType.Unknown31]                    = (QuestSupportLevel.Partial,     "CraftingManager (work order: any craft of the quest's TradeSkillProfession tier, inferred)"),
                [QuestObjectiveType.KillCreature2]                = (QuestSupportLevel.Supported,   "UnitEntity.RewardKiller (creature difficulty or higher)"),
                [QuestObjectiveType.Unknown15]                    = (QuestSupportLevel.Supported,   "UnitEntity.RewardKiller (target groups)"),
                [QuestObjectiveType.SpellSuccess3]                = (QuestSupportLevel.Supported,   "Spell.UpdateQuestObjectives (killing blow, or each target of a spell without damage)"),
                [QuestObjectiveType.CompleteChallenge]            = (QuestSupportLevel.Supported,   "ChallengeManager.Finish (any challenge)"),
            }.ToImmutableDictionary();

        /// <summary>
        /// Events that evaluate communicator messages, a message with no quest state condition only arrives through the others.
        /// </summary>
        public static ImmutableDictionary<string, QuestSupportLevel> CommunicatorTriggers { get; } =
            new Dictionary<string, QuestSupportLevel>
            {
                ["QuestState"]   = QuestSupportLevel.Supported,
                ["Level"]        = QuestSupportLevel.Supported,
                ["Zone"]         = QuestSupportLevel.Supported,
                ["World"]        = QuestSupportLevel.Supported,
                ["Prerequisite"] = QuestSupportLevel.Supported,
                ["None"]         = QuestSupportLevel.Unsupported,
            }.ToImmutableDictionary();

        /// <summary>
        /// Return how well the server supports the supplied <see cref="QuestObjectiveType"/>.
        /// </summary>
        public static QuestSupportLevel GetLevel(QuestObjectiveType type)
        {
            return Types.TryGetValue(type, out var support) ? support.Level : QuestSupportLevel.Unsupported;
        }
    }
}
