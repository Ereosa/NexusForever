namespace NexusForever.Database.World.Model
{
    public class PathSoldierHoldoutSpawnModel
    {
        public ushort PathSoldierEventId { get; set; }
        public byte WaveIndex { get; set; }
        public uint CreatureId { get; set; }
        public byte Count { get; set; }
        public byte Flags { get; set; }
        public float Distance { get; set; }
        public string Comment { get; set; }
    }
}
