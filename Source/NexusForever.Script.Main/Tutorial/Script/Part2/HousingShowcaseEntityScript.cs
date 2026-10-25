using System.Numerics;
using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Spell;
using NexusForever.Game.Static.Quest;
using NexusForever.Script.Template;
using NexusForever.Script.Template.Filter;
using NexusForever.Shared;

namespace NexusForever.Script.Main.Tutorial.Script.Part2
{
    /// <summary>
    /// Protostar housing NPCs in A Claim to Stake.
    /// </summary>
    /// <remarks>
    /// Talking to one starts its 2 second "Inquiring About ..." activate cast. On success the NPC counts towards the
    /// quest and shows off its example (decoration, sky or music).
    /// </remarks>
    [ScriptFilterCreatureId(LandscapeArchitect, DecorationsExpert, SkySpecialist, MusicManager)]
    public class HousingShowcaseEntityScript : IWorldEntityScript, IOwnedScript<IWorldEntity>
    {
        private const uint LandscapeArchitect = 73422;
        private const uint DecorationsExpert  = 73662;
        private const uint SkySpecialist      = 73663;
        private const uint MusicManager       = 73664;
        private const uint DecorDemo          = 73690;

        private const uint LandscapeArchitectTargetGroup = 14368;
        private const uint DecorationsExpertTargetGroup  = 14369;

        private enum SpellId
        {
            SkyboxChange     = 85631,
            MusicChange      = 85632,
            DecorStatue      = 85662,
            DecorFountain    = 85672,
            DecorTikiBar     = 85673,
            DecorLoppAltar   = 86878,
            DecorLoppPlushie = 86879,
            DecorGardenArch  = 86880
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
        public bool OnTalkTo(IPlayer player)
        {
            owner.OnActivateCast(player, 0u);
            return true;
        }

        /// <summary>
        /// Invoked when <see cref="IWorldEntity"/> is successfully activated by <see cref="IPlayer"/>.
        /// </summary>
        public void OnActivateSuccess(IPlayer activator)
        {
            // each architect and expert has its own checklist slot, so the same NPC only counts once
            switch (owner.CreatureId)
            {
                case LandscapeArchitect:
                    activator.QuestManager.ObjectiveUpdate(QuestObjectiveType.TalkToTargetGroup, LandscapeArchitectTargetGroup, owner.QuestChecklistMask);
                    break;
                case DecorationsExpert:
                    activator.QuestManager.ObjectiveUpdate(QuestObjectiveType.TalkToTargetGroup, DecorationsExpertTargetGroup, owner.QuestChecklistMask);
                    break;
                default:
                    activator.QuestManager.ObjectiveUpdate(QuestObjectiveType.TalkTo, owner.CreatureId, 1u);
                    break;
            }

            PlayDemo(activator);
        }

        private void PlayDemo(IPlayer player)
        {
            uint targetId = owner.Guid;
            SpellId? spell = owner.CreatureId switch
            {
                SkySpecialist     => SpellId.SkyboxChange,
                MusicManager      => SpellId.MusicChange,
                DecorationsExpert => decorations.MinBy(d => Vector3.Distance(d.Position, owner.Position)).Spell,
                // TODO: Landscape Architect plot example
                _                 => null
            };

            if (spell == null)
                return;

            // the decoration examples swap the visuals of the "Decor Demo" display standing next to the expert
            if (owner.CreatureId == DecorationsExpert)
            {
                IWorldEntity display = player.GetVisibleCreature<IWorldEntity>(DecorDemo)
                    .MinBy(d => Vector3.Distance(d.Position, owner.Position));
                if (display != null)
                    targetId = display.Guid;
            }

            ISpellParameters parameters = spellParameterFactory.Resolve();
            parameters.UserInitiatedSpellCast = false;
            parameters.PrimaryTargetId        = targetId;
            player.CastSpell((uint)spell.Value, parameters);
        }
    }
}
