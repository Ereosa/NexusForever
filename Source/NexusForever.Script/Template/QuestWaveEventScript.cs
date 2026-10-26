using System.Numerics;
using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Entity.Creature;
using NexusForever.Game.Abstract.Quest;
using NexusForever.Game.Static.Quest;
using NexusForever.GameTable;
using NexusForever.GameTable.Model;

namespace NexusForever.Script.Template
{
    /// <summary>
    /// A quest step where waves of attackers come for the player at the objective's marker ("Defend the memorial service", "Kill incoming Stormwing Falkrin").
    /// </summary>
    /// <remarks>
    /// Retail ran these as server-only events, the objective data (type 25) points at event data the client doesn't have.
    /// Each quest supplies its attackers and waves, reconstructed from the quest text, guides and the event-only creatures in the client data
    /// (variants of the zone's creatures that are never spawned in the world), see the remarks of each quest script for what is a guess.
    /// The event starts once the objective is the current step and the player is at the marker, it resets if the player dies or leaves.
    /// </remarks>
    public abstract class QuestWaveEventScript : IQuestScript, IOwnedScript<IQuest>
    {
        /// <summary>
        /// Objective completed by surviving the last wave, and the creatures of each wave in order.
        /// </summary>
        /// <remarks>
        /// A quest with several such steps (one per signal flare) lists one event per objective, each runs once it is the current step.
        /// </remarks>
        protected abstract IReadOnlyList<(uint ObjectiveId, uint[][] Waves)> Events { get; }

        /// <summary>
        /// Distance to the objective's marker that starts the event.
        /// </summary>
        protected virtual float StartDistance => 30f;

        /// <summary>
        /// Distance from the marker the attackers arrive from.
        /// </summary>
        protected virtual float SpawnDistance => 25f;

        /// <summary>
        /// Distance to the marker that abandons the event.
        /// </summary>
        protected virtual float LeaveDistance => 90f;

        /// <summary>
        /// Seconds between a wave being defeated and the next one arriving.
        /// </summary>
        protected virtual double WaveDelay => 6d;

        private IQuest owner;
        private readonly Dictionary<uint, WorldLocation2Entry> locations = [];

        private (uint ObjectiveId, uint[][] Waves) current;
        private WorldLocation2Entry location;
        private bool running;
        private Vector3 centre;
        private int waveIndex;
        private double delay;
        private int pending;
        private readonly HashSet<uint> units = [];

        #region Dependency Injection

        private readonly IGameTableManager gameTableManager;
        private readonly ICreatureInfoManager creatureInfoManager;

        protected QuestWaveEventScript(
            IGameTableManager gameTableManager,
            ICreatureInfoManager creatureInfoManager)
        {
            this.gameTableManager    = gameTableManager;
            this.creatureInfoManager = creatureInfoManager;
        }

        #endregion

        /// <summary>
        /// Invoked when <see cref="IQuestScript"/> is loaded.
        /// </summary>
        public void OnLoad(IQuest owner)
        {
            this.owner = owner;

            foreach ((uint objectiveId, _) in Events)
            {
                QuestObjectiveEntry objective = gameTableManager.QuestObjective.GetEntry(objectiveId);
                WorldLocation2Entry indicator = objective != null ? gameTableManager.WorldLocation2.GetEntry(objective.WorldLocationsIdIndicator00) : null;
                if (indicator != null)
                    locations[objectiveId] = indicator;
            }
        }

        /// <summary>
        /// Invoked when <see cref="IQuestScript"/> is unloaded.
        /// </summary>
        public void OnUnload()
        {
            Stop();
        }

