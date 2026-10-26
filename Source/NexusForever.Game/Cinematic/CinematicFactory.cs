using Microsoft.Extensions.DependencyInjection;
using NexusForever.Game.Abstract.Cinematic;
using NexusForever.Game.Abstract.Cinematic.Cinematics;
using NexusForever.Game.Cinematic.Scene;
using NexusForever.Game.Static.Cinematic;

namespace NexusForever.Game.Cinematic
{
    public class CinematicFactory : ICinematicFactory
    {
        private readonly IServiceProvider serviceProvider;

        public CinematicFactory(
            IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        public ICinematicBase CreateCinematic<T>() where T : ICinematicBase
        {
            return serviceProvider.GetRequiredService<T>();
        }

        /// <summary>
        /// Create the cinematic for <see cref="CinematicSceneId"/>, returns null if the scene isn't implemented.
        /// </summary>
        public ICinematicBase CreateCinematic(CinematicSceneId id)
        {
            // scenes that were rebuilt from packet captures keep their own classes
            switch (id)
            {
                case CinematicSceneId.NpeIntroDominion:
                case CinematicSceneId.NpeIntroExile:
                    return CreateCinematic<INoviceTutorialOnEnter>();
                case CinematicSceneId.NpeCombatExile:
                    return CreateCinematic<INoviceTutorialCombatProjector>();
                case CinematicSceneId.LevianBayCassianArrival:
                    return CreateCinematic<ILevianBayOnCreate>();
                case CinematicSceneId.EverstarGroveAurinArrival:
                    return CreateCinematic<IEverstarGroveOnCreate>();
            }

            CinematicScene scene = CinematicSceneCatalog.Get(id);
            return scene != null ? new SceneCinematic(scene) : null;
        }
    }
}
