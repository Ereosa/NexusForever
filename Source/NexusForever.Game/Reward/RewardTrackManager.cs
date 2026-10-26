using System.Numerics;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using NexusForever.Database.Character;
using NexusForever.Database.Character.Model;
using NexusForever.Game.Abstract.Account.Reward;
using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Reward;
using NexusForever.Game.Prerequisite;
using NexusForever.Game.Static.Entity;
using NexusForever.Game.Static.Reward;
using NexusForever.GameTable;
using NexusForever.GameTable.Model;
using NexusForever.Network;
using NexusForever.Network.World.Message.Model.Reward;
using NexusForever.Network.World.Message.Static;
using NLog;
using NetworkRewardTrack = NexusForever.Network.World.Message.Model.Reward.RewardTrack;

namespace NexusForever.Game.Reward
{
    /// <summary>
    /// Character reward tracks, the challenge reward tracks of each zone level range.
    /// </summary>
    /// <remarks>
    /// Each <see cref="RewardTrackEntry"/> has up to 10 reward points (RewardPointCost00-09, points needed for each). Every reward point
    /// has one or more <see cref="RewardTrackRewardsEntry"/> candidates (bit n of RewardPointFlags is reward point n), the client shows
    /// the candidate sent in <see cref="NetworkRewardTrack.RewardTrackRewardsIds"/> and claims it by id with the choice the player picked.
    /// A track is restarted once every reward point has been claimed, giving a new pick of candidates.
    /// </remarks>
    public class RewardTrackManager : IRewardTrackManager
    {
        private class RewardTrack
        {
            public RewardTrackEntry Entry { get; init; }
            public uint Points { get; set; }
            public uint ClaimedRewards { get; set; }
            public uint Iteration { get; set; }
            public List<RewardTrackRewardsEntry> Rewards { get; set; } = [];

            public bool IsNew { get; set; }
            public bool IsDirty { get; set; }
        }

        private static readonly ILogger log = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Candidates for each reward point of each reward track.
        /// </summary>
        private static readonly Lazy<Dictionary<uint, List<RewardTrackRewardsEntry>[]>> rewardCandidates = new(() =>
        {
            var candidates = new Dictionary<uint, List<RewardTrackRewardsEntry>[]>();
            foreach (RewardTrackRewardsEntry entry in GameTableManager.Instance.RewardTrackRewards.Entries)
            {
                if (!candidates.TryGetValue(entry.RewardTrackId, out List<RewardTrackRewardsEntry>[] points))
                {
                    points = Enumerable.Range(0, 10).Select(_ => new List<RewardTrackRewardsEntry>()).ToArray();
                    candidates.Add(entry.RewardTrackId, points);
                }

                for (int i = 0; i < points.Length; i++)
                    if ((entry.RewardPointFlags & (1u << i)) != 0u)
                        points[i].Add(entry);
            }

            return candidates;
        });

        private readonly IPlayer player;
        private readonly Dictionary<uint, RewardTrack> rewardTracks = [];
        private uint activeChallengeTrackId;

        /// <summary>
        /// Create a new <see cref="IRewardTrackManager"/> from existing <see cref="CharacterModel"/> database model.
        /// </summary>
        public RewardTrackManager(IPlayer owner, CharacterModel model)
        {
            player = owner;

            foreach (CharacterRewardTrackModel trackModel in model.RewardTrack)
            {
                RewardTrackEntry entry = GameTableManager.Instance.RewardTrack.GetEntry(trackModel.RewardTrackId);
                if (entry == null)
                {
                    log.Warn($"Character {model.Id} has unknown reward track {trackModel.RewardTrackId}!");
                    continue;
                }

                var track = new RewardTrack
                {
                    Entry          = entry,
                    Points         = trackModel.Points,
                    ClaimedRewards = trackModel.ClaimedRewards,
                    Iteration      = trackModel.Iteration
                };
                rewardTracks.Add(entry.Id, track);
            }

            // every character has every challenge reward track, they are shown in the challenge log of each zone
            foreach (RewardTrackEntry entry in GameTableManager.Instance.RewardTrack.Entries)
            {
                if ((RewardTrackType)entry.RewardTrackTypeEnum != RewardTrackType.Challenge || rewardTracks.ContainsKey(entry.Id))
                    continue;

                rewardTracks.Add(entry.Id, new RewardTrack
                {
                    Entry = entry,
                    IsNew = true
                });
            }
        }

        public void Save(CharacterContext context)
        {
            foreach (RewardTrack track in rewardTracks.Values)
            {
                if (!track.IsDirty)
                    continue;

                var model = new CharacterRewardTrackModel
                {
                    Id             = player.CharacterId,
                    RewardTrackId  = track.Entry.Id,
                    Points         = track.Points,
                    ClaimedRewards = track.ClaimedRewards,
                    Iteration      = track.Iteration
                };

                if (track.IsNew)
                    context.Add(model);
                else
                {
                    EntityEntry<CharacterRewardTrackModel> entity = context.Attach(model);
                    entity.Property(p => p.Points).IsModified         = true;
                    entity.Property(p => p.ClaimedRewards).IsModified = true;
                    entity.Property(p => p.Iteration).IsModified      = true;
                }

                track.IsNew   = false;
                track.IsDirty = false;
            }
        }

