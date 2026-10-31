using NexusForever.GameTable;
using NexusForever.GameTable.Model;
using NexusForever.GameTable.Static;

namespace NexusForever.Script.Template.Filter.Dynamic
{
    public class ScriptFilterDynamicCombatAI : IScriptFilterDynamicCombatAI
    {
        private HashSet<uint> creatureIds;

        /// <summary>
        /// Creatures flagged NoAI that still need the combat AI, eg: soldier holdout wave units.
        /// </summary>
        /// <remarks>
        /// Most holdout wave creatures are NoAI in the game tables, retail drove them with the holdout's server script.
        /// </remarks>
        private static readonly HashSet<uint> additionalCreatureIds = [];

        /// <summary>
        /// Give creatures that are flagged NoAI the combat AI anyway.
        /// </summary>
        public static void AddCreatures(IEnumerable<uint> creatureIds)
        {
            lock (additionalCreatureIds)
                additionalCreatureIds.UnionWith(creatureIds);
        }

        #region Dependency Injection

        private readonly IGameTableManager gameTableManager;

        public ScriptFilterDynamicCombatAI(
            IGameTableManager gameTableManager)
        {
            this.gameTableManager = gameTableManager;
        }

        #endregion

        public void Initialise(Type scriptType)
        {
            creatureIds = gameTableManager.Creature2.Entries
                .Where(e => (e.Flags & CreatureFlags.NoAI) == 0)
                .Where(e => !IsEnvironmentalHazard(e))
                .Select(e => e.Id)
                .ToHashSet();
        }

        /// <summary>
        /// Environmental hazards (security beams, traps) only move along their path and hurt what they touch, they never chase players.
        /// </summary>
        public static bool IsEnvironmentalHazard(Creature2Entry entry)
        {
            // townie, object and trap archetypes, combat archetypes flagged as hazards are bosses and adds that do fight
            return entry.Creature2ArcheTypeId is 25u or 27u or 29u
                && ((entry.Description?.Contains("Hazard", StringComparison.OrdinalIgnoreCase) ?? false)
                    || (entry.Description?.Contains("Security Beam", StringComparison.OrdinalIgnoreCase) ?? false)
                    || (entry.Description?.Contains(" Mine ", StringComparison.OrdinalIgnoreCase) ?? false));
        }

        /// <summary>
        /// Returns if <see cref="IScriptFilterSearch"/> can match dynamic filter.
        /// </summary>
        public bool Match(IScriptFilterSearch search)
        {
            if (creatureIds != null)
            {
                if (search.CreatureId == null)
                    return false;

                if (creatureIds.Contains(search.CreatureId.Value))
                    return true;

                lock (additionalCreatureIds)
                    return additionalCreatureIds.Contains(search.CreatureId.Value);
            }

            return true;
        }
    }
}
