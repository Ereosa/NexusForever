using NexusForever.Game.Abstract.Entity;
using NexusForever.Script.Template;
using NexusForever.Script.Template.Filter;

namespace NexusForever.Script.Main.Challenge
{
    /// <summary>
    /// Creatures collected by running or jumping through them, touching one starts their challenge, see <see cref="TouchChallengeScript"/>.
    /// </summary>
    /// <remarks>
    /// These challenges have nothing else to start them besides the challenge log and none of them is flagged to start by hand only.
    /// </remarks>
    [ScriptFilterCreatureId(28977, 30974, 9374, 20791, 20870, 20871, 20875, 18481, 39679)]
    public class TouchChallengeStarterEntityScript : IWorldEntityScript, IOwnedScript<IWorldEntity>, IOwnedScript<INonPlayerEntity>
    {
        private const float TouchRange = 3f;

        private static readonly Dictionary<uint, uint> challengeByCreature = new()
        {
            [28977] = 410, // Flitterfly Chaser, Everstar Butterfly
            [30974] = 493, // Chasing Fireflies
            [9374]  = 172, // Whimsical Whimfibers
            [20791] = 172,
            [20870] = 172,
            [20871] = 172,
            [20875] = 172,
            [18481] = 191, // Uplifting Updrafts
            [39679] = 658  // Demolition Expert, Strain Mine
        };

        private IWorldEntity owner;

        public void OnLoad(IWorldEntity owner)
        {
            this.owner = owner;
            owner.SetInRangeCheck(TouchRange);
        }

        public void OnLoad(INonPlayerEntity owner)
        {
            OnLoad((IWorldEntity)owner);
        }

        public void OnEnterRange(IGridEntity entity)
        {
            if (entity is IPlayer player && challengeByCreature.TryGetValue(owner.CreatureId, out uint challengeId))
                player.ChallengeManager.TryAutoStart(challengeId);
        }
    }
}
