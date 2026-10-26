using NexusForever.Game.Static.Cinematic;

namespace NexusForever.Game.Abstract.Cinematic
{
    public interface ICinematicFactory
    {
        ICinematicBase CreateCinematic<T>() where T : ICinematicBase;

        /// <summary>
        /// Create the cinematic for <see cref="CinematicSceneId"/>, returns null if the scene isn't implemented.
        /// </summary>
        ICinematicBase CreateCinematic(CinematicSceneId id);
    }
}