        public void SendInitialPackets()
        {
            activeChallengeTrackId = GetZoneChallengeTrackId();

            foreach (RewardTrack track in rewardTracks.Values)
                SelectRewards(track);

            player.Session.EnqueueMessageEncrypted(new ServerRewardTracksReload
            {
                RewardsTracks = rewardTracks.Values
                    .Select(BuildTrack)
                    .ToList()
            });
        }

        /// <summary>
        /// Invoked when the player enters a new zone, the challenge reward track of the zone becomes the active one.
        /// </summary>
        public void OnZoneUpdate()
        {
            uint trackId = GetZoneChallengeTrackId();
            if (trackId == 0u || trackId == activeChallengeTrackId)
                return;

            activeChallengeTrackId = trackId;
            if (rewardTracks.TryGetValue(trackId, out RewardTrack track))
                SendUpdate(track);
        }

        /// <summary>
        /// Challenge reward track for the zone the player is in, the one most of the zone's challenges give points to.
        /// </summary>
        private uint GetZoneChallengeTrackId()
        {
            for (WorldZoneEntry zone = player.Zone; zone != null; zone = zone.ParentZoneId != 0u ? GameTableManager.Instance.WorldZone.GetEntry(zone.ParentZoneId) : null)
                if (zoneChallengeTracks.Value.TryGetValue(zone.Id, out uint trackId))
                    return trackId;

            return 0u;
        }

        private static readonly Lazy<Dictionary<uint, uint>> zoneChallengeTracks = new(() =>
            GameTableManager.Instance.Challenge.Entries
                .Where(e => e.RewardTrackId != 0u && e.WorldZoneId != 0u)
                .GroupBy(e => e.WorldZoneId)
                .ToDictionary(g => g.Key, g => g
                    .GroupBy(e => e.RewardTrackId)
                    .OrderByDescending(t => t.Count())
                    .First().Key));

        /// <summary>
        /// Add reward points to the supplied reward track, applying the player's reward property multipliers.
        /// </summary>
        /// <remarks>
        /// The client works out the points shown in the challenge log the same way, see ChallengesLib.GetRewardTrackPoints:
        /// base points times the RewardTrackType reward property for the track type (Signature players get +25% on challenge tracks).
        /// </remarks>
        public uint AddPoints(uint rewardTrackId, uint points)
        {
            if (!rewardTracks.TryGetValue(rewardTrackId, out RewardTrack track))
                return 0u;

            float multiplier = GetRewardPropertyMultiplier(RewardPropertyType.RewardTrackType, track.Entry.RewardTrackTypeEnum)
                * GetRewardPropertyMultiplier(RewardPropertyType.RewardTrackId, track.Entry.Id);
            points = (uint)(points * multiplier);

            uint[] costs = GetCosts(track.Entry);
            if (costs.Length == 0)
                return 0u;

            // points stop at the final reward point until it is claimed and the track restarts
            uint before  = track.Points;
            track.Points = Math.Min(track.Points + points, costs[^1]);
            track.IsDirty = true;

            SendUpdate(track);
            return track.Points - before;
        }

        private float GetRewardPropertyMultiplier(RewardPropertyType type, uint data)
        {
            IRewardProperty rewardProperty = player.Account.RewardPropertyManager.GetRewardProperty(type);
            float? value = rewardProperty?.GetValue(data);
            if (value == null)
                return 1f;

            return (RewardModifierValueType)rewardProperty.Entry.RewardModifierValueTypeEnum switch
            {
                RewardModifierValueType.AdditiveScalar       => 1f + value.Value,
                RewardModifierValueType.MultiplicativeScalar => value.Value,
                _                                            => 1f
            };
        }

