using NexusForever.Game.Abstract.Entity;
using NexusForever.Script.Template;
using NexusForever.Script.Template.Filter;

namespace NexusForever.Script.Main.Challenge
{
    /// <summary>
    /// Objects used by challenges that are counted by a challenge script, using one starts the challenge and counts as its first.
    /// </summary>
    /// <remarks>
    /// Green Thumb, Defusing the Situation, Lowland Lavender Eggs, What's on Tap?, Scrap Yard, Veggie Delight, Shifting Priorities and Soul Sanctification
    /// aren't flagged to start by hand only and have nothing in the game data that starts them.
    /// </remarks>
    [ScriptFilterCreatureId(21185, 21187, 23151, 33152, 31482, 18441, 25444, 25445, 25448, 29893, 47132)]
    public class ActivateChallengeStarterEntityScript : IWorldEntityScript, IOwnedScript<IWorldEntity>, IOwnedScript<INonPlayerEntity>
    {
        private static readonly Dictionary<uint, (uint ChallengeId, bool Counts)> challengeByCreature = new()
        {
            [21185] = (185, true),  // Green Thumb, Galettuce
            [21187] = (185, true),  // Green Thumb, Galaraddish
            [23151] = (236, true),  // Defusing the Situation, Sunderstone Proximity Mine
            [33152] = (505, true),  // Lowland Lavender Eggs, Lowland Lavender Eggy
            [31482] = (460, false), // What's on Tap?, Bartender's Bottle (the first drink is poured when it starts)
            [18441] = (80, true),   // Scrap Yard, Eldan Scrap
            [25444] = (294, true),  // Veggie Delight, Sallec Radish
            [25445] = (294, true),  // Veggie Delight, Sallec Carrot
            [25448] = (294, true),  // Veggie Delight, Sallec Casca Carrot
            [29893] = (437, true),  // Shifting Priorities, Eldan Relay
            [47132] = (977, false)  // Soul Sanctification, Sacred Carving for CH977
        };

        private IWorldEntity owner;

        public void OnLoad(IWorldEntity owner)
        {
            this.owner = owner;
        }

        public void OnLoad(INonPlayerEntity owner)
        {
            this.owner = owner;
        }

        public void OnActivateSuccess(IPlayer activator)
        {
            if (!challengeByCreature.TryGetValue(owner.CreatureId, out (uint ChallengeId, bool Counts) starter)
                || activator.ChallengeManager.IsActive(starter.ChallengeId))
                return;

            // the challenge script only hears about objects used while it runs, this one started it
            if (activator.ChallengeManager.TryAutoStart(starter.ChallengeId) && starter.Counts)
                activator.ChallengeManager.UpdateProgress(starter.ChallengeId);
        }
    }
}
