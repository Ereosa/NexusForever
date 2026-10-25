using NexusForever.Database.Auth;
using NexusForever.GameTable.Model;
using NexusForever.Network.Message;
using NexusForever.Network.World.Message.Model.AccountInventory;

namespace NexusForever.Game.Abstract.Account.Inventory
{
    public interface IAccountItem : IDatabaseAuth, INetworkBuildable<AccountInventoryItem>
    {
        /// <summary>
        /// Unique id of the item in the account inventory, sent to the client as the user inventory id.
        /// </summary>
        ulong Id { get; }

        AccountItemEntry Entry { get; }

        /// <summary>
        /// Mark the <see cref="IAccountItem"/> for deletion.
        /// </summary>
        void Delete();
    }
}
