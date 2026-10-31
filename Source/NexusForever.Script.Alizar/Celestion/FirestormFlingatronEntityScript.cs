using System.Numerics;
using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Entity.Creature;
using NexusForever.Game.Abstract.Quest;
using NexusForever.Game.Static.Quest;
using NexusForever.Script.Template;
using NexusForever.Script.Template.Filter;
using NexusForever.Shared;

namespace NexusForever.Script.Alizar.Celestion
{
    /// <summary>
    /// Firestorm Flingatron catapults at Point Firestorm (Greenbough Guardian, "Call a Greenbough Guardian to destroy Firestorm Flingatron catapults").
    /// </summary>
    /// <remarks>
    /// The quest's Call of the Guardian ability (34493) only sends a ravel signal and the catapults can't be targeted, the retail script
    /// that brought the Guardian down on them isn't in the client data (report 84). A player on the quest who comes up to a catapult
    /// calls a Greenbough Guardian beside it, the Guardian smashes it and the catapult's checklist bit is set. Range and timings guessed.
    /// </remarks>
    [ScriptFilterCreatureId(FlingatronCreatureId)]
    public class FirestormFlingatronEntityScript : IWorldEntityScript, IOwnedScript<IWorldEntity>
    {
        private const uint FlingatronCreatureId = 30840;
        private const uint GuardianCreatureId   = 51696;
        private const ushort GreenboughGuardian = 6672;

        private const float CallRange = 12f;
        private const double SmashDelay = 3d;
        private const double GuardianDuration = 10d;

        private IWorldEntity owner;
        private readonly List<(ulong CharacterId, uint PlayerGuid, double Timer)> pending = [];
        private readonly HashSet<ulong> credited = [];
        private IWorldEntity guardian;
        private double guardianTimer;

        #region Dependency Injection

        private readonly ICreatureInfoManager creatureInfoManager;

        public FirestormFlingatronEntityScript(
            ICreatureInfoManager creatureInfoManager)
        {
            this.creatureInfoManager = creatureInfoManager;
        }

        #endregion

        /// <summary>
        /// Invoked when <see cref="IScript"/> is loaded.
        /// </summary>
        public void OnLoad(IWorldEntity owner)
        {
            this.owner = owner;
            owner.SetInRangeCheck(CallRange);
        }

        /// <summary>
        /// Invoked when <see cref="IGridEntity"/> is added to range check range.
        /// </summary>
        public void OnEnterRange(IGridEntity entity)
        {
            if (entity is not IPlayer player || credited.Contains(player.CharacterId) || pending.Any(p => p.CharacterId == player.CharacterId))
                return;

            IQuestObjective objective = player.QuestManager.GetActiveQuest(GreenboughGuardian)?
                .FirstOrDefault(o => o.ObjectiveInfo.Type == QuestObjectiveType.Unknown10 && o.ObjectiveInfo.Entry.Data == FlingatronCreatureId);
            if (objective == null || objective.IsComplete() || (objective.Progress & owner.QuestChecklistMask) != 0u)
                return;

            SummonGuardian(player);
            pending.Add((player.CharacterId, player.Guid, SmashDelay));
        }

        private void SummonGuardian(IPlayer player)
        {
            if (guardian?.Map != null)
            {
                guardianTimer = GuardianDuration;
                return;
            }

            ICreatureInfo creatureInfo = creatureInfoManager.GetCreatureInfo(GuardianCreatureId);
            if (creatureInfo == null)
                return;

            // the Guardian rises between the player and the catapult, facing it
            Vector3 direction = player.Position - owner.Position;
            direction.Y = 0f;
            direction = direction.LengthSquared() > 0.01f ? Vector3.Normalize(direction) : Vector3.UnitX;
            Vector3 position = owner.Position + direction * 6f;
            float angle = MathF.Atan2(-direction.X, -direction.Z);

            guardian = owner.SummonFactory.Summon<INonPlayerEntity>(creatureInfo, position, new Vector3(angle, 0f, 0f));
            guardianTimer = GuardianDuration;
        }

        /// <summary>
        /// Invoked each world tick with the delta since the previous tick occurred.
        /// </summary>
        public void Update(double lastTick)
        {
            for (int i = pending.Count - 1; i >= 0; i--)
            {
                (ulong characterId, uint playerGuid, double timer) = pending[i];
                timer -= lastTick;
                if (timer > 0d)
                {
                    pending[i] = (characterId, playerGuid, timer);
                    continue;
                }

                pending.RemoveAt(i);
                if (owner.Map?.GetEntity<IPlayer>(playerGuid) is not IPlayer player)
                    continue;

                credited.Add(characterId);
                player.QuestManager.ObjectiveUpdate(QuestObjectiveType.Unknown10, FlingatronCreatureId, owner.QuestChecklistMask);
            }

            if (guardian == null)
                return;

            guardianTimer -= lastTick;
            if (guardianTimer > 0d || pending.Count > 0)
                return;

            if (guardian.Map != null)
                guardian.RemoveFromMap();
            guardian = null;
        }
    }
}
