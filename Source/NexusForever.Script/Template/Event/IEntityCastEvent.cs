using NexusForever.Game.Abstract.Entity;

namespace NexusForever.Script.Template.Event
{
    public interface IEntityCastEvent : IScriptEvent
    {
        public uint SpellId { get; }

        void Initialise<T>(IUnitEntity entity, T spellId, bool target) where T : Enum;
        void Initialise(IUnitEntity entity, uint spellId, bool target);
    }
}
