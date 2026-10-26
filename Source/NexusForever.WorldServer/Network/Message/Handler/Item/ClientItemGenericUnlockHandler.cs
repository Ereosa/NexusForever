using System.Collections.Generic;
using System.Linq;
using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Prerequisite;
using NexusForever.Game.Static.GenericUnlock;
using NexusForever.GameTable;
using NexusForever.GameTable.Model;
using NexusForever.Network;
using NexusForever.Network.Message;
using NexusForever.Network.World.Message.Model.GenericUnlock;

namespace NexusForever.WorldServer.Network.Message.Handler.Item
{
    public class ClientItemGenericUnlockHandler : IMessageHandler<IWorldSession, ClientItemGenericUnlock>
    {
        #region Dependency Injection

        private readonly IGameTableManager gameTableManager;
        private readonly IPrerequisiteManager prerequisiteManager;

        public ClientItemGenericUnlockHandler(
            IGameTableManager gameTableManager,
            IPrerequisiteManager prerequisiteManager)
        {
            this.gameTableManager    = gameTableManager;
            this.prerequisiteManager = prerequisiteManager;
        }

        #endregion

        public void HandleMessage(IWorldSession session, ClientItemGenericUnlock itemGenericUnlock)
        {
            IItem item = session.Player.Inventory.GetItem(itemGenericUnlock.Location);
            if (item == null)
                throw new InvalidPacketValueException();

            // items reference a generic unlock set, which holds up to 6 generic unlock entries (eg: a dye)
            GenericUnlockSetEntry set = gameTableManager.GenericUnlockSet.GetEntry(item.Info.Entry.GenericUnlockSetId);
            if (set == null)
                throw new InvalidPacketValueException();

            List<GenericUnlockEntryEntry> entries = new uint[]
                {
                    set.GenericUnlockEntryId00, set.GenericUnlockEntryId01, set.GenericUnlockEntryId02,
                    set.GenericUnlockEntryId03, set.GenericUnlockEntryId04, set.GenericUnlockEntryId05
                }
                .Where(id => id != 0u)
                .Select(id => gameTableManager.GenericUnlockEntry.GetEntry(id))
                .Where(e => e != null)
                .ToList();

            List<GenericUnlockEntryEntry> locked = entries
                .Where(e => !session.Account.GenericUnlockManager.IsUnlocked(e.GenericUnlockTypeEnum, e.UnlockObject))
                .ToList();
            if (locked.Count == 0)
            {
                // keep the item, everything it unlocks is already known
                session.Account.GenericUnlockManager.SendUnlockResult(GenericUnlockResult.AlreadyUnlocked);
                return;
            }

            if (item.Info.Entry.PrerequisiteId != 0u && !prerequisiteManager.Meets(session.Player, item.Info.Entry.PrerequisiteId))
            {
                session.Account.GenericUnlockManager.SendUnlockResult(GenericUnlockResult.Invalid);
                return;
            }

            if (!session.Player.Inventory.ItemUse(item))
                return;

            foreach (GenericUnlockEntryEntry entry in locked)
                session.Account.GenericUnlockManager.Unlock((ushort)entry.Id);
        }
    }
}
