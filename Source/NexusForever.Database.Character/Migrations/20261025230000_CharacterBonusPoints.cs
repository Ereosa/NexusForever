using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NexusForever.Database.Character.Migrations
{
    /// <inheritdoc />
    [DbContext(typeof(CharacterContext))]
    [Migration("20261025230000_CharacterBonusPoints")]
    public partial class CharacterBonusPoints : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<byte>(
                name: "bonusAbilityPoints",
                table: "character",
                type: "tinyint(3) unsigned",
                nullable: false,
                defaultValue: (byte)0);

            migrationBuilder.AddColumn<byte>(
                name: "bonusAmpPower",
                table: "character",
                type: "tinyint(3) unsigned",
                nullable: false,
                defaultValue: (byte)0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "bonusAbilityPoints",
                table: "character");

            migrationBuilder.DropColumn(
                name: "bonusAmpPower",
                table: "character");
        }
    }
}
