using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Quest;
using NexusForever.Game.Static.Entity;
using NexusForever.Game.Static.Quest;
using NexusForever.Script.Template;
using NexusForever.Script.Template.Filter;

namespace NexusForever.Script.Alizar.Algoroc.Quest
{
    /// <summary>
    /// Fiery Vengeance: "Use incendiary grenades to burn slaver shacks in the Cartel's Slave Camp".
    /// </summary>
    /// <remarks>
    /// The quest is given by the Arms Cache (17402) but nothing in the client data hands over the Incendiary Grenade (12444, casts 26663
    /// which activates the shack it is thrown at, report 116). The player gets a grenade for each shack still standing when the quest is
    /// taken, and another one whenever they run out before all three burn (guessed).
    /// </remarks>
    [ScriptFilterOwnerId(4743)]
    public class FieryVengeanceQuestScript : IQuestScript, IOwnedScript<IQuest>
    {
        private const uint BurnObjectiveId     = 6553u;
        private const uint IncendiaryGrenade   = 12444u;

        private IQuest owner;
        private double check;

        public void OnLoad(IQuest owner)
        {
            this.owner = owner;
        }

        public void Update(double lastTick)
        {
            check -= lastTick;
            if (check > 0d)
                return;
            check = 2d;

            IPlayer player = owner.GetOwner();
            if (player?.Map == null || player.IsLoading || owner.State != QuestState.Accepted)
                return;

            IQuestObjective burn = owner.GetQuestObjective(BurnObjectiveId);
            if (burn == null || burn.IsComplete() || player.Inventory.GetItemCount(IncendiaryGrenade) > 0u)
                return;

            // one grenade per shack still standing
            uint standing = burn.ObjectiveInfo.Entry.Count - (uint)System.Numerics.BitOperations.PopCount(burn.Progress);
            player.Inventory.ItemCreate(InventoryLocation.Inventory, IncendiaryGrenade, Math.Max(1u, standing));
        }
    }
}
