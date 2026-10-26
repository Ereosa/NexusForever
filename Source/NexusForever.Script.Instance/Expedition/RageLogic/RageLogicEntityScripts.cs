using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.PublicEvent;
using NexusForever.Game.Static.Entity;
using NexusForever.Script.Template;
using NexusForever.Script.Template.Filter;

namespace NexusForever.Script.Instance.Expedition.RageLogic
{
    /// <summary>
    /// Rage Logic creatures whose deaths drive the expedition, normal and veteran versions.
    /// </summary>
    [ScriptFilterCreatureId(
        32302, 32303, 32322, 32324, 32325, 43273, 43276, 43284, 43827, 44232, 43879, // ragebots
        69095, 69096, 69097, 69098, 69100, 69103, 69104, 69105, 69106, 69107, 69179,
        32323, 32368, 32370, 69094, 69101, 69102,                                    // infected freebots and infector bots
        32326, 69099, 69807)]                                                        // Axiom, the broadcast transmitter
    public class RageLogicEntityScript : ExpeditionEntityScript
    {
    }

    /// <summary>
    /// The assault speeders: boarding one in the Positron's hangar chooses it for the assault, on the asteroid it is simply ridden.
    /// </summary>
    [ScriptFilterCreatureId(32249, 43809, 43810)]
    public class RageLogicSpeederEntityScript : IWorldEntityScript, IOwnedScript<IWorldEntity>
    {
        private IWorldEntity owner;

        public void OnLoad(IWorldEntity owner)
        {
            this.owner = owner;
        }

        public void OnBoard(IPlayer player)
        {
            if (owner is not IVehicleEntity speeder)
                return;

            IPublicEvent launch = owner.Map.PublicEventManager.GetEvent(213u);
            if (launch != null && !launch.HasFinished && owner.Position.Y > -100f)
            {
                launch.InvokeScriptCollection<RageLogicLaunchEventScript>(s => s.OnSpeederBoarded(speeder, player));
                return;
            }

            if (speeder.GetPassenger(VehicleSeatType.Pilot, 0) == null)
                speeder.EnqueuePassengerAdd(player, VehicleSeatType.Pilot, 0);
        }
    }

    /// <summary>
    /// The Positron's doors between the bridge and the hangar.
    /// </summary>
    [ScriptFilterCreatureId(32336)]
    public class RageLogicDoorEntityScript : IUnitScript, IOwnedScript<IUnitEntity>
    {
        private IUnitEntity door;

        public void OnLoad(IUnitEntity owner)
        {
            door = owner;
        }

        public void OnActivateSuccess(IPlayer activator)
        {
            // the door has its own open spell (34512), it steps out of the way once used
            if (door.InWorld)
                door.RemoveFromMap();
        }
    }
}
