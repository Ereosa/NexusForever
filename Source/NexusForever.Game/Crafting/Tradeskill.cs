using Microsoft.EntityFrameworkCore.ChangeTracking;
using NexusForever.Database.Character;
using NexusForever.Database.Character.Model;
using NexusForever.Game.Abstract.Crafting;
using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Static.Crafting;
using NexusForever.GameTable;
using NexusForever.GameTable.Model;
using NexusForever.Network.World.Message.Model.Shared;

namespace NexusForever.Game.Crafting
{
    public class Tradeskill : ITradeskill
    {
        [Flags]
        private enum SaveMask
        {
            None   = 0x00,
            Create = 0x01,
            Xp     = 0x02,
            Active = 0x04
        }

        /// <summary>
        /// Flags from <see cref="TradeskillEntry"/>.
        /// </summary>
        [Flags]
        private enum TradeskillFlags
        {
            Hobby              = 0x01,
            Harvesting         = 0x02,
            CircuitCrafting    = 0x04,
            CoordinateCrafting = 0x08,
            AutoLearn          = 0x10
        }

        public TradeskillType Id => (TradeskillType)Entry.Id;
        public TradeskillEntry Entry { get; }

        public uint Xp
        {
            get => xp;
            set
            {
                xp = value;
                saveMask |= SaveMask.Xp;
            }
        }
        private uint xp;

        public bool Active
        {
            get => active;
            set
            {
                active = value;
                saveMask |= SaveMask.Active;
            }
        }
        private bool active;

        public TradeskillTierEntry Tier
        {
            get
            {
                TradeskillTierEntry current = tiers.FirstOrDefault();
                foreach (TradeskillTierEntry tier in tiers)
                    if (xp >= tier.RequiredXp)
                        current = tier;

                return current;
            }
        }

        public TradeskillTierEntry MaxTier => tiers.LastOrDefault();

        public bool IsHobby => ((TradeskillFlags)Entry.Flags & TradeskillFlags.Hobby) != 0;
        public bool IsHarvesting => ((TradeskillFlags)Entry.Flags & TradeskillFlags.Harvesting) != 0;
        public bool IsAutoLearn => ((TradeskillFlags)Entry.Flags & TradeskillFlags.AutoLearn) != 0;

        private SaveMask saveMask;

        private readonly IPlayer player;
        private readonly List<TradeskillTierEntry> tiers;

        /// <summary>
        /// Create a new <see cref="ITradeskill"/> from an existing database model.
        /// </summary>
        public Tradeskill(IPlayer player, TradeskillEntry entry, CharacterTradeskillModel model)
        {
            this.player = player;
            Entry       = entry;
            tiers       = GetTiers(entry);
            xp          = model.Xp;
            active      = model.Active;
            saveMask    = SaveMask.None;
        }

        /// <summary>
        /// Create a new <see cref="ITradeskill"/> from the supplied <see cref="TradeskillEntry"/>.
        /// </summary>
        public Tradeskill(IPlayer player, TradeskillEntry entry)
        {
            this.player = player;
            Entry       = entry;
            tiers       = GetTiers(entry);
            active      = true;
            saveMask    = SaveMask.Create;
        }

        private static List<TradeskillTierEntry> GetTiers(TradeskillEntry entry)
        {
            // only tiers that require more experience than the previous can be reached through experience,
            // some later tiers (eg: Architect tier 6) share the requirement of the tier before them
            var tiers = new List<TradeskillTierEntry>();
            foreach (TradeskillTierEntry tier in GameTableManager.Instance.TradeskillTier.Entries
                .Where(t => t.TradeSkillId == entry.Id)
                .OrderBy(t => t.Tier))
            {
                if (tiers.Count > 0 && tier.RequiredXp <= tiers[^1].RequiredXp)
                    continue;

                tiers.Add(tier);
            }

            return tiers;
        }

        public void Save(CharacterContext context)
        {
            if (saveMask == SaveMask.None)
                return;

            var model = new CharacterTradeskillModel
            {
                Id           = player.CharacterId,
                TradeskillId = (uint)Id,
                Xp           = Xp,
                Active       = Active
            };

            if ((saveMask & SaveMask.Create) != 0)
                context.Add(model);
            else
            {
                EntityEntry<CharacterTradeskillModel> entity = context.Attach(model);
                if ((saveMask & SaveMask.Xp) != 0)
                    entity.Property(p => p.Xp).IsModified = true;
                if ((saveMask & SaveMask.Active) != 0)
                    entity.Property(p => p.Active).IsModified = true;
            }

            saveMask = SaveMask.None;
        }

        public TradeskillInfo Build()
        {
            return new TradeskillInfo
            {
                TradeskillId = Id,
                TradeskillXp = Xp,
                IsActive     = Active ? 1u : 0u
            };
        }
    }
}
