using System.Numerics;
using System.Text.RegularExpressions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NexusForever.Database.Character;
using NexusForever.Database.Character.Model;
using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.PublicEvent;
using NexusForever.Game.Abstract.Quest;
using NexusForever.Game.Static.Entity.Movement.Spline;
using NexusForever.Game.Abstract.Map;
using NexusForever.Game.Abstract.Matching;
using NexusForever.Game.Abstract.Entity.Creature;
using NexusForever.Game.Prerequisite;
using NexusForever.Game.Quest;
using NexusForever.Game.Static;
using NexusForever.Game.Static.Entity;
using MatchType = NexusForever.Game.Static.Matching.MatchType;
using NexusForever.Game.Static.PublicEvent;
using NexusForever.Game.Static.Quest;
using NexusForever.Game.Static.Reputation;
using NexusForever.GameTable;
using NexusForever.GameTable.Model;
using NexusForever.Network.World.Message.Model;
using NexusForever.Network.World.Message.Static;
using NexusForever.Shared;
using NLog;

namespace NexusForever.Game.Entity
{
    public class QuestManager : IQuestManager
    {
        [Flags]
        private enum GetQuestFlags
        {
            Completed = 0x01,
            Inactive  = 0x02,
            Active    = 0x04,
            All       = Completed | Inactive | Active
        }

        private IPlayer player;

        private readonly Dictionary<ushort, IQuest> completedQuests = [];
        private readonly Dictionary<ushort, IQuest> inactiveQuests = [];
        private readonly Dictionary<ushort, IQuest> activeQuests = [];

        #region Dependency Injection

        private readonly ILogger<QuestManager> log;
        private readonly IFactory<IQuest> questFactory;

        public QuestManager(
            ILogger<QuestManager> log,
            IFactory<IQuest> questFactory)
        {
            this.log          = log;
            this.questFactory = questFactory;
        }

        #endregion

        /// <summary>
        /// Initialise a new <see cref="IQuestManager"/> from existing <see cref="CharacterModel"/> database model.
        /// </summary>
        public void Initialise(IPlayer owner, CharacterModel model)
        {
            if (player != null)
                throw new InvalidOperationException("QuestManager has already been initialised!");

            player = owner;

            foreach (CharacterQuestModel questModel in model.Quest)
            {
                IQuestInfo info = GlobalQuestManager.Instance.GetQuestInfo(questModel.QuestId);
                if (info == null)
                {
                    log.LogError($"Player {player.CharacterId} has an invalid quest {questModel.QuestId}!");
                    continue;
                }

                IQuest quest = questFactory.Resolve();
                quest.Initialise(player, info, questModel);

                switch (quest.State)
                {
                    case QuestState.Completed:
                        completedQuests.Add(quest.Id, quest);
                        break;
                    case QuestState.Botched:
                    case QuestState.Ignored:
                    case QuestState.Mentioned:
                        inactiveQuests.Add(quest.Id, quest);
                        break;
                    case QuestState.Accepted:
                    case QuestState.Achieved:
                        activeQuests.Add(quest.Id, quest);
                        break;
                }
            }
        }

        public void Dispose()
        {
            foreach (IQuest quest in completedQuests.Values
                .Concat(inactiveQuests.Values)
                .Concat(activeQuests.Values))
                quest.Dispose();
        }

        public void Save(CharacterContext context)
        {
            foreach (IQuest quest in completedQuests.Values)
                quest.Save(context);

            foreach (IQuest quest in inactiveQuests.Values.ToList())
            {
                if (quest.PendingDelete)
                    inactiveQuests.Remove(quest.Id);

                quest.Save(context);
            }

            foreach (IQuest quest in activeQuests.Values.ToList())
            {
                if (quest.PendingDelete)
                    activeQuests.Remove(quest.Id);

                quest.Save(context);
            }
        }

        /// <summary>
        /// Invoked each world tick with the delta since the previous tick occurred.
        /// </summary>
        public void Update(double lastTick)
        {
            if (publicEventCheckCooldown > 0d)
                publicEventCheckCooldown -= lastTick;

            UpdateWaitObjectives(lastTick);
            UpdateQuestHoldouts(lastTick);

            var botchedQuests = new List<IQuest>();
            foreach (IQuest quest in activeQuests.Values)
            {
                quest.Update(lastTick);
                if (quest.State == QuestState.Botched)
                    botchedQuests.Add(quest);
            }

            foreach (IQuest quest in botchedQuests)
            {
                activeQuests.Remove(quest.Id);
                inactiveQuests.Add(quest.Id, quest);

                log.LogTrace($"Failed to complete quest {quest.Id} before the timer expired!");
            }
        }

        /// <summary>
        /// Distance to a holdout objective's marker that starts the holdout.
        /// </summary>
        private const float QuestHoldoutStartDistance = 25f;

        /// <summary>
        /// Seconds before a failed quest holdout can start again.
        /// </summary>
        private const double QuestHoldoutRetryDelay = 15d;

        private double questHoldoutCheck;
        private double questHoldoutRetry;
        private bool questHoldoutRunning;

        /// <summary>
        /// Start the soldier holdout of a type 25 (CompleteEvent) objective once it is the current step and the player is at its marker.
        /// </summary>
        /// <remarks>
        /// The objective data is the PathSoldierEvent id, the holdout waves come from the world database (path_soldier_holdout_spawn).
        /// Objectives without holdout spawns are left to quest scripts.
        /// </remarks>
        private void UpdateQuestHoldouts(double lastTick)
        {
            if (questHoldoutRetry > 0d)
                questHoldoutRetry -= lastTick;

            // a holdout that ended (failed, or the step completed) can't start again right away
            if (questHoldoutRunning && !player.PathSoldierManager.InHoldout)
            {
                questHoldoutRunning = false;
                questHoldoutRetry   = QuestHoldoutRetryDelay;
            }

            questHoldoutCheck -= lastTick;
            if (questHoldoutCheck > 0d)
                return;
            questHoldoutCheck = 0.5d;

            if (player.Map == null || player.IsLoading || !player.IsAlive || player.PathSoldierManager.InHoldout)
                return;

            if (questHoldoutRetry > 0d)
                return;

            foreach (IQuest quest in activeQuests.Values.Where(q => q.State == QuestState.Accepted).ToList())
            {
                foreach (IQuestObjective objective in quest.Where(o => o.ObjectiveInfo.Type == QuestObjectiveType.CompleteEvent && !o.IsComplete()).ToList())
                {
                    uint eventId = objective.ObjectiveInfo.Entry.Data;
                    if (AssetManager.Instance.GetHoldoutSpawns(eventId).Count == 0)
                        continue;

                    if (quest.Any(o => o.Index < objective.Index && !o.ObjectiveInfo.IsOptional() && !o.IsComplete()))
                        continue;

                    // at the marker, a large marked area is defended where the player stands
                    Vector3? position = null;
                    var locations = objective.ObjectiveInfo.GetAreaLocations().ToList();
                    if (locations.Count == 0)
                        position = player.Position;
                    foreach (WorldLocation2Entry location in locations.Where(l => l.WorldId == player.Map.Entry.Id))
                    {
                        var marker = new Vector3(location.Position0, location.Position1, location.Position2);
                        if (Vector3.Distance(player.Position, marker) > Math.Max(QuestHoldoutStartDistance, location.Radius))
                            continue;
                        position = location.Radius > QuestHoldoutStartDistance ? player.Position : marker;
                        break;
                    }

                    if (position == null)
                        continue;

                    questHoldoutRunning = player.PathSoldierManager.StartQuestHoldout(quest.Id, objective.ObjectiveInfo.Id, eventId, position.Value);
                    return;
                }
            }
        }

