using System.Numerics;
using NexusForever.Game.Abstract.Cinematic;
using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Static.Cinematic;
using NexusForever.Game.Static.Quest;
using NexusForever.Game.Static.Reputation;
using NexusForever.Script.Template;
using NexusForever.Script.Template.Filter;

namespace NexusForever.Script.Main.Tutorial.Script.Part2
{
    /// <summary>
    /// Hologram projectors that move the player between the tutorial simulations.
    /// </summary>
    [ScriptFilterCreatureId(73741, 73743, 73745)]
    public class ProjectorEntityScript : IWorldEntityScript, IOwnedScript<IWorldEntity>
    {
        private record Destination(Vector3 Position, uint ZoneId);

        /// <summary>
        /// Transition cinematics (Exile, Dominion) that play once the player arrives in the next simulation.
        /// </summary>
        private static readonly Dictionary<uint, (CinematicSceneId Exile, CinematicSceneId Dominion)> transitions = new()
        {
            { 73741, (CinematicSceneId.NpeHousingExile,       CinematicSceneId.NpeHousingDominion) },
            { 73743, (CinematicSceneId.NpeQuestingExile,      CinematicSceneId.NpeQuestingDominion) },
            { 73745, (CinematicSceneId.NpeDepartureBayExile,  CinematicSceneId.NpeDepartureBayDominion) }
        };

        private static readonly Dictionary<uint, Destination> destinations = new()
        {
            // Housing Projector -> Protostar Model Skyplot, next to the mission projector
            { 73741, new(new Vector3(1373.29f, -494.3044f, 153.853f), 4965) },
            // Mission Projector -> Mission Simulation, next to Dorian Walker
            { 73743, new(new Vector3(606.0665f, -399.62097f, 283.1857f), 4964) },
            // Cryo Projector -> Exile Arkship, wake from cryosleep
            { 73745, new(new Vector3(-4329.78f, -446.27277f, -4104.64f), 5998) }
        };

        private static readonly Dictionary<uint, Destination> dominionDestinations = new()
        {
            // Cryo Projector -> Dominion Arkship, in front of the cryo pod (WorldLocation2 52851, 9 m from the pod at 51657 like the Exile 51675 is from 51658)
            { 73745, new(new Vector3(-3680.6614f, -437.85535f, -4355.892f), 5999) }
        };

        private IWorldEntity owner;

        #region Dependency Injection

        private readonly ICinematicFactory cinematicFactory;

        public ProjectorEntityScript(
            ICinematicFactory cinematicFactory)
        {
            this.cinematicFactory = cinematicFactory;
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
        /// Invoked when <see cref="IWorldEntity"/> is successfully activated by <see cref="IPlayer"/>.
        /// </summary>
        public void OnActivateSuccess(IPlayer activator)
        {
            if ((activator.Faction2 != Faction.Dominion || !dominionDestinations.TryGetValue(owner.CreatureId, out Destination destination))
                && !destinations.TryGetValue(owner.CreatureId, out destination))
                return;

            // the arrival point isn't always inside the zone the objective expects
            activator.QuestManager.ObjectiveUpdate(QuestObjectiveType.EnterZone, destination.ZoneId, 1u);
            if (!transitions.TryGetValue(owner.CreatureId, out (CinematicSceneId Exile, CinematicSceneId Dominion) transition))
            {
                activator.TeleportToLocal(destination.Position);
                return;
            }

            // the transition cinematic of the next simulation plays once the player has arrived there
            CinematicSceneId sceneId = activator.Faction2 == Faction.Dominion ? transition.Dominion : transition.Exile;
            activator.TeleportToLocal(destination.Position, callback: _ =>
            {
                ICinematicBase cinematic = cinematicFactory.CreateCinematic(sceneId);
                if (cinematic != null)
                    activator.CinematicManager.QueueCinematic(cinematic);
            });
        }
    }
}
