using NexusForever.Game.Abstract.Cinematic;
using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Static.Cinematic;
using NexusForever.GameTable;
using NexusForever.GameTable.Model;
using NexusForever.Script.Template;
using NexusForever.Script.Template.Filter;

namespace NexusForever.Script.Alizar.EverstarGrove.Script
{
    /// <summary>
    /// Portal to Celestion at Fort Firestorm (The Path to Celestion), plays Through the Portal before the player crosses to Celestion.
    /// </summary>
    /// <remarks>
    /// The portal's Depart spell carries no destination and the quest objective is entering the Celestion zone, so the player is sent to
    /// the quest's hand in location once the cinematic ends.
    /// </remarks>
    [ScriptFilterCreatureId(32269)]
    public class PortalToCelestionEntityScript : IWorldEntityScript, IOwnedScript<IWorldEntity>
    {
        private const ushort ThePathToCelestion = 6844;
        private const uint EverstarGroveWorldId = 990;

        #region Dependency Injection

        private readonly ICinematicFactory cinematicFactory;

        public PortalToCelestionEntityScript(
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
            void Cross()
            {
                // the generic departure may already have sent the player
                if (activator.Map?.Entry.Id != EverstarGroveWorldId || !activator.CanTeleport())
                    return;

                Quest2Entry quest = GameTableManager.Instance.Quest2.GetEntry(ThePathToCelestion);
                WorldLocation2Entry destination = quest != null ? GameTableManager.Instance.WorldLocation2.GetEntry(quest.WorldLocation2IdReceiver) : null;
                if (destination != null)
                    activator.TeleportTo((ushort)destination.WorldId, destination.Position0, destination.Position1, destination.Position2);
            }

            ICinematicBase cinematic = cinematicFactory.CreateCinematic(CinematicSceneId.EverstarGroveThroughThePortal);
            if (cinematic != null)
                activator.CinematicManager.QueueCinematic(cinematic, Cross);
            else
                Cross();
        }
    }
}