        /// <summary>
        /// Distance to a wait objective's marker the player has to stay within.
        /// </summary>
        private const float WaitObjectiveDistance = 30f;

        private readonly Dictionary<uint /*objective id*/, double> waitObjectiveElapsed = [];

        /// <summary>
        /// Complete target-less "Wait for ...", "Observe ...", "Remain in ..." objectives once the player waited them out at their place.
        /// </summary>
        /// <remarks>
        /// Retail scripts played something out meanwhile (an NPC drinking until he collapses, a test running), the client data only has
        /// the text. The wait starts once the objective is the current step and restarts if the player leaves its marker.
        /// </remarks>
        private void UpdateWaitObjectives(double lastTick)
        {
            if (player.Map == null || player.IsLoading)
                return;

            var waiting = new HashSet<uint>();
            foreach (IQuest quest in activeQuests.Values.Where(q => q.State == QuestState.Accepted).ToList())
            {
                foreach (IQuestObjective objective in quest.Where(o => !o.IsComplete()).ToList())
                {
                    (QuestTextTrigger Kind, uint Id, string Name) trigger = objective.ObjectiveInfo.GetTextTrigger();
                    if (trigger.Kind != QuestTextTrigger.Wait)
                        continue;

                    // only the current step of the quest
                    if (quest.Any(o => o.Index < objective.Index && !o.ObjectiveInfo.IsOptional() && !o.IsComplete()))
                        continue;

                    var locations = objective.ObjectiveInfo.GetAreaLocations().Where(l => l.WorldId == player.Map.Entry.Id).ToList();
                    if (objective.ObjectiveInfo.GetAreaLocations().Any() && !locations.Any(l =>
                        Vector3.Distance(player.Position, new Vector3(l.Position0, l.Position1, l.Position2)) <= Math.Max(WaitObjectiveDistance, l.Radius)))
                        continue;

                    uint id = objective.ObjectiveInfo.Id;
                    waiting.Add(id);
                    double elapsed = waitObjectiveElapsed.GetValueOrDefault(id) + lastTick;
                    if (elapsed < trigger.Id)
                    {
                        waitObjectiveElapsed[id] = elapsed;
                        continue;
                    }

                    waitObjectiveElapsed.Remove(id);
                    quest.ObjectiveUpdate(id, Math.Max(1u, objective.ObjectiveInfo.Entry.Count));
                }
            }

            foreach (uint id in waitObjectiveElapsed.Keys.Where(k => !waiting.Contains(k)).ToList())
                waitObjectiveElapsed.Remove(id);
        }

        public void SendInitialPackets()
        {
            DateTime now = DateTime.UtcNow;
            player.Session.EnqueueMessageEncrypted(new ServerQuestInit
            {
                Completed = completedQuests.Values
                    .Select(q => new ServerQuestInit.QuestComplete
                    {
                        QuestId        = q.Id,
                        CompletedToday = now < q.Reset
                    }).ToList(),
                Inactive = inactiveQuests.Values
                    .Select(q => new ServerQuestInit.QuestInactive
                    {
                        QuestId = q.Id,
                        State   = q.State
                    }).ToList(),
                Active = activeQuests.Values
                    .Select(q => new ServerQuestInit.QuestActive
                    {
                        QuestId    = q.Id,
                        State      = q.State,
                        Flags      = q.Flags,
                        QuestTimeElapsed      = q.Timer ?? 0u,
                        Objectives = q.Select(o => new ServerQuestInit.QuestActive.Objective
                        {
                            Progress = o.Progress,
                            TimeElapsed    = 0u
                        }).ToList()
                    }).ToList()
            });

            // the client doesn't always pick up objective progress from the init message, resend it explicitly
            foreach (IQuest quest in activeQuests.Values)
                quest.SendObjectiveProgress();
        }

        /// <summary>
        /// Get an active <see cref="IQuest"/> from supplied quest id.
        /// </summary>
        public IQuest GetActiveQuest<T>(T questId) where T : Enum
        {
            return GetActiveQuest(questId.As<T, ushort>());
        }

        /// <summary>
        /// Get an active <see cref="IQuest"/> from supplied quest id.
        /// </summary>
        public IQuest GetActiveQuest(ushort questId)
        {
            return GetQuest(questId, GetQuestFlags.Active);
        }

        /// <summary>
        /// Returns a collection of all active quests.
        /// </summary>
        public IEnumerable<IQuest> GetActiveQuests()
        {
            return activeQuests.Values;
        }

        /// <summary>
        /// Return <see cref="QuestState"/> for supplied quest.
        /// </summary>
        public QuestState? GetQuestState<T>(T questId) where T : Enum
        {
            return GetQuestState(questId.As<T, ushort>());
        }

        /// <summary>
        /// Return <see cref="QuestState"/> for supplied quest.
        /// </summary>
        public QuestState? GetQuestState(ushort questId)
        {
            return GetQuest(questId)?.State;
        }

        private IQuest GetQuest(ushort questId, GetQuestFlags flags = GetQuestFlags.All)
        {
            if ((flags & GetQuestFlags.Active) != 0
                && activeQuests.TryGetValue(questId, out IQuest quest))
                return quest;

            if ((flags & GetQuestFlags.Inactive) != 0
                && inactiveQuests.TryGetValue(questId, out quest))
                return quest;

            if ((flags & GetQuestFlags.Completed) != 0
                && completedQuests.TryGetValue(questId, out quest))
                return quest;

            return null;
        }

        /// <summary>
        /// Mention a quest from supplied quest id, skipping any prerequisites checks.
        /// </summary>
        public void QuestMention(ushort questId)
        {
            IQuestInfo info = GlobalQuestManager.Instance.GetQuestInfo(questId);
            if (info == null)
                throw new ArgumentException($"Invalid quest {questId}!");

            if (DisableManager.Instance.IsDisabled(DisableType.Quest, questId))
            {
                player.SendSystemMessage($"Unable to add quest {questId} because it is disabled.");
                return;
            }

            if (GetQuest(questId) != null)
                return;

            QuestMention(info);
        }

