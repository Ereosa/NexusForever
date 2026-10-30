using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Static.Quest;
using NexusForever.Script.Template;
using NexusForever.Script.Template.Filter;

namespace NexusForever.Script.Main.Quest
{
    /// <summary>
    /// Stasis, Interrupted (Crimson Isle, 8855 and 8856): "Jump through stasis fields to free Dominion Soldiers near the Megatech Shield Generator".
    /// </summary>
    /// <remarks>
    /// The objective has no data, its reward pane target group 6671 lists the Captured Dominion Soldiers (47687, 47688) standing in the Exile
    /// stasis fields. Retail freed a soldier when the player jumped through its field, so passing within 3 m of a captured soldier frees it.
    /// Each soldier only counts once per character.
    /// </remarks>
    [ScriptFilterCreatureId(47687, 47688)]
    public class StasisInterruptedEntityScript : IWorldEntityScript, IOwnedScript<IWorldEntity>
    {
        private const ushort StasisInterruptedA = 8855;
        private const ushort StasisInterruptedB = 8856;
        private const uint CapturedSoldierTargetGroup = 6671;
        private const float StasisFieldRadius = 3f;

        private readonly HashSet<ulong> freedFor = [];

        /// <summary>
        /// Invoked when <see cref="IScript"/> is loaded.
        /// </summary>
        public void OnLoad(IWorldEntity owner)
        {
            owner.SetInRangeCheck(StasisFieldRadius);
        }

        /// <summary>
        /// Invoked when <see cref="IGridEntity"/> is added to range check range.
        /// </summary>
        public void OnEnterRange(IGridEntity entity)
        {
            if (entity is not IPlayer player)
                return;

            if (player.QuestManager.GetActiveQuest(StasisInterruptedA) == null && player.QuestManager.GetActiveQuest(StasisInterruptedB) == null)
                return;

            if (!freedFor.Add(player.CharacterId))
                return;

            player.QuestManager.ObjectiveUpdateRewardPane(QuestObjectiveType.ActivateEntity, CapturedSoldierTargetGroup, 1u);
        }
    }
}
