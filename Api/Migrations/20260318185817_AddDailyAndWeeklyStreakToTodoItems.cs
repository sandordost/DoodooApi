using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DoodooApi.Migrations
{
    /// <inheritdoc />
    public partial class AddDailyAndWeeklyStreakToTodoItems : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastSeen",
                table: "AspNetUsers");

            migrationBuilder.AddColumn<int>(
                name: "DailyStreak",
                table: "TodoItems",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastCompletedTimestamp",
                table: "TodoItems",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastWeeklyCheck",
                table: "TodoItems",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "PreviousCompletedTimestamp",
                table: "TodoItems",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "WeeklyStreak",
                table: "TodoItems",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastDailyReset",
                table: "AspNetUsers",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DailyStreak",
                table: "TodoItems");

            migrationBuilder.DropColumn(
                name: "LastCompletedTimestamp",
                table: "TodoItems");

            migrationBuilder.DropColumn(
                name: "LastWeeklyCheck",
                table: "TodoItems");

            migrationBuilder.DropColumn(
                name: "PreviousCompletedTimestamp",
                table: "TodoItems");

            migrationBuilder.DropColumn(
                name: "WeeklyStreak",
                table: "TodoItems");

            migrationBuilder.DropColumn(
                name: "LastDailyReset",
                table: "AspNetUsers");

            migrationBuilder.AddColumn<DateTime>(
                name: "LastSeen",
                table: "AspNetUsers",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }
    }
}
