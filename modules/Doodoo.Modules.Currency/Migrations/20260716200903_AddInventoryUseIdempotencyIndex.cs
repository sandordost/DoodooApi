using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Doodoo.Modules.Currency.Migrations
{
    /// <inheritdoc />
    public partial class AddInventoryUseIdempotencyIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                CREATE UNIQUE INDEX IF NOT EXISTS "IX_Transactions_InventoryUse_SourceIdGuid"
                ON currency."Transactions" ("SourceType", "SourceIdGuid")
                WHERE "SourceIdGuid" IS NOT NULL AND "SourceType" = 2;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DROP INDEX IF EXISTS currency."IX_Transactions_InventoryUse_SourceIdGuid";
                """);
        }
    }
}
