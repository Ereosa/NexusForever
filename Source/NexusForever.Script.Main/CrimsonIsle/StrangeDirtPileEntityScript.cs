using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Spell;
using NexusForever.Script.Template;
using NexusForever.Script.Template.Filter;
using NexusForever.Shared;

namespace NexusForever.Script.Main.CrimsonIsle
{
    /// <summary>
    /// Strange Dirt Pile on Crimson Isle (Spatial Anomaly), digging in it gives the Gravitational Damping high jump to follow the anomaly up the cliffs.
    /// </summary>
    /// <remarks>
    /// Nothing in the game tables casts Gravitational Damping (31091, 26 seconds of higher jumps and slower falls), retail applied it
    /// when the pile was used (report 89).
    /// </remarks>
    [ScriptFilterCreatureId(25017)]
    public class StrangeDirtPileEntityScript : IWorldEntityScript, IOwnedScript<IWorldEntity>
    {
        private const uint GravitationalDampingSpell4Id = 31091;

        #region Dependency Injection

        private readonly IFactory<ISpellParameters> spellParametersFactory;

        public StrangeDirtPileEntityScript(
            IFactory<ISpellParameters> spellParametersFactory)
        {
            this.spellParametersFactory = spellParametersFactory;
        }

        #endregion

        public void OnLoad(IWorldEntity owner)
        {
        }

        /// <summary>
        /// Invoked when <see cref="IWorldEntity"/> is successfully activated by <see cref="IPlayer"/>.
        /// </summary>
        public void OnActivateSuccess(IPlayer activator)
        {
            ISpellParameters parameters = spellParametersFactory.Resolve();
            parameters.UserInitiatedSpellCast = false;
            parameters.PrimaryTargetId        = activator.Guid;
            activator.CastSpell(GravitationalDampingSpell4Id, parameters);
        }
    }
}
