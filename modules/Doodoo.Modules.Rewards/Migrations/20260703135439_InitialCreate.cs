using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Doodoo.Modules.Rewards.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "rewards");

            migrationBuilder.CreateTable(
                name: "DifficultyRewardRules",
                schema: "rewards",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Difficulty = table.Column<int>(type: "integer", nullable: false),
                    GoldAmount = table.Column<decimal>(type: "numeric", nullable: false),
                    SapphireAmount = table.Column<int>(type: "integer", nullable: false),
                    SapphireChance = table.Column<float>(type: "real", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DifficultyRewardRules", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Rewards",
                schema: "rewards",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false),
                    OwnerId = table.Column<Guid>(type: "uuid", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    Icon = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Rewards", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RewardClaims",
                schema: "rewards",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RewardId = table.Column<int>(type: "integer", nullable: false),
                    TransactionId = table.Column<Guid>(type: "uuid", nullable: true),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ClaimedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RewardClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RewardClaims_Rewards_RewardId",
                        column: x => x.RewardId,
                        principalSchema: "rewards",
                        principalTable: "Rewards",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RewardCosts",
                schema: "rewards",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RewardId = table.Column<int>(type: "integer", nullable: true),
                    CurrencyType = table.Column<int>(type: "integer", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RewardCosts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RewardCosts_Rewards_RewardId",
                        column: x => x.RewardId,
                        principalSchema: "rewards",
                        principalTable: "Rewards",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                schema: "rewards",
                table: "DifficultyRewardRules",
                columns: new[] { "Id", "Difficulty", "GoldAmount", "SapphireAmount", "SapphireChance" },
                values: new object[,]
                {
                    { 1, 0, 0.4m, 1, 0.01f },
                    { 2, 1, 0.7m, 1, 0.05f },
                    { 3, 2, 1m, 1, 0.1f },
                    { 4, 3, 1.3m, 1, 0.15f }
                });

            migrationBuilder.CreateIndex(
                name: "IX_RewardClaims_RewardId",
                schema: "rewards",
                table: "RewardClaims",
                column: "RewardId");

            migrationBuilder.CreateIndex(
                name: "IX_RewardClaims_UserId",
                schema: "rewards",
                table: "RewardClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_RewardCosts_RewardId",
                schema: "rewards",
                table: "RewardCosts",
                column: "RewardId");

            migrationBuilder.CreateIndex(
                name: "IX_Rewards_OwnerId",
                schema: "rewards",
                table: "Rewards",
                column: "OwnerId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DifficultyRewardRules",
                schema: "rewards");

            migrationBuilder.DropTable(
                name: "RewardClaims",
                schema: "rewards");

            migrationBuilder.DropTable(
                name: "RewardCosts",
                schema: "rewards");

            migrationBuilder.DropTable(
                name: "Rewards",
                schema: "rewards");
        }
    }
}