        /// <summary>
        /// Mention a quest from supplied <see cref="IQuestInfo"/>, skipping any prerequisites checks.
        /// </summary>
        public void QuestMention(IQuestInfo info)
        {
            IQuest quest = GetQuest((ushort)info.Entry.Id);
            if (quest == null)
            {
                quest = questFactory.Resolve();
                quest.Initialise(player, info);
            }
            else
                QuestReset(quest);

            quest.State = QuestState.Mentioned;
            inactiveQuests.Add((ushort)info.Entry.Id, quest);

            log.LogTrace($"Mentioned new quest {info.Entry.Id}.");
        }

        /// <summary>
        /// Add a quest from supplied id, optionally supplying <see cref="IItem"/> which was used to start the quest.
        /// </summary>
        public void QuestAdd(ushort questId, IItem item)
        {
            IQuestInfo info = GlobalQuestManager.Instance.GetQuestInfo(questId);
            if (info == null)
                throw new ArgumentException($"Invalid quest {questId}!");

            if (DisableManager.Instance.IsDisabled(DisableType.Quest, questId))
            {
                player.SendSystemMessage($"Unable to add quest {questId} because it is disabled.");
                return;
            }

            IQuest quest = GetQuest(questId);
            QuestAdd(info, quest, item);
        }

        private void QuestAdd(IQuestInfo info, IQuest quest, IItem item)
        {
            if (quest?.State is QuestState.Accepted or QuestState.Achieved)
                throw new QuestException($"Player {player.CharacterId} tried to start quest {info.Entry.Id} which is already in progress!");

            // if quest has already been completed make sure it's repeatable and the reset period has elapsed
            if (quest?.State == QuestState.Completed)
            {
                if (info.Entry.QuestRepeatPeriodEnum == 0u)
                    throw new QuestException($"Player {player.CharacterId} tried to start quest {info.Entry.Id} which they have already completed!");

                DateTime? resetTime = GetQuest((ushort)info.Entry.Id, GetQuestFlags.Completed).Reset;
                if (DateTime.UtcNow < resetTime)
                    throw new QuestException($"Player {player.CharacterId} tried to start quest {info.Entry.Id} which hasn't reset yet!");
            }

            if (item != null)
            {
                if (info.Entry.Id != item.Info.Entry.Quest2IdActivation)
                    throw new QuestException($"Player {player.CharacterId} tried to start quest {info.Entry.Id} from invalid item {item.Info.Entry.Id}!");

                // TODO: consume charge
            }
            else
            {
                // make sure the player is in range of a quest giver or they are eligible for a communicator message that starts the quest,
                // a quest the communicator already delivered (mentioned) can be accepted from the communicator even though the message's
                // conditions (eg: "quest unknown") no longer hold
                if (quest?.State != QuestState.Mentioned
                    && !GlobalQuestManager.Instance.GetQuestGivers((ushort)info.Entry.Id)
                        .Any(c => player.GetVisibleCreature<WorldEntity>(c).Any())
                    && !GlobalQuestManager.Instance.GetQuestCommunicatorMessages((ushort)info.Entry.Id)
                        .Any(m => m.Meets(player, false)))
                    throw new QuestException($"Player {player.CharacterId} tried to start quest {info.Entry.Id} without quest giver!");
            }

            // server doesn't send an error message for prerequisites since the client does the exact same checks
            // it's assumed that a player could never get here without cheating in some way
            if (!MeetsPrerequisites(info, out string reason))
                throw new QuestException($"Player {player.CharacterId} tried to start quest {info.Entry.Id} without meeting the prerequisites ({reason})!");

            QuestAdd(info);
        }

        private bool MeetsPrerequisites(IQuestInfo info)
        {
            return MeetsPrerequisites(info, out _);
        }

        private bool MeetsPrerequisites(IQuestInfo info, out string reason)
        {
            reason = null;
            if (info.Entry.QuestPlayerFactionEnum == 0u && player.Faction1 != Faction.Exile)
                reason = "faction (Exile only)";
            else if (info.Entry.QuestPlayerFactionEnum == 1u && player.Faction1 != Faction.Dominion)
                reason = "faction (Dominion only)";
            else if (info.Entry.PrerequisiteRace != 0u && player.Race != (Race)info.Entry.PrerequisiteRace)
                reason = $"race {info.Entry.PrerequisiteRace}";
            else if (info.Entry.PrerequisiteClass != 0u && player.Class != (Class)info.Entry.PrerequisiteClass)
                reason = $"class {info.Entry.PrerequisiteClass}";
            else if (player.Level < info.Entry.PrerequisiteLevel)
                reason = $"level {info.Entry.PrerequisiteLevel}";
            else if (!info.PrerequisiteQuests.IsEmpty)
            {
                bool preReqQuestsCompleted;
                if ((info.Entry.PrerequisiteFlags & 1) != 0u)
                    preReqQuestsCompleted = info.PrerequisiteQuests.Any(q => GetQuestState((ushort)q.Id) == QuestState.Completed);
                else
                    preReqQuestsCompleted = info.PrerequisiteQuests.All(q => GetQuestState((ushort)q.Id) == QuestState.Completed);

                if (!preReqQuestsCompleted)
                    reason = $"prerequisite quests {string.Join(",", info.PrerequisiteQuests.Select(q => q.Id))}";
            }

            if (reason == null && info.Entry.PrerequisiteId != 0u && !PrerequisiteManager.Instance.Meets(player, info.Entry.PrerequisiteId))
                reason = $"prerequisite {info.Entry.PrerequisiteId}";

            if (reason == null)
            {
                if (!info.IsContract())
                {
                    GameFormulaEntry entry = GameTableManager.Instance.GameFormula.GetEntry(655);
                    // client also hard codes 40 if entry doesn't exist
                    if (activeQuests.Count > (entry?.Dataint0 ?? 40u))
                        reason = "quest log full";
                }
                else
                {
                    // TODO: contracts use reward property for max slots, RewardProperty.ActiveContractSlots
                }
            }

            return reason == null;
        }

        /// <summary>
        /// Add a quest from supplied quest id.
        /// </summary>
        public void QuestAdd<T>(T questId) where T : Enum
        {
            QuestAdd(questId.As<T, ushort>());
        }

        /// <summary>
        /// Add a quest from supplied quest id.
        /// </summary>
        public void QuestAdd(ushort questId)
        {
            IQuestInfo info = GlobalQuestManager.Instance.GetQuestInfo(questId);
            if (info == null)
                throw new ArgumentException($"Invalid quest {questId}!");

            QuestAdd(info);
        }

