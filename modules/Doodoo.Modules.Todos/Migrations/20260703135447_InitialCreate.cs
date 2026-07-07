using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Doodoo.Modules.Todos.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "todos");

            migrationBuilder.CreateTable(
                name: "TodoItems",
                schema: "todos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    ItemDifficulty = table.Column<int>(type: "integer", nullable: false),
                    OwnerId = table.Column<Guid>(type: "uuid", nullable: false),
                    CompletedTimestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastCompletedTimestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    PreviousCompletedTimestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedTimestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ItemCategory = table.Column<int>(type: "integer", nullable: false),
                    DailyStreak = table.Column<int>(type: "integer", nullable: true),
                    WeeklyStreak = table.Column<int>(type: "integer", nullable: true),
                    LastWeeklyCheck = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastResetDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ActiveDays = table.Column<int>(type: "integer", nullable: false),
                    Order = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TodoItems", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TodoItems_OwnerId",
                schema: "todos",
                table: "TodoItems",
                column: "OwnerId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TodoItems",
                schema: "todos");
        }
    }
}
