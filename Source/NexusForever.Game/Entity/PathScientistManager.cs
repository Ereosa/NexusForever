using System.Numerics;
using Microsoft.Extensions.DependencyInjection;
using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Entity.Creature;
using NexusForever.Game.Prerequisite;
using NexusForever.Game.Spell;
using NexusForever.Game.Static.Entity;
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
    public class PathScientistManager : IPathScientistManager
    {
        private const uint DefaultScanbotProfileId = 1u;
        private const float DefaultMaxSeekDistance = 30f;

        /// <summary>
        /// Scan data percentage gained for a scan mission per scan before the scanbot profile thoroughness multiplier.
        /// </summary>
        /// <remarks>
        /// The retail value isn't known, the client only receives the resulting percentage.
        /// </remarks>
        private const float BaseScanProgress = 20f;

        private static readonly ILogger log = LogManager.GetCurrentClassLogger();

        public uint? ScanbotGuid { get; private set; }

        private readonly IPlayer player;

        private PathScientistScanBotProfileEntry profile;
        private DateTime cooldownEnd = DateTime.MinValue;

        private uint? scanTargetGuid;
        private UpdateTimer scanTimer;

        // units already scanned by the player, they can't be scanned again until the rescan cooldown expires
        private readonly Dictionary<uint, DateTime> scannedUnits = new();

        public PathScientistManager(IPlayer owner)
        {
            player = owner;
        }

        public void Update(double lastTick)
        {
            if (scanTimer == null)
                return;

            scanTimer.Update(lastTick);
            if (!scanTimer.HasElapsed)
                return;

            scanTimer = null;
            FinishScan();
        }

        public void SummonScanbot(uint profileId)
        {
            if (player.Path != Path.Scientist || ScanbotGuid != null)
                return;

            if (DateTime.UtcNow < cooldownEnd)
                return;

            PathScientistScanBotProfileEntry entry = GameTableManager.Instance.PathScientistScanBotProfile.GetEntry(profileId != 0u ? profileId : DefaultScanbotProfileId);
            if (entry == null || !IsProfileUnlocked(entry.Id))
                entry = GameTableManager.Instance.PathScientistScanBotProfile.GetEntry(DefaultScanbotProfileId);
            if (entry == null)
                return;

            ICreatureInfo creatureInfo = LegacyServiceProvider.Provider.GetService<ICreatureInfoManager>().GetCreatureInfo(entry.Creature2Id);
            if (creatureInfo == null)
                return;

            profile = entry;

            var scanbot = LegacyServiceProvider.Provider.GetService<IEntityFactory>().CreateEntity<IScannerUnitEntity>();
            scanbot.Initialise(player, creatureInfo, player.PetCustomisationManager.GetCustomisation(PetType.ScanBot, entry.Id)?.Name);
            scanbot.AddToMap(player.Map, player.Position, (map, guid, position) =>
            {
                ScanbotGuid = guid;
                player.Session.EnqueueMessageEncrypted(new ServerPathScientistScanbotState
                {
                    ScanbotUnitId = guid
                });
                scanbot.ReturnToOwner();
            });
        }

        private bool IsProfileUnlocked(uint profileId)
        {
            if (profileId == DefaultScanbotProfileId)
                return true;

            // additional scanbot profiles are path rewards
            return GameTableManager.Instance.PathReward.Entries
                .Any(r => r != null && r.PathScientistScanBotProfileId == profileId && IsRewardEarned(r));
        }

        private bool IsRewardEarned(PathRewardEntry reward)
        {
            switch (reward.PathRewardTypeEnum)
            {
                case 0u:
                {
                    PathLevelEntry level = GameTableManager.Instance.PathLevel.GetEntry(reward.ObjectId);
                    return level != null && player.PathManager.GetCurrentLevel((Path)level.PathTypeEnum) >= level.PathLevel;
                }
                case 1u:
                    return player.PathMissionManager.IsMissionComplete(reward.ObjectId);
                default:
                    return false;
            }
        }

        public void DismissScanbot()
        {
            if (ScanbotGuid == null)
                return;

            player.GetVisible<IScannerUnitEntity>(ScanbotGuid.Value)?.RemoveFromMap();
            ScanbotGuid    = null;
            scanTimer      = null;
            scanTargetGuid = null;

            uint cooldown = profile?.MinCooldownTimeMs ?? 0u;
            cooldownEnd = DateTime.UtcNow.AddMilliseconds(cooldown);

            player.Session.EnqueueMessageEncrypted(new ServerPathScientistScanbotState
            {
                ScanbotUnitId     = 0u,
                ScanbotCooldownMS = cooldown
            });
        }

        public void ScanTarget()
        {
            if (player.Path != Path.Scientist || ScanbotGuid == null || scanTimer != null)
                return;

            if (player.TargetGuid == null)
                return;

            IWorldEntity target = player.GetVisible<IWorldEntity>(player.TargetGuid.Value);
            if (target == null || !CanScan(target))
                return;

            float maxDistance = profile?.MaxSeekDistance > 0f ? profile.MaxSeekDistance : DefaultMaxSeekDistance;
            if (Vector3.Distance(player.Position, target.Position) > maxDistance)
                return;

            IScannerUnitEntity scanbot = player.GetVisible<IScannerUnitEntity>(ScanbotGuid.Value);
            if (scanbot == null)
                return;

            scanbot.MoveToTarget(target);
            scanTargetGuid = target.Guid;

            uint scanTime = (profile?.ScanTimeMS ?? 3200u) + (profile?.ProcessingTimeMS ?? 0u);
            scanTimer = new UpdateTimer(scanTime / 1000d);
        }

        private void FinishScan()
        {
            IScannerUnitEntity scanbot = ScanbotGuid != null ? player.GetVisible<IScannerUnitEntity>(ScanbotGuid.Value) : null;
            scanbot?.ReturnToOwner();

            if (scanTargetGuid == null)
                return;

            IWorldEntity target = player.GetVisible<IWorldEntity>(scanTargetGuid.Value);
            scanTargetGuid = null;
            if (target == null || !CanScan(target))
                return;

            Creature2Entry creatureEntry = GameTableManager.Instance.Creature2.GetEntry(target.CreatureId);
            uint rescanCooldown = creatureEntry?.RescanCooldown ?? 0u;
            scannedUnits[target.Guid] = rescanCooldown > 0u ? DateTime.UtcNow.AddSeconds(rescanCooldown) : DateTime.MaxValue;

            foreach (uint creatureInfoId in AssetManager.Instance.GetScientistCreatureInfoIds(target.CreatureId))
                ScanCreatureInfo(target, creatureInfoId);

            SendScanParameters(target);
        }

        private void ScanCreatureInfo(IWorldEntity target, uint creatureInfoId)
        {
            PathScientistCreatureInfoEntry info = GameTableManager.Instance.PathScientistCreatureInfo.GetEntry(creatureInfoId);
            if (info == null)
                return;

            if (info.PrerequisiteIdScan != 0u && !PrerequisiteManager.Instance.Meets(player, info.PrerequisiteIdScan))
                return;

            bool progressed = false;
            foreach (IPathMission mission in GetScanMissions(creatureInfoId))
            {
                switch (mission.Type)
                {
                    case PathMissionType.ScientistScan:
                    {
                        float thoroughness = profile?.ThoroughnessMultiplier > 0f ? profile.ThoroughnessMultiplier : 1f;
                        uint progress = Math.Min(100u, mission.Progress + (uint)MathF.Ceiling(BaseScanProgress * thoroughness));
                        player.PathMissionManager.UpdateMission(mission, progress, mission.State | 1u);
                        progressed = true;
                        break;
                    }
                    case PathMissionType.ScientistScanChecklist:
                    {
                        if (target.QuestChecklistIdx >= 32)
                            break;

                        player.PathMissionManager.UpdateMission(mission, mission.Progress | (1u << target.QuestChecklistIdx), mission.State | 1u);
                        progressed = true;
                        break;
                    }
                }
            }

            if (!progressed)
                return;

            // scan buffs, e.g. "Vitalium Glow - Scan Spell"
            foreach (uint spell4Id in new[] { info.Spell4IdBuff00, info.Spell4IdBuff01, info.Spell4IdBuff02, info.Spell4IdBuff03 })
                if (spell4Id != 0u)
                    player.CastSpell(spell4Id, new SpellParameters());
        }

        private IEnumerable<IPathMission> GetScanMissions(uint creatureInfoId)
        {
            return player.PathMissionManager
                .Where(m => !m.Completed
                    && m.Type is PathMissionType.ScientistScan or PathMissionType.ScientistScanChecklist
                    && m.Entry.ObjectId == creatureInfoId)
                .ToList();
        }

        private bool CanScan(IWorldEntity target)
        {
            if (scannedUnits.TryGetValue(target.Guid, out DateTime until))
            {
                if (DateTime.UtcNow < until)
                    return false;
                scannedUnits.Remove(target.Guid);
            }

            return AssetManager.Instance.GetScientistCreatureInfoIds(target.CreatureId)
                .Any(id => GetScanMissions(id).Any());
        }

        public void OnAddVisible(IWorldEntity entity)
        {
            if (player.Path != Path.Scientist || entity is IPlayer)
                return;

            var infoIds = AssetManager.Instance.GetScientistCreatureInfoIds(entity.CreatureId);
            if (infoIds.Count == 0)
                return;

            foreach (uint infoId in infoIds)
            {
                player.Session.EnqueueMessageEncrypted(new ServerPathScientistAddCreatureInfoToCreature
                {
                    UnitId                      = entity.Guid,
                    PathScientistCreatureInfoId = infoId
                });
            }

            SendScanParameters(entity);
        }

        private void SendScanParameters(IWorldEntity entity)
        {
            bool scannable = CanScan(entity);
            player.Session.EnqueueMessageEncrypted(new ServerPathScientistUnitScanParameters
            {
                UnitId          = entity.Guid,
                ScanRewardFlags = scannable ? ScanReward.RewardForScan : 0,
                IsScannable     = scannable
            });
        }

        public void RefreshScannableEntities()
        {
            if (player.Path != Path.Scientist)
                return;

            foreach (IWorldEntity entity in player.GetVisible().OfType<IWorldEntity>().ToList())
                if (AssetManager.Instance.GetScientistCreatureInfoIds(entity.CreatureId).Count > 0)
                    SendScanParameters(entity);
        }
    }
}
