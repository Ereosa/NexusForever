using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Map.Search;
using NexusForever.Game.Abstract.Spell;
using NexusForever.Game.Static.Spell;
using NexusForever.Script.Template;
using NexusForever.Script.Template.Filter;
using NexusForever.Shared;
using NexusForever.Shared.Game;

namespace NexusForever.Script.Alizar.Algoroc.Script
{
    /// <summary>
    /// Loftite Crystals on the Loftite Cliffs (By Leaps and Bounds), standing next to one grants the Loftite Aura jump boost.
    /// </summary>
    /// <remarks>
    /// Nothing in the game tables casts the cliffs jump buff (spell 29931), retail applied it near the crystals.
    /// The buff has no duration, it's removed a while after the player leaves the crystal so it doesn't last forever.
    /// </remarks>
    [ScriptFilterCreatureId(13120)]
    public class LoftiteCliffsCrystalEntityScript : IWorldEntityScript, IOwnedScript<IWorldEntity>
    {
        private const uint LoftiteAuraSpell4Id = 29931;
        private const float AuraRange = 6f;
        private static readonly TimeSpan BuffDuration = TimeSpan.FromSeconds(15d);

        private IWorldEntity owner;
        private readonly Dictionary<uint, double> buffed = [];
        private readonly UpdateTimer checkTimer = new(TimeSpan.FromSeconds(1d));

        #region Dependency Injection

        private readonly IFactory<ISpellParameters> spellParametersFactory;

        public LoftiteCliffsCrystalEntityScript(
            IFactory<ISpellParameters> spellParametersFactory)
        {
            this.spellParametersFactory = spellParametersFactory;
        }

        #endregion

        /// <summary>
        /// Invoked when <see cref="IScript"/> is loaded.
        /// </summary>
        public void OnLoad(IWorldEntity owner)
        {
            this.owner = owner;
            owner.SetInRangeCheck(AuraRange);
        }

        /// <summary>
        /// Invoked when <see cref="IGridEntity"/> is added to range check range.
        /// </summary>
        public void OnEnterRange(IGridEntity entity)
        {
            if (entity is not IPlayer player)
                return;

            if (!HasAura(player))
            {
                ISpellParameters parameters = spellParametersFactory.Resolve();
                parameters.UserInitiatedSpellCast = false;
                parameters.PrimaryTargetId        = player.Guid;
                player.CastSpell(LoftiteAuraSpell4Id, parameters);
            }

            buffed[player.Guid] = BuffDuration.TotalSeconds;
        }

        /// <summary>
        /// Invoked each world tick with the delta since the previous tick occurred.
        /// </summary>
        public void Update(double lastTick)
        {
            checkTimer.Update(lastTick);
            if (!checkTimer.HasElapsed)
                return;

            double elapsed = checkTimer.Duration;
            checkTimer.Reset();

            foreach (uint guid in buffed.Keys.ToList())
            {
                IPlayer player = owner.Map?.GetEntity<IPlayer>(guid);
                if (player == null)
                {
                    buffed.Remove(guid);
                    continue;
                }

                // keep the buff while the player stays near the crystal
                if (player.Position.GetDistance(owner.Position) <= AuraRange)
                {
                    buffed[guid] = BuffDuration.TotalSeconds;
                    continue;
                }

                buffed[guid] -= elapsed;
                if (buffed[guid] > 0d)
                    continue;

                buffed.Remove(guid);

                // another crystal the player moved on to keeps the buff going
                if (owner.Map.Search(player.Position, AuraRange, new SearchCheckCrystal()).Any())
                    continue;

                foreach (ISpell spell in GetAuras(player).ToList())
                    spell.Finish();
            }
        }

        private static IEnumerable<ISpell> GetAuras(IPlayer player)
        {
            return player.GetSpellsByEffect(SpellEffectType.UnitPropertyModifier)
                .Where(s => !s.IsFinished && s.Spell4Id == LoftiteAuraSpell4Id);
        }

        private static bool HasAura(IPlayer player)
        {
            return GetAuras(player).Any();
        }

        private class SearchCheckCrystal : ISearchCheck<IWorldEntity>
        {
            public bool CheckEntity(IWorldEntity entity)
            {
                return entity.CreatureId == 13120u;
            }
        }
    }
}
