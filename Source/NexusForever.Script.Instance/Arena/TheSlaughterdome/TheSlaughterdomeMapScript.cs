using NexusForever.Script.Template.Filter;

namespace NexusForever.Script.Instance.Arena.TheSlaughterdome
{
    [ScriptFilterOwnerId(1535)]
    public class TheSlaughterdomeMapScript : EventBasePvpContentMapScript
    {
        public override uint PublicEventId => 205u;
        public override uint PublicSubEventId => 209u;
    }
}
