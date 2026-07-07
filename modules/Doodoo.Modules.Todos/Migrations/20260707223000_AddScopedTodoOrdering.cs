using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Doodoo.Modules.Todos.Migrations
{
    public partial class AddScopedTodoOrdering : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                WITH ordered AS (
                    SELECT
                        "Id",
                        (ROW_NUMBER() OVER (
                            PARTITION BY
                                "OwnerId",
                                "ParentId",
                                CASE WHEN "ParentId" IS NULL THEN "ItemCategory" ELSE 0 END
                            ORDER BY "Order", "Id"
                        ) - 1)::integer AS "NewOrder"
                    FROM "todos"."TodoItems"
                    WHERE "DeletedTimestamp" IS NULL
                )
                UPDATE "todos"."TodoItems" AS todo
                SET "Order" = ordered."NewOrder"
                FROM ordered
                WHERE todo."Id" = ordered."Id";
                """);

            migrationBuilder.CreateIndex(
                name: "IX_TodoItems_OwnerId_ItemCategory_ParentId_Order",
                schema: "todos",
                table: "TodoItems",
                columns: new[] { "OwnerId", "ItemCategory", "ParentId", "Order" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TodoItems_OwnerId_ItemCategory_ParentId_Order",
                schema: "todos",
                table: "TodoItems");
        }
    }
}
