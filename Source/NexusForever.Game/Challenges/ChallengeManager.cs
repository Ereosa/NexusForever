using System.Collections.Immutable;
using System.Numerics;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.Extensions.DependencyInjection;
using NexusForever.Database.Character;
using NexusForever.Database.Character.Model;
using NexusForever.Game.Abstract.Challenges;
using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Group;
using NexusForever.Game.Abstract.Map.Search;
using NexusForever.Game.Entity;
using NexusForever.Game.Static.Challenges;
using NexusForever.Game.Static.Prerequisite;
using NexusForever.Game.Static.Quest;
using NexusForever.GameTable;
using NexusForever.GameTable.Model;
using NexusForever.Network.World.Message.Model;
using NexusForever.Script;
using NexusForever.Script.Template.Collection;
using NexusForever.Network.World.Message.Model.Challenges;
using NexusForever.Shared;
using NexusForever.Shared.Game;
using NLog;

namespace NexusForever.Game.Challenges
{
    /// <summary>
    /// Challenges of a player: unlocking, starting, progress, tiers, timers, cooldowns and sharing with the group.
    /// </summary>
    /// <remarks>
    /// Retail behaviour (WildStar wiki, client challenge UI): challenges are unlocked by exploring their area or using a challenge
    /// starter object and are then listed in the challenge log, from where they can be started again. Only one challenge of each
    /// type can run at a time. A challenge fails when its timer runs out without reaching bronze, when the player stays out of its
    /// area for too long or when it is abandoned, failing has no cooldown. A completed challenge goes on a 30 minute cooldown that
    /// only runs while the player is online, and earns points on the zone's challenge reward track for the tier reached.
    ///
    /// The time limit of each challenge is server data the client doesn't have, 5 minutes is the most common retail value.
    /// </remarks>
    public class ChallengeManager : IChallengeManager
    {
        public static readonly TimeSpan DefaultDuration  = TimeSpan.FromMinutes(5d);
        public static readonly TimeSpan CooldownDuration = TimeSpan.FromMinutes(30d);
        public static readonly TimeSpan AreaFailDuration = TimeSpan.FromSeconds(10d);

        /// <summary>
        /// GameFormula with the reward track points given for each tier (Dataint0 bronze, Dataint01 silver, Dataint02 gold), the
        /// client sums them to show the points of a tier in the challenge log.
        /// </summary>
        private const uint RewardPointsFormulaId = 1268u;

        /// <summary>
        /// GameFormula with the time a shared challenge offer stays open.
        /// </summary>
        private const uint ShareTimeoutFormulaId = 1046u;

        /// <summary>
        /// Source type of T-spells granted by challenges in <see cref="ServerQuestSpellShortcut"/>.
        /// </summary>
        /// <remarks>
        /// The client handles 0 (path mission), 1 (quest) and 2 (quest objective) itself, challenges come next.
        /// </remarks>
        private const uint SpellShortcutReasonChallenge = 3u;

        /// <summary>
        /// Distance from a challenge start or indicator location within which the challenge is unlocked.
        /// </summary>
        private const float UnlockRange = 20f;

        /// <summary>
        /// Largest indicator radius that is taken as the goal of a General challenge (reach the top of a cliff, race to a spot).
        /// </summary>
        private const float ReachGoalMaxRadius = 10f;

        /// <summary>
        /// Height an indicator must be above the start location to be the goal of a climb (Up the Falls, Eldan Wall Scaler, Swiftfoot Supplies).
        /// </summary>
        private const float ClimbGoalMinHeight = 20f;

        private static readonly ILogger log = LogManager.GetCurrentClassLogger();

        private class ChallengeIndex
        {
            public Dictionary<uint, List<ChallengeEntry>> ByZone { get; } = [];
            public Dictionary<uint, List<ChallengeEntry>> ByTargetGroup { get; } = [];
            public Dictionary<uint, List<ChallengeEntry>> ByItem { get; } = [];
            public Dictionary<uint, List<ChallengeEntry>> BySpell { get; } = [];
            public Dictionary<uint, List<uint>> StartersByCreature { get; } = [];
            public Dictionary<uint, List<ChallengeEntry>> ByCreature { get; } = [];
        }

        private static readonly Lazy<ChallengeIndex> index = new(BuildIndex);

