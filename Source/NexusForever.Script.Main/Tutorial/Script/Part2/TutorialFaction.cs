using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Static.Reputation;

namespace NexusForever.Script.Main.Tutorial.Script.Part2
{
    public static class TutorialFaction
    {
        /// <summary>
        /// Faction used by the hostile Dominion simulation creatures (e.g. Virtual Battle Beast).
        /// </summary>
        /// <remarks>
        /// The turrets and the construct are friendly to all in the game tables, the server is expected to make them hostile.
        /// </remarks>
        public const Faction HostileSimulation = (Faction)1441;

        public static void MakeHostile(IWorldEntity entity)
        {
            entity.Faction1 = HostileSimulation;
            entity.Faction2 = HostileSimulation;
        }
    }
}
