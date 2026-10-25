using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NexusForever.Database.Character.Migrations
{
    /// <inheritdoc />
    [DbContext(typeof(CharacterContext))]
    [Migration("20261025200000_CharacterChallenges")]
    public partial class CharacterChallenges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "character_challenge",
                columns: table => new
                {
                    id = table.Column<ulong>(type: "bigint(20) unsigned", nullable: false, defaultValue: 0ul),
                    challengeId = table.Column<uint>(type: "int(10) unsigned", nullable: false, defaultValue: 0u),
                    completionCount = table.Column<uint>(type: "int(10) unsigned", nullable: false, defaultValue: 0u),
                    lastRewardTier = table.Column<uint>(type: "int(10) unsigned", nullable: false, defaultValue: 0u),
                    cooldownRemaining = table.Column<uint>(type: "int(10) unsigned", nullable: false, defaultValue: 0u)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => new { x.id, x.challengeId });
                    table.ForeignKey(
                        name: "FK__character_challenge_id__character_id",
                        column: x => x.id,
                        principalTable: "character",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "character_reward_track",
                columns: table => new
                {
                    id = table.Column<ulong>(type: "bigint(20) unsigned", nullable: false, defaultValue: 0ul),
                    rewardTrackId = table.Column<uint>(type: "int(10) unsigned", nullable: false, defaultValue: 0u),
                    points = table.Column<uint>(type: "int(10) unsigned", nullable: false, defaultValue: 0u),
                    claimedRewards = table.Column<uint>(type: "int(10) unsigned", nullable: false, defaultValue: 0u),
                    iteration = table.Column<uint>(type: "int(10) unsigned", nullable: false, defaultValue: 0u)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => new { x.id, x.rewardTrackId });
                    table.ForeignKey(
                        name: "FK__character_reward_track_id__character_id",
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
                name: "character_challenge");

            migrationBuilder.DropTable(
                name: "character_reward_track");
        }
    }
}
