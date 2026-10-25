using System.Collections;
using System.Numerics;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using NexusForever.Database.Character;
using NexusForever.Database.Character.Model;
using NexusForever.Game.Abstract.Crafting;
using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Static.Crafting;
using NexusForever.Game.Static.Entity;
using NexusForever.Game.Static.Quest;
using NexusForever.GameTable;
using NexusForever.GameTable.Model;
using NexusForever.Network.World.Message.Model.Crafting;
using NLog;

namespace NexusForever.Game.Crafting
{
    public class TradeskillManager : ITradeskillManager
    {
        /// <summary>
        /// Maximum number of active tradeskills, hobbies don't count towards this limit.
        /// </summary>
        /// <remarks>
        /// Matches knMaxTradeskills in the client TradeskillTrainer addon.
        /// </remarks>
        private const int MaxActiveTradeskills = 2;

        /// <summary>
        /// Cooldown applied after swapping an active tradeskill for another.
        /// </summary>
        private static readonly TimeSpan RelearnCooldownDuration = TimeSpan.FromDays(1);

        /// <summary>
        /// Hobbies that are learned automatically and the level they are granted at.
        /// </summary>
        /// <remarks>
        /// Levels come from the tradeskill descriptions ("This hobby is learned automatically at level X.").
        /// </remarks>
        private static readonly Dictionary<TradeskillType, byte> autoLearnTradeskills = new()
        {
            { TradeskillType.Cooking,      1  },
            { TradeskillType.Farmer,       10 },
            { TradeskillType.Runecrafting, 15 }
        };

        /// <summary>
        /// Flags from <see cref="TradeskillSchematic2Entry"/>.
        /// </summary>
        [Flags]
        private enum SchematicFlags
        {
            AutoLearn = 0x01
        }

        private static readonly ILogger log = LogManager.GetCurrentClassLogger();

        public TimeSpan RelearnCooldown
        {
            get
            {
                if (relearnCooldown == null)
                    return TimeSpan.Zero;

                TimeSpan remaining = relearnCooldown.Value - DateTime.UtcNow;
                return remaining > TimeSpan.Zero ? remaining : TimeSpan.Zero;
            }
        }

        private DateTime? relearnCooldown;
        private bool relearnCooldownDirty;

        private bool sentInitialPackets;

        private readonly IPlayer player;
        private readonly Dictionary<TradeskillType, ITradeskill> tradeskills = [];
        private readonly Dictionary<uint, ITradeskillSchematic> schematics = [];

        /// <summary>
        /// Create a new <see cref="ITradeskillManager"/> from existing <see cref="CharacterModel"/> database model.
        /// </summary>
        public TradeskillManager(IPlayer owner, CharacterModel model)
        {
            player          = owner;
            relearnCooldown = model.TradeskillRelearnCooldown;

            foreach (CharacterTradeskillModel tradeskillModel in model.Tradeskill)
            {
                TradeskillEntry entry = GameTableManager.Instance.Tradeskill.GetEntry(tradeskillModel.TradeskillId);
                if (entry == null)
                {
                    log.Warn($"Character {model.Id} has unknown tradeskill {tradeskillModel.TradeskillId}!");
                    continue;
                }

                tradeskills.Add((TradeskillType)entry.Id, new Tradeskill(owner, entry, tradeskillModel));
            }

            foreach (CharacterTradeskillSchematicModel schematicModel in model.TradeskillSchematic)
            {
                TradeskillSchematic2Entry entry = GameTableManager.Instance.TradeskillSchematic2.GetEntry(schematicModel.SchematicId);
                if (entry == null)
                {
                    log.Warn($"Character {model.Id} has unknown tradeskill schematic {schematicModel.SchematicId}!");
                    continue;
                }

                schematics.Add(entry.Id, new TradeskillSchematic(owner, entry, schematicModel));
            }

            LearnAutoTradeskills();
        }

        public void Save(CharacterContext context)
        {
            foreach (ITradeskill tradeskill in tradeskills.Values)
                tradeskill.Save(context);

            foreach (ITradeskillSchematic schematic in schematics.Values)
                schematic.Save(context);

            if (relearnCooldownDirty)
            {
                // the character model is attached by the player before the managers are saved
                EntityEntry<CharacterModel> entity = context.ChangeTracker
                    .Entries<CharacterModel>()
                    .SingleOrDefault(e => e.Entity.Id == player.CharacterId)
                    ?? context.Attach(new CharacterModel { Id = player.CharacterId });

                entity.Entity.TradeskillRelearnCooldown = relearnCooldown;
                entity.Property(p => p.TradeskillRelearnCooldown).IsModified = true;

                relearnCooldownDirty = false;
            }
        }

