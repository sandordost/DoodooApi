using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Doodoo.Modules.Currency.Migrations
{
    /// <inheritdoc />
    public partial class AdjustTransactionSourceIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Transactions_SourceType_SourceIdGuid",
                schema: "currency",
                table: "Transactions");

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_SourceType_SourceIdGuid",
                schema: "currency",
                table: "Transactions",
                columns: new[] { "SourceType", "SourceIdGuid" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Transactions_SourceType_SourceIdGuid",
                schema: "currency",
                table: "Transactions");

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_SourceType_SourceIdGuid",
                schema: "currency",
                table: "Transactions",
                columns: new[] { "SourceType", "SourceIdGuid" },
                unique: true,
                filter: "\"SourceIdGuid\" IS NOT NULL");
        }
    }
}
