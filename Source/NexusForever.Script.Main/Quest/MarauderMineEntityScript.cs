using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Spell;
using NexusForever.Script.Template;
using NexusForever.Script.Template.Filter;
using NexusForever.Shared;

namespace NexusForever.Script.Main.Quest
{
    /// <summary>
    /// Marauder Mines of Tremor Ridge (Q4544, creature 16718): sit where they were placed, go off when a player walks up to them and
    /// can be disarmed with the quest activate.
    /// </summary>
    /// <remarks>
    /// The creature is a quest object (archetype 29, spell 1817 "Instant Activate" to disarm), not a fighter. The mine's own spells in the
    /// client data are "Mine Explosion - Tremor Ridge" (26440: tech damage in 5 m, knock down state, despawns the caster) and the 2 s "Detonate Mine - Fluff"
    /// (26537) visual. The trigger distance and the fuse are guesses, there is no retail data for either.
    /// </remarks>
    [ScriptFilterCreatureId(16718)]
    public class MarauderMineEntityScript : IUnitScript, IOwnedScript<IUnitEntity>
    {
        private const uint MineExplosionSpell4Id = 26440;
        private const uint DetonateFluffSpell4Id = 26537;

        /// <summary>
        /// Distance a player triggers the mine from, inside the 5 m of the explosion.
        /// </summary>
        private const float TriggerRange = 3.5f;

        /// <summary>
        /// Time between the mine being triggered and the explosion.
        /// </summary>
        private const double FuseSeconds = 1d;

        private static readonly TimeSpan RespawnDelay = TimeSpan.FromMinutes(5d);

        private IUnitEntity owner;
        private uint triggerGuid;
        private double fuse;
        private bool armed;

        #region Dependency Injection

        private readonly IFactory<ISpellParameters> spellParametersFactory;

        public MarauderMineEntityScript(
            IFactory<ISpellParameters> spellParametersFactory)
        {
            this.spellParametersFactory = spellParametersFactory;
        }

        #endregion

        /// <summary>
        /// Invoked when <see cref="IScript"/> is loaded.
        /// </summary>
        public void OnLoad(IUnitEntity owner)
        {
            this.owner = owner;
            owner.SetInRangeCheck(TriggerRange);
        }

        /// <summary>
        /// Invoked when <see cref="IGridEntity"/> is added to range check range.
        /// </summary>
        public void OnEnterRange(IGridEntity entity)
        {
            if (armed || !owner.IsAlive)
                return;

            // only a character stepping on it sets it off, a player flying over on a taxi doesn't
            if (entity is not IPlayer { IsAlive: true, PlatformGuid: null } player)
                return;

            armed   = true;
            fuse    = FuseSeconds;
            triggerGuid = player.Guid;

            var parameters = spellParametersFactory.Resolve();
            owner.CastSpell(DetonateFluffSpell4Id, parameters);
        }

        public void Update(double lastTick)
        {
            if (!armed)
                return;

            fuse -= lastTick;
            if (fuse > 0d)
                return;

            armed = false;
            if (!owner.IsAlive || owner.Map == null)
                return;

            // the explosion picks the players within its range itself, the one that triggered it is the primary target
            var parameters = spellParametersFactory.Resolve();
            parameters.PrimaryTargetId = triggerGuid;
            owner.CastSpell(MineExplosionSpell4Id, parameters);
        }

        /// <summary>
        /// Invoked when <see cref="IWorldEntity"/> is successfully activated by <see cref="IPlayer"/>.
        /// </summary>
        public void OnActivateSuccess(IPlayer activator)
        {
            // disarmed: no explosion, the mine is gone until it respawns
            armed = false;
            owner.DespawnWithRespawn(RespawnDelay);
        }
    }
}
