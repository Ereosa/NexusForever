using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Static.Reputation;

namespace NexusForever.Script.Main.Tutorial.Script.Part2
{
    public static class TutorialFaction
    {
        /// <summary>
        /// Faction used by the hostile Dominion simulation creatures the Exile fight (e.g. Virtual Battle Beast).
        /// </summary>
        /// <remarks>
        /// The turrets and the construct are friendly to all in the game tables, the server is expected to make them hostile.
        /// </remarks>
        public const Faction HostileSimulation = (Faction)1441;

        /// <summary>
        /// Faction used by the hostile Exile simulation creatures the Dominion fight (e.g. Virtual Defense Dagun).
        /// </summary>
        public const Faction HostileSimulationDominion = (Faction)1442;

        /// <summary>
        /// Make the simulation entity hostile to players of the supplied <see cref="Faction"/>.
        /// </summary>
        public static void MakeHostile(IWorldEntity entity, Faction playerFaction = Faction.Exile)
        {
            Faction faction = playerFaction == Faction.Dominion ? HostileSimulationDominion : HostileSimulation;
            entity.Faction1 = faction;
            entity.Faction2 = faction;
        }
    }
}
