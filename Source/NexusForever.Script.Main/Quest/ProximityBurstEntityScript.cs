using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Spell;
using NexusForever.GameTable;
using NexusForever.Script.Template;
using NexusForever.Script.Template.Filter;
using NexusForever.Shared;

namespace NexusForever.Script.Main.Quest
{
    /// <summary>
    /// Hazards that sit where they were placed, go off when a player walks up to them and are gone until they respawn:
    /// Marauder Mines (Q4544, creature 16718) and Fungal Spores (creature 19193).
    /// </summary>
    /// <remarks>
    /// These creatures are objects (archetype 25 and 29), not fighters, but their factions made CombatAI chase and punch with them.
    /// <see cref="ScriptFilterDynamicCombatAI.IsEnvironmentalHazard"/> keeps CombatAI off them, this script gives them the behaviour.
    /// The spells are the ones the client data names for each: "Mine Explosion - Tremor Ridge" (26440: tech damage in 5 m, knock down state, despawns the
    /// caster) with the 2 s "Detonate Mine - Fluff" visual (26537), and "Fungal Cloud - 2 - Base [B] (Tier 1)" (27631: a 4.2 s cast that proxies to the
    /// 10 m magic damage over time cloud, 27632). The trigger distances and the mine's fuse are guesses, there is no retail data for them.
    /// Add a creature here to give it the same behaviour (and to <see cref="ScriptFilterDynamicCombatAI.IsEnvironmentalHazard"/> if CombatAI loads on it).
    /// </remarks>
    [ScriptFilterCreatureId(16718, 19193)]
    public class ProximityBurstEntityScript : IUnitScript, IOwnedScript<IUnitEntity>
    {
        /// <param name="TriggerRange">Distance a player sets it off from.</param>
        /// <param name="Fuse">Seconds between the trigger and the burst spell.</param>
        /// <param name="FluffSpell4Id">Visual played when it is triggered, 0 for none.</param>
        /// <param name="BurstSpell4Id">Spell cast at the end of the fuse.</param>
        /// <param name="DespawnAfterBurst">The script despawns the hazard when the burst spell is done, false if the spell does it itself.</param>
        /// <param name="DisarmOnActivate">Activating the hazard (quest disarm) removes it without a burst.</param>
        private sealed record Burst(float TriggerRange, double Fuse, uint FluffSpell4Id, uint BurstSpell4Id, bool DespawnAfterBurst, bool DisarmOnActivate);

        private static readonly Dictionary<uint, Burst> bursts = new()
        {
            // Marauder Mine
            [16718] = new Burst(3.5f, 1d, 26537, 26440, false, true),
            // fungal spore, the scientist scan object
            [19193] = new Burst(4f, 0d, 0, 27631, true, false)
        };

        /// <summary>
        /// Players further above or below than this don't set it off.
        /// </summary>
        private const float VerticalLimit = 3f;

        /// <summary>
        /// Time to let the burst spell finish before the hazard is removed.
        /// </summary>
        private const double DespawnGrace = 0.5d;

        private static readonly TimeSpan RespawnDelay = TimeSpan.FromMinutes(5d);

        private IUnitEntity owner;
        private Burst burst;
        private uint triggerGuid;
        private double fuse;
        private bool armed;
        private bool burstCast;
        private double despawnIn;

        #region Dependency Injection

        private readonly IFactory<ISpellParameters> spellParametersFactory;
        private readonly IGameTableManager gameTableManager;

        public ProximityBurstEntityScript(
            IFactory<ISpellParameters> spellParametersFactory,
            IGameTableManager gameTableManager)
        {
            this.spellParametersFactory = spellParametersFactory;
            this.gameTableManager       = gameTableManager;
        }

        #endregion

        /// <summary>
        /// Invoked when <see cref="IScript"/> is loaded.
        /// </summary>
        public void OnLoad(IUnitEntity owner)
        {
            this.owner = owner;
            burst      = bursts[owner.CreatureId];
            owner.SetInRangeCheck(burst.TriggerRange);
        }

        /// <summary>
        /// Invoked when <see cref="IGridEntity"/> is added to range check range.
        /// </summary>
        public void OnEnterRange(IGridEntity entity)
        {
            if (armed || burstCast || !owner.IsAlive)
                return;

            // only a character walking up to it sets it off, a player flying over on a taxi doesn't
            if (entity is not IPlayer { IsAlive: true, PlatformGuid: null } player)
                return;

            if (MathF.Abs(player.Position.Y - owner.Position.Y) > VerticalLimit)
                return;

            armed       = true;
            fuse        = burst.Fuse;
            triggerGuid = player.Guid;

            if (burst.FluffSpell4Id != 0u)
                owner.CastSpell(burst.FluffSpell4Id, spellParametersFactory.Resolve());
        }

        public void Update(double lastTick)
        {
            if (burstCast)
            {
                if (!burst.DespawnAfterBurst)
                    return;

                despawnIn -= lastTick;
                if (despawnIn <= 0d && owner.Map != null)
                {
                    burstCast = false;
                    owner.DespawnWithRespawn(RespawnDelay);
                }

                return;
            }

            if (!armed)
                return;

            fuse -= lastTick;
            if (fuse > 0d)
                return;

            armed = false;
            if (!owner.IsAlive || owner.Map == null)
                return;

            // the burst picks the players within its range itself, the one that triggered it is the primary target
            var parameters = spellParametersFactory.Resolve();
            parameters.PrimaryTargetId = triggerGuid;
            owner.CastSpell(burst.BurstSpell4Id, parameters);

            burstCast = true;
            double castTime = (gameTableManager.Spell4.GetEntry(burst.BurstSpell4Id)?.CastTime ?? 0u) / 1000d;
            despawnIn = castTime + DespawnGrace;
        }

        /// <summary>
        /// Invoked when <see cref="IWorldEntity"/> is successfully activated by <see cref="IPlayer"/>.
        /// </summary>
        public void OnActivateSuccess(IPlayer activator)
        {
            if (!burst.DisarmOnActivate || burstCast)
                return;

            // disarmed: no explosion, the hazard is gone until it respawns
            armed = false;
            owner.DespawnWithRespawn(RespawnDelay);
        }
    }
}
