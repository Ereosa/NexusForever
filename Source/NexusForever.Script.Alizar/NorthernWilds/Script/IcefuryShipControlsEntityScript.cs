using NexusForever.Game.Abstract.Cinematic;
using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Static.Cinematic;
using NexusForever.Game.Static.Quest;
using NexusForever.Script.Template;
using NexusForever.Script.Template.Filter;

namespace NexusForever.Script.Alizar.NorthernWilds.Script
{
    /// <summary>
    /// Dominion shuttle controls at Camp Icefury (More Important Than Revenge), the shuttle takes off before the player is sent to Algoroc.
    /// </summary>
    /// <remarks>
    /// On the quest the departure itself is the generic quest Depart handling, it waits for the cinematic and the Algoroc map
    /// script plays the landing. Once the quest is done the controls still fly to Tremor Ridge (report 58, the player came back
    /// to Camp Icefury after the takeoff), landing next to the Tremor Ridge controls that fly back here, spot guessed.
    /// </remarks>
    [ScriptFilterCreatureId(27196)]
    public class IcefuryShipControlsEntityScript : IWorldEntityScript, IOwnedScript<IWorldEntity>
    {
        private const ushort MoreImportantThanRevenge = 3963;
        private const ushort AlgorocWorldId = 51;

        #region Dependency Injection

        private readonly ICinematicFactory cinematicFactory;

        public IcefuryShipControlsEntityScript(
            ICinematicFactory cinematicFactory)
        {
            this.cinematicFactory = cinematicFactory;
        }

        #endregion

        public void OnLoad(IWorldEntity owner)
        {
        }

        public void OnActivateSuccess(IPlayer activator)
        {
            QuestState? state = activator.QuestManager.GetQuestState(MoreImportantThanRevenge);
            bool questDeparture = state is QuestState.Accepted or QuestState.Achieved;

            void Depart()
            {
                if (questDeparture || !activator.CanTeleport())
                    return;

                ICinematicBase arrival = cinematicFactory.CreateCinematic(CinematicSceneId.AlgorocArrival);
                if (arrival != null)
                    activator.CinematicManager.QueueAfterEnteringWorld(arrival);
                activator.TeleportTo(AlgorocWorldId, 3830f, -1001f, -4469f);
            }

            ICinematicBase cinematic = cinematicFactory.CreateCinematic(CinematicSceneId.NorthernWildsDeparture);
            if (cinematic != null)
                activator.CinematicManager.QueueCinematic(cinematic, Depart);
            else
                Depart();
        }
    }
}