        private static ChallengeIndex BuildIndex()
        {
            var challengeIndex = new ChallengeIndex();

            static void Add(Dictionary<uint, List<ChallengeEntry>> dictionary, uint key, ChallengeEntry entry)
            {
                if (key == 0u)
                    return;

                if (!dictionary.TryGetValue(key, out List<ChallengeEntry> entries))
                {
                    entries = [];
                    dictionary.Add(key, entries);
                }

                if (!entries.Contains(entry))
                    entries.Add(entry);
            }

            foreach (ChallengeEntry entry in GameTableManager.Instance.Challenge.Entries)
            {
                // unnamed challenges are test or cut content
                if (string.IsNullOrWhiteSpace(GameTableManager.Instance.TextEnglish?.GetEntry(entry.LocalizedTextIdName)))
                    continue;

                Add(challengeIndex.ByZone, GetAreaZoneId(entry), entry);

                switch ((ChallengeType)entry.ChallengeTypeEnum)
                {
                    case ChallengeType.Combat:
                    case ChallengeType.ChecklistActivate:
                        Add(challengeIndex.ByTargetGroup, entry.Target, entry);
                        break;
                    case ChallengeType.Collect:
                    case ChallengeType.Ability:
                        Add(challengeIndex.ByTargetGroup, entry.TargetGroupIdRewardPane, entry);
                        break;
                    case ChallengeType.General:
                    {
                        Add(challengeIndex.ByTargetGroup, entry.TargetGroupIdRewardPane, entry);

                        // the target of a General challenge is a target group to kill or use (Gold Grab, Squirg Run) or
                        // the spell of the action to repeat (Roan Tipping, pulling Wiggleroot Radishes, Whack-a-Pole)
                        // races only finish at their goal, whatever they name as their target
                        if (IsReachChallenge(entry))
                            break;

                        if (GameTableManager.Instance.TargetGroup.GetEntry(entry.Target) != null)
                            Add(challengeIndex.ByTargetGroup, entry.Target, entry);
                        else if (GameTableManager.Instance.Spell4.GetEntry(entry.Target) != null)
                            Add(challengeIndex.BySpell, entry.Target, entry);
                        break;
                    }
                    case ChallengeType.Item:
                        Add(challengeIndex.ByItem, entry.Target, entry);
                        break;
                }

                if ((ChallengeType)entry.ChallengeTypeEnum == ChallengeType.Ability)
                    Add(challengeIndex.BySpell, entry.Target, entry);

                // General and Collect challenges with nothing else to count name what to count in their text with a plural creature
                // link ("Extinguish $m(creature=31600) around Meadow Point", "Gather $m(creature=53520)"), killing or using one counts
                if ((ChallengeType)entry.ChallengeTypeEnum is ChallengeType.General or ChallengeType.Collect
                    && entry.Target == 0u && entry.TargetGroupIdRewardPane == 0u)
                {
                    string progress = GameTableManager.Instance.TextEnglish?.GetEntry(entry.LocalizedTextIdProgress) ?? "";
                    foreach (System.Text.RegularExpressions.Match match in System.Text.RegularExpressions.Regex.Matches(progress, @"\$m\(creature=(\d+)\)"))
                        Add(challengeIndex.ByCreature, uint.Parse(match.Groups[1].Value), entry);
                }
            }

            // challenge starter objects are shown or usable while their challenge is locked (ChallengeLocked prerequisite),
            // using one unlocks and starts the challenge (EMP Pistol, Experimental Jellibolli Catchers, the first Beer Can)
            foreach (Creature2Entry creature in GameTableManager.Instance.Creature2.Entries)
            {
                var prerequisiteIds = new List<uint> { creature.PrerequisiteIdVisibility, creature.PrerequisiteIdAnimState };
                prerequisiteIds.AddRange(creature.PrerequisiteIdActivateSpells);

                foreach (uint prerequisiteId in prerequisiteIds.Where(p => p != 0u))
                {
                    PrerequisiteEntry prerequisite = GameTableManager.Instance.Prerequisite.GetEntry(prerequisiteId);
                    if (prerequisite == null)
                        continue;

                    for (int i = 0; i < prerequisite.PrerequisiteTypeId.Length; i++)
                    {
                        if (prerequisite.PrerequisiteTypeId[i] != PrerequisiteType.ChallengeLocked
                            || prerequisite.PrerequisiteComparisonId[i] != PrerequisiteComparison.NotEqual
                            || prerequisite.Value[i] != 0u)
                            continue;

                        uint challengeId = prerequisite.ObjectId[i];
                        if (GameTableManager.Instance.Challenge.GetEntry(challengeId) == null)
                            continue;

                        if (!challengeIndex.StartersByCreature.TryGetValue(creature.Id, out List<uint> challengeIds))
                        {
                            challengeIds = [];
                            challengeIndex.StartersByCreature.Add(creature.Id, challengeIds);
                        }

                        if (!challengeIds.Contains(challengeId))
                            challengeIds.Add(challengeId);
                    }
                }
            }

            return challengeIndex;
        }

        /// <summary>
        /// Zone the challenge is restricted to, the player must stay in it while the challenge runs.
        /// </summary>
        private static uint GetAreaZoneId(ChallengeEntry entry)
        {
            // TriggerVolume2IdRestriction volumes are server data that isn't in the client tables, the zone restriction is used instead
            return entry.WorldZoneIdRestriction != 0u ? entry.WorldZoneIdRestriction : entry.WorldZoneId;
        }

        private readonly IPlayer player;
        private readonly Dictionary<uint, Challenge> challenges = [];
        private readonly HashSet<uint> zoneIds = [];

        private SharedChallengePreference sharedChallengePreference = SharedChallengePreference.AutoAccept;
        private readonly Dictionary<uint, double> sharedOffers = [];

        private readonly UpdateTimer locationTimer = new(TimeSpan.FromSeconds(1d));
        private readonly List<IScriptCollection> stoppedScripts = [];

        /// <summary>
        /// Create a new <see cref="IChallengeManager"/> from existing <see cref="CharacterModel"/> database model.
        /// </summary>
        public ChallengeManager(IPlayer owner, CharacterModel model)
        {
            player = owner;

            foreach (CharacterChallengeModel challengeModel in model.Challenge)
            {
                ChallengeEntry entry = GameTableManager.Instance.Challenge.GetEntry(challengeModel.ChallengeId);
                if (entry == null)
                {
                    log.Warn($"Character {model.Id} has unknown challenge {challengeModel.ChallengeId}!");
                    continue;
                }

                challenges.Add(entry.Id, new Challenge(owner, entry, challengeModel));
            }
        }

