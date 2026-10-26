using Microsoft.Extensions.Logging;
using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Prerequisite;
using NexusForever.Game.Abstract.Quest;
using NexusForever.Game.Static.Prerequisite;
using NexusForever.Game.Static.Quest;
using NexusForever.Game.Static.Reputation;

namespace NexusForever.Game.Prerequisite.Check
{
    /// <summary>
    /// Shared comparison for prerequisites that compare a number against the value.
    /// </summary>
    public abstract class PrerequisiteCheckCompare : IPrerequisiteCheck
    {
        private readonly ILogger log;
        private readonly PrerequisiteType type;

        protected PrerequisiteCheckCompare(ILogger log, PrerequisiteType type)
        {
            this.log  = log;
            this.type = type;
        }

        public virtual bool Meets(IPlayer player, PrerequisiteComparison comparison, uint value, uint objectId, IPrerequisiteParameters parameters)
        {
            long actual = GetValue(player, objectId, value);
            switch (comparison)
            {
                case PrerequisiteComparison.Equal:
                    return actual == value;
                case PrerequisiteComparison.NotEqual:
                    return actual != value;
                case PrerequisiteComparison.GreaterThan:
                    return actual > value;
                case PrerequisiteComparison.GreaterThanOrEqual:
                    return actual >= value;
                case PrerequisiteComparison.LessThan:
                    return actual < value;
                case PrerequisiteComparison.LessThanOrEqual:
                    return actual <= value;
                default:
                    log.LogWarning($"Unhandled PrerequisiteComparison {comparison} for {type}!");
                    return false;
            }
        }

        /// <summary>
        /// Return the number the value is compared against.
        /// </summary>
        protected abstract long GetValue(IPlayer player, uint objectId, uint value);
    }

    /// <remarks>
    /// Value 0 compares against "not moving" (eg: "Moving, Equal, 0" = standing still).
    /// </remarks>
    [PrerequisiteCheck(PrerequisiteType.Moving)]
    public class PrerequisiteCheckMoving : PrerequisiteCheckCompare
    {
        public PrerequisiteCheckMoving(ILogger<PrerequisiteCheckMoving> log)
            : base(log, PrerequisiteType.Moving)
        {
        }

        protected override long GetValue(IPlayer player, uint objectId, uint value)
        {
            return player.MovementManager.GetVelocity().LengthSquared() > 0.01f ? 1 : 0;
        }
    }

    /// <remarks>
    /// A roll of 0 to 99 against the value (eg: "RandomPercent, LessThan, 10" = 10% chance).
    /// </remarks>
    [PrerequisiteCheck(PrerequisiteType.RandomPercent)]
    public class PrerequisiteCheckRandomPercent : PrerequisiteCheckCompare
    {
        public PrerequisiteCheckRandomPercent(ILogger<PrerequisiteCheckRandomPercent> log)
            : base(log, PrerequisiteType.RandomPercent)
        {
        }

        protected override long GetValue(IPlayer player, uint objectId, uint value)
        {
            return Random.Shared.Next(100);
        }
    }

    /// <remarks>
    /// Value is an Item2 id, "Equal" when the player carries one (eg: hoverboard items gating their unlock).
    /// </remarks>
    [PrerequisiteCheck(PrerequisiteType.Inventory)]
    public class PrerequisiteCheckInventory : IPrerequisiteCheck
    {
        private readonly ILogger<PrerequisiteCheckInventory> log;

        public PrerequisiteCheckInventory(ILogger<PrerequisiteCheckInventory> log)
        {
            this.log = log;
        }

        public bool Meets(IPlayer player, PrerequisiteComparison comparison, uint value, uint objectId, IPrerequisiteParameters parameters)
        {
            bool has = player.Inventory.GetItemCount(value) > 0u;
            switch (comparison)
            {
                case PrerequisiteComparison.Equal:
                    return has;
                case PrerequisiteComparison.NotEqual:
                    return !has;
                default:
                    log.LogWarning($"Unhandled PrerequisiteComparison {comparison} for {PrerequisiteType.Inventory}!");
                    return false;
            }
        }
    }

    /// <remarks>
    /// Same data as <see cref="PrerequisiteType.BaseFaction"/> (value 166 Dominion, 167 Exile), used beside it.
    /// </remarks>
    [PrerequisiteCheck(PrerequisiteType.Faction243)]
    public class PrerequisiteCheckFaction243 : IPrerequisiteCheck
    {
        private readonly ILogger<PrerequisiteCheckFaction243> log;

        public PrerequisiteCheckFaction243(ILogger<PrerequisiteCheckFaction243> log)
        {
            this.log = log;
        }

        public bool Meets(IPlayer player, PrerequisiteComparison comparison, uint value, uint objectId, IPrerequisiteParameters parameters)
        {
            switch (comparison)
            {
                case PrerequisiteComparison.Equal:
                    return player.Faction1 == (Faction)value;
                case PrerequisiteComparison.NotEqual:
                    return player.Faction1 != (Faction)value;
                default:
                    log.LogWarning($"Unhandled PrerequisiteComparison {comparison} for {PrerequisiteType.Faction243}!");
                    return false;
            }
        }
    }

    /// <remarks>
    /// The level before mentor scaling, the same as the player level here.
    /// </remarks>
    [PrerequisiteCheck(PrerequisiteType.TrueLevel)]
    public class PrerequisiteCheckTrueLevel : PrerequisiteCheckCompare
    {
        public PrerequisiteCheckTrueLevel(ILogger<PrerequisiteCheckTrueLevel> log)
            : base(log, PrerequisiteType.TrueLevel)
        {
        }

