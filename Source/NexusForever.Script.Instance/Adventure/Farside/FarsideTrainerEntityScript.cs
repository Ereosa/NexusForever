using NexusForever.Game.Abstract.Entity;
using NexusForever.Game.Abstract.PublicEvent;
using NexusForever.Script.Template;
using NexusForever.Script.Template.Filter;

namespace NexusForever.Script.Instance.Adventure.Farside
{
    /// <summary>
    /// The colony's trainers: talking to one trains a survivor.
    /// </summary>
    [ScriptFilterCreatureId(62550, 62551, 62552, 62553, 62554, 62555, 62556, 62557, 62622, 62634)]
    public class FarsideTrainerEntityScript : IWorldEntityScript, IOwnedScript<IWorldEntity>
    {
        private IWorldEntity trainer;

        public void OnLoad(IWorldEntity owner)
        {
            trainer = owner;
        }

        public bool OnTalkTo(IPlayer player)
        {
            IPublicEvent adventure = trainer.Map?.PublicEventManager.GetEvent(579u);
            if (adventure == null)
                return false;

            adventure.InvokeScriptCollection<FarsideEventScript>(s => s.Train(trainer, player));
            return true;
        }
    }
}