        public void Save(CharacterContext context)
        {
            foreach (Challenge challenge in challenges.Values)
            {
                if (!challenge.IsDirty)
                    continue;

                var model = new CharacterChallengeModel
                {
                    Id                = player.CharacterId,
                    ChallengeId       = challenge.Entry.Id,
                    CompletionCount   = challenge.CompletionCount,
                    LastRewardTier    = challenge.LastRewardTier,
                    CooldownRemaining = (uint)challenge.CooldownRemaining
                };

                if (challenge.IsNew)
                    context.Add(model);
                else
                {
                    EntityEntry<CharacterChallengeModel> entity = context.Attach(model);
                    entity.Property(p => p.CompletionCount).IsModified   = true;
                    entity.Property(p => p.LastRewardTier).IsModified    = true;
                    entity.Property(p => p.CooldownRemaining).IsModified = true;
                }

                challenge.IsNew   = false;
                challenge.IsDirty = false;
            }
        }

        public void SendInitialPackets()
        {
            UpdateZoneIds();

            player.Session.EnqueueMessageEncrypted(new ServerChallengeUpdate
            {
                ActiveChallenges = challenges.Values
                    .Select(c => c.Build())
                    .ToList()
            });

            UpdateLocation();
        }

        private void UpdateZoneIds()
        {
            zoneIds.Clear();
            for (WorldZoneEntry zone = player.Zone; zone != null && zoneIds.Add(zone.Id); zone = zone.ParentZoneId != 0u ? GameTableManager.Instance.WorldZone.GetEntry(zone.ParentZoneId) : null)
            {
            }
        }

        /// <summary>
        /// Invoked when the player enters a new zone.
        /// </summary>
        public void OnZoneUpdate()
        {
            UpdateZoneIds();
            UpdateArea();
            UpdateLocation();
        }

        /// <summary>
        /// Invoked when the player logs out, running challenges fail and their T-spells are removed.
        /// </summary>
        public void OnLogout()
        {
            foreach (Challenge challenge in challenges.Values.Where(c => c.Activated).ToList())
                Fail(challenge, ChallengeResult.AbandonRemoveArrows, 0);

            UnloadStoppedScripts();
        }

        private void UnloadStoppedScripts()
        {
            if (stoppedScripts.Count == 0)
                return;

            IScriptManager scriptManager = LegacyServiceProvider.Provider.GetService<IScriptManager>();
            foreach (IScriptCollection collection in stoppedScripts)
                scriptManager.Unload(collection);
            stoppedScripts.Clear();
        }

        public void Update(double lastTick)
        {
            double elapsed = lastTick * 1000d;

            foreach (Challenge challenge in challenges.Values)
            {
                if (challenge.CooldownRemaining <= 0d)
                    continue;

                challenge.CooldownRemaining = Math.Max(challenge.CooldownRemaining - elapsed, 0d);
                challenge.IsDirty = true;
                if (challenge.CooldownRemaining <= 0d)
                    SendUpdate(challenge);
            }

            foreach (Challenge challenge in challenges.Values.Where(c => c.Activated).ToList())
            {
                challenge.Elapsed += elapsed;
                challenge.UpdateScripts(lastTick);
                if (!challenge.Activated)
                    continue;

                if (challenge.Elapsed >= challenge.Duration)
                {
                    // challenges that reached a tier before the timer ran out are completed on that tier
                    if (!challenge.IsTimeTiered && challenge.CurrentTier > 0u)
                        Finish(challenge, challenge.CurrentTier, ChallengeResult.TimerExpired);
                    else
                        Fail(challenge, ChallengeResult.TimerExpired, 0);
                    continue;
                }

                if (!challenge.LeftArea)
                    continue;

                challenge.AreaFailRemaining -= elapsed;
                if (challenge.AreaFailRemaining > 0d)
                    continue;

                if (!challenge.IsTimeTiered && challenge.CurrentTier > 0u)
                    Finish(challenge, challenge.CurrentTier, ChallengeResult.LeftArea);
                else
                    Fail(challenge, ChallengeResult.LeftArea, 0);
            }

            foreach ((uint challengeId, double remaining) in sharedOffers.ToList())
            {
                if (remaining - elapsed > 0d)
                {
                    sharedOffers[challengeId] = remaining - elapsed;
                    continue;
                }

                sharedOffers.Remove(challengeId);
                player.Session.EnqueueMessageEncrypted(new ServerChallengeShareTimeout
                {
                    ChallengeId = (ushort)challengeId
                });
            }

            locationTimer.Update(lastTick);
            if (locationTimer.HasElapsed)
            {
                UpdateLocation();
                locationTimer.Reset();
            }

            // scripts of challenges that stopped are unloaded once no script call is running
            UnloadStoppedScripts();
        }

        /// <summary>
        /// Leaving the area of a running challenge starts a short timer to come back before it fails.
        /// </summary>
        private void UpdateArea()
        {
            foreach (Challenge challenge in challenges.Values.Where(c => c.Activated))
            {
                bool inArea = IsInArea(challenge.Entry);
                if (!inArea && !challenge.LeftArea)
                {
                    challenge.LeftArea          = true;
                    challenge.AreaFailRemaining = AreaFailDuration.TotalMilliseconds;
                    SendUpdate(challenge);
                    SendResult(challenge.Entry.Id, ChallengeResult.LeftAreaWarning, 0);
                }
                else if (inArea && challenge.LeftArea)
                {
                    challenge.LeftArea = false;
                    SendUpdate(challenge);
                }
            }
        }

