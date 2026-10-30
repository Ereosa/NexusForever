namespace NexusForever.Database.Auth.Model
{
    /// <summary>
    /// A row of the player_report table that the report website (web-signup\report_lib.php) owns.
    /// </summary>
    /// <remarks>
    /// Not part of <see cref="AuthContext"/>: the website creates and migrates the table, the world server only inserts with <see cref="AuthDatabase.AddPlayerReportAsync"/>.
    /// </remarks>
    public class PlayerReportModel
    {
        public string Category { get; set; }
        public string Summary { get; set; }
        public string Description { get; set; }
        public string Zone { get; set; }
        public uint AccountId { get; set; }
        public string Email { get; set; }
        public ulong CharacterId { get; set; }
        public string CharacterName { get; set; }
        public uint Level { get; set; }
        public byte Class { get; set; }
        public ushort WorldId { get; set; }
        public ushort WorldZoneId { get; set; }
        public float LocationX { get; set; }
        public float LocationY { get; set; }
        public float LocationZ { get; set; }
        public string Ip { get; set; }
    }
}
