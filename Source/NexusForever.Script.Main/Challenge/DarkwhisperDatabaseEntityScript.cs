using System.Numerics;
using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Entity.Creature;
using NexusForever.Game.Abstract.Spell;
using NexusForever.Game.Static.Entity.Movement.Spline;
using NexusForever.Game.Static.Spell;
using NexusForever.Script.Template;
using NexusForever.Script.Template.Filter;
using NexusForever.Shared;

namespace NexusForever.Script.Main.Challenge
{
    /// <summary>
    /// Data mining minigame of the Darkwhisper Database (Datapad Interface, Darkwhisper Enclave in Celestion), used by quest 7027 and
    /// the Dancing with Data challenge.
    /// </summary>
    /// <remarks>
    /// The datapad gives the player the Data Mining action bar (spell 36494, set 328) with three buttons, Data Mining 1 to 3 (spells
    /// 36481 to 36483), which send ravel signals 12511 to 12513. Data points (yellow 33271, green 33272 and red 33273) drop onto the
    /// three File Selector Reticles above the datapad (creature 34323, checklist index 0 to 2), mining one means pressing its button
    /// while it is inside its reticle. Retail ran this as a server event script, the speed, count and hit window are not in any data.
    ///
    /// Quest 7027 asks for one full session (objective 13554), Dancing with Data counts data points mined in a row, missing one ends it.
    /// </remarks>
    [ScriptFilterCreatureId(33347)]
    public class DarkwhisperDatabaseEntityScript : IWorldEntityScript, IOwnedScript<IWorldEntity>
    {
        private const uint ActionBarSpell4Id      = 36494;
        private const uint ReticleCreatureId      = 34323;
        private const uint FirstSignal            = 12511;
        private const uint SessionObjectiveId     = 13554;
        private const ushort DataMiningQuestId    = 7027;
        private const uint DancingWithDataId      = 536;

        private static readonly uint[] dataPointCreatureIds = [33271, 33272, 33273];

        /// <summary>
        /// Data points in a session.
        /// </summary>
        private const int SessionDataPoints = 40;

        /// <summary>
        /// Height above a reticle a data point starts from, it drops through the reticle and the same distance below it.
        /// </summary>
        private const float DropHeight   = 6f;
        private const double DropTime    = 3d;
        private const double HitWindow   = 0.45d;
        private const double MinInterval = 1.2d;
        private const double MaxInterval = 2.0d;

        /// <summary>
        /// Distance from the datapad at which a session is abandoned.
        /// </summary>
        private const float LeaveRange = 20f;

        private class DataPoint
        {
            public int Lane { get; init; }
            public uint Guid { get; set; }
            public double ArrivalTime { get; init; }
        }

        private class Session
        {
            public IPlayer Player { get; init; }
            public double Time { get; set; }
            public double NextSpawn { get; set; }
            public int Spawned { get; set; }
            public List<DataPoint> DataPoints { get; } = [];
        }

        #region Dependency Injection

        private readonly ICreatureInfoManager creatureInfoManager;
        private readonly IFactory<ISpellParameters> spellParameterFactory;
        private IWorldEntity owner;

        public DarkwhisperDatabaseEntityScript(
            ICreatureInfoManager creatureInfoManager,
            IFactory<ISpellParameters> spellParameterFactory)
        {
            this.creatureInfoManager   = creatureInfoManager;
            this.spellParameterFactory = spellParameterFactory;
        }

        #endregion

        private readonly Dictionary<uint, Session> sessions = [];

        /// <summary>
        /// Invoked when <see cref="IScript"/> is loaded.
        /// </summary>
        public void OnLoad(IWorldEntity owner)
        {
            this.owner = owner;
        }

        /// <summary>
        /// Invoked when <see cref="IWorldEntity"/> is successfully activated by <see cref="IPlayer"/>.
        /// </summary>
        public void OnActivateSuccess(IPlayer activator)
        {
            if (sessions.ContainsKey(activator.Guid) || GetReticles().Count < dataPointCreatureIds.Length)
                return;

            ISpellParameters parameters = spellParameterFactory.Resolve();
            parameters.PrimaryTargetId        = activator.Guid;
            parameters.UserInitiatedSpellCast = false;
            activator.CastSpell(ActionBarSpell4Id, parameters);

            sessions.Add(activator.Guid, new Session
            {
                Player    = activator,
                NextSpawn = 1d
            });
        }

        /// <summary>
        /// Invoked each world tick with the delta since the previous tick occurred.
        /// </summary>
        public void Update(double lastTick)
        {
            foreach (Session session in sessions.Values.ToList())
            {
                session.Time += lastTick;

                if (session.Player.Map != owner.Map || Vector3.Distance(session.Player.Position, owner.Position) > LeaveRange)
                {
                    EndSession(session, false);
                    continue;
                }

                // data points that dropped past their reticle without being mined
                foreach (DataPoint dataPoint in session.DataPoints.Where(d => session.Time > d.ArrivalTime + HitWindow).ToList())
                {
                    RemoveDataPoint(session, dataPoint);
                    OnMiss(session);
                }

                if (session.Spawned < SessionDataPoints && session.Time >= session.NextSpawn)
                    SpawnDataPoint(session);

                if (session.Spawned >= SessionDataPoints && session.DataPoints.Count == 0)
                    EndSession(session, true);
            }
        }

