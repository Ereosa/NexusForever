using NexusForever.Database.Character;
using NexusForever.Database.Character.Model;
using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Map;
using NexusForever.GameTable;
using NexusForever.GameTable.Model;

namespace NexusForever.Game.Map
{
    /// <summary>
    /// Prime levels unlocked per instance world, a prime level instance completed unlocks the next prime level.
    /// </summary>
    public class PrimeLevelManager : IPrimeLevelManager
    {
        private class PrimeLevel
        {
            public byte Level { get; set; }
            public bool IsNew { get; set; }
            public bool IsDirty { get; set; }
        }

        private readonly IPlayer player;
        private readonly Dictionary<ushort, PrimeLevel> primeLevels = [];

        /// <summary>
        /// Create a new <see cref="IPrimeLevelManager"/> from existing <see cref="CharacterModel"/> database model.
        /// </summary>
        public PrimeLevelManager(IPlayer owner, CharacterModel characterModel)
        {
            player = owner;

            foreach (CharacterPrimeLevelModel model in characterModel.PrimeLevel)
                primeLevels[model.WorldId] = new PrimeLevel { Level = model.PrimeLevel };
        }

        public void Save(CharacterContext context)
        {
            foreach ((ushort worldId, PrimeLevel primeLevel) in primeLevels)
            {
                if (!primeLevel.IsDirty)
                    continue;

                var model = new CharacterPrimeLevelModel
                {
                    Id         = player.CharacterId,
                    WorldId    = worldId,
                    PrimeLevel = primeLevel.Level
                };

                if (primeLevel.IsNew)
                    context.Add(model);
                else
                    context.Entry(model).Property(p => p.PrimeLevel).IsModified = true;

                primeLevel.IsNew   = false;
                primeLevel.IsDirty = false;
            }
        }

        /// <summary>
        /// Return the highest prime level unlocked for the world, completing a prime level unlocks the next one.
        /// </summary>
        public byte GetPrimeLevelUnlocked(uint worldId)
        {
            return primeLevels.TryGetValue((ushort)worldId, out PrimeLevel primeLevel) ? primeLevel.Level : (byte)0;
        }

        /// <summary>
        /// Unlock the prime level after the supplied completed prime level for the world, capped at the world's maximum.
        /// </summary>
        public void CompletePrimeLevel(uint worldId, byte primeLevel)
        {
            WorldEntry entry = GameTableManager.Instance.World.GetEntry(worldId);
            if (entry == null)
                return;

            byte unlocked = (byte)Math.Min(primeLevel + 1, entry.PrimeLevelMax);
            if (unlocked <= GetPrimeLevelUnlocked(worldId))
                return;

            if (!primeLevels.TryGetValue((ushort)worldId, out PrimeLevel existing))
            {
                existing = new PrimeLevel { IsNew = true };
                primeLevels.Add((ushort)worldId, existing);
            }

            existing.Level   = unlocked;
            existing.IsDirty = true;
        }
    }
}
