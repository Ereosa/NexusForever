namespace NexusForever.Database.Character.Model
{
    public class CharacterPathMissionModel
    {
        public ulong Id { get; set; }
        public ushort MissionId { get; set; }
        public byte Completed { get; set; }
        public uint Progress { get; set; }
        public uint State { get; set; }

        public CharacterModel Character { get; set; }
    }
}