        /// <summary>
        /// Add a quest from supplied <see cref="IQuestInfo"/>, skipping any prerequisites checks.
        /// </summary>
        public void QuestAdd(IQuestInfo info)
        {
            // make sure player has room for all pushed items
            if (player.Inventory.GetInventorySlotsRemaining(InventoryLocation.Inventory)
                < info.Entry.PushedItemIds.Count(i => i != 0u))
            {
                player.SendGenericError(GenericError.ItemInventoryFull);
                return;
            }

            IQuest quest = GetQuest((ushort)info.Entry.Id);
            if (quest == null)
                quest = questFactory.Resolve();
            else
                QuestReset(quest);

            quest.Initialise(player, info);
            activeQuests.Add((ushort)info.Entry.Id, quest);

            // items already in the inventory count towards collect objectives
            foreach (IQuestObjectiveInfo objectiveInfo in info.Objectives
                .Where(o => (QuestObjectiveType)o.Entry.Type == QuestObjectiveType.CollectItem))
            {
                uint count = player.Inventory.GetItemCount(objectiveInfo.Entry.Data);
                if (count > 0u)
                    quest.ObjectiveUpdate(QuestObjectiveType.CollectItem, objectiveInfo.Entry.Data, count);
            }

            // items already equipped count towards equip objectives
            foreach (IQuestObjectiveInfo objectiveInfo in info.Objectives)
            {
                (QuestTextTrigger Kind, uint Id, string Name) trigger = objectiveInfo.GetTextTrigger();
                if (trigger.Kind == QuestTextTrigger.EquipItem && player.Inventory.GetItemCount(trigger.Id, InventoryLocation.Equipped) > 0u)
                    quest.ObjectiveUpdate(objectiveInfo.Id, 1u);
            }

            // already standing in the zone an enter zone objective asks for
            for (WorldZoneEntry zone = player.Zone; zone != null; zone = zone.ParentZoneId != 0u ? GameTableManager.Instance.WorldZone.GetEntry(zone.ParentZoneId) : null)
                quest.ObjectiveUpdate(QuestObjectiveType.EnterZone, zone.Id, 1u);

            // and the area an enter area objective asks for
            OnRelocate(player.Position);

            // objectives the server has no event for never progress, make them visible in the log
            foreach (IQuestObjectiveInfo objectiveInfo in info.Objectives
                .Where(o => !o.IsOptional() && QuestObjectiveSupport.GetLevel(o.Type) == QuestSupportLevel.Unsupported))
                log.LogWarning($"Quest {info.Entry.Id} objective {objectiveInfo.Id} has objective type {objectiveInfo.Type} ({(uint)objectiveInfo.Type}) which the server doesn't support, it won't progress!");

            log.LogTrace($"Accepted new quest {info.Entry.Id}.");
        }

        private void QuestReset(IQuest quest)
        {
            // remove existing quest from its current home before
            switch (quest.State)
            {
                case QuestState.Abandoned:
                    activeQuests.Remove(quest.Id);
                    break;
                case QuestState.Completed:
                    completedQuests.Remove(quest.Id);
                    break;
                case QuestState.Botched:
                case QuestState.Ignored:
                case QuestState.Mentioned:
                    inactiveQuests.Remove(quest.Id);
                    break;
            }

            if (quest.PendingDelete)
                quest.EnqueueDelete(false);

            // reset previous objective progress
            foreach (IQuestObjective objective in quest)
                objective.Progress = 0u;
        }

        /// <summary>
        /// Retry an inactive quest id that was previously failed.
        /// </summary>
        public void QuestRetry(ushort questId)
        {
            IQuestInfo info = GlobalQuestManager.Instance.GetQuestInfo(questId);
            if (info == null)
                throw new ArgumentException($"Invalid quest {questId}!");

            IQuest quest = GetQuest(questId, GetQuestFlags.Inactive);
            if (quest == null)
                throw new QuestException($"Player {player.CharacterId} tried to restart quest {questId} which they don't have!");

            if (quest.State != QuestState.Botched)
                throw new QuestException($"Player {player.CharacterId} tried to restart quest {questId} which hasn't been failed!");

            QuestAdd(info, quest, null);
        }

        /// <summary>
        /// Abandon an active quest.
        /// </summary>
        public void QuestAbandon(ushort questId)
        {
            if (GlobalQuestManager.Instance.GetQuestInfo(questId) == null)
                throw new ArgumentException($"Invalid quest {questId}!");

            IQuest quest = GetQuest(questId, GetQuestFlags.Active | GetQuestFlags.Inactive);
            if (quest == null || quest.PendingDelete)
                throw new QuestException($"Player {player.CharacterId} tried to abandon quest {questId} which they don't have!");

            if (!quest.CanAbandon())
                throw new QuestException($"Player {player.CharacterId} tried to abandon quest {questId} which can't be abandoned!");

            // don't delete quests that have been mentioned, they may not be able to be re-collected.
            if (!quest.PendingCreate && quest.CanDelete())
                quest.EnqueueDelete(true);
            else
            {
                switch (quest.State)
                {
                    case QuestState.Accepted:
                    case QuestState.Achieved:
                        activeQuests.Remove(questId);
                        break;
                    case QuestState.Botched:
                        inactiveQuests.Remove(quest.Id);
                        break;
                }
            }

            foreach (IQuestObjective objective in quest)
                objective.Progress = 0u;

            if (quest.Info.IsQuestMentioned)
            {
                quest.State = QuestState.Mentioned;
                inactiveQuests.Add(quest.Id, quest);
            }
            else
                quest.State = QuestState.Abandoned;

            log.LogTrace($"Abandoned quest {questId}.");
        }

        /// <summary>
        /// Complete all <see cref="IQuestObjective"/>'s for supplied active quest id.
        /// </summary>
        public void QuestAchieve(ushort questId)
        {
            if (GlobalQuestManager.Instance.GetQuestInfo(questId) == null)
                throw new ArgumentException($"Invalid quest {questId}!");

            IQuest quest = GetQuest(questId);
            if (quest == null || quest.PendingDelete)
                throw new QuestException($"Player {player.CharacterId} tried to achieve quest {questId} which they don't have!");

            if (quest.State != QuestState.Accepted)
                throw new QuestException($"Player {player.CharacterId} tried to achieve quest {questId} with invalid state!");

            foreach (IQuestObjectiveInfo info in quest.Info.Objectives)
                quest.ObjectiveUpdate(info.Type, info.Entry.Data, info.Entry.Count);
        }

        /// <summary>
        /// Complete single <see cref="IQuestObjective"/> for supplied active quest id.
        /// </summary>
        public void QuestAchieveObjective(ushort questId, byte index)
        {
            if (GlobalQuestManager.Instance.GetQuestInfo(questId) == null)
                throw new ArgumentException($"Invalid quest {questId}!");

            IQuest quest = GetQuest(questId);
            if (quest == null || quest.PendingDelete)
                throw new QuestException();

            if (quest.State != QuestState.Accepted)
                throw new QuestException();

            IQuestObjective objective = quest.SingleOrDefault(o => o.Index == index);
            if (objective == null)
                throw new QuestException();

            quest.ObjectiveUpdate(objective.ObjectiveInfo.Type, objective.ObjectiveInfo.Entry.Data, objective.ObjectiveInfo.Entry.Count);
        }