        /// <summary>
        /// Invoked when a Data Mining button is pressed.
        /// </summary>
        public void OnRavelSignal(IUnitEntity source, uint signal)
        {
            if (signal < FirstSignal || signal >= FirstSignal + dataPointCreatureIds.Length || !sessions.TryGetValue(source.Guid, out Session session))
                return;

            int lane = (int)(signal - FirstSignal);
            DataPoint dataPoint = session.DataPoints
                .Where(d => d.Lane == lane && Math.Abs(session.Time - d.ArrivalTime) <= HitWindow)
                .MinBy(d => Math.Abs(session.Time - d.ArrivalTime));

            if (dataPoint == null)
            {
                // a press with nothing in the reticle breaks the run
                OnMiss(session);
                return;
            }

            RemoveDataPoint(session, dataPoint);
            if (session.Player.ChallengeManager.IsActive(DancingWithDataId))
                session.Player.ChallengeManager.UpdateProgress(DancingWithDataId);
        }

        private List<IWorldEntity> GetReticles()
        {
            return owner.GetVisibleCreature<IWorldEntity>(ReticleCreatureId)
                .OrderBy(r => r.QuestChecklistIdx)
                .ToList();
        }

        private void SpawnDataPoint(Session session)
        {
            List<IWorldEntity> reticles = GetReticles();
            if (reticles.Count < dataPointCreatureIds.Length)
            {
                EndSession(session, false);
                return;
            }

            int lane = Random.Shared.Next(dataPointCreatureIds.Length);
            ICreatureInfo creatureInfo = creatureInfoManager.GetCreatureInfo(dataPointCreatureIds[lane]);
            if (creatureInfo == null)
                return;

            Vector3 reticle = reticles[lane].Position;
            Vector3 start   = reticle + new Vector3(0f, DropHeight, 0f);
            Vector3 end     = reticle - new Vector3(0f, DropHeight, 0f);

            var dataPoint = new DataPoint
            {
                Lane        = lane,
                ArrivalTime = session.Time + DropTime
            };

            IWorldEntity entity = owner.SummonFactory.Summon(creatureInfo, start, reticles[lane].Rotation, (map, guid, position) =>
            {
                dataPoint.Guid = guid;
                IWorldEntity summoned = map.GetEntity<IWorldEntity>(guid);
                summoned?.MovementManager.LaunchSpline([start, end], SplineType.Linear, SplineMode.OneShot, (float)(DropHeight / DropTime));
            });

            session.DataPoints.Add(dataPoint);
            session.Spawned++;
            session.NextSpawn = session.Time + MinInterval + Random.Shared.NextDouble() * (MaxInterval - MinInterval);
        }

        private void RemoveDataPoint(Session session, DataPoint dataPoint)
        {
            session.DataPoints.Remove(dataPoint);
            if (dataPoint.Guid != 0u)
                owner.SummonFactory.Unsummon(dataPoint.Guid);
        }

        private void OnMiss(Session session)
        {
            if (!session.Player.ChallengeManager.IsActive(DancingWithDataId))
                return;

            // Dancing with Data counts data points mined in a row, the run ends on the tier it reached
            if (session.Player.ChallengeManager.GetCurrentTier(DancingWithDataId) > 0u)
                session.Player.ChallengeManager.Complete(DancingWithDataId);
            else
                session.Player.ChallengeManager.Fail(DancingWithDataId);
        }

        private void EndSession(Session session, bool completed)
        {
            foreach (DataPoint dataPoint in session.DataPoints.ToList())
                RemoveDataPoint(session, dataPoint);

            sessions.Remove(session.Player.Guid);

            foreach (ISpell spell in session.Player.GetSpellsByEffect(SpellEffectType.ActionBarSet).Where(s => s.Spell4Id == ActionBarSpell4Id).ToList())
                spell.Finish();

            if (!completed)
                return;

            if (session.Player.QuestManager.GetActiveQuest(DataMiningQuestId) != null)
                session.Player.QuestManager.ObjectiveUpdate(SessionObjectiveId, 1u);

            // a full session without a miss ends Dancing with Data on the tier it reached
            if (session.Player.ChallengeManager.IsActive(DancingWithDataId))
            {
                if (session.Player.ChallengeManager.GetCurrentTier(DancingWithDataId) > 0u)
                    session.Player.ChallengeManager.Complete(DancingWithDataId);
                else
                    session.Player.ChallengeManager.Fail(DancingWithDataId);
            }
        }
    }
}
