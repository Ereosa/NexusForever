using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Spell;
using NexusForever.Game.Static.Spell;
using NexusForever.Script.Template;
using NexusForever.Script.Template.Filter;
using NexusForever.Shared;

namespace NexusForever.Script.Alizar.NorthernWilds.Script
{
    /// <summary>
    /// Speed Boost pads along the roads, running through one doubles move speed for a few seconds.
    /// </summary>
    /// <remarks>
    /// The creature's aura (spell 72260) proxies 72259 onto units in range, the aura isn't pulsed server side so the range check applies it instead.
    /// </remarks>
    [ScriptFilterCreatureId(62476)]
    public class SpeedBoostEntityScript : IWorldEntityScript, IOwnedScript<IWorldEntity>
    {
        private const uint SpeedBoostSpell4Id = 72259;

        #region Dependency Injection

        private readonly IFactory<ISpellParameters> spellParametersFactory;

        public SpeedBoostEntityScript(
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
            owner.SetInRangeCheck(3f);
        }

        /// <summary>
        /// Invoked when <see cref="IGridEntity"/> is added to range check range.
        /// </summary>
        public void OnEnterRange(IGridEntity entity)
        {
            if (entity is not IPlayer player)
                return;

            // a fresh boost replaces one still running from the previous pad
            foreach (ISpell spell in player.GetSpellsByEffect(SpellEffectType.UnitPropertyModifier)
                .Where(s => !s.IsFinished && s.Spell4Id == SpeedBoostSpell4Id)
                .ToList())
                spell.Finish();

            ISpellParameters parameters = spellParametersFactory.Resolve();
            parameters.UserInitiatedSpellCast = false;
            parameters.PrimaryTargetId        = player.Guid;
            player.CastSpell(SpeedBoostSpell4Id, parameters);
        }
    }
}