        /// <summary>
        /// Complete an achieved quest supplying an optional reward and whether the quest was completed from the communicator.
        /// </summary>
        public void QuestComplete(ushort questId, ushort reward, bool communicator)
        {
            IQuestInfo info = GlobalQuestManager.Instance.GetQuestInfo(questId);
            if (info == null)
                throw new ArgumentException($"Invalid quest {questId}!");

            if (DisableManager.Instance.IsDisabled(DisableType.Quest, questId))
            {
                player.SendSystemMessage($"Unable to complete quest {questId} because it is disabled.");
                return;
            }

            IQuest quest = GetQuest(questId, GetQuestFlags.Active);
            if (quest == null)
            {
                if (!info.IsAutoComplete())
                    throw new QuestException($"Player {player.CharacterId} tried to complete quest {questId} which they don't have!");

                QuestAdd(questId, null);
                quest = GetQuest(questId);
                quest.State = QuestState.Achieved;
            }

            if (quest.State != QuestState.Achieved)
                throw new QuestException($"Player {player.CharacterId} tried to complete quest {questId} which wasn't complete!");

            if (communicator)
            {
                // the client shows the quest tracker call back button for quests received over the communicator and quests that can be called back (flag 0x04),
                // see QuestTracker:HelperShowQuestCallbackBtn
                if (!quest.Info.IsCommunicatorReceived() && !quest.Info.CanBeCalledBack())
                    throw new QuestException($"Player {player.CharacterId} tried to complete quest {questId} without communicator message!");
            }
            else
            {
                if (!GlobalQuestManager.Instance.GetQuestReceivers(questId).Any(c => player.GetVisibleCreature<WorldEntity>(c).Any()))
                    throw new QuestException($"Player {player.CharacterId} tried to complete quest {questId} without any quest receiver!");
            }

            quest.RewardQuest(reward);
            TakeCollectedQuestItems(quest);

            activeQuests.Remove(questId);
            completedQuests.Add(questId, quest);
            ObjectiveUpdate(QuestObjectiveType.CompleteQuest, questId, 1u);
            ObjectiveUpdateQuestName(quest.Info);
        }

        /// <summary>
        /// Complete an achieved <see cref="IQuest"/> from supplied quest id.
        /// </summary>
        public void QuestComplete<T>(T questId) where T : Enum
        {
            QuestComplete(questId.As<T, ushort>());
        }

        /// <summary>
        /// Complete an achieved <see cref="IQuest"/> from supplied quest id.
        /// </summary>
        public void QuestComplete(ushort questId)
        {
            IQuest quest = GetQuest(questId, GetQuestFlags.Active);
            if (quest == null)
                return;

            quest.CompleteQuest();
            TakeCollectedQuestItems(quest);

            activeQuests.Remove(questId);
            completedQuests.Add(questId, quest);
            ObjectiveUpdate(QuestObjectiveType.CompleteQuest, questId, 1u);
            ObjectiveUpdateQuestName(quest.Info);
        }

        /// <summary>
        /// Remove quest items gathered for the supplied <see cref="IQuest"/>'s collect objectives, they are handed in with the quest.
        /// </summary>
        private void TakeCollectedQuestItems(IQuest quest)
        {
            // only quest item family, collect objectives for regular items (ore, armor) leave them with the player
            const uint questItemFamily = 24u;

            foreach (IQuestObjective objective in quest
                .Where(o => (QuestObjectiveType)o.ObjectiveInfo.Entry.Type == QuestObjectiveType.CollectItem))
            {
                uint itemId = objective.ObjectiveInfo.Entry.Data;
                Item2Entry itemEntry = GameTableManager.Instance.Item.GetEntry(itemId);
                if (itemEntry == null || itemEntry.Item2FamilyId != questItemFamily)
                    continue;

                uint count = Math.Min(objective.ObjectiveInfo.Entry.Count, player.Inventory.GetItemCount(itemId));
                if (count > 0u)
                    player.Inventory.ItemDelete(itemId, count, ItemUpdateReason.Quest);
            }
        }

        /// <summary>
        /// Ignore or acknowledge an inactive quest.
        /// </summary>
        public void QuestIgnore(ushort questId, bool ignored)
        {
            IQuestInfo questInfo = GlobalQuestManager.Instance.GetQuestInfo(questId);
            if (questInfo == null)
                throw new ArgumentException($"Invalid quest {questId}!");

            IQuest quest = GetQuest((ushort)questInfo.Entry.Id);
            if (quest == null)
            {
                quest = questFactory.Resolve(); // Add quest so we can set it to ignored.
                quest.Initialise(player, questInfo);
            }
            else
                QuestReset(quest); // Removes from quest log. Might not be the cleanest way to do this?

            quest.State = ignored ? QuestState.Ignored : QuestState.Mentioned;

            inactiveQuests.Add(questId, quest);
        }

        /// <summary>
        /// Track or hide an active quest.
        /// </summary>
        public void QuestTrack(ushort questId, bool tracked)
        {
            if (GlobalQuestManager.Instance.GetQuestInfo(questId) == null)
                throw new ArgumentException($"Invalid quest {questId}!");

            IQuest quest = GetQuest(questId, GetQuestFlags.Active);
            if (quest == null)
                throw new QuestException($"Player {player.CharacterId} tried to track quest {questId} which they don't have!");

            if (quest.State != QuestState.Accepted && quest.State != QuestState.Achieved)
                throw new QuestException($"Player {player.CharacterId} tried to track quest {questId} with invalid state!");

            if (tracked)
                quest.Flags |= QuestStateFlags.Tracked;
            else
                quest.Flags &= ~QuestStateFlags.Tracked;

            log.LogTrace($"Updated tracked state of quest {questId} to {tracked}.");
        }

        /// <summary>
        /// Share supplied quest with another <see cref="IPlayer"/>.
        /// </summary>
        public void QuestShare(ushort questId)
        {
            IQuestInfo info = GlobalQuestManager.Instance.GetQuestInfo(questId);
            if (info == null)
                throw new ArgumentException($"Invalid quest {questId}!");

            IQuest quest = GetQuest(questId);
            if (quest == null)
                throw new QuestException($"Player {player.CharacterId} tried to share quest {questId} which they don't have!");

            if (!quest.CanShare())
                throw new QuestException($"Player {player.CharacterId} tried to share quest {questId} which can't be shared!");

            if (player.TargetGuid == null)
                throw new QuestException($"Player {player.CharacterId} tried to share quest {questId} without a target!");

            IPlayer recipient = player.GetVisible<IPlayer>(player.TargetGuid.Value);
            if (recipient == null)
                throw new QuestException($"Player {player.CharacterId} tried to share quest {questId} to an invalid player!");

            // TODO

            log.LogTrace($"Shared quest {questId} with player {recipient.Name}.");
        }

        /// <summary>
        /// Accept or deny a shared quest from another <see cref="IPlayer"/>.
        /// </summary>
        public void QuestShareResult(ushort questId, bool result)
        {
            // TODO
        }

