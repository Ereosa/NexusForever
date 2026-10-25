using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Quest;
using NexusForever.Game.Prerequisite;
using NexusForever.Game.Static.Entity;
using NexusForever.Game.Static.Quest;
using NexusForever.Game.Static.Reputation;
using NexusForever.GameTable;
using NexusForever.GameTable.Model;
using NexusForever.Network.Session;
using NexusForever.Network.World.Message.Model.Story;

namespace NexusForever.Game.Quest
{
    public class CommunicatorMessage : ICommunicatorMessage
    {
        public uint Id => entry.Id;
        public ushort QuestId => (ushort)entry.QuestIdDelivered;

        private readonly CommunicatorMessagesEntry entry;

        /// <summary>
        /// Create a new <see cref="ICommunicatorMessage"/> with supplied <see cref="CommunicatorMessagesEntry"/>.
        /// </summary>
        public CommunicatorMessage(CommunicatorMessagesEntry entry)
        {
            this.entry = entry;
        }

        /// <summary>
        /// Checks if <see cref="IPlayer"/> meets the required conditions for this quest to be added to their communicator.
        /// </summary>
        public bool Meets(IPlayer player, bool checkLocation = true)
        {
            if (checkLocation)
            {
                if (entry.WorldId != 0u && entry.WorldId != player.Map?.Entry.Id)
                    return false;

                // the message is sent while in the zone or one of its sub zones
                if (entry.WorldZoneId != 0u && !IsInZone(player, entry.WorldZoneId))
                    return false;
            }

            if (entry.MinLevel != 0u && player.Level < entry.MinLevel)
                return false;

            if (entry.MaxLevel != 0u && player.Level > entry.MaxLevel)
                return false;

            for (int i = 0; i < entry.Quests.Length; i++)
            {
                ushort questId = (ushort)entry.Quests[i];
                if (questId == 0)
                    continue;

                if (!MeetsQuestState(player.QuestManager.GetQuestState(questId) ?? QuestState.Unknown, entry.States[i]))
                    return false;
            }

            if (entry.FactionId != 0u && (Faction)entry.FactionId != player.Faction1)
                return false;

            if (entry.ClassId != 0u && (Class)entry.ClassId != player.Class)
                return false;

            if (entry.RaceId != 0u && (Race)entry.RaceId != player.Race)
                return false;

            if (entry.FactionIdReputation != 0u)
            {
                float reputation = player.ReputationManager.GetReputation((Faction)entry.FactionIdReputation)?.Amount ?? 0f;
                if (reputation < entry.ReputationMin)
                    return false;
                if (entry.ReputationMax != 0u && reputation > entry.ReputationMax)
                    return false;
            }

            if (entry.PrerequisiteId != 0u && !PrerequisiteManager.Instance.Meets(player, entry.PrerequisiteId))
                return false;

            return true;
        }

        /// <summary>
        /// Returns if the message has any condition that decides when it is sent.
        /// </summary>
        public bool HasConditions()
        {
            return entry.WorldId != 0u
                || entry.WorldZoneId != 0u
                || entry.MinLevel != 0u
                || entry.MaxLevel != 0u
                || entry.PrerequisiteId != 0u
                || entry.Quests.Any(q => q != 0u);
        }

        /// <remarks>
        /// States above <see cref="QuestState.Ignored"/> have bit 8 set, they are inferred to be the negated state (eg: 11 is any state but completed),
        /// they are used on messages that call the player back to a quest they haven't done yet.
        /// </remarks>
        private static bool MeetsQuestState(QuestState state, uint required)
        {
            if ((required & 8u) != 0u)
                return state != (QuestState)(required & 7u);

            return state == (QuestState)required;
        }

        private static bool IsInZone(IPlayer player, uint zoneId)
        {
            for (WorldZoneEntry zone = player.Zone; zone != null; zone = zone.ParentZoneId != 0u ? GameTableManager.Instance.WorldZone.GetEntry(zone.ParentZoneId) : null)
                if (zone.Id == zoneId)
                    return true;

            return false;
        }

        /// <summary>
        /// Send communicator message to <see cref="IGameSession"/>.
        /// </summary>
        public void Send(IGameSession session)
        {
            session.EnqueueMessageEncrypted(new ServerCommunicatorMessage
            {
                CommunicatorMessagesId = (ushort)entry.Id
            });
        }
    }
}
