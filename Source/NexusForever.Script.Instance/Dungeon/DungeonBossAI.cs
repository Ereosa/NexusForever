using System.Numerics;
using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Entity.Creature;
using NexusForever.Game.Abstract.Spell;
using NexusForever.GameTable;
using NexusForever.Script.Instance.Expedition;
using NexusForever.Shared;

namespace NexusForever.Script.Instance.Dungeon
{
    /// <summary>
    /// Dungeon boss whose fight is a list of health thresholds and timed events on top of its abilities (<see cref="ExpeditionCreatureAI"/>).
    /// </summary>
    /// <remarks>
    /// Used for the bosses no written guide survives for: the events are the boss' own spells from the client data (their Spell4
    /// descriptions name the boss and the mechanic, eg: "Tugga - Magma Totem - Base - Summon"), their order and timings are guesses.
    /// </remarks>
    public abstract class DungeonBossAI : ExpeditionCreatureAI
    {
        private readonly List<(float Health, Action Action)> thresholds = [];
        private readonly List<(double Interval, double Timer, Action Action)> timers = [];
        private int nextThreshold;

        #region Dependency Injection

        protected readonly ICreatureInfoManager creatureInfoManager;

        protected DungeonBossAI(
            IFactory<ISpellParameters> spellParametersFactory,
            IGameTableManager gameTableManager,
            ICreatureInfoManager creatureInfoManager)
            : base(spellParametersFactory, gameTableManager)
        {
            this.creatureInfoManager = creatureInfoManager;
        }

        #endregion

        /// <summary>
        /// Run the action once when the boss' health drops below the fraction.
        /// </summary>
        protected void AtHealth(float health, Action action)
        {
            thresholds.Add((health, action));
            thresholds.Sort((a, b) => b.Health.CompareTo(a.Health));
        }

        /// <summary>
        /// Run the action every interval seconds in combat.
        /// </summary>
        protected void Every(double interval, Action action, double first = -1d)
        {
            timers.Add((interval, first < 0d ? interval : first, action));
        }

        public override void OnEnterCombat()
        {
            base.OnEnterCombat();
            if (entity.Health < entity.MaxHealth)
                return;

            nextThreshold = 0;
            for (int i = 0; i < timers.Count; i++)
                timers[i] = (timers[i].Interval, timers[i].Interval, timers[i].Action);
        }

        public override void Update(double lastTick)
        {
            base.Update(lastTick);
            if (!entity.IsAlive || !entity.InCombat)
                return;

            if (nextThreshold < thresholds.Count && entity.Health <= entity.MaxHealth * thresholds[nextThreshold].Health)
                thresholds[nextThreshold++].Action();

            for (int i = 0; i < timers.Count; i++)
            {
                (double interval, double timer, Action action) = timers[i];
                timer -= lastTick;
                if (timer <= 0d && !entity.IsCasting())
                {
                    timer = interval;
                    action();
                }
                timers[i] = (interval, timer, action);
            }
        }

        /// <summary>
        /// Cast a spell on the boss' current target (or itself).
        /// </summary>
        protected void Cast(uint spellId)
        {
            IUnitEntity target = entity.TargetGuid.HasValue ? entity.GetVisible<IUnitEntity>(entity.TargetGuid.Value) : null;
            CastSpell(spellId, target);
        }

        /// <summary>
        /// Summon adds (their veteran version on veteran) around the boss, they go for the boss' target.
        /// </summary>
        protected void SummonAdds(uint normalId, uint veteranId, int count, float radius)
        {
            ICreatureInfo creatureInfo = creatureInfoManager.GetCreatureInfo(IsVeteran && veteranId != 0u ? veteranId : normalId);
            if (creatureInfo == null || entity.Map == null)
                return;

            IUnitEntity target = entity.TargetGuid.HasValue ? entity.GetVisible<IUnitEntity>(entity.TargetGuid.Value) : null;
            for (int i = 0; i < count; i++)
            {
                float angle = MathF.Tau / count * i + Random.Shared.NextSingle();
                Vector3 position = entity.Position + new Vector3(MathF.Cos(angle) * radius, 0f, MathF.Sin(angle) * radius);
                entity.SummonFactory.Summon<INonPlayerEntity>(creatureInfo, position, new Vector3(angle + MathF.PI, 0f, 0f), (map, guid, vector) =>
                {
                    if (target != null && map.GetEntity<IUnitEntity>(guid) is IUnitEntity add)
                        add.ThreatManager.UpdateThreat(target, 100);
                });
            }
        }
    }
}
