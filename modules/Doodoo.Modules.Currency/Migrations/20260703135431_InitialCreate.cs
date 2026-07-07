using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Doodoo.Modules.Currency.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "currency");

            migrationBuilder.CreateTable(
                name: "CurrencyAccounts",
                schema: "currency",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OwnerId = table.Column<Guid>(type: "uuid", nullable: false),
                    Gold = table.Column<decimal>(type: "numeric", nullable: false),
                    Sapphires = table.Column<int>(type: "integer", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CurrencyAccounts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Transactions",
                schema: "currency",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SourceType = table.Column<int>(type: "integer", nullable: false),
                    SourceIdInt = table.Column<int>(type: "integer", nullable: true),
                    SourceIdGuid = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedTimestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CurrencyAccountId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Transactions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Transactions_CurrencyAccounts_CurrencyAccountId",
                        column: x => x.CurrencyAccountId,
                        principalSchema: "currency",
                        principalTable: "CurrencyAccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TransactionRecords",
                schema: "currency",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TransactionId = table.Column<Guid>(type: "uuid", nullable: false),
                    CurrencyType = table.Column<int>(type: "integer", nullable: false),
                    Value = table.Column<decimal>(type: "numeric", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TransactionRecords", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TransactionRecords_Transactions_TransactionId",
                        column: x => x.TransactionId,
                        principalSchema: "currency",
                        principalTable: "Transactions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CurrencyAccounts_OwnerId",
                schema: "currency",
                table: "CurrencyAccounts",
                column: "OwnerId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TransactionRecords_TransactionId",
                schema: "currency",
                table: "TransactionRecords",
                column: "TransactionId");

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_CurrencyAccountId",
                schema: "currency",
                table: "Transactions",
                column: "CurrencyAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_SourceType_SourceIdGuid",
                schema: "currency",
                table: "Transactions",
                columns: new[] { "SourceType", "SourceIdGuid" },
                unique: true,
                filter: "\"SourceIdGuid\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_SourceType_SourceIdInt",
                schema: "currency",
                table: "Transactions",
                columns: new[] { "SourceType", "SourceIdInt" },
                unique: true,
                filter: "\"SourceIdInt\" IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TransactionRecords",
                schema: "currency");

            migrationBuilder.DropTable(
                name: "Transactions",
                schema: "currency");

            migrationBuilder.DropTable(
                name: "CurrencyAccounts",
                schema: "currency");
        }
    }
}
