using Microsoft.Extensions.DependencyInjection;
using NexusForever.Database.Character.Model;
using NexusForever.Game.Abstract.Challenges;
using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Static.Challenges;
using NexusForever.GameTable;
using NexusForever.GameTable.Model;
using NexusForever.Script;
using NexusForever.Script.Template;
using NexusForever.Script.Template.Collection;
using NexusForever.Shared;
using NetworkChallenge = NexusForever.Network.World.Message.Model.Challenges.ServerChallengeUpdate.Challenge;

namespace NexusForever.Game.Challenges
{
    /// <summary>
    /// State of a challenge the player has unlocked.
    /// </summary>
    public class Challenge : IChallenge
    {
        public ChallengeEntry Entry { get; }
        public IPlayer Player { get; }
        public ChallengeType Type => (ChallengeType)Entry.ChallengeTypeEnum;
        public ChallengeFlags Flags => (ChallengeFlags)Entry.ChallengeFlags;
        public bool IsTimeTiered => (Flags & ChallengeFlags.TimeTiered) != 0;

        /// <summary>
        /// Goal of each tier, a count or for time tiered challenges the milliseconds that must be left on the timer.
        /// </summary>
        public uint[] Tiers { get; }

        public uint CompletionCount { get; set; }

        /// <summary>
        /// Tier the challenge was last completed on, 1 for bronze to 3 for gold.
        /// </summary>
        public uint LastRewardTier { get; set; }

        /// <summary>
        /// Milliseconds of cooldown left, the cooldown only runs while the player is online.
        /// </summary>
        public double CooldownRemaining { get; set; }

        public bool Activated { get; set; }
        public uint Count { get; set; }

        /// <summary>
        /// Tiers reached in the current or last run, 1 for bronze to 3 for gold.
        /// </summary>
        public uint CurrentTier { get; set; }

        /// <summary>
        /// Checklist bits of the objects used by a running ChecklistActivate challenge.
        /// </summary>
        public uint ChecklistMask { get; set; }

        /// <summary>
        /// Count needed to finish the challenge, 0 if the goal is decided by a script.
        /// </summary>
        public uint Goal { get; set; }

        public double Elapsed { get; set; }
        public double Duration { get; }
        public bool LeftArea { get; set; }
        public double AreaFailRemaining { get; set; }

        public bool IsNew { get; set; }
        public bool IsDirty { get; set; }

        private IScriptCollection scriptCollection;

        public Challenge(IPlayer player, ChallengeEntry entry)
        {
            Player   = player;
            Entry    = entry;
            Tiers    = new[] { entry.ChallengeTierId00, entry.ChallengeTierId01, entry.ChallengeTierId02 }
                .TakeWhile(t => t != 0u)
                .Select(t => GameTableManager.Instance.ChallengeTier.GetEntry(t)?.Count ?? 0u)
                .ToArray();
            Duration = GetDuration();
        }

        public Challenge(IPlayer player, ChallengeEntry entry, CharacterChallengeModel model)
            : this(player, entry)
        {
            CompletionCount   = model.CompletionCount;
            LastRewardTier    = model.LastRewardTier;
            CooldownRemaining = model.CooldownRemaining;
        }

        /// <summary>
        /// Load the scripts of the challenge, they run while the challenge runs.
        /// </summary>
        public void LoadScripts()
        {
            IScriptManager scriptManager = LegacyServiceProvider.Provider.GetService<IScriptManager>();
            scriptCollection = scriptManager.InitialiseOwnedCollection<IChallenge>(this);
            scriptManager.InitialiseOwnedScripts<IChallenge>(scriptCollection, Entry.Id);
            scriptCollection.Invoke<IChallengeScript>(s => s.OnActivate());
        }

        /// <summary>
        /// Invoke the challenge scripts each world tick while the challenge runs.
        /// </summary>
        public void UpdateScripts(double lastTick)
        {
            scriptCollection?.Invoke<IUpdate>(s => s.Update(lastTick));
        }

        public void InvokeScripts(Action<IChallengeScript> action)
        {
            scriptCollection?.Invoke(action);
        }

