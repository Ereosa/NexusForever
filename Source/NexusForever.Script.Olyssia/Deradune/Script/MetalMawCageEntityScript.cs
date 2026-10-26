using NexusForever.Game.Abstract.Cinematic;
using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Static.Cinematic;
using NexusForever.Script.Template;
using NexusForever.Script.Template.Filter;

namespace NexusForever.Script.Olyssia.Deradune.Script
{
    /// <summary>
    /// Metal Maw's cage in the Bloodfire arena, the Metal Maw intro (GC016) plays the first time a player comes near it.
    /// </summary>
    /// <remarks>
    /// The retail trigger isn't known, no quest or event references the cinematic. Range and once per character are guessed.
    /// </remarks>
    [ScriptFilterCreatureId(26018)]
    public class MetalMawCageEntityScript : IWorldEntityScript, IOwnedScript<IWorldEntity>
    {
        #region Dependency Injection

        private readonly ICinematicFactory cinematicFactory;

        public MetalMawCageEntityScript(
            ICinematicFactory cinematicFactory)
        {
            this.cinematicFactory = cinematicFactory;
        }

        #endregion

        private readonly HashSet<ulong> played = [];

        /// <summary>
        /// Invoked when <see cref="IScript"/> is loaded.
        /// </summary>
        public void OnLoad(IWorldEntity owner)
        {
            owner.SetInRangeCheck(40f);
        }

        /// <summary>
        /// Invoked when <see cref="IGridEntity"/> is added to range check range.
        /// </summary>
        public void OnEnterRange(IGridEntity entity)
        {
            if (entity is not IPlayer player || player.CinematicManager.IsPlaying)
                return;

            if (!played.Add(player.CharacterId))
                return;

            ICinematicBase cinematic = cinematicFactory.CreateCinematic(CinematicSceneId.DeraduneMetalMawIntro);
            if (cinematic != null)
                player.CinematicManager.QueueCinematic(cinematic);
        }
    }
}