        /// <summary>
        /// Invoked each world tick with the delta since the previous tick occurred.
        /// </summary>
        public void Update(double lastTick)
        {
            IPlayer player = owner.GetOwner();
            if (owner.State != QuestState.Accepted || player?.Map == null)
            {
                Stop();
                return;
            }

            if (!running)
            {
                // the first event whose objective is the current step of the quest
                current = Events.FirstOrDefault(e => owner.GetQuestObjective(e.ObjectiveId) is IQuestObjective o && !o.IsComplete()
                    && !owner.Any(p => p.Index < o.Index && !p.ObjectiveInfo.IsOptional() && !p.IsComplete()));
                if (current.Waves == null || !locations.TryGetValue(current.ObjectiveId, out location))
                    return;
            }

            IQuestObjective objective = owner.GetQuestObjective(current.ObjectiveId);
            if (objective == null || objective.IsComplete() || player.Map.Entry.Id != location.WorldId)
            {
                Stop();
                return;
            }

            if (!running)
            {
                var marker = new Vector3(location.Position0, location.Position1, location.Position2);
                if (!player.IsAlive || Vector3.Distance(player.Position, marker) > Math.Max(StartDistance, location.Radius))
                    return;

                // a large marked area (a whole pool or camp) is defended where the player stands
                centre    = location.Radius > StartDistance ? player.Position : marker;
                running   = true;
                waveIndex = -1;
                delay     = 2d;
                return;
            }

            float distance = Vector3.Distance(player.Position, centre);

            if (!player.IsAlive || distance > LeaveDistance)
            {
                Stop();
                return;
            }

            units.RemoveWhere(guid => player.Map.GetEntity<IUnitEntity>(guid) is not { IsAlive: true });
            if (units.Count > 0 || pending > 0)
                return;

            if (waveIndex >= current.Waves.Length - 1)
            {
                Stop();
                owner.ObjectiveUpdate(current.ObjectiveId, Math.Max(1u, objective.ObjectiveInfo.Entry.Count));
                return;
            }

            delay -= lastTick;
            if (delay > 0d)
                return;

            waveIndex++;
            delay = WaveDelay;
            SpawnWave(player, centre, current.Waves[waveIndex]);
        }

        private void SpawnWave(IPlayer player, Vector3 centre, uint[] creatures)
        {
            // the wave arrives from one side, spread over a quarter circle
            float side = Random.Shared.NextSingle() * MathF.PI * 2f;
            for (int i = 0; i < creatures.Length; i++)
            {
                ICreatureInfo creatureInfo = creatureInfoManager.GetCreatureInfo(creatures[i]);
                if (creatureInfo == null)
                    continue;

                float angle = side + (i - (creatures.Length - 1) / 2f) * 0.3f;
                float x = centre.X + MathF.Cos(angle) * SpawnDistance;
                float z = centre.Z + MathF.Sin(angle) * SpawnDistance;
                // indoors and on platforms the terrain is far below, the attackers arrive at the marker's height
                float? terrain = player.Map.GetTerrainHeight(x, z);
                var position = new Vector3(x, terrain != null && MathF.Abs(terrain.Value - centre.Y) < 6f ? terrain.Value : centre.Y, z);
                float yaw = MathF.Atan2(centre.X - x, centre.Z - z);

                int wave = waveIndex;
                pending++;
                player.SummonFactory.Summon<INonPlayerEntity>(creatureInfo, position, new Vector3(yaw, 0f, 0f), (map, guid, vector) =>
                {
                    pending = Math.Max(0, pending - 1);
                    IUnitEntity summoned = map.GetEntity<IUnitEntity>(guid);
                    if (summoned == null)
                        return;

                    // the event was abandoned before the unit was added
                    if (!running || wave != waveIndex)
                    {
                        summoned.RemoveFromMap();
                        return;
                    }

                    units.Add(guid);
                    summoned.ThreatManager.UpdateThreat(player, 1);
                });
            }
        }

        private void Stop()
        {
            if (!running)
                return;

            running = false;
            pending = 0;

            IPlayer player = owner.GetOwner();
            foreach (uint guid in units)
                if (player?.Map?.GetEntity<IUnitEntity>(guid) is IUnitEntity unit && unit.IsAlive)
                    unit.RemoveFromMap();
            units.Clear();
        }
    }
}