        protected override long GetValue(IPlayer player, uint objectId, uint value)
        {
            return player.Level;
        }
    }

    /// <remarks>
    /// Value is the path mission, "Equal" while the mission is unlocked and not complete (eg: "scan this creature while the mission runs").
    /// </remarks>
    [PrerequisiteCheck(PrerequisiteType.PathMissionRequirement)]
    public class PrerequisiteCheckPathMissionRequirement : IPrerequisiteCheck
    {
        private readonly ILogger<PrerequisiteCheckPathMissionRequirement> log;

        public PrerequisiteCheckPathMissionRequirement(ILogger<PrerequisiteCheckPathMissionRequirement> log)
        {
            this.log = log;
        }

        public bool Meets(IPlayer player, PrerequisiteComparison comparison, uint value, uint objectId, IPrerequisiteParameters parameters)
        {
            IPathMission mission = player.PathMissionManager.GetMission(value);
            bool active = mission != null && !mission.Completed;
            switch (comparison)
            {
                case PrerequisiteComparison.Equal:
                    return active;
                case PrerequisiteComparison.NotEqual:
                    return !active;
                default:
                    log.LogWarning($"Unhandled PrerequisiteComparison {comparison} for {PrerequisiteType.PathMissionRequirement}!");
                    return false;
            }
        }
    }

    /// <summary>
    /// QuestObjective0 to QuestObjective5: progress of that objective index of the quest (object id) compared to the value.
    /// </summary>
    /// <remarks>
    /// An object id of 0 refers to the quest of the context (unknown here) and counts as met. Objectives of a completed quest count as full,
    /// checklist objectives count their set bits.
    /// </remarks>
    public abstract class PrerequisiteCheckQuestObjectiveProgress : PrerequisiteCheckCompare
    {
        private readonly byte index;

        protected PrerequisiteCheckQuestObjectiveProgress(ILogger log, PrerequisiteType type, byte index)
            : base(log, type)
        {
            this.index = index;
        }

        public override bool Meets(IPlayer player, PrerequisiteComparison comparison, uint value, uint objectId, IPrerequisiteParameters parameters)
        {
            return objectId == 0u || base.Meets(player, comparison, value, objectId, parameters);
        }

        protected override long GetValue(IPlayer player, uint objectId, uint value)
        {
            IQuestInfo info = Quest.GlobalQuestManager.Instance.GetQuestInfo((ushort)objectId);
            if (info == null || index >= info.Objectives.Count)
                return 0;

            if (player.QuestManager.GetQuestState((ushort)objectId) is QuestState.Completed or QuestState.Achieved)
                return info.Objectives[index].Entry.Count;

            IQuestObjective objective = player.QuestManager.GetActiveQuest((ushort)objectId)?.GetQuestObjectiveByIndex(index);
            if (objective == null)
                return 0;

            return objective.ObjectiveInfo.Type is QuestObjectiveType.ActivateTargetGroup
                or QuestObjectiveType.ActivateTargetGroupChecklist
                or QuestObjectiveType.TalkToTargetGroup
                or QuestObjectiveType.Unknown10
                ? System.Numerics.BitOperations.PopCount(objective.Progress)
                : objective.Progress;
        }
    }

    [PrerequisiteCheck(PrerequisiteType.QuestObjective0)]
    public class PrerequisiteCheckQuestObjective0 : PrerequisiteCheckQuestObjectiveProgress
    {
        public PrerequisiteCheckQuestObjective0(ILogger<PrerequisiteCheckQuestObjective0> log) : base(log, PrerequisiteType.QuestObjective0, 0) { }
    }

    [PrerequisiteCheck(PrerequisiteType.QuestObjective1)]
    public class PrerequisiteCheckQuestObjective1 : PrerequisiteCheckQuestObjectiveProgress
    {
        public PrerequisiteCheckQuestObjective1(ILogger<PrerequisiteCheckQuestObjective1> log) : base(log, PrerequisiteType.QuestObjective1, 1) { }
    }

    [PrerequisiteCheck(PrerequisiteType.QuestObjective2)]
    public class PrerequisiteCheckQuestObjective2 : PrerequisiteCheckQuestObjectiveProgress
    {
        public PrerequisiteCheckQuestObjective2(ILogger<PrerequisiteCheckQuestObjective2> log) : base(log, PrerequisiteType.QuestObjective2, 2) { }
    }

    [PrerequisiteCheck(PrerequisiteType.QuestObjective3)]
    public class PrerequisiteCheckQuestObjective3 : PrerequisiteCheckQuestObjectiveProgress
    {
        public PrerequisiteCheckQuestObjective3(ILogger<PrerequisiteCheckQuestObjective3> log) : base(log, PrerequisiteType.QuestObjective3, 3) { }
    }

    [PrerequisiteCheck(PrerequisiteType.QuestObjective4)]
    public class PrerequisiteCheckQuestObjective4 : PrerequisiteCheckQuestObjectiveProgress
    {
        public PrerequisiteCheckQuestObjective4(ILogger<PrerequisiteCheckQuestObjective4> log) : base(log, PrerequisiteType.QuestObjective4, 4) { }
    }

    [PrerequisiteCheck(PrerequisiteType.QuestObjective5)]
    public class PrerequisiteCheckQuestObjective5 : PrerequisiteCheckQuestObjectiveProgress
    {
        public PrerequisiteCheckQuestObjective5(ILogger<PrerequisiteCheckQuestObjective5> log) : base(log, PrerequisiteType.QuestObjective5, 5) { }
    }
}
