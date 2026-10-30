using NexusForever.Network.Message;

namespace NexusForever.Network.World.Message.Model.Support
{
    // From the Report Bug window (GameLib.ReportBug)
    [Message(GameMessageOpcode.ClientReportBug)]
    public class ClientReportBug : IReadable
    {
        public ushort BugSubcategoryId { get; private set; }
        public uint SelectedUnitId { get; private set; }
        public uint Quest2Id { get; private set; }
        public string Description { get; private set; }

        public void Read(GamePacketReader reader)
        {
            BugSubcategoryId = reader.ReadUShort();
            SelectedUnitId   = reader.ReadUInt();
            Quest2Id         = reader.ReadUInt();
            Description      = reader.ReadWideString();
        }
    }
}
