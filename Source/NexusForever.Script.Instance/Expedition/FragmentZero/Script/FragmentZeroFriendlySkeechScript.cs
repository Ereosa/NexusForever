using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.Map.Instance;
using NexusForever.Script.Template;
using NexusForever.Script.Template.Filter;
using NexusForever.Shared;
using NexusForever.Shared.Game;

namespace NexusForever.Script.Instance.Expedition.FragmentZero.Script
{
    /// <summary>
    /// The friendly skeech at the crash site (E-754 "Escort"): reaching it starts the escort, checked on the skeech itself so it doesn't
    /// depend on the event's own tick.
    /// </summary>
    [ScriptFilterCreatureId(67991, 69984)]
    public class FragmentZeroFriendlySkeechScript : IUnitScript, IOwnedScript<IUnitEntity>
    {
        private IUnitEntity skeech;
        private bool found;
        private readonly UpdateTimer checkTimer = new(TimeSpan.FromSeconds(0.5d));

        public void OnLoad(IUnitEntity owner)
        {
            skeech = owner;
        }

        public void Update(double lastTick)
        {
            if (found || skeech?.Map is not IContentMapInstance map)
                return;

            checkTimer.Update(lastTick);
            if (!checkTimer.HasElapsed)
                return;
            checkTimer.Reset();

            if (!map.GetPlayers().Any(p => p.IsAlive && p.Position.GetDistance(skeech.Position) < 15f))
                return;

            found = true;
            skeech.Map.PublicEventManager.GetEvent(680)?.InvokeScriptCollection<FragmentZeroEventScript>(s => s.OnFriendlySkeechFound());
        }
    }
}
