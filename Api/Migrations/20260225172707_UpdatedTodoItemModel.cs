using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DoodooApi.Migrations
{
    /// <inheritdoc />
    public partial class UpdatedTodoItemModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsCompleted",
                table: "TodoItems");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "TodoItems");

            migrationBuilder.RenameColumn(
                name: "TaskDifficulty",
                table: "TodoItems",
                newName: "ItemDifficulty");

            migrationBuilder.AddColumn<DateTime>(
                name: "CompletedTimestamp",
                table: "TodoItems",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedTimestamp",
                table: "TodoItems",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ItemCategory",
                table: "TodoItems",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CompletedTimestamp",
                table: "TodoItems");

            migrationBuilder.DropColumn(
                name: "DeletedTimestamp",
                table: "TodoItems");

            migrationBuilder.DropColumn(
                name: "ItemCategory",
                table: "TodoItems");

            migrationBuilder.RenameColumn(
                name: "ItemDifficulty",
                table: "TodoItems",
                newName: "TaskDifficulty");

            migrationBuilder.AddColumn<bool>(
                name: "IsCompleted",
                table: "TodoItems",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "TodoItems",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }
    }
}