        /// <summary>
        /// Unlock challenges the player is exploring and finish General challenges whose goal is reaching a spot.
        /// </summary>
        private void UpdateLocation()
        {
            if (player.Map == null)
                return;

            foreach (uint zoneId in zoneIds)
            {
                if (!index.Value.ByZone.TryGetValue(zoneId, out List<ChallengeEntry> entries))
                    continue;

                foreach (ChallengeEntry entry in entries)
                {
                    if (challenges.ContainsKey(entry.Id))
                        continue;

                    if (IsNearUnlockLocation(entry))
                        Unlock(entry);
                }
            }

            foreach (Challenge challenge in challenges.Values.Where(c => c.Activated && IsReachChallenge(c.Entry)).ToList())
            {
                WorldLocation2Entry goal = GameTableManager.Instance.WorldLocation2.GetEntry(challenge.Entry.WorldLocation2IdIndicator);
                if (!IsAtLocation(goal, 3f, Math.Max(goal?.MaxVerticalDistance ?? 0f, 5f)))
                    continue;

                challenge.Count = Math.Max(challenge.Goal, 1u);
                FinishTimeOrCount(challenge);
            }
        }

        private bool IsInArea(ChallengeEntry entry)
        {
            uint zoneId = GetAreaZoneId(entry);
            return zoneId != 0u && zoneIds.Contains(zoneId);
        }

        private bool IsNearUnlockLocation(ChallengeEntry entry)
        {
            if (!IsInArea(entry))
                return false;

            uint locationId = entry.WorldLocation2IdStartLocation != 0u ? entry.WorldLocation2IdStartLocation : entry.WorldLocation2IdIndicator;
            if (locationId == 0u)
                return true;

            WorldLocation2Entry location = GameTableManager.Instance.WorldLocation2.GetEntry(locationId);
            return location == null || IsAtLocation(location, UnlockRange, float.MaxValue);
        }

        private bool IsAtStartLocation(ChallengeEntry entry)
        {
            if (entry.WorldLocation2IdStartLocation == 0u)
                return true;

            WorldLocation2Entry location = GameTableManager.Instance.WorldLocation2.GetEntry(entry.WorldLocation2IdStartLocation);
            return location == null || IsAtLocation(location, 5f, float.MaxValue);
        }

        private bool IsAtLocation(WorldLocation2Entry location, float minRadius, float maxHeight)
        {
            if (location == null || player.Map == null || location.WorldId != player.Map.Entry.Id)
                return false;

            float radius = Math.Max(location.Radius, minRadius);
            float dx     = player.Position.X - location.Position0;
            float dz     = player.Position.Z - location.Position2;
            return dx * dx + dz * dz <= radius * radius
                && Math.Abs(player.Position.Y - location.Position1) <= maxHeight;
        }

        /// <summary>
        /// General challenges with nothing to count whose indicator is a small spot, or a spot well above the start location,
        /// apart from the start location are races to that spot.
        /// </summary>
        private static bool IsReachChallenge(ChallengeEntry entry)
        {
            // timed races can name the vortices or currents used on the way as their target (Windrunner, Windy Climb)
            bool timeTiered = (((ChallengeFlags)entry.ChallengeFlags) & ChallengeFlags.TimeTiered) != 0;
            if ((ChallengeType)entry.ChallengeTypeEnum != ChallengeType.General
                || (entry.Target != 0u && !timeTiered)
                || entry.TargetGroupIdRewardPane != 0u
                || entry.WorldLocation2IdIndicator == 0u
                || entry.WorldLocation2IdIndicator == entry.WorldLocation2IdStartLocation)
                return false;

            WorldLocation2Entry indicator = GameTableManager.Instance.WorldLocation2.GetEntry(entry.WorldLocation2IdIndicator);
            if (indicator == null)
                return false;

            if (indicator.Radius <= ReachGoalMaxRadius)
                return true;

            WorldLocation2Entry start = GameTableManager.Instance.WorldLocation2.GetEntry(entry.WorldLocation2IdStartLocation);
            return start != null && indicator.Position1 - start.Position1 >= ClimbGoalMinHeight;
        }

        public bool Unlock(uint challengeId)
        {
            if (challenges.ContainsKey(challengeId))
                return true;

            ChallengeEntry entry = GameTableManager.Instance.Challenge.GetEntry(challengeId);
            if (entry == null)
                return false;

            Unlock(entry);
            return true;
        }

        private Challenge Unlock(ChallengeEntry entry)
        {
            var challenge = new Challenge(player, entry)
            {
                IsNew   = true,
                IsDirty = true
            };
            challenges.Add(entry.Id, challenge);

            // the client only accepts results for challenges it knows about
            SendUpdate(challenge);
            SendResult(entry.Id, ChallengeResult.Unlock, 0);

            // challenge starter objects hide once their challenge is unlocked
            player.VisibilityUpdate();
            return challenge;
        }

        /// <summary>
        /// Invoked when the player picks a challenge option in the client (start, abandon, accept or decline a shared challenge).
        /// </summary>
        public void HandleChoice(uint challengeId, ChallengeChoice choice)
        {
            switch (choice)
            {
                case ChallengeChoice.Activate:
                {
                    if (challenges.TryGetValue(challengeId, out Challenge challenge))
                        TryActivate(challenge, true, true);
                    break;
                }
                case ChallengeChoice.Abandon:
                {
                    if (challenges.TryGetValue(challengeId, out Challenge challenge) && challenge.Activated)
                        Fail(challenge, ChallengeResult.AbandonRemoveArrows, 0);
                    break;
                }
                case ChallengeChoice.AcceptShared:
                {
                    if (!sharedOffers.Remove(challengeId))
                        break;

                    ChallengeEntry entry = GameTableManager.Instance.Challenge.GetEntry(challengeId);
                    if (entry == null)
                        break;

                    if (!challenges.TryGetValue(challengeId, out Challenge challenge))
                        challenge = Unlock(entry);
                    TryActivate(challenge, true, false, false);
                    break;
                }
                case ChallengeChoice.DeclineShared:
                    sharedOffers.Remove(challengeId);
                    break;
                default:
                    log.Warn($"Unhandled challenge choice {choice} for challenge {challengeId}!");
                    break;
            }
        }

