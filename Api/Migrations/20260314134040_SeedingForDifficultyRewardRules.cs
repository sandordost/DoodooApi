using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace DoodooApi.Migrations
{
    /// <inheritdoc />
    public partial class SeedingForDifficultyRewardRules : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "RewardId",
                table: "RewardCosts",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddColumn<int>(
                name: "Difficulty",
                table: "DifficultyRewardRules",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.InsertData(
                table: "DifficultyRewardRules",
                columns: new[] { "Id", "Difficulty", "GoldAmount", "SapphireAmount", "SapphireChance" },
                values: new object[,]
                {
                    { 1, 0, 0.4m, 1, 0.01f },
                    { 2, 1, 0.7m, 1, 0.05f },
                    { 3, 2, 1m, 1, 0.1f },
                    { 4, 3, 1.3m, 1, 0.15f }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "DifficultyRewardRules",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "DifficultyRewardRules",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "DifficultyRewardRules",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "DifficultyRewardRules",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DropColumn(
                name: "Difficulty",
                table: "DifficultyRewardRules");

            migrationBuilder.AlterColumn<int>(
                name: "RewardId",
                table: "RewardCosts",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);
        }
    }
}
