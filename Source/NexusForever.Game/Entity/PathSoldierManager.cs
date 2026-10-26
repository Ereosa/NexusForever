using System.Numerics;
using Microsoft.Extensions.DependencyInjection;
using NexusForever.Database.World.Model;
using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Entity.Creature;
using NexusForever.Game.Static.PlayerPath;
using NexusForever.GameTable;
using NexusForever.GameTable.Model;
using NexusForever.Network.World.Message.Model.PlayerPath;
using NexusForever.Shared;
using NexusForever.Shared.Game;
using NLog;
using Path = NexusForever.Game.Static.PlayerPath.Path;

namespace NexusForever.Game.Entity
{
    public class PathSoldierManager : IPathSoldierManager
    {
        [Flags]
        private enum HoldoutSpawnFlags
        {
            None   = 0x00,
            Boss   = 0x01,
            Defend = 0x02
        }

        private enum HoldoutEventType
        {
            Holdout     = 0,
            Defend      = 1,
            Timed       = 2,
            TimedDefend = 3
        }

        private const float DefaultSpawnDistance  = 25f;
        private const float DefaultDefendDistance = 4f;
        private const float LeaveAreaDistance     = 60f;
        private const float ProximityStartDistance = 15f;

        private static readonly ILogger log = LogManager.GetCurrentClassLogger();

        private sealed class Holdout
        {
            public PathSoldierEventEntry Entry;
            public HoldoutEventType Type;
            public IPathMission Mission;
            public uint ControlPointGuid;
            public Vector3 Position;
            public uint WaveCount;
            public int WaveIndex = -1;
            public PlayerPathSoldierEventMode Mode;
            public double Elapsed;
            public double WaveElapsed;
            public readonly HashSet<uint> WaveUnits = new();
            public readonly HashSet<uint> DefendUnits = new();
        }

        public bool InHoldout => holdout != null;

        private readonly IPlayer player;
        private Holdout holdout;
        private double initialDelay;
        private readonly UpdateTimer checkTimer = new(0.5d);

        public PathSoldierManager(IPlayer owner)
        {
            player = owner;
        }

        public void Update(double lastTick)
        {
            checkTimer.Update(lastTick);
            bool check = checkTimer.HasElapsed;
            if (check)
                checkTimer.Reset();

            if (holdout == null)
            {
                if (check)
                    CheckProximityStart();
                return;
            }

            holdout.Elapsed += lastTick;

            if (holdout.Mode == PlayerPathSoldierEventMode.InitialDelay)
            {
                initialDelay -= lastTick;
                if (initialDelay <= 0d)
                    BeginWaves();
                return;
            }

            holdout.WaveElapsed += lastTick;
            if (check)
                CheckHoldout();
        }

        private void CheckProximityStart()
        {
            if (player.Path != Path.Soldier || player.Map == null || !player.IsAlive)
                return;

            foreach (IWorldEntity entity in player.GetVisible().OfType<IWorldEntity>())
            {
                Creature2Entry entry = entity.CreatureInfo?.Entry;
                if (entry == null || entry.PathMissionIdSoldier == 0u)
                    continue;

                // control points with an activate spell are started by activating them
                if (entry.Spell4IdActivate.Any(s => s != 0u))
                    continue;

                if (Vector3.Distance(player.Position, entity.Position) > ProximityStartDistance)
                    continue;

                if (TryStart(entity, false))
                    return;
            }
        }

        public void OnActivate(IWorldEntity entity)
        {
            if (holdout != null)
                return;

            if (entity.CreatureInfo?.Entry?.PathMissionIdSoldier is not > 0u)
                return;

            TryStart(entity, true);
        }