        public bool Activate(uint challengeId)
        {
            ChallengeEntry entry = GameTableManager.Instance.Challenge.GetEntry(challengeId);
            if (entry == null)
                return false;

            if (!challenges.TryGetValue(challengeId, out Challenge challenge))
                challenge = Unlock(entry);

            return TryActivate(challenge, true, true);
        }

        /// <summary>
        /// Start the supplied challenge the way its own targets start it (touching a flitterfly, using a starter object), silently
        /// doing nothing if it only starts by hand, is outside its area, on cooldown or already running.
        /// </summary>
        public bool TryAutoStart(uint challengeId)
        {
            ChallengeEntry entry = GameTableManager.Instance.Challenge.GetEntry(challengeId);
            if (entry == null || (((ChallengeFlags)entry.ChallengeFlags) & ChallengeFlags.ManualStart) != 0 || !IsInArea(entry))
                return false;

            if (!challenges.TryGetValue(challengeId, out Challenge challenge))
                challenge = Unlock(entry);

            return challenge.Activated || TryActivate(challenge, false, true, false);
        }

        /// <summary>
        /// Start the supplied challenge if it can be started, reporting why not when <paramref name="report"/> is set.
        /// </summary>
        /// <remarks>
        /// Challenges started from the challenge log must be started from their start location, the client only enables the
        /// start button there. Challenges started by killing or using their targets only need the player in their area.
        /// </remarks>
        private bool TryActivate(Challenge challenge, bool report, bool share, bool requireStartLocation = true)
        {
            if (challenge.Activated || !challenge.IsStartable)
                return false;

            ChallengeResult? failure = null;
            if (challenge.CooldownRemaining > 0d)
                failure = ChallengeResult.CooldownActive;
            else if (!IsInArea(challenge.Entry) || (requireStartLocation && !IsAtStartLocation(challenge.Entry)))
                failure = ChallengeResult.AreaRestriction;
            else if (challenges.Values.Any(c => c.Activated && c.Type == challenge.Type))
                failure = ChallengeResult.TypeAlreadyActive;

            if (failure != null)
            {
                if (report)
                    SendResult(challenge.Entry.Id, failure.Value, 0);
                return false;
            }

            challenge.Activated     = true;
            challenge.Count         = 0u;
            challenge.CurrentTier   = 0u;
            challenge.ChecklistMask = 0u;
            challenge.Elapsed       = 0d;
            challenge.LeftArea      = false;
            challenge.Goal          = GetGoal(challenge);

            SendUpdate(challenge);
            SendResult(challenge.Entry.Id, ChallengeResult.Activate, 0);

            // Ability challenges grant a temporary ability (T-spell), the client casts it with ClientCastObjectiveAbility
            if (challenge.Type == ChallengeType.Ability && challenge.Entry.Target != 0u)
                SendSpellShortcut(challenge, true);

            // challenge objects that are only usable while the challenge runs appear
            player.VisibilityUpdate();

            challenge.LoadScripts();

            if (share)
                Share(challenge);

            return true;
        }

        /// <summary>
        /// Count needed to finish the challenge.
        /// </summary>
        private uint GetGoal(Challenge challenge)
        {
            if (!challenge.IsTimeTiered)
                return challenge.Tiers.Length > 0 ? challenge.Tiers[^1] : 0u;

            // time tiered challenges are a race to a goal, the tiers are the time left
            if (IsReachChallenge(challenge.Entry))
                return 1u;

            if (challenge.Type == ChallengeType.ChecklistActivate)
                return CountChecklistObjects(challenge.Entry);

            // anything else is finished by a script
            return 0u;
        }

        /// <summary>
        /// Number of distinct checklist objects of a ChecklistActivate challenge around its indicator location.
        /// </summary>
        private uint CountChecklistObjects(ChallengeEntry entry)
        {
            WorldLocation2Entry location = GameTableManager.Instance.WorldLocation2.GetEntry(entry.WorldLocation2IdIndicator);
            Vector3 centre = location != null ? new Vector3(location.Position0, location.Position1, location.Position2) : player.Position;
            float radius   = Math.Max(location?.Radius ?? 0f, 100f);

            return (uint)player.Map.Search(centre, radius, new SearchCheckTargetGroup(entry.Target))
                .Select(e => e.QuestChecklistIdx)
                .Distinct()
                .Count();
        }

        private class SearchCheckTargetGroup : ISearchCheck<IWorldEntity>
        {
            private readonly uint targetGroupId;

            public SearchCheckTargetGroup(uint targetGroupId)
            {
                this.targetGroupId = targetGroupId;
            }

            public bool CheckEntity(IWorldEntity entity)
            {
                return AssetManager.Instance.GetTargetGroupsForCreatureId(entity.CreatureId).Contains(targetGroupId);
            }
        }

        /// <summary>
        /// Offer a challenge the player started to the members of their group.
        /// </summary>
        private void Share(Challenge challenge)
        {
            IGroup group = LegacyServiceProvider.Provider.GetService<IGroupManager>()?.GetGroup(player.Identity);
            if (group == null)
                return;

            foreach (IGroupMember member in group)
            {
                IPlayer memberPlayer = PlayerManager.Instance.GetPlayer(member.Identity);
                if (memberPlayer == null || memberPlayer == player || memberPlayer.Map == null || memberPlayer.Map != player.Map)
                    continue;

                memberPlayer.ChallengeManager.OnChallengeShared(challenge.Entry.Id, player);
            }
        }