        /// <summary>
        /// Notify the challenge scripts that the challenge stopped and unload them.
        /// </summary>
        /// <returns>The unloaded collection, to be released once no script call is running.</returns>
        public IScriptCollection StopScripts(bool completed)
        {
            IScriptCollection collection = scriptCollection;
            scriptCollection = null;
            collection?.Invoke<IChallengeScript>(s => s.OnStop(completed));
            return collection;
        }

        /// <summary>
        /// Time limit of the challenge in milliseconds.
        /// </summary>
        /// <remarks>
        /// Not in the client data. Time tiered challenges need at least their gold time left at the end, a few of them have tiers
        /// over 5 minutes (Entrance Exam asks for 9.5 minutes left), those get a minute on top of their gold time.
        /// </remarks>
        private double GetDuration()
        {
            // Speedy Delivery: "Make as many deliveries as you can in 3 minutes", the only challenge that states its time limit
            if (Entry.Id == 345u)
                return TimeSpan.FromMinutes(3d).TotalMilliseconds;

            double duration = ChallengeManager.DefaultDuration.TotalMilliseconds;
            if (IsTimeTiered && Tiers.Length > 0 && Tiers.Max() + 60000d > duration)
                duration = Math.Ceiling((Tiers.Max() + 60000d) / 60000d) * 60000d;
            return duration;
        }

        /// <summary>
        /// Returns if the challenge can still be completed, some can only be completed a limited number of times.
        /// </summary>
        public bool IsStartable => Entry.CompletionCount == uint.MaxValue || CompletionCount < Entry.CompletionCount;

        /// <summary>
        /// Tier reached when finishing a time tiered challenge now, from the time left on the timer.
        /// </summary>
        public uint GetTimeTier()
        {
            double remaining = Math.Max(Duration - Elapsed, 0d);
            return (uint)Tiers.Count(t => remaining >= t);
        }

        public NetworkChallenge Build()
        {
            uint finalGoal = Tiers.Length > 0 ? Tiers[^1] : 0u;
            uint goal      = IsTimeTiered ? Goal : Tiers.Length > 0 ? Tiers[Math.Min((int)CurrentTier, Tiers.Length - 1)] : Goal;

            uint objectiveCompletion;
            if (Type == ChallengeType.ChecklistActivate)
                objectiveCompletion = ChecklistMask;
            else if (IsTimeTiered)
                objectiveCompletion = Goal > 0u ? Math.Min(Count * 100u / Goal, 100u) : 0u;
            else
                // percentage of the final tier, the client shows Combat challenge progress from it
                objectiveCompletion = finalGoal > 0u ? Math.Min(Count * 100u / finalGoal, 100u) : 0u;

            return new NetworkChallenge
            {
                ChallengeId         = Entry.Id,
                Type                = Type,
                // the client marks the target group on the map, collect challenges mark the creatures that drop the item
                TargetGroupId       = Type is ChallengeType.Combat or ChallengeType.ChecklistActivate ? Entry.Target : Entry.TargetGroupIdRewardPane,
                CurrentCount        = Count,
                GoalCount           = goal,
                ObjectiveCompletion = objectiveCompletion,
                CurrentTier         = CurrentTier,
                LastRewardTier      = LastRewardTier,
                CompletionCount     = CompletionCount,
                Unlocked            = true,
                Activated           = Activated,
                OnCooldown          = CooldownRemaining > 0d,
                LeftArea            = LeftArea,
                // timers are time left, the client adds its own clock to them
                TimeActivatedDt     = Activated ? (uint)Math.Max(Duration - Elapsed, 0d) : 0u,
                TimeTotalActive     = (uint)Duration,
                TimeCooldownDt      = (uint)CooldownRemaining,
                TimeTotalCooldown   = (uint)ChallengeManager.CooldownDuration.TotalMilliseconds,
                TimeAreaFailDt      = LeftArea ? (uint)AreaFailRemaining : 0u,
                TimeTotalAreaFail   = (uint)ChallengeManager.AreaFailDuration.TotalMilliseconds,
                TierGoalCount       = [Tiers.ElementAtOrDefault(0), Tiers.ElementAtOrDefault(1), Tiers.ElementAtOrDefault(2)]
            };
        }
    }
}