        private bool TryStart(IWorldEntity controlPoint, bool activated)
        {
            if (player.Path != Path.Soldier)
                return false;

            // using the control point also gives its mission, in case the player missed the area unlock
            IPathMission mission = player.PathMissionManager.GetOrUnlockMission(controlPoint.CreatureInfo.Entry.PathMissionIdSoldier);
            if (mission == null || mission.Completed || mission.Type != PathMissionType.SoldierHoldout)
            {
                if (activated)
                    log.Debug($"Player {player.CharacterId} can't start the holdout of control point {controlPoint.CreatureId}: mission {controlPoint.CreatureInfo.Entry.PathMissionIdSoldier} is {(mission == null ? "not available" : "completed")}.");
                return false;
            }

            PathSoldierEventEntry entry = GameTableManager.Instance.PathSoldierEvent.GetEntry(mission.Entry.ObjectId);
            if (entry == null)
                return false;

            var spawns = AssetManager.Instance.GetHoldoutSpawns(entry.Id);
            if (spawns.Count == 0)
            {
                log.Warn($"No holdout spawns for soldier event {entry.Id}, mission {mission.Id} can't be started.");
                return false;
            }

            holdout = new Holdout
            {
                Entry            = entry,
                Type             = (HoldoutEventType)entry.PathSoldierEventType,
                Mission          = mission,
                ControlPointGuid = controlPoint.Guid,
                Position         = controlPoint.Position,
                WaveCount        = (uint)GameTableManager.Instance.PathSoldierEventWave.Entries
                    .Where(w => w != null && w.PathSoldierEventId == entry.Id)
                    .Select(w => w.WaveIndex)
                    .Distinct()
                    .Count(),
                Mode             = PlayerPathSoldierEventMode.InitialDelay
            };
            initialDelay = entry.InitialSpawnTime / 1000d;

            foreach (PathSoldierHoldoutSpawnModel spawn in spawns.Where(s => ((HoldoutSpawnFlags)s.Flags & HoldoutSpawnFlags.Defend) != 0))
                foreach (IWorldEntity unit in Spawn(spawn, DefaultDefendDistance, false))
                    holdout.DefendUnits.Add(unit.Guid);

            log.Trace($"Player {player.CharacterId} started holdout {entry.Id} for mission {mission.Id}.");

            SendStatus((int)entry.InitialSpawnTime);
            return true;
        }

        private void BeginWaves()
        {
            holdout.Mode    = PlayerPathSoldierEventMode.Active;
            holdout.Elapsed = 0d;
            ReleaseWave(false);
        }

        private void ReleaseWave(bool notify = true)
        {
            holdout.WaveIndex++;
            holdout.WaveElapsed = 0d;

            bool boss = false;
            foreach (PathSoldierHoldoutSpawnModel spawn in AssetManager.Instance.GetHoldoutSpawns(holdout.Entry.Id)
                .Where(s => s.WaveIndex == holdout.WaveIndex && ((HoldoutSpawnFlags)s.Flags & HoldoutSpawnFlags.Defend) == 0))
            {
                boss |= ((HoldoutSpawnFlags)spawn.Flags & HoldoutSpawnFlags.Boss) != 0;
                foreach (IWorldEntity unit in Spawn(spawn, DefaultSpawnDistance, true))
                    holdout.WaveUnits.Add(unit.Guid);
            }

            if (notify)
            {
                player.Session.EnqueueMessageEncrypted(new ServerPathSoldierHoldOutNextWave
                {
                    PathSoldierEventId = (ushort)holdout.Entry.Id,
                    WaveIndex          = (uint)holdout.WaveIndex,
                    IsBoss             = boss
                });
            }
            else
                SendStatus(IsTimed ? 0 : (int)holdout.Entry.MaxTimeBetweenWaves, boss);
        }

        private bool IsTimed => holdout.Type is HoldoutEventType.Timed or HoldoutEventType.TimedDefend;

        private IEnumerable<IWorldEntity> Spawn(PathSoldierHoldoutSpawnModel spawn, float defaultDistance, bool hostile)
        {
            ICreatureInfo creatureInfo = LegacyServiceProvider.Provider.GetService<ICreatureInfoManager>().GetCreatureInfo(spawn.CreatureId);
            if (creatureInfo == null)
                yield break;

            float distance = spawn.Distance > 0f ? spawn.Distance : defaultDistance;
            List<uint> defendGuids = hostile ? holdout.DefendUnits.ToList() : [];
            for (int i = 0; i < Math.Max(1, (int)spawn.Count); i++)
            {
                float angle = Random.Shared.NextSingle() * MathF.PI * 2f;
                float x = holdout.Position.X + MathF.Cos(angle) * distance;
                float z = holdout.Position.Z + MathF.Sin(angle) * distance;
                float y = player.Map.GetTerrainHeight(x, z) ?? holdout.Position.Y;
                var position = new Vector3(x, y, z);

                // face the control point
                float yaw = MathF.Atan2(holdout.Position.X - x, holdout.Position.Z - z);

                IWorldEntity unit = player.SummonFactory.Summon<INonPlayerEntity>(creatureInfo, position, new Vector3(yaw, 0f, 0f), (map, guid, vector) =>
                {
                    if (!hostile)
                        return;

                    IUnitEntity summoned = map.GetEntity<IUnitEntity>(guid);
                    if (summoned == null)
                        return;

                    summoned.ThreatManager.UpdateThreat(player, 1);

                    // in defend events the waves go for the units being defended first
                    IUnitEntity defend = defendGuids.Count > 0 ? map.GetEntity<IUnitEntity>(defendGuids[Random.Shared.Next(defendGuids.Count)]) : null;
                    if (defend != null)
                        summoned.ThreatManager.UpdateThreat(defend, 2);
                });

                if (unit != null)
                    yield return unit;
            }
        }