        /// <summary>
        /// Invoked when a group member starts a challenge.
        /// </summary>
        public void OnChallengeShared(uint challengeId, IPlayer sharer)
        {
            ChallengeEntry entry = GameTableManager.Instance.Challenge.GetEntry(challengeId);
            if (entry == null || !IsInArea(entry))
                return;

            if (challenges.TryGetValue(challengeId, out Challenge challenge) && (challenge.Activated || challenge.CooldownRemaining > 0d || !challenge.IsStartable))
                return;

            switch (sharedChallengePreference)
            {
                case SharedChallengePreference.AutoAccept:
                    challenge ??= Unlock(entry);
                    TryActivate(challenge, false, false, false);
                    break;
                case SharedChallengePreference.Prompt:
                    // the client needs the challenge in its list before it can show the offer
                    if (challenge == null)
                        Unlock(entry);

                    sharedOffers[challengeId] = GameTableManager.Instance.GameFormula.GetEntry(ShareTimeoutFormulaId)?.Dataint0 ?? 30000u;
                    player.Session.EnqueueMessageEncrypted(new ServerChallengeShared
                    {
                        ChallengeId  = (ushort)challengeId,
                        SharerUnitId = sharer.Guid
                    });
                    break;
            }
        }

        /// <summary>
        /// Set how challenges shared by group members are handled.
        /// </summary>
        public void SetSharedChallengePreference(SharedChallengePreference preference)
        {
            sharedChallengePreference = preference;
        }

        /// <summary>
        /// Invoked when the player gets kill credit for an entity.
        /// </summary>
        public void OnKill(IUnitEntity entity)
        {
            if (entity is IPlayer)
                return;

            foreach (Challenge challenge in challenges.Values.Where(c => c.Activated).ToList())
                challenge.InvokeScripts(s => s.OnEntityKilled(entity));

            OnTarget(entity, ChallengeType.Combat, ChallengeType.Collect, ChallengeType.General);

            // Combat challenges whose target group isn't a list of creatures (factions, races, groups of other kinds)
            ImmutableList<uint> creatureGroups = AssetManager.Instance.GetTargetGroupsForCreatureId(entity.CreatureId);
            foreach (uint zoneId in zoneIds.ToList())
            {
                if (!index.Value.ByZone.TryGetValue(zoneId, out List<ChallengeEntry> entries))
                    continue;

                foreach (ChallengeEntry entry in entries)
                {
                    if ((ChallengeType)entry.ChallengeTypeEnum != ChallengeType.Combat
                        || entry.Target == 0u
                        || creatureGroups.Contains(entry.Target))
                        continue;

                    // groups made only of the unknown kind count any kill in the challenge area
                    if (HasKnownMembers(entry.Target, 0) && !MatchesTargetGroup(entity, entry.Target, 0))
                        continue;

                    Progress(entry, 1u, 0u);
                }
            }
        }

        /// <summary>
        /// Returns if a target group has members of a known kind, directly or through the groups it contains.
        /// </summary>
        private static bool HasKnownMembers(uint targetGroupId, int depth)
        {
            TargetGroupEntry targetGroup = GameTableManager.Instance.TargetGroup.GetEntry(targetGroupId);
            if (targetGroup == null || depth > 4)
                return false;

            return (TargetGroupType)targetGroup.Type switch
            {
                TargetGroupType.OtherTargetGroup or TargetGroupType.OtherTargetGroupCreatures
                    => targetGroup.DataEntries.Where(d => d != 0u).Any(e => HasKnownMembers(e, depth + 1)),
                TargetGroupType.Unknown9 => false,
                _ => true
            };
        }

        /// <summary>
        /// Returns if the supplied unit belongs to a target group of a kind that isn't a creature list.
        /// </summary>
        /// <remarks>
        /// TargetGroup types: 1 and 2 creature ids (matched through <see cref="AssetManager"/>), 3 faction ids, 12 creature race ids,
        /// 10 and 11 other target groups. Type 9 holds ids of an unknown kind (Skeech Slayer, Cavern Clearing, The Stillstalker Den),
        /// these challenges are all in caves or dens of their target, so a group made only of them counts any kill in the challenge area.
        /// </remarks>
        private static bool MatchesTargetGroup(IUnitEntity entity, uint targetGroupId, int depth)
        {
            TargetGroupEntry targetGroup = GameTableManager.Instance.TargetGroup.GetEntry(targetGroupId);
            if (targetGroup == null || depth > 4)
                return false;

            IEnumerable<uint> entries = targetGroup.DataEntries.Where(d => d != 0u);
            switch ((TargetGroupType)targetGroup.Type)
            {
                case TargetGroupType.FactionIdGroup:
                    return entries.Contains((uint)entity.Faction1);
                case TargetGroupType.CreatureRaceIdGroup:
                    return entity.CreatureInfo?.Entry != null && entries.Contains(entity.CreatureInfo.Entry.UnitRaceId);
                case TargetGroupType.OtherTargetGroup:
                case TargetGroupType.OtherTargetGroupCreatures:
                    return entries.Any(e => MatchesTargetGroup(entity, e, depth + 1));
                default:
                    return false;
            }
        }

        /// <summary>
        /// Invoked when the player successfully activates an entity.
        /// </summary>
        public void OnActivateSuccess(IWorldEntity entity)
        {
            // using a challenge starter object unlocks and starts its challenge
            if (index.Value.StartersByCreature.TryGetValue(entity.CreatureId, out List<uint> starterIds))
            {
                foreach (uint challengeId in starterIds)
                {
                    ChallengeEntry entry = GameTableManager.Instance.Challenge.GetEntry(challengeId);
                    if (entry == null)
                        continue;

                    if (!challenges.TryGetValue(challengeId, out Challenge challenge))
                        challenge = Unlock(entry);
                    TryActivate(challenge, true, true);
                }
            }

            // the starter object also counts for the challenge it started
            foreach (Challenge challenge in challenges.Values.Where(c => c.Activated).ToList())
                challenge.InvokeScripts(s => s.OnEntityActivated(entity));

            OnTarget(entity, ChallengeType.Collect, ChallengeType.ChecklistActivate, ChallengeType.General);
        }

