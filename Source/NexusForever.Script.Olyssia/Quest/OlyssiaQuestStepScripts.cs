using NexusForever.Game.Abstract.Quest;
using NexusForever.Script.Template;
using NexusForever.Script.Template.Filter;

namespace NexusForever.Script.Olyssia.Quest
{
    /// <summary>
    /// The New Plague (Auroria): "The cure worked!" once the cure was tested on the prisoners.
    /// </summary>
    /// <remarks>
    /// The step has no target, it states the outcome of the test step before it, retail completed it with that step.
    /// </remarks>
    [ScriptFilterOwnerId(5618)]
    public class TheNewPlagueQuestScript : IQuestScript, IOwnedScript<IQuest>
    {
        private const uint TestCureObjectiveId  = 8405u;
        private const uint CureWorkedObjectiveId = 13062u;

        private IQuest owner;

        public void OnLoad(IQuest owner)
        {
            this.owner = owner;
        }

        public void OnObjectiveUpdate(IQuestObjective objective)
        {
            if (objective.ObjectiveInfo.Id == TestCureObjectiveId && objective.IsComplete())
                owner.ObjectiveUpdate(CureWorkedObjectiveId, 1u);
        }
    }
}
