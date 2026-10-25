using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NexusForever.Database.Auth.Migrations
{
    /// <inheritdoc />
    [DbContext(typeof(AuthContext))]
    [Migration("20261025210000_AccountDailyLogin")]
    public partial class AccountDailyLogin : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "account_daily_login",
                columns: table => new
                {
                    id = table.Column<uint>(type: "int(10) unsigned", nullable: false, defaultValue: 0u),
                    loginDays = table.Column<uint>(type: "int(10) unsigned", nullable: false, defaultValue: 0u),
                    rewardsClaimed = table.Column<uint>(type: "int(10) unsigned", nullable: false, defaultValue: 0u),
                    lastLoginDay = table.Column<DateTime>(type: "datetime", nullable: true),
                    lastLockboxKey = table.Column<DateTime>(type: "datetime", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.id);
                    table.ForeignKey(
                        name: "FK__account_daily_login_id__account_id",
                        column: x => x.id,
                        principalTable: "account",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "account_daily_login");
        }
    }
}