        private void CheckHoldout()
        {
            if (!player.IsAlive)
            {
                End(PlayerPathSoldierResult.FailDeath);
                return;
            }

            if (Vector3.Distance(player.Position, holdout.Position) > LeaveAreaDistance)
            {
                End(PlayerPathSoldierResult.FailLeaveArea);
                return;
            }

            holdout.WaveUnits.RemoveWhere(guid => player.Map.GetEntity<IUnitEntity>(guid) is not { IsAlive: true });

            if (holdout.DefendUnits.Count > 0 && holdout.DefendUnits.Any(guid => player.Map.GetEntity<IUnitEntity>(guid) is not { IsAlive: true }))
            {
                End(PlayerPathSoldierResult.FailDefenceDeath);
                return;
            }

            double maxEventTime = holdout.Entry.MaxEventTime / 1000d;
            bool lastWave = holdout.WaveIndex >= (int)holdout.WaveCount - 1;

            if (IsTimed)
            {
                if (holdout.Elapsed >= maxEventTime)
                {
                    End(PlayerPathSoldierResult.Success);
                    return;
                }
            }
            else
            {
                if (lastWave && holdout.WaveUnits.Count == 0)
                {
                    End(PlayerPathSoldierResult.Success);
                    return;
                }

                if (maxEventTime > 0d && holdout.Elapsed >= maxEventTime)
                {
                    End(PlayerPathSoldierResult.FailTimeOut);
                    return;
                }
            }

            if (lastWave)
                return;

            // the next wave is released when the current wave is defeated or the time between waves runs out
            if (holdout.WaveUnits.Count == 0 || holdout.WaveElapsed >= holdout.Entry.MaxTimeBetweenWaves / 1000d)
                ReleaseWave();
        }

        private void SendStatus(int delayTime, bool boss = false)
        {
            player.Session.EnqueueMessageEncrypted(new ServerPathSoldierHoldoutStatus
            {
                PathSoldierEventId = holdout.Entry.Id,
                Units              = holdout.DefendUnits
                    .Select(guid => new TowerDefenseUnit
                    {
                        UnitId = guid,
                        Type   = TowerDefenseUnitType.Defend
                    })
                    .ToList(),
                UnitId             = holdout.ControlPointGuid,
                IsBoss             = boss,
                Mode               = holdout.Mode,
                DelayTime          = delayTime,
                WaveIndex          = Math.Max(0, holdout.WaveIndex),
                StartTimeOffset    = (int)(holdout.Elapsed * 1000d)
            });
        }

        private void End(PlayerPathSoldierResult result)
        {
            Holdout ended = holdout;
            holdout = null;

            foreach (uint guid in ended.WaveUnits.Concat(ended.DefendUnits))
                player.Map?.GetEntity<IWorldEntity>(guid)?.RemoveFromMap();

            player.Session.EnqueueMessageEncrypted(new ServerPathSoldierHoldoutEnd
            {
                PathSoldierEventId = (ushort)ended.Entry.Id,
                Reason             = result
            });

            log.Trace($"Player {player.CharacterId} holdout {ended.Entry.Id} ended with {result}.");

            if (result == PlayerPathSoldierResult.Success)
                player.PathMissionManager.CompleteMission(ended.Mission);
        }

        public void OnDeath()
        {
            if (holdout == null)
                return;

            player.Session.EnqueueMessageEncrypted(new ServerPathSoldierHoldoutDeath
            {
                PathSoldierEventId = (ushort)holdout.Entry.Id
            });
            End(PlayerPathSoldierResult.FailDeath);
        }

        public void Cancel()
        {
            if (holdout != null)
                End(PlayerPathSoldierResult.ScriptCancel);
        }
    }
}
