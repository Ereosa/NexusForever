using System.Numerics;
using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Quest;
using NexusForever.Game.Static.Quest;
using NexusForever.Script.Template;
using NexusForever.Script.Template.Filter;

namespace NexusForever.Script.Alizar.Quest
{
    /// <summary>
    /// Mutually Assured Destruction (Whitevale): "Escape from the Terraformer in the Blasted Fields" once its data is decrypted.
    /// </summary>
    /// <remarks>
    /// The step has no marker or target, retail scripted it. It completes once the player is away from the Terraformer, measured from
    /// the decrypt step's marker (WorldLocation2 27838). The distance is a guess.
    /// </remarks>
    [ScriptFilterOwnerId(5727)]
    public class MutuallyAssuredDestructionQuestScript : IQuestScript, IOwnedScript<IQuest>
    {
        private const uint EscapeObjectiveId = 13692u;
        private const float EscapeDistance   = 80f;

        private static readonly Vector3 terraformer = new(3736.3f, -917.4f, 313.4f);

        private IQuest owner;
        private double check;

        public void OnLoad(IQuest owner)
        {
            this.owner = owner;
        }

        public void Update(double lastTick)
        {
            check -= lastTick;
            if (check > 0d)
                return;
            check = 1d;

            IPlayer player = owner.GetOwner();
            IQuestObjective objective = owner.GetQuestObjective(EscapeObjectiveId);
            if (player?.Map == null || player.IsLoading || owner.State != QuestState.Accepted || objective == null || objective.IsComplete())
                return;
            if (owner.Any(o => o.Index < objective.Index && !o.ObjectiveInfo.IsOptional() && !o.IsComplete()))
                return;

            if (Vector2.Distance(new Vector2(player.Position.X, player.Position.Z), new Vector2(terraformer.X, terraformer.Z)) >= EscapeDistance)
                owner.ObjectiveUpdate(EscapeObjectiveId, 1u);
        }
    }
}