        private void OnTarget(IWorldEntity entity, params ChallengeType[] types)
        {
            if (index.Value.ByCreature.TryGetValue(entity.CreatureId, out List<ChallengeEntry> named))
                foreach (ChallengeEntry entry in named.Where(e => types.Contains((ChallengeType)e.ChallengeTypeEnum)))
                    Progress(entry, 1u, 0u);

            foreach (uint targetGroupId in AssetManager.Instance.GetTargetGroupsForCreatureId(entity.CreatureId))
            {
                if (!index.Value.ByTargetGroup.TryGetValue(targetGroupId, out List<ChallengeEntry> entries))
                    continue;

                foreach (ChallengeEntry entry in entries)
                {
                    if (!types.Contains((ChallengeType)entry.ChallengeTypeEnum))
                        continue;

                    if (entry.ChallengeTypeEnum == (uint)ChallengeType.ChecklistActivate)
                        Progress(entry, 0u, entity.QuestChecklistMask);
                    else
                        Progress(entry, 1u, 0u);
                }
            }
        }

        /// <summary>
        /// Invoked when items arrive in the player's inventory.
        /// </summary>
        public void OnItemAdded(uint itemId, uint count)
        {
            if (!index.Value.ByItem.TryGetValue(itemId, out List<ChallengeEntry> entries))
                return;

            foreach (ChallengeEntry entry in entries)
                Progress(entry, count, 0u);
        }

        /// <summary>
        /// Invoked when a spell cast by the player lands an effect on a unit, <paramref name="firstHit"/> is set for the first unit of the cast.
        /// </summary>
        public void OnSpellEffectHit(uint spell4Id, IUnitEntity target, bool firstHit)
        {
            if (target == null || !index.Value.BySpell.TryGetValue(spell4Id, out List<ChallengeEntry> entries))
                return;

            foreach (ChallengeEntry entry in entries)
            {
                if ((ChallengeType)entry.ChallengeTypeEnum == ChallengeType.General)
                {
                    // General challenges count each cast of their action spell once
                    if (firstHit)
                        Progress(entry, 1u, 0u);
                    continue;
                }

                if (target is IPlayer || !challenges.TryGetValue(entry.Id, out Challenge challenge) || !challenge.Activated)
                    continue;

                // the reward pane group is what the T-spell has to be used on (Lopp children, dreadbots, Lightreach commoners)
                if (entry.TargetGroupIdRewardPane != 0u
                    && !AssetManager.Instance.GetTargetGroupsForCreatureId(target.CreatureId).Contains(entry.TargetGroupIdRewardPane))
                    continue;

                challenge.InvokeScripts(s => s.OnEntitySpellHit(spell4Id, target));
                Count(challenge, 1u, 0u);
            }
        }

        /// <summary>
        /// Returns if the supplied spell is a T-spell granted by one of the player's running challenges.
        /// </summary>
        public bool CanCastObjectiveAbility(uint spell4Id)
        {
            return challenges.Values.Any(c => c.Activated && c.Type == ChallengeType.Ability && c.Entry.Target == spell4Id);
        }

        /// <summary>
        /// Progress from an event in the world, challenges that don't need to be started by hand start on their first event in their area.
        /// </summary>
        private void Progress(ChallengeEntry entry, uint count, uint checklistMask)
        {
            if (!challenges.TryGetValue(entry.Id, out Challenge challenge))
            {
                if (!IsInArea(entry) || (((ChallengeFlags)entry.ChallengeFlags) & ChallengeFlags.ManualStart) != 0)
                    return;

                challenge = Unlock(entry);
            }

            if (!challenge.Activated)
            {
                if ((challenge.Flags & ChallengeFlags.ManualStart) != 0 || !TryActivate(challenge, false, true, false))
                    return;
            }

            Count(challenge, count, checklistMask);
        }

        public void UpdateProgress(uint challengeId, uint count = 1u)
        {
            if (challenges.TryGetValue(challengeId, out Challenge challenge) && challenge.Activated)
                Count(challenge, count, 0u);
        }

        private void Count(Challenge challenge, uint count, uint checklistMask)
        {
            if (challenge.Type == ChallengeType.ChecklistActivate)
            {
                if (checklistMask == 0u || (challenge.ChecklistMask & checklistMask) != 0u)
                    return;

                challenge.ChecklistMask |= checklistMask;
                challenge.Count = (uint)BitOperations.PopCount(challenge.ChecklistMask);
                // checklist objects used by the challenge change state
                player.VisibilityUpdate();
            }
            else
                challenge.Count += count;

            if (challenge.IsTimeTiered)
            {
                if (challenge.Goal != 0u && challenge.Count >= challenge.Goal)
                    FinishTimeOrCount(challenge);
                else
                    SendUpdate(challenge);
                return;
            }

            bool tierAchieved = false;
            while (challenge.CurrentTier < challenge.Tiers.Length && challenge.Count >= challenge.Tiers[challenge.CurrentTier])
            {
                challenge.CurrentTier++;
                tierAchieved = true;
            }

            if (challenge.CurrentTier >= challenge.Tiers.Length && challenge.Tiers.Length > 0)
            {
                Finish(challenge, challenge.CurrentTier, ChallengeResult.Completed);
                return;
            }

            SendUpdate(challenge);
            if (tierAchieved)
                SendResult(challenge.Entry.Id, ChallengeResult.TierAchieved, (int)challenge.CurrentTier - 1);
        }

