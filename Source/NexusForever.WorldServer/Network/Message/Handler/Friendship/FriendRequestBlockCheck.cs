using System;
using NexusForever.Database;
using NexusForever.Database.Character;
using NexusForever.Database.Character.Model;
using NexusForever.Game;
using NexusForever.Game.Abstract;
using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Static.Entity;
using NexusForever.Shared.Game.Events;

namespace NexusForever.WorldServer.Network.Message.Handler.Friendship
{
    /// <summary>
    /// Checks the "block friend requests" lock (<see cref="CharacterFlag.FriendBlock"/>, FriendshipLib.SetLock) of a friend request target.
    /// </summary>
    public static class FriendRequestBlockCheck
    {
        /// <summary>
        /// Invoke <paramref name="callback"/> with whether the character blocks friend requests.
        /// </summary>
        /// <remarks>
        /// The flag lives on the character, so an offline target is read from the character database.
        /// </remarks>
        public static void Check(IWorldSession session, IPlayerManager playerManager, IDatabaseManager databaseManager, ulong characterId, Action<bool> callback)
        {
            IPlayer target = playerManager.GetPlayer(new Identity
            {
                Id      = characterId,
                RealmId = RealmContext.Instance.RealmId
            });
            if (target != null)
            {
                callback(target.HasFlag(CharacterFlag.FriendBlock));
                return;
            }

            session.Events.EnqueueEvent(new TaskGenericEvent<CharacterModel>(
                databaseManager.GetDatabase<CharacterDatabase>().GetCharacterById(characterId),
                model => callback(model != null && ((CharacterFlag)model.Flags & CharacterFlag.FriendBlock) != 0)));
        }
    }
}