        /// <summary>
        /// Update any active quest <see cref="IQuestObjective"/>'s with supplied <see cref="QuestObjectiveType"/> and data with progress.
        /// </summary>
        public void ObjectiveUpdate(QuestObjectiveType type, uint data, uint progress)
        {
            // it is possible for an objective update to achieve a quest which has auto complete
            // this may start a new quest right away
            foreach (IQuest quest in activeQuests.Values.ToList())
                quest.ObjectiveUpdate(type, data, progress);
        }

        /// <summary>
        /// Update any active quest <see cref="IQuestObjective"/>'s with supplied <see cref="QuestObjectiveType"/> that have no target of their own
        /// and list their targets in the supplied reward pane target group instead.
        /// </summary>
        public void ObjectiveUpdateRewardPane(QuestObjectiveType type, uint targetGroupId, uint progress)
        {
            foreach (IQuest quest in activeQuests.Values.ToList())
                quest.ObjectiveUpdateRewardPane(type, targetGroupId, progress);
        }

        /// <summary>
        /// Fill work order objectives (type 31) of active quests asking for crafts of the supplied tradeskill tier.
        /// </summary>
        public void OnCraft(uint tradeskillTierId, uint count)
        {
            foreach (IQuest quest in activeQuests.Values.ToList())
            {
                if (quest.State != QuestState.Accepted || quest.Info.GetWorkOrderTradeskillTierId() != tradeskillTierId)
                    continue;

                foreach (IQuestObjective objective in quest.Where(o => o.ObjectiveInfo.Type == QuestObjectiveType.Unknown31 && !o.IsComplete()).ToList())
                    quest.ObjectiveUpdate(objective.ObjectiveInfo.Id, count);
            }
        }

        /// <summary>
        /// Update any active quest <see cref="IQuestObjective"/>'s with supplied ID with progress.
        /// </summary>
        public void ObjectiveUpdate(uint id, uint progress)
        {
            foreach (IQuest quest in activeQuests.Values.ToList())
                quest.ObjectiveUpdate(id, progress);
        }

        /// <summary>
        /// Update target-less "Use/Open/Select the" objectives naming the creature the player interacted with.
        /// </summary>
        public void ObjectiveUpdateUseCreature(uint creatureId)
        {
            if (creatureId == 0u)
                return;

            foreach (IQuest quest in activeQuests.Values.ToList())
            {
                if (quest.State != QuestState.Accepted)
                    continue;

                foreach (IQuestObjective objective in quest
                    .Where(o => !o.IsComplete() && o.ObjectiveInfo.GetUseCreatureId() == creatureId)
                    .ToList())
                    quest.ObjectiveUpdate(objective.ObjectiveInfo.Id, 1u);
            }

            // "Learn more about $(creature=N)"
            ObjectiveUpdateText(QuestTextTrigger.TalkTo, creatureId);
        }

        /// <summary>
        /// Update target-less objectives whose text asks for the supplied <see cref="QuestTextTrigger"/> with the supplied id (eg: the item equipped or bought).
        /// </summary>
        public void ObjectiveUpdateText(QuestTextTrigger kind, uint id)
        {
            if (id != 0u)
                ObjectiveUpdateText(kind, i => i == id, null);
        }

        /// <summary>
        /// Update target-less objectives whose text asks for the supplied <see cref="QuestTextTrigger"/> and whose id and name match.
        /// </summary>
        private void ObjectiveUpdateText(QuestTextTrigger kind, Func<uint, bool> idMatches, Func<string, bool> nameMatches)
        {
            // quest state changes before the client has its quest init (still loading the map) confuse it
            if (player.Map == null || player.IsLoading)
                return;

            foreach (IQuest quest in activeQuests.Values.ToList())
            {
                if (quest.State != QuestState.Accepted)
                    continue;

                foreach (IQuestObjective objective in quest.Where(o => !o.IsComplete()).ToList())
                {
                    (QuestTextTrigger Kind, uint Id, string Name) trigger = objective.ObjectiveInfo.GetTextTrigger();
                    if (trigger.Kind != kind)
                        continue;
                    if (idMatches != null && !idMatches(trigger.Id))
                        continue;
                    if (nameMatches != null && !nameMatches(trigger.Name))
                        continue;

                    quest.ObjectiveUpdate(objective.ObjectiveInfo.Id, 1u);
                }
            }
        }

        /// <summary>
        /// Invoked when a public event the owner took part in finishes, <paramref name="won"/> if the owner was on the winning team.
        /// </summary>
        /// <remarks>
        /// Updates objectives asking for a public event in a zone ("Complete a Levian Bay event", "Participate in a Public Event") and,
        /// when it is the event of an adventure, dungeon or expedition, objectives naming that content ("Complete Veteran Bay of Betrayal").
        /// </remarks>
        public void OnPublicEventFinish(PublicEventEntry entry, IMatchingMap matchingMap, bool won)
        {
            // participate objectives (id 1) count either way, complete objectives need the event won
            ObjectiveUpdateText(QuestTextTrigger.PublicEvent, i => won || i == 1u, n => PublicEventZoneMatches(n, entry));

            if (won && matchingMap != null)
                ObjectiveUpdateText(QuestTextTrigger.CompleteContent, null, n => ContentNameMatches(n, matchingMap));
        }