        public void SendInitialPackets()
        {
            var professionsLoad = new ServerProfessionsLoad
            {
                Tradeskills     = tradeskills.Values.Select(t => t.Build()).ToList(),
                RelearnCooldown = (uint)RelearnCooldown.TotalMilliseconds
            };

            uint flags = 0u;
            foreach (ITradeskillSchematic schematic in schematics.Values)
            {
                int index = professionsLoad.LearnedSchematics.Count;
                professionsLoad.LearnedSchematics.Add(schematic.Id);

                if (schematic.Discovered)
                {
                    flags |= 1u << (index % 32);
                    professionsLoad.DiscoveredSchematics.Add(new ServerProfessionsLoad.DiscoveredSchematic
                    {
                        TradeskillSchematic2Id = schematic.Id,
                        Coordinates            = schematic.DiscoveryCoordinates
                    });
                }

                if (index % 32 == 31)
                {
                    professionsLoad.LearnedSchematicDiscoveredFlags.Add(flags);
                    flags = 0u;
                }
            }

            if (professionsLoad.LearnedSchematics.Count % 32 != 0)
                professionsLoad.LearnedSchematicDiscoveredFlags.Add(flags);

            player.Session.EnqueueMessageEncrypted(professionsLoad);
            sentInitialPackets = true;
        }

        public ITradeskill GetTradeskill(TradeskillType tradeskillId)
        {
            return tradeskills.TryGetValue(tradeskillId, out ITradeskill tradeskill) ? tradeskill : null;
        }

        public bool IsTradeskillActive(TradeskillType tradeskillId)
        {
            return GetTradeskill(tradeskillId)?.Active ?? false;
        }

        public void LearnTradeskill(TradeskillType learnTradeskillId, TradeskillType dropTradeskillId)
        {
            TradeskillEntry entry = GameTableManager.Instance.Tradeskill.GetEntry((ulong)learnTradeskillId);
            if (entry == null)
                throw new ArgumentException($"Invalid tradeskill {learnTradeskillId}!");

            ITradeskill learn = GetTradeskill(learnTradeskillId);
            if (learn?.Active == true)
                throw new InvalidOperationException($"Tradeskill {learnTradeskillId} is already active!");

            learn ??= new Tradeskill(player, entry);
            if (learn.IsHobby || learn.IsAutoLearn)
                throw new InvalidOperationException($"Tradeskill {learnTradeskillId} can't be learned from a trainer!");

            ITradeskill drop = null;
            if (dropTradeskillId != 0)
            {
                drop = GetTradeskill(dropTradeskillId);
                if (drop?.Active != true || drop.IsHobby)
                    throw new InvalidOperationException($"Tradeskill {dropTradeskillId} isn't an active tradeskill!");

                if (RelearnCooldown > TimeSpan.Zero)
                    throw new InvalidOperationException("Tradeskill relearn is on cooldown!");
            }
            else if (tradeskills.Values.Count(t => t.Active && !t.IsHobby) >= MaxActiveTradeskills)
                throw new InvalidOperationException("Maximum number of tradeskills are already active!");

            // relearning a previously known tradeskill keeps progress but costs credits based on the tier reached
            ulong cost = tradeskills.ContainsKey(learnTradeskillId) ? GetRelearnCost(learn) : 0ul;
            if (cost > 0ul)
            {
                if (!player.CurrencyManager.CanAfford(CurrencyType.Credits, cost))
                    throw new InvalidOperationException($"Not enough credits to relearn tradeskill {learnTradeskillId}!");

                player.CurrencyManager.CurrencySubtractAmount(CurrencyType.Credits, cost);
            }

            if (drop != null)
            {
                drop.Active = false;
                SendTradeskillUpdate(drop);

                relearnCooldown      = DateTime.UtcNow.Add(RelearnCooldownDuration);
                relearnCooldownDirty = true;
                player.Session.EnqueueMessageEncrypted(new ServerTradeskillRelearnCooldown
                {
                    RelearnCooldown = (uint)RelearnCooldown.TotalMilliseconds
                });
            }

            learn.Active = true;
            tradeskills.TryAdd(learnTradeskillId, learn);
            SendTradeskillUpdate(learn);

            LearnTierSchematics(learn);
            player.QuestManager.ObjectiveUpdate(QuestObjectiveType.LearnTradeskill, 0u, 1u);

            log.Trace($"Player {player.CharacterId} learned tradeskill {learnTradeskillId}, dropped {dropTradeskillId} for {cost} credits.");
        }

