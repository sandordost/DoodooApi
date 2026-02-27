using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace DoodooApi.Migrations
{
    /// <inheritdoc />
    public partial class FinishingModelsV2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CurrencyType",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "Value",
                table: "Transactions");

            migrationBuilder.AddColumn<Guid>(
                name: "UserId",
                table: "RewardClaims",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateTable(
                name: "TransactionRecords",
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
                        principalTable: "Transactions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RewardClaims_UserId",
                table: "RewardClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_TransactionRecords_TransactionId",
                table: "TransactionRecords",
                column: "TransactionId");

            migrationBuilder.AddForeignKey(
                name: "FK_RewardClaims_AspNetUsers_UserId",
                table: "RewardClaims",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RewardClaims_AspNetUsers_UserId",
                table: "RewardClaims");

            migrationBuilder.DropTable(
                name: "TransactionRecords");

            migrationBuilder.DropIndex(
                name: "IX_RewardClaims_UserId",
                table: "RewardClaims");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "RewardClaims");

            migrationBuilder.AddColumn<int>(
                name: "CurrencyType",
                table: "Transactions",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "Value",
                table: "Transactions",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);
        }
    }
}