        private static bool PublicEventZoneMatches(string zoneName, PublicEventEntry entry)
        {
            // "Participate in a Public Event": any event
            if (string.IsNullOrEmpty(zoneName))
                return true;

            TextTable textTable = GameTableManager.Instance.GetTextTable(Language.English);
            for (WorldZoneEntry zone = GameTableManager.Instance.WorldZone.GetEntry(entry.WorldZoneId); zone != null;
                zone = zone.ParentZoneId != 0u ? GameTableManager.Instance.WorldZone.GetEntry(zone.ParentZoneId) : null)
            {
                if (string.Equals(textTable?.GetEntry(zone.LocalizedTextIdName), zoneName, StringComparison.OrdinalIgnoreCase))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Returns if the content an objective names ("Veteran Bay of Betrayal", "Malgrave Trail adventure", "a Prime Dungeon") is the supplied <see cref="IMatchingMap"/>.
        /// </summary>
        private static bool ContentNameMatches(string name, IMatchingMap matchingMap)
        {
            TextTable textTable = GameTableManager.Instance.GetTextTable(Language.English);
            string mapName = textTable?.GetEntry(matchingMap.GameMapEntry.LocalizedTextIdName) ?? "";
            MatchType type = matchingMap.GameTypeEntry?.MatchTypeEnum ?? MatchType.None;
            bool veteran = mapName.StartsWith("Veteran:", StringComparison.OrdinalIgnoreCase);
            bool prime   = type is MatchType.PrimeLevelDungeon or MatchType.PrimeLevelExpedition or MatchType.PrimeLevelAdventure
                or MatchType.ScaledPrimeLevelAdventure or MatchType.ScaledPrimeLevelDungeon or MatchType.ScaledPrimeLevelExpedition;

            // a kind of content, eg: "a Prime Dungeon", "a Veteran Adventure"
            Match category = Regex.Match(name, @"^an? (?<words>.+)$", RegexOptions.IgnoreCase);
            if (category.Success)
            {
                var words = category.Groups["words"].Value.ToLowerInvariant().Split(' ', StringSplitOptions.RemoveEmptyEntries).ToList();
                if (words.Remove("veteran") && !veteran)
                    return false;
                if (words.Remove("prime") && !prime)
                    return false;
                if (words.Count != 1)
                    return false;

                return words[0] switch
                {
                    "dungeon"    => type is MatchType.Dungeon or MatchType.PrimeLevelDungeon or MatchType.ScaledPrimeLevelDungeon,
                    "expedition" => type is MatchType.Shiphand or MatchType.PrimeLevelExpedition or MatchType.ScaledPrimeLevelExpedition,
                    "adventure"  => type is MatchType.Adventure or MatchType.PrimeLevelAdventure or MatchType.ScaledPrimeLevelAdventure,
                    _            => false
                };
            }

            if (Regex.IsMatch(name, @"^Veteran\s", RegexOptions.IgnoreCase) && !veteran)
                return false;

            static string Normalise(string text)
            {
                text = Regex.Replace(text.Trim(), @"^(Veteran|Prime|Holiday Expedition):?\s+", "", RegexOptions.IgnoreCase);
                text = Regex.Replace(text, @"^The\s+", "", RegexOptions.IgnoreCase);
                text = Regex.Replace(text, @"\s+(adventure|dungeon|expedition|shiphand)$", "", RegexOptions.IgnoreCase);
                return text.Trim();
            }

            return string.Equals(Normalise(name), Normalise(mapName), StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Update objectives naming the quests they ask to complete ("Complete the Hidden Files, Paired Programming, and Data Recovery quests").
        /// </summary>
        private void ObjectiveUpdateQuestName(IQuestInfo info)
        {
            TextTable textTable = GameTableManager.Instance.GetTextTable(Language.English);
            string title = textTable?.GetEntry(info.Entry.LocalizedTextIdTitle);
            if (string.IsNullOrEmpty(title))
                return;

            ObjectiveUpdateText(QuestTextTrigger.CompleteContent, null, n =>
            {
                Match quests = Regex.Match(n, @"^(?<titles>.+?)\s+quests?$", RegexOptions.IgnoreCase);
                return quests.Success && Regex.Split(quests.Groups["titles"].Value, @",\s*(?:and\s+)?|\s+and\s+")
                    .Any(t => string.Equals(t.Trim(), title, StringComparison.OrdinalIgnoreCase));
            });
        }

        /// <summary>
        /// Mention quests from communicator messages whose conditions the owner now meets (zone, level, world, quest states).
        /// </summary>
        /// <remarks>
        /// Invoked on login, zone change and level up, quest state changes are handled by the state triggers in <see cref="Quest"/>.
        /// </remarks>
        public void CheckCommunicatorMessages()
        {
            // quest state changes before the client has its quest init (still loading the map) confuse it, this runs again on entering the world
            if (player.Map == null || player.IsLoading)
                return;

            foreach (ICommunicatorMessage message in GlobalQuestManager.Instance.GetQuestCommunicatorMessages())
            {
                // messages without any condition are sent by scripts or other content
                if (!message.HasConditions())
                    continue;

                // quests the player already has, or has finished, aren't delivered again
                QuestState? state = GetQuestState(message.QuestId);
                if (state is not (null or QuestState.Abandoned))
                    continue;

                IQuestInfo info = GlobalQuestManager.Instance.GetQuestInfo(message.QuestId);
                if (info == null || DisableManager.Instance.IsDisabled(DisableType.Quest, message.QuestId))
                    continue;

                if (!message.Meets(player) || !MeetsPrerequisites(info))
                    continue;

                QuestMention(info);
            }
        }

        private double publicEventCheckCooldown;

        /// <remarks>
        /// Open world public events (eg: defend the memorial service in Gallow) were started by retail scripts, the event's objectives,
        /// teams and spawns are all in the data. A player with a "complete the event" objective (type 25) inside the event's zone starts
        /// the event if it isn't running and joins it, leaving the zone leaves it.
        /// </remarks>
        private void CheckQuestPublicEvents()
        {
            if (publicEventCheckCooldown > 0d || player.Map == null || player.IsLoading)
                return;

            publicEventCheckCooldown = 2d;

            foreach (IQuest quest in activeQuests.Values.Where(q => q.State == QuestState.Accepted).ToList())
            {
                foreach (IQuestObjective objective in quest.Where(o => o.ObjectiveInfo.Type == QuestObjectiveType.CompleteEvent && !o.IsComplete()).ToList())
                {
                    PublicEventEntry entry = GameTableManager.Instance.PublicEvent.GetEntry(objective.ObjectiveInfo.Entry.Data);
                    if (entry == null || entry.WorldId != player.Map.Entry.Id)
                        continue;

                    bool inZone = entry.WorldZoneId == 0u || IsInZone(entry.WorldZoneId);
                    IPublicEvent publicEvent = player.Map.PublicEventManager.GetEvents().FirstOrDefault(e => e.Id == entry.Id && !e.HasFinished);
                    bool joined = publicEvent != null && player.Map.PublicEventManager.GetEvents()
                        .Any(e => e == publicEvent && e.GetTeams().Any(t => t.GetMembers().Any(m => m.CharacterId == player.CharacterId)));

                    if (!inZone)
                    {
                        if (joined)
                            publicEvent.LeaveEvent(player, PublicEventRemoveReason.LeftArea);
                        continue;
                    }

                    if (joined || player.Level < entry.MinPlayerLevel)
                        continue;

                    if (publicEvent == null)
                    {
                        // a finished event stays in the store until it is removed, it is started again once it is gone
                        if (player.Map.PublicEventManager.GetEvents().Any(e => e.Id == entry.Id))
                            continue;

                        publicEvent = player.Map.PublicEventManager.CreateEvent(entry.Id);
                        if (publicEvent == null)
                            continue;
                    }

                    PublicEventTeam team = publicEvent.GetTeams().Any(t => t.Team == PublicEventTeam.PublicTeam)
                        ? PublicEventTeam.PublicTeam
                        : player.Faction1 == Faction.Dominion ? PublicEventTeam.Dominion : PublicEventTeam.Exiles;
                    if (publicEvent.GetTeams().Any(t => t.Team == team))
                        publicEvent.JoinEvent(player, team);
                }
            }
        }

        private bool IsInZone(uint zoneId)
        {
            for (WorldZoneEntry zone = player.Zone; zone != null; zone = zone.ParentZoneId != 0u ? GameTableManager.Instance.WorldZone.GetEntry(zone.ParentZoneId) : null)
                if (zone.Id == zoneId)
                    return true;

            return false;
        }

        /// <summary>
        /// Speed of a summoned "Follow the ..." guide, a walking pace the player can keep up with.
        /// </summary>
        private const float FollowGuideSpeed = 3.5f;

        private readonly Dictionary<uint /*objective id*/, (uint Guid, Vector3 End)> followGuides = [];

        /// <summary>
        /// Distance to a guide at the end of its walk that catches it.
        /// </summary>
        private const float FollowCatchDistance = 12f;

        /// <summary>
        /// Summon the creature a "Follow the ..." objective names once the objective is the current step, it walks to the objective's marker.
        /// </summary>
        /// <remarks>
        /// Retail scripts summoned these (eg: the Spatial Anomaly), the client data only names the creature and the destination.
        /// </remarks>
        /// <returns>True once the player caught up with the guide at the end of its walk.</returns>
        private bool StartFollowGuide(IQuest quest, IQuestObjective objective)
        {
            if (followGuides.TryGetValue(objective.ObjectiveInfo.Id, out (uint Guid, Vector3 End) existing))
            {
                IWorldEntity current = player.Map.GetEntity<IWorldEntity>(existing.Guid);
                if (current != null)
                    return Vector3.Distance(current.Position, existing.End) < 3f
                        && Vector3.Distance(player.Position, current.Position) <= FollowCatchDistance;
                followGuides.Remove(objective.ObjectiveInfo.Id);
            }

            // only the current step of the quest
            if (quest.Any(o => o.Index < objective.Index && !o.ObjectiveInfo.IsOptional() && !o.IsComplete()))
                return false;

            ICreatureInfo creatureInfo = LegacyServiceProvider.Provider.GetService<ICreatureInfoManager>().GetCreatureInfo(objective.ObjectiveInfo.GetFollowCreatureId());
            if (creatureInfo == null)
                return false;

            List<WorldLocation2Entry> destinations = objective.ObjectiveInfo.GetAreaLocations()
                .Where(l => l.WorldId == player.Map.Entry.Id)
                .ToList();
            if (destinations.Count == 0)
                return false;

            var first = new Vector3(destinations[0].Position0, destinations[0].Position1, destinations[0].Position2);
            Vector3 direction = Vector3.Normalize(new Vector3(first.X - player.Position.X, 0f, first.Z - player.Position.Z));
            if (float.IsNaN(direction.X))
                direction = Vector3.UnitX;
            Vector3 start = player.Position + direction * 4f;
            start.Y = player.Map.GetTerrainHeight(start.X, start.Z) ?? player.Position.Y;

            var nodes = new List<Vector3> { start };
            foreach (WorldLocation2Entry destination in destinations)
            {
                var centre = new Vector3(destination.Position0, destination.Position1, destination.Position2);
                if (Vector3.Distance(start, centre) > destination.Radius || destination.Radius < 10f)
                {
                    nodes.Add(centre);
                    continue;
                }

                // already inside the marked area (the anomaly's area around the wreckage): it leads the player around the area
                float startAngle = MathF.Atan2(start.Z - centre.Z, start.X - centre.X);
                for (int i = 1; i <= 6; i++)
                {
                    float angle = startAngle + i * MathF.PI / 3f;
                    float x = centre.X + MathF.Cos(angle) * destination.Radius * 0.7f, z = centre.Z + MathF.Sin(angle) * destination.Radius * 0.7f;
                    nodes.Add(new Vector3(x, player.Map.GetTerrainHeight(x, z) ?? centre.Y, z));
                }
            }

            void OnAdd(IBaseMap map, uint guid, Vector3 vector)
            {
                map.GetEntity<IWorldEntity>(guid)?.MovementManager.LaunchSpline(nodes, SplineType.Linear, SplineMode.OneShot, FollowGuideSpeed);
            }

            IWorldEntity guide = creatureInfo.Entry.CreationTypeEnum == EntityType.NonPlayer
                ? player.SummonFactory.Summon<INonPlayerEntity>(creatureInfo, start, Vector3.Zero, OnAdd)
                : player.SummonFactory.Summon<ISimpleEntity>(creatureInfo, start, Vector3.Zero, OnAdd);
            followGuides[objective.ObjectiveInfo.Id] = (guide.Guid, nodes[^1]);
            return false;
        }

        /// <summary>
        /// Remove guides of follow objectives that are done or no longer in the quest log.
        /// </summary>
        private void CleanupFollowGuides()
        {
            if (followGuides.Count == 0)
                return;

            foreach ((uint objectiveId, (uint guid, Vector3 _)) in followGuides.ToList())
            {
                bool active = activeQuests.Values.Any(q => q.State == QuestState.Accepted
                    && q.Any(o => o.ObjectiveInfo.Id == objectiveId && !o.IsComplete()));
                if (active)
                    continue;

                player.Map?.GetEntity<IWorldEntity>(guid)?.RemoveFromMap();
                followGuides.Remove(objectiveId);
            }
        }

        /// <summary>
        /// Distance from an enter area objective's location the objective completes at.
        /// </summary>
        /// <remarks>
        /// Retail used server trigger volumes that aren't in the client data, most indicator locations are points (radius 1).
        /// </remarks>
        private const float EnterAreaMinimumRadius = 20f;

        /// <summary>
        /// Distance from the speaking creature a "Listen to" objective completes at (inferred, retail scripts finished them after the speech).
        /// </summary>
        private const float ListenRadius = 15f;

        /// <summary>
        /// Invoked when the owner moves, completes enter area objectives the owner is now inside of.
        /// </summary>
        public void OnRelocate(Vector3 position)
        {
            if (player.Map == null)
                return;

            CleanupFollowGuides();
            CheckQuestPublicEvents();

            foreach (IQuest quest in activeQuests.Values.ToList())
            {
                if (quest.State != QuestState.Accepted)
                    continue;

                foreach (IQuestObjective objective in quest
                    .Where(o => !o.IsComplete())
                    .ToList())
                {
                    if (objective.ObjectiveInfo.IsAreaObjective())
                    {
                        // follow objectives complete by catching up with the guide at the end of its walk, not by standing in the area
                        if (objective.ObjectiveInfo.GetFollowCreatureId() != 0u)
                        {
                            if (StartFollowGuide(quest, objective))
                                quest.ObjectiveUpdate(objective.ObjectiveInfo.Id, 1u);
                        }
                        else if (objective.ObjectiveInfo.GetAreaLocations()
                            .Any(l => l.WorldId == player.Map.Entry.Id
                                && Vector3.Distance(position, new Vector3(l.Position0, l.Position1, l.Position2)) <= Math.Max(l.Radius, EnterAreaMinimumRadius)))
                            quest.ObjectiveUpdate(objective.ObjectiveInfo.Id, 1u);
                    }
                    else
                    {
                        uint listenCreatureId = objective.ObjectiveInfo.GetListenCreatureId();
                        if (listenCreatureId != 0u && player.GetVisibleCreature<IWorldEntity>(listenCreatureId)
                                .Any(e => Vector3.Distance(position, e.Position) <= ListenRadius))
                            quest.ObjectiveUpdate(objective.ObjectiveInfo.Id, 1u);
                    }
                }
            }
        }
    }
}
