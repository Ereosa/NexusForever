using System.Numerics;
using NexusForever.Game.Abstract.Quest;
using NexusForever.GameTable;
using NexusForever.GameTable.Model;
using NexusForever.Script.Template;
using NexusForever.Script.Template.Filter;

namespace NexusForever.Script.Alizar.NorthernWilds.Quest
{
    /// <summary>
    /// Empowered Tower: "Follow the power cables that lead north of the Ancient Tower".
    /// </summary>
    /// <remarks>
    /// The objective has no data in the game tables, it completes when the player reaches its indicator location (the area outside Exo-Lab 729).
    /// </remarks>
    [ScriptFilterOwnerId(3486)]
    public class EmpoweredTowerQuestScript : IQuestScript, IOwnedScript<IQuest>
    {
        private const uint FollowCablesObjectiveId = 4987;

        private IQuest owner;
        private WorldLocation2Entry location;

        #region Dependency Injection

        private readonly IGameTableManager gameTableManager;

        public EmpoweredTowerQuestScript(
            IGameTableManager gameTableManager)
        {
            this.gameTableManager = gameTableManager;
        }

        #endregion

        /// <summary>
        /// Invoked when <see cref="IQuestScript"/> is loaded.
        /// </summary>
        public void OnLoad(IQuest owner)
        {
            this.owner = owner;

            QuestObjectiveEntry objective = gameTableManager.QuestObjective.GetEntry(FollowCablesObjectiveId);
            location = gameTableManager.WorldLocation2.GetEntry(objective.WorldLocationsIdIndicator00);
        }

        /// <summary>
        /// Invoked each world tick with the delta since the previous tick occurred.
        /// </summary>
        public void Update(double lastTick)
        {
            IQuestObjective objective = owner.GetQuestObjective(FollowCablesObjectiveId);
            if (objective == null || objective.IsComplete() || location == null)
                return;

            var player = owner.GetOwner();
            if (player?.Map == null || player.Map.Entry.Id != location.WorldId)
                return;

            var position = new Vector3(location.Position0, location.Position1, location.Position2);
            if (Vector3.Distance(player.Position, position) <= location.Radius)
                owner.ObjectiveUpdate(FollowCablesObjectiveId, 1u);
        }
    }
}
