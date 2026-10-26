using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NexusForever.Database.World.Migrations
{
    /// <inheritdoc />
    [DbContext(typeof(WorldContext))]
    [Migration("20261025230000_PathExplorerSwitch")]
    public partial class PathExplorerSwitch : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "path_explorer_switch",
                columns: table => new
                {
                    creatureId = table.Column<uint>(type: "int(10) unsigned", nullable: false, defaultValue: 0u),
                    platformCreatureId = table.Column<uint>(type: "int(10) unsigned", nullable: false, defaultValue: 0u),
                    pathMissionId = table.Column<ushort>(type: "smallint(5) unsigned", nullable: false, defaultValue: (ushort)0),
                    comment = table.Column<string>(type: "varchar(200)", nullable: false, defaultValue: "")
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.creatureId);
                })
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "path_explorer_switch");
        }
    }
}
