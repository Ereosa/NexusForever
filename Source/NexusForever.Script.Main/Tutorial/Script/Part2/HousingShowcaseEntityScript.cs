using System.Numerics;
using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Spell;
using NexusForever.Script.Template;
using NexusForever.Script.Template.Filter;
using NexusForever.Shared;

namespace NexusForever.Script.Main.Tutorial.Script.Part2
{
    /// <summary>
    /// Housing NPCs in A Claim to Stake that show off a decoration, sky or music when talked to.
    /// </summary>
    /// <remarks>
    /// In retail this came from a dialog option, which the server doesn't support yet, so the demo plays on talk.
    /// </remarks>
    [ScriptFilterCreatureId(DecorationsExpert, SkySpecialist, MusicManager)]
    public class HousingShowcaseEntityScript : IWorldEntityScript, IOwnedScript<IWorldEntity>
    {
        private const uint DecorationsExpert = 73662;
        private const uint SkySpecialist     = 73663;
        private const uint MusicManager      = 73664;

        private enum SpellId
        {
            SkyboxChange       = 85631,
            MusicChange        = 85632,
            DecorStatue        = 85662,
            DecorFountain      = 85672,
            DecorTikiBar       = 85673,
            DecorLoppAltar     = 86878,
            DecorLoppPlushie   = 86879,
            DecorGardenArch    = 86880
        }

        // the six decorations experts stand at different plots, each shows a different example
        private static readonly (Vector3 Position, SpellId Spell)[] decorations =
        [
            (new Vector3(1299.263f,  -494.7637f, 107.50546f), SpellId.DecorStatue),
            (new Vector3(1207.9496f, -494.7637f, 209.54959f), SpellId.DecorFountain),
            (new Vector3(1204.6017f, -494.7637f, 236.3515f),  SpellId.DecorTikiBar),
            (new Vector3(1284.7495f, -494.7113f, 355.8576f),  SpellId.DecorLoppAltar),
            (new Vector3(1286.3859f, -494.6626f, 99.546455f), SpellId.DecorLoppPlushie),
            (new Vector3(1298.2299f, -494.7090f, 334.4261f),  SpellId.DecorGardenArch)
        ];

        private IWorldEntity owner;

        #region Dependency Injection

        private readonly IFactory<ISpellParameters> spellParameterFactory;

        public HousingShowcaseEntityScript(
            IFactory<ISpellParameters> spellParameterFactory)
        {
            this.spellParameterFactory = spellParameterFactory;
        }

        #endregion

        /// <summary>
        /// Invoked when <see cref="IScript"/> is loaded.
        /// </summary>
        public void OnLoad(IWorldEntity owner)
        {
            this.owner = owner;
        }

        /// <summary>
        /// Invoked when <see cref="IPlayer"/> talks to <see cref="IWorldEntity"/>.
        /// </summary>
        public void OnTalkTo(IPlayer player)
        {
            SpellId spell = owner.CreatureId switch
            {
                SkySpecialist => SpellId.SkyboxChange,
                MusicManager  => SpellId.MusicChange,
                _             => decorations.MinBy(d => Vector3.Distance(d.Position, owner.Position)).Spell
            };

            ISpellParameters parameters = spellParameterFactory.Resolve();
            parameters.UserInitiatedSpellCast = false;
            parameters.PrimaryTargetId        = owner.Guid;
            player.CastSpell((uint)spell, parameters);
        }
    }
}
