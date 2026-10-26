using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.PublicEvent;
using NexusForever.Script.Template;
using NexusForever.Script.Template.Filter;

namespace NexusForever.Script.Instance.Adventure.WarOfTheWilds
{
    /// <summary>
    /// The skeech base's warrior, hunter and shaman huts: using one trains its line of stronger soldiers.
    /// </summary>
    [ScriptFilterCreatureId(26704, 26705, 26706)]
    public class WarOfTheWildsHutEntityScript : IWorldEntityScript, IOwnedScript<IWorldEntity>
    {
        private IWorldEntity hut;

        public void OnLoad(IWorldEntity owner)
        {
            hut = owner;
        }

        public void OnActivateSuccess(IPlayer activator)
        {
            Use(activator);
        }

        public bool OnTalkTo(IPlayer player)
        {
            Use(player);
            return true;
        }

        private void Use(IPlayer player)
        {
            IPublicEvent battle = hut.Map?.PublicEventManager.GetEvent(158u);
            battle?.InvokeScriptCollection<WarOfTheWildsEventScript>(s => s.UseHut(hut, player));
        }
    }
}
