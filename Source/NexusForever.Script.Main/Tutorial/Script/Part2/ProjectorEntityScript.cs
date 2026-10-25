using System.Numerics;
using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Static.Quest;
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

        private static readonly Dictionary<uint, Destination> destinations = new()
        {
            // Housing Projector -> Protostar Model Skyplot, next to the mission projector
            { 73741, new(new Vector3(1373.29f, -494.3044f, 153.853f), 4965) },
            // Mission Projector -> Mission Simulation, next to Dorian Walker
            { 73743, new(new Vector3(606.0665f, -399.62097f, 283.1857f), 4964) },
            // Cryo Projector -> Arkship, wake from cryosleep
            { 73745, new(new Vector3(-4329.78f, -446.27277f, -4104.64f), 5998) }
        };

        private IWorldEntity owner;

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
            if (!destinations.TryGetValue(owner.CreatureId, out Destination destination))
                return;

            // the arrival point isn't always inside the zone the objective expects
            activator.QuestManager.ObjectiveUpdate(QuestObjectiveType.EnterZone, destination.ZoneId, 1u);
            activator.TeleportToLocal(destination.Position);
        }
    }
}
