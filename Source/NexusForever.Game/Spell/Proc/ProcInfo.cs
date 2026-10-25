using Microsoft.Extensions.Logging;
using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Prerequisite;
using NexusForever.Game.Abstract.Spell.Effect.Data;
using NexusForever.Game.Abstract.Spell.Proc;
using NexusForever.Game.Static.Spell.Proc;
using NexusForever.GameTable;
using NexusForever.GameTable.Model;
using NexusForever.Shared.Game;

namespace NexusForever.Game.Spell.Proc
{
    public class ProcInfo : IProcInfo
    {
        /// <summary>
        /// Proc flag to cast the proc spell on the unit that triggered it, eg: "Landing a direct heal grants X to the target".
        /// </summary>
        private const uint FlagCastOnTarget = 0x04;

        /// <summary>
        /// Proc filter type limiting the proc to spells in a Spell4GroupList, eg: "Landing a hit with a Consumer".
        /// </summary>
        private const uint FilterSpellGroupList = 2;

        public IUnitEntity Owner { get; private set; }
        public ISpellEffectProcData Data { get; private set; }
        public ProcType Type { get; private set; }

        /// <summary>
        /// Internal cooldown, "Can only occur every Xs".
        /// </summary>
        private UpdateTimer cooldownTimer;

        #region Dependency Injection

        private readonly ILogger<ProcInfo> log;
        private readonly IPrerequisiteManager prerequisiteManager;
        private readonly IGameTableManager gameTableManager;

        public ProcInfo(
            ILogger<ProcInfo> log,
            IPrerequisiteManager prerequisiteManager,
            IGameTableManager gameTableManager)
        {
            this.log                 = log;
            this.prerequisiteManager = prerequisiteManager;
            this.gameTableManager    = gameTableManager;
        }

        #endregion

        /// <summary>
        /// Initialise <see cref="IProcInfo"/> with supplied <see cref="IUnitEntity"/> owner and <see cref="ISpellEffectProcData"/>.
        /// </summary>
        public void Initialise(IUnitEntity owner, ISpellEffectProcData data)
        {
            if (Owner != null)
                throw new InvalidOperationException("ProcInfo already initialised.");

            Owner         = owner;
            Data          = data;
            Type          = data.ProcType;

            cooldownTimer = new UpdateTimer(TimeSpan.FromMilliseconds(Data.TriggerTime).TotalSeconds, false);
        }

        /// <summary>
        /// Invoked each world tick with the delta since the previous tick occurred.
        /// </summary>
        public void Update(double lastTick)
        {
            if (!cooldownTimer.IsTicking)
                return;

            cooldownTimer.Update(lastTick);
            if (cooldownTimer.HasElapsed)
                cooldownTimer.Reset(false);
        }

        /// <summary>
        /// Trigger the <see cref="IProcInfo"/> with supplied <see cref="IProcParameters"/>.
        /// </summary>
        public void Trigger(IProcParameters parameters)
        {
            if (!CanTrigger(parameters))
                return;

            if (Data.TriggerTime > 0u)
                cooldownTimer.Reset(true);

            log.LogTrace($"Triggering Proc {Data.Entry.Id} of {Type}.");

            uint primaryTargetId = (Data.Flags & FlagCastOnTarget) != 0 && parameters.Target != null ? parameters.Target.Guid : 0u;
            Owner.CastSpell(Data.SpellId, new SpellParameters
            {
                PrimaryTargetId        = primaryTargetId,
                UserInitiatedSpellCast = false,
                IsProcTriggered        = true
            });
        }

        private bool CanTrigger(IProcParameters parameters)
        {
            if (cooldownTimer.IsTicking)
                return false;

            if (Data.TargetPrerequisiteId != 0)
            {
                // TODO: once the prerequisite system is updated to handle IUnitEntity, this needs to be updated
                if (parameters.Target is IPlayer player && !prerequisiteManager.Meets(player, Data.TargetPrerequisiteId))
                    return false;
            }

            if (Data.CasterPrerequisiteId != 0)
            {
                if (Owner is IPlayer player && !prerequisiteManager.Meets(player, Data.CasterPrerequisiteId))
                    return false;
            }

            if (Data.FilterType == FilterSpellGroupList && Data.FilterValue != 0u)
            {
                Spell4GroupListEntry groupList = gameTableManager.Spell4GroupList.GetEntry(Data.FilterValue);
                if (groupList != null)
                {
                    if (parameters.SpellInfo == null || !groupList.SpellGroupIds.Any(g => g != 0u && parameters.SpellInfo.SpellGroups.Contains(g)))
                        return false;
                }
            }

            if (Data.Chance < 1f && Random.Shared.NextSingle() >= Data.Chance)
                return false;

            return true;
        }
    }
}
