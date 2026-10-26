using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NexusForever.Database.Character.Migrations
{
    /// <inheritdoc />
    [DbContext(typeof(CharacterContext))]
    [Migration("20261026010000_ItemCircuitData")]
    public partial class ItemCircuitData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<ulong>(
                name: "circuitData",
                table: "item",
                type: "bigint(20) unsigned",
                nullable: false,
                defaultValue: 0ul);

            migrationBuilder.AddColumn<byte[]>(
                name: "circuitMultipliers",
                table: "item",
                type: "varbinary(20)",
                nullable: true,
                defaultValue: null);

            migrationBuilder.AddColumn<uint>(
                name: "powerCoreItemId",
                table: "item",
                type: "int(10) unsigned",
                nullable: false,
                defaultValue: 0u);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "circuitData",
                table: "item");

            migrationBuilder.DropColumn(
                name: "circuitMultipliers",
                table: "item");

            migrationBuilder.DropColumn(
                name: "powerCoreItemId",
                table: "item");
        }
    }
}
