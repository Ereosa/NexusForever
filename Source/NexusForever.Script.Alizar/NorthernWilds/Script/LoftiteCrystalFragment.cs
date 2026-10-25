using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Quest;

namespace NexusForever.Script.Alizar.NorthernWilds.Script
{
    /// <summary>
    /// Empowered Tower: "Collect Loftite Crystal Fragments outside Exo-Lab 729 by jumping into them or by destroying Crystal Guardians".
    /// </summary>
    public static class LoftiteCrystalFragment
    {
        public const ushort EmpoweredTowerQuestId  = 3486;
        public const uint   FragmentObjectiveId    = 4485;

        /// <summary>
        /// Give the supplied <see cref="IPlayer"/> a fragment if they are collecting them.
        /// </summary>
        public static void Grant(IPlayer player)
        {
            IQuest quest = player.QuestManager.GetActiveQuest(EmpoweredTowerQuestId);
            if (quest == null)
                return;

            IQuestObjective objective = quest.GetQuestObjective(FragmentObjectiveId);
            if (objective == null || objective.IsComplete())
                return;

            quest.ObjectiveUpdate(FragmentObjectiveId, 1u);
        }
    }
}
