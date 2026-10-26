using NexusForever.Game.Static.Cinematic;
using NexusForever.Game.Static.Entity;

namespace NexusForever.Game.Cinematic.Scene
{
    /// <summary>
    /// Definitions of the cinematics that are rebuilt from client data.
    /// </summary>
    /// <remarks>
    /// No packet captures exist for these cinematics. Actor creatures, model sequences, scene lengths and camera cuts come from the client
    /// (Creature2 "GCxxx" entries and their Art\Cinematics models), sound mixes from SoundEvent/VisualEffect and scene origins from
    /// unreferenced WorldLocation2 entries the captured cinematics sit on. What couldn't be read from data is noted on each scene.
    /// </remarks>
    public static partial class CinematicSceneCatalog
    {
        private static readonly Dictionary<CinematicSceneId, CinematicScene> scenes = [];

        static CinematicSceneCatalog()
        {
            AddNewPlayerExperience();
        }

        /// <summary>
        /// Return the <see cref="CinematicScene"/> for <see cref="CinematicSceneId"/>, null if it isn't defined.
        /// </summary>
        public static CinematicScene Get(CinematicSceneId id)
        {
            return scenes.GetValueOrDefault(id);
        }

        private static CinematicScene Add(CinematicSceneId id)
        {
            var scene = new CinematicScene(id);
            scenes.Add(id, scene);
            return scene;
        }

        /// <summary>
        /// Attachment on the NPE player platform models the player copy is attached to, it depends on the height of the race.
        /// </summary>
        /// <remarks>
        /// From the captured NPE intro, races marked in that capture as unconfirmed use the tall attachment.
        /// </remarks>
        private static uint NpePlatformAttachment(Race race, Sex sex)
        {
            return race switch
            {
                Race.Mechari or Race.Mordesh or Race.Granok or Race.Human or Race.Draken => 69u,
                _ => 24u
            };
        }
    }
}
