using NexusForever.Game.Abstract.PublicEvent;
using NexusForever.Game.Static.PublicEvent;
using NexusForever.Script.Template;
using NexusForever.Script.Template.Filter;

namespace NexusForever.Script.Olyssia.Deradune.Event
{
    /// <summary>
    /// Metal Maw world boss in the Bloodfire arena (public event 150): the boss waits in his cage, breaks out after a countdown and has to be defeated.
    /// </summary>
    /// <remarks>
    /// The objectives are from the client: 4993 (fill the Progenitor Data Cache or wait for the escape), 326 (two minute break out countdown),
    /// 327 (defeat Metal Maw) and 4992 (one hour reset of the cache after the kill). How long the boss stays caged isn't known,
    /// <see cref="CagedTime"/> is guessed, and nothing hands out the Progenitor Access Particles the cache takes, so the cage
    /// always opens on the timer. The cage, boss and players are handled by <see cref="Script.MetalMawCageEntityScript"/>.
    /// </remarks>
    [ScriptFilterOwnerId(EventId)]
    public class MetalMawEventScript : IPublicEventScript, IOwnedScript<IPublicEvent>
    {
        public const uint EventId = 150;

        public enum Phase : uint
        {
            Caged       = 1,
            BreakingOut = 2,
            Fight       = 3
        }

        public enum Objective : uint
        {
            BreakOut       = 326,
            DefeatMetalMaw = 327,
            FillDataCache  = 4993
        }

        /// <summary>
        /// Time Metal Maw spends in his cage before the break out countdown, guessed.
        /// </summary>
        private static readonly TimeSpan CagedTime = TimeSpan.FromMinutes(8);

        private IPublicEvent publicEvent;
        private double cagedTimer;

        public void OnLoad(IPublicEvent owner)
        {
            publicEvent = owner;
            publicEvent.SetPhase(Phase.Caged);
        }

        public void OnPublicEventPhase(uint phase)
        {
            switch ((Phase)phase)
            {
                case Phase.Caged:
                    publicEvent.ActivateObjective(Objective.FillDataCache);
                    cagedTimer = CagedTime.TotalSeconds;
                    break;
                case Phase.BreakingOut:
                    publicEvent.ActivateObjective(Objective.BreakOut);
                    break;
                case Phase.Fight:
                    publicEvent.ActivateObjective(Objective.DefeatMetalMaw);
                    break;
            }
        }

        public void Update(double lastTick)
        {
            if ((Phase)publicEvent.Phase != Phase.Caged || cagedTimer <= 0d)
                return;

            cagedTimer -= lastTick;
            if (cagedTimer <= 0d)
                publicEvent.SetPhase(Phase.BreakingOut);
        }

        public void OnPublicEventObjectiveStatus(IPublicEventObjective objective)
        {
            if (objective.Status is not (PublicEventStatus.Succeeded or PublicEventStatus.Failed))
                return;

            switch ((Objective)objective.Entry.Id)
            {
                // the countdown is a timed objective, it ends either way once the time is up
                case Objective.BreakOut:
                    if ((Phase)publicEvent.Phase == Phase.BreakingOut)
                        publicEvent.SetPhase(Phase.Fight);
                    break;
                case Objective.DefeatMetalMaw:
                    if (objective.Status == PublicEventStatus.Succeeded)
                        publicEvent.Finish(PublicEventTeam.PublicTeam);
                    break;
            }
        }
    }
}
