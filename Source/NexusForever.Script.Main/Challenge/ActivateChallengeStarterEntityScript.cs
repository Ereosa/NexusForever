using NexusForever.Game.Abstract.Entity;
using NexusForever.Script.Template;
using NexusForever.Script.Template.Filter;

namespace NexusForever.Script.Main.Challenge
{
    /// <summary>
    /// Objects used by challenges that are counted by a challenge script, using one starts the challenge and counts as its first.
    /// </summary>
    /// <remarks>
    /// Green Thumb (Ch185 vegetables) and Defusing the Situation (Sunderstone Proximity Mines) aren't flagged to start by hand only
    /// and have nothing in the game data that starts them.
    /// </remarks>
    [ScriptFilterCreatureId(21185, 21187, 23151)]
    public class ActivateChallengeStarterEntityScript : IWorldEntityScript, IOwnedScript<IWorldEntity>, IOwnedScript<INonPlayerEntity>
    {
        private static readonly Dictionary<uint, uint> challengeByCreature = new()
        {
            [21185] = 185, // Green Thumb, Galettuce
            [21187] = 185, // Green Thumb, Galaraddish
            [23151] = 236  // Defusing the Situation, Sunderstone Proximity Mine
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
            if (!challengeByCreature.TryGetValue(owner.CreatureId, out uint challengeId) || activator.ChallengeManager.IsActive(challengeId))
                return;

            // the challenge script only hears about objects used while it runs, this one started it
            if (activator.ChallengeManager.TryAutoStart(challengeId))
                activator.ChallengeManager.UpdateProgress(challengeId);
        }
    }
}