        /// <summary>
        /// Claim the reward of a reward point that has been reached, with the supplied choice for rewards with several choices.
        /// </summary>
        public void ClaimReward(uint rewardTrackRewardsId, uint choiceIndex)
        {
            RewardTrackRewardsEntry rewardEntry = GameTableManager.Instance.RewardTrackRewards.GetEntry(rewardTrackRewardsId);
            if (rewardEntry == null || !rewardTracks.TryGetValue(rewardEntry.RewardTrackId, out RewardTrack track))
                return;

            int index = track.Rewards.IndexOf(rewardEntry);
            if (index < 0)
                return;

            uint[] costs = GetCosts(track.Entry);
            if (index >= costs.Length || track.Points < costs[index] || (track.ClaimedRewards & (1u << index)) != 0u)
                return;

            uint[] choiceIds    = [rewardEntry.RewardChoiceId00, rewardEntry.RewardChoiceId01, rewardEntry.RewardChoiceId02];
            uint[] choiceTypes  = [rewardEntry.RewardTrackRewardTypeEnum00, rewardEntry.RewardTrackRewardTypeEnum01, rewardEntry.RewardTrackRewardTypeEnum02];
            uint[] choiceCounts = [rewardEntry.RewardChoiceCount00, rewardEntry.RewardChoiceCount01, rewardEntry.RewardChoiceCount02];
            int choices = choiceIds.Count(c => c != 0u);

            // the client sends -1 when there is nothing to choose and 0 when there is a single choice
            if (choices > 0 && choiceIndex >= choices)
                return;

            if (choices > 0)
            {
                uint count = Math.Max(choiceCounts[choiceIndex], 1u);
                switch ((RewardTrackRewardType)choiceTypes[choiceIndex])
                {
                    case RewardTrackRewardType.Item:
                    {
                        if (player.Inventory.IsInventoryFull(InventoryLocation.Inventory))
                        {
                            player.SendGenericError(GenericError.ItemInventoryFull);
                            return;
                        }

                        player.Inventory.ItemCreate(InventoryLocation.Inventory, choiceIds[choiceIndex], count, ItemUpdateReason.RewardTrack);
                        break;
                    }
                    case RewardTrackRewardType.AccountItem:
                    {
                        for (uint i = 0u; i < count; i++)
                            player.Account.ItemManager.AddItem(choiceIds[choiceIndex]);
                        break;
                    }
                }
            }

            if (rewardEntry.CurrencyTypeId != 0u && rewardEntry.CurrencyAmount != 0u)
                player.CurrencyManager.CurrencyAddAmount((CurrencyType)rewardEntry.CurrencyTypeId, rewardEntry.CurrencyAmount);

            track.ClaimedRewards |= 1u << index;
            track.IsDirty = true;

            if (BitOperations.PopCount(track.ClaimedRewards) >= costs.Length)
            {
                // finished tracks restart from the beginning with a new pick of rewards
                track.Points         = 0u;
                track.ClaimedRewards = 0u;
                track.Iteration++;
                SelectRewards(track);

                player.Session.EnqueueMessageEncrypted(new ServerRewardTrackAdd
                {
                    RewardTrack = BuildTrack(track)
                });
            }
            else
                SendUpdate(track);
        }

        private static uint[] GetCosts(RewardTrackEntry entry)
        {
            return new[]
            {
                entry.RewardPointCost00, entry.RewardPointCost01, entry.RewardPointCost02, entry.RewardPointCost03, entry.RewardPointCost04,
                entry.RewardPointCost05, entry.RewardPointCost06, entry.RewardPointCost07, entry.RewardPointCost08, entry.RewardPointCost09
            }
            .TakeWhile(c => c != 0u)
            .ToArray();
        }

        /// <summary>
        /// Pick the reward of each reward point from the candidates the player meets the prerequisite of.
        /// </summary>
        /// <remarks>
        /// Candidates of a reward point differ in the gear bag they offer (shield, weapon attachment, support system, implant...),
        /// the pick is random for each pass through the track but stays the same until the track restarts.
        /// </remarks>
        private void SelectRewards(RewardTrack track)
        {
            track.Rewards.Clear();
            if (!rewardCandidates.Value.TryGetValue(track.Entry.Id, out List<RewardTrackRewardsEntry>[] points))
                return;

            // seeded from the character, track and pass so the pick survives logging out and server restarts
            var random = new Random((int)((player.CharacterId * 7919ul + track.Entry.Id * 104729ul + track.Iteration * 15485863ul) % int.MaxValue));
            for (int i = 0; i < GetCosts(track.Entry).Length; i++)
            {
                List<RewardTrackRewardsEntry> candidates = points[i]
                    .Where(e => e.PrerequisiteId == 0u || PrerequisiteManager.Instance.Meets(player, e.PrerequisiteId))
                    .ToList();
                if (candidates.Count == 0)
                    break;

                track.Rewards.Add(candidates[random.Next(candidates.Count)]);
            }
        }

        private NetworkRewardTrack BuildTrack(RewardTrack track)
        {
            var networkTrack = new NetworkRewardTrack
            {
                RewardTrackId         = track.Entry.Id,
                RewardPointsEarned    = track.Points,
                ClaimedRewards        = BuildClaimedRewards(track),
                Unlocked              = 1u,
                RewardTrackRewardsIds = track.Rewards.Select(r => r.Id).ToList(),
                IsActive              = track.Entry.Id == activeChallengeTrackId
            };
            return networkTrack;
        }

        private static NetworkBitArray BuildClaimedRewards(RewardTrack track)
        {
            var claimed = new NetworkBitArray(32, NetworkBitArray.BitOrder.LeastSignificantBit);
            for (uint i = 0u; i < 32u; i++)
                if ((track.ClaimedRewards & (1u << (int)i)) != 0u)
                    claimed.SetBit(i, true);
            return claimed;
        }

        private void SendUpdate(RewardTrack track)
        {
            player.Session.EnqueueMessageEncrypted(new ServerRewardTrackUpdate
            {
                RewardTrackId      = track.Entry.Id,
                RewardPointsEarned = track.Points,
                ClaimedRewards     = BuildClaimedRewards(track),
                Unlocked           = 1u,
                IsActive           = track.Entry.Id == activeChallengeTrackId
            });
        }
    }
}
