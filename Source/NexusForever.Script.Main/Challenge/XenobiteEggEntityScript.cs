using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Static.Reputation;
using NexusForever.Script.Template;
using NexusForever.Script.Template.Filter;

namespace NexusForever.Script.Main.Challenge
{
    /// <summary>
    /// Xenobite Eggs in Coldburrow Cavern, the targets of the Xenobite Egg Smasher challenge ("Destroy Xenobite Eggs").
    /// </summary>
    /// <remarks>
    /// The eggs are Friendly To All in the game tables and the world spawns, so nothing could attack them. They are given the Critter
    /// faction, like the Northern Wilds jabbits, so they can be destroyed without fighting back.
    /// </remarks>
    [ScriptFilterCreatureId(12836)]
    public class XenobiteEggEntityScript : IUnitScript, IOwnedScript<ICreatureEntity>
    {
        private const Faction Critter = (Faction)174;

        public void OnLoad(ICreatureEntity owner)
        {
            owner.Faction1 = Critter;
            owner.Faction2 = Critter;
        }
    }
}
