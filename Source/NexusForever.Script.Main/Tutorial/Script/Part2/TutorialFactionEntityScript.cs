using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Static.Reputation;
using NexusForever.Script.Template;
using NexusForever.Script.Template.Filter;

namespace NexusForever.Script.Main.Tutorial.Script.Part2
{
    /// <summary>
    /// Tutorial creatures that only one faction sees in the simulations both factions share (mission simulation and Protostar skyplot).
    /// </summary>
    /// <remarks>
    /// The mount displays carry faction visibility prerequisites in the game tables (18 Dominion, 19 Exile), the guides and shoppers
    /// don't but stand on the same spots as their counterparts from the other faction.
    /// </remarks>
    public abstract class TutorialFactionEntityScript : ICanSeeMeScript
    {
        protected abstract Faction Faction { get; }

        /// <summary>
        /// Determines whether the specified <see cref="IGridEntity"/> can see this entity.
        /// </summary>
        public bool CanSeeMe(IGridEntity entity)
        {
            if (entity is not IPlayer player)
                return true;

            return player.Faction2 == Faction;
        }
    }

    [ScriptFilterCreatureId(
        73491,                                          // Dorian Walker (virtual)
        74781, 74782, 74783, 74784,                     // Woolie, Trask, Equivar and Grinder mount displays
        74992, 74993, 74997, 74998, 74999, 75000, 75007, 75008)] // Exile housing shoppers
    public class ExileTutorialEntityScript : TutorialFactionEntityScript
    {
        protected override Faction Faction => Faction.Exile;
    }

    [ScriptFilterCreatureId(
        74861,                                          // Artemis Zin (virtual)
        74785, 74786, 74788, 74789,                     // Uniblade, Warpig, Orbitron and Velocirex mount displays
        74994, 74995, 74996, 75001, 75002, 75003, 75004, 75005, 75006)] // Dominion housing shoppers
    public class DominionTutorialEntityScript : TutorialFactionEntityScript
    {
        protected override Faction Faction => Faction.Dominion;
    }
}