        private static ulong GetRelearnCost(ITradeskill tradeskill)
        {
            // client also shifts this value, the cost is stored in the upper 32 bits of the game table field
            return tradeskill.Tier?.RelearnCost >> 32 ?? 0ul;
        }

        public void LearnAutoTradeskills()
        {
            foreach ((TradeskillType tradeskillId, byte level) in autoLearnTradeskills)
            {
                if (player.Level < level || tradeskills.ContainsKey(tradeskillId))
                    continue;

                TradeskillEntry entry = GameTableManager.Instance.Tradeskill.GetEntry((ulong)tradeskillId);
                if (entry == null)
                    continue;

                var tradeskill = new Tradeskill(player, entry);
                tradeskills.Add(tradeskillId, tradeskill);

                if (sentInitialPackets)
                    SendTradeskillUpdate(tradeskill);

                LearnTierSchematics(tradeskill);
            }
        }

        public void AddXp(TradeskillType tradeskillId, uint xp)
        {
            if (xp == 0u)
                return;

            ITradeskill tradeskill = GetTradeskill(tradeskillId);
            if (tradeskill?.Active != true)
                return;

            uint maxXp = tradeskill.MaxTier?.RequiredXp ?? 0u;
            if (tradeskill.Xp >= maxXp)
                return;

            TradeskillTierEntry oldTier = tradeskill.Tier;
            tradeskill.Xp = Math.Min(tradeskill.Xp + xp, maxXp);
            SendTradeskillUpdate(tradeskill);

            if (tradeskill.Tier != oldTier)
                LearnTierSchematics(tradeskill);
        }

        /// <summary>
        /// Learn any automatically learned schematics for the current and previous tiers of the supplied <see cref="ITradeskill"/>.
        /// </summary>
        private void LearnTierSchematics(ITradeskill tradeskill)
        {
            uint tier = tradeskill.Tier?.Tier ?? 1u;
            foreach (TradeskillSchematic2Entry entry in GameTableManager.Instance.TradeskillSchematic2.Entries
                .Where(s => s.TradeSkillId == (uint)tradeskill.Id
                    && s.Tier <= tier
                    && ((SchematicFlags)s.Flags & SchematicFlags.AutoLearn) != 0))
            {
                if (!schematics.ContainsKey(entry.Id))
                    LearnSchematic(entry);
            }
        }

        public bool IsSchematicKnown(uint schematicId)
        {
            return schematics.ContainsKey(schematicId);
        }

        public void LearnSchematic(uint schematicId, Vector2? discoveryCoordinates = null)
        {
            TradeskillSchematic2Entry entry = GameTableManager.Instance.TradeskillSchematic2.GetEntry(schematicId);
            if (entry == null)
                throw new ArgumentException($"Invalid tradeskill schematic {schematicId}!");

            if (schematics.ContainsKey(schematicId))
                return;

            LearnSchematic(entry, discoveryCoordinates);
        }

        private void LearnSchematic(TradeskillSchematic2Entry entry, Vector2? discoveryCoordinates = null)
        {
            var schematic = new TradeskillSchematic(player, entry, discoveryCoordinates);
            schematics.Add(entry.Id, schematic);

            if (sentInitialPackets)
            {
                player.Session.EnqueueMessageEncrypted(new ServerSchematicAddLearned
                {
                    TradeskillId           = (TradeskillType)entry.TradeSkillId,
                    TradeskillSchematic2Id = entry.Id,
                    DiscoveryCoordinates   = schematic.DiscoveryCoordinates
                });
            }

            player.QuestManager.ObjectiveUpdate(QuestObjectiveType.ObtainSchematic, entry.Id, 1u);
        }

        private void SendTradeskillUpdate(ITradeskill tradeskill)
        {
            if (!sentInitialPackets)
                return;

            player.Session.EnqueueMessageEncrypted(new ServerProfessionUpdate
            {
                Tradeskill = tradeskill.Build()
            });
        }

        public IEnumerator<ITradeskill> GetEnumerator()
        {
            return tradeskills.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
