using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NexusForever.Database.Character.Migrations
{
    /// <inheritdoc />
    [DbContext(typeof(CharacterContext))]
    [Migration("20261025120000_CharacterTradeskills")]
    public partial class CharacterTradeskills : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "tradeskillRelearnCooldown",
                table: "character",
                type: "datetime",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "character_tradeskill",
                columns: table => new
                {
                    id = table.Column<ulong>(type: "bigint(20) unsigned", nullable: false, defaultValue: 0ul),
                    tradeskillId = table.Column<uint>(type: "int(10) unsigned", nullable: false, defaultValue: 0u),
                    xp = table.Column<uint>(type: "int(10) unsigned", nullable: false, defaultValue: 0u),
                    active = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => new { x.id, x.tradeskillId });
                    table.ForeignKey(
                        name: "FK__character_tradeskill_id__character_id",
                        column: x => x.id,
                        principalTable: "character",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "character_tradeskill_schematic",
                columns: table => new
                {
                    id = table.Column<ulong>(type: "bigint(20) unsigned", nullable: false, defaultValue: 0ul),
                    schematicId = table.Column<uint>(type: "int(10) unsigned", nullable: false, defaultValue: 0u),
                    discovered = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false),
                    discoveryX = table.Column<float>(type: "float", nullable: false, defaultValue: 0f),
                    discoveryY = table.Column<float>(type: "float", nullable: false, defaultValue: 0f)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => new { x.id, x.schematicId });
                    table.ForeignKey(
                        name: "FK__character_tradeskill_schematic_id__character_id",
                        column: x => x.id,
                        principalTable: "character",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "character_tradeskill");

            migrationBuilder.DropTable(
                name: "character_tradeskill_schematic");

            migrationBuilder.DropColumn(
                name: "tradeskillRelearnCooldown",
                table: "character");
        }
    }
}
