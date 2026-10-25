using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NexusForever.Database.World.Migrations
{
    /// <inheritdoc />
    [DbContext(typeof(WorldContext))]
    [Migration("20261025110000_PathSoldierHoldoutSpawn")]
    public partial class PathSoldierHoldoutSpawn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "path_soldier_holdout_spawn",
                columns: table => new
                {
                    pathSoldierEventId = table.Column<ushort>(type: "smallint(5) unsigned", nullable: false, defaultValue: (ushort)0),
                    waveIndex = table.Column<byte>(type: "tinyint(3) unsigned", nullable: false, defaultValue: (byte)0),
                    creatureId = table.Column<uint>(type: "int(10) unsigned", nullable: false, defaultValue: 0u),
                    count = table.Column<byte>(type: "tinyint(3) unsigned", nullable: false, defaultValue: (byte)1),
                    flags = table.Column<byte>(type: "tinyint(3) unsigned", nullable: false, defaultValue: (byte)0),
                    distance = table.Column<float>(type: "float", nullable: false, defaultValue: 0f),
                    comment = table.Column<string>(type: "varchar(200)", nullable: false, defaultValue: "")
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => new { x.pathSoldierEventId, x.waveIndex, x.creatureId });
                })
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "path_soldier_holdout_spawn");
        }
    }
}
