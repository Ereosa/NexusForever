using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Static.Reputation;
using NexusForever.Script.Template;
using NexusForever.Script.Template.Filter;

namespace NexusForever.Script.Main.Challenge
{
    /// <summary>
    /// Eggs that Combat challenges ask to destroy: Xenobite Eggs in Coldburrow Cavern (Xenobite Egg Smasher) and Ripjaw Eggs in
    /// Owanee Basin (Ravaging Ripjaws, smashed with their activate spell 72385 which damages them for their maximum health).
    /// </summary>
    /// <remarks>
    /// The eggs are Friendly To All in the game tables and the world spawns, so nothing could damage them. They are given the Critter
    /// faction, like the Northern Wilds jabbits, so they can be destroyed without fighting back.
    /// </remarks>
    [ScriptFilterCreatureId(12836, 26400)]
    public class ChallengeEggEntityScript : IUnitScript, IOwnedScript<ICreatureEntity>
    {
        private const Faction Critter = (Faction)174;

        public void OnLoad(ICreatureEntity owner)
        {
            owner.Faction1 = Critter;
            owner.Faction2 = Critter;
        }
    }
}
