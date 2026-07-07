using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Doodoo.Modules.Todos.Migrations
{
    /// <inheritdoc />
    public partial class AddSagaSupport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsSaga",
                schema: "todos",
                table: "TodoItems",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "ParentId",
                schema: "todos",
                table: "TodoItems",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_TodoItems_ParentId",
                schema: "todos",
                table: "TodoItems",
                column: "ParentId");

            migrationBuilder.AddForeignKey(
                name: "FK_TodoItems_TodoItems_ParentId",
                schema: "todos",
                table: "TodoItems",
                column: "ParentId",
                principalSchema: "todos",
                principalTable: "TodoItems",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TodoItems_TodoItems_ParentId",
                schema: "todos",
                table: "TodoItems");

            migrationBuilder.DropIndex(
                name: "IX_TodoItems_ParentId",
                schema: "todos",
                table: "TodoItems");

            migrationBuilder.DropColumn(
                name: "IsSaga",
                schema: "todos",
                table: "TodoItems");

            migrationBuilder.DropColumn(
                name: "ParentId",
                schema: "todos",
                table: "TodoItems");
        }
    }
}