        public void Complete(uint challengeId)
        {
            if (challenges.TryGetValue(challengeId, out Challenge challenge) && challenge.Activated)
            {
                if (!challenge.IsTimeTiered)
                    challenge.CurrentTier = Math.Max(challenge.CurrentTier, 1u);
                FinishTimeOrCount(challenge);
            }
        }

        private void FinishTimeOrCount(Challenge challenge)
        {
            uint tier = challenge.IsTimeTiered ? challenge.GetTimeTier() : challenge.CurrentTier;
            if (tier == 0u)
                Fail(challenge, ChallengeResult.TimerExpired, 0);
            else
                Finish(challenge, tier, ChallengeResult.Completed);
        }

        public void Fail(uint challengeId, uint localizedTextId = 0u)
        {
            if (challenges.TryGetValue(challengeId, out Challenge challenge) && challenge.Activated)
                Fail(challenge, ChallengeResult.GenericFail, (int)localizedTextId);
        }

        /// <summary>
        /// Complete a running challenge on the supplied tier (1 bronze to 3 gold).
        /// </summary>
        private void Finish(Challenge challenge, uint tier, ChallengeResult result)
        {
            challenge.Activated         = false;
            challenge.LeftArea          = false;
            challenge.CurrentTier       = tier;
            challenge.LastRewardTier    = tier;
            challenge.CompletionCount++;
            challenge.CooldownRemaining = CooldownDuration.TotalMilliseconds;
            challenge.IsDirty           = true;

            OnStop(challenge, true);

            // the client shows timer and area results as a completion when the challenge has a tier
            SendUpdate(challenge);
            SendResult(challenge.Entry.Id, result, result == ChallengeResult.Completed ? (int)tier - 1 : 0);

            uint points = GetRewardPoints(tier);
            if (challenge.Entry.RewardTrackId != 0u && points > 0u)
                player.RewardTrackManager.AddPoints(challenge.Entry.RewardTrackId, points);

            player.QuestManager.ObjectiveUpdate(QuestObjectiveType.CompleteChallenge, 0u, 1u);
        }

        /// <summary>
        /// Reward track points for completing a challenge on the supplied tier, the sum of the points of every tier up to it.
        /// </summary>
        private static uint GetRewardPoints(uint tier)
        {
            GameFormulaEntry formula = GameTableManager.Instance.GameFormula.GetEntry(RewardPointsFormulaId);
            if (formula == null)
                return 0u;

            uint[] tierPoints = [formula.Dataint0, formula.Dataint01, formula.Dataint02, formula.Dataint03, formula.Dataint04];
            return (uint)tierPoints.Take((int)Math.Min(tier, (uint)tierPoints.Length)).Sum(p => p);
        }

        private void Fail(Challenge challenge, ChallengeResult result, int data)
        {
            challenge.Activated   = false;
            challenge.LeftArea    = false;
            challenge.CurrentTier = 0u;

            OnStop(challenge, false);

            SendUpdate(challenge);
            SendResult(challenge.Entry.Id, result, data);
        }

        private void OnStop(Challenge challenge, bool completed)
        {
            IScriptCollection collection = challenge.StopScripts(completed);
            if (collection != null)
                stoppedScripts.Add(collection);

            if (challenge.Type == ChallengeType.Ability && challenge.Entry.Target != 0u)
                SendSpellShortcut(challenge, false);

            player.VisibilityUpdate();
        }

        private void SendSpellShortcut(Challenge challenge, bool add)
        {
            player.Session.EnqueueMessageEncrypted(new ServerQuestSpellShortcut
            {
                SpellId   = challenge.Entry.Target,
                Reason    = SpellShortcutReasonChallenge,
                SourceId  = challenge.Entry.Id,
                AddRemove = add
            });
        }

        private void SendUpdate(Challenge challenge)
        {
            player.Session.EnqueueMessageEncrypted(new ServerChallengeUpdate
            {
                ActiveChallenges = [challenge.Build()]
            });
        }

        private void SendResult(uint challengeId, ChallengeResult result, int data)
        {
            player.Session.EnqueueMessageEncrypted(new ServerChallengeResult
            {
                ChallengeId = (ushort)challengeId,
                Result      = result,
                Data        = data
            });
        }

        public bool IsUnlocked(uint challengeId) => challenges.ContainsKey(challengeId);

        public bool IsActive(uint challengeId) => challenges.TryGetValue(challengeId, out Challenge challenge) && challenge.Activated;

        public uint GetCompletionCount(uint challengeId) => challenges.TryGetValue(challengeId, out Challenge challenge) ? challenge.CompletionCount : 0u;

        public uint GetCurrentTier(uint challengeId) => challenges.TryGetValue(challengeId, out Challenge challenge) && challenge.Activated ? challenge.CurrentTier : 0u;

        public uint GetCurrentCount(uint challengeId) => challenges.TryGetValue(challengeId, out Challenge challenge) && challenge.Activated ? challenge.Count : 0u;

        /// <summary>
        /// Returns if the supplied entity has already been used by a running checklist challenge.
        /// </summary>
        public bool IsChecklistObjectUsed(uint challengeId, IWorldEntity entity)
        {
            return entity != null
                && challenges.TryGetValue(challengeId, out Challenge challenge)
                && challenge.Activated
                && (challenge.ChecklistMask & entity.QuestChecklistMask) != 0u;
        }
    }
}
