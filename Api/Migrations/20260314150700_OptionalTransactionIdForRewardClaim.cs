using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DoodooApi.Migrations
{
    /// <inheritdoc />
    public partial class OptionalTransactionIdForRewardClaim : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RewardClaims_Transactions_TransactionId",
                table: "RewardClaims");

            migrationBuilder.AlterColumn<Guid>(
                name: "TransactionId",
                table: "RewardClaims",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddForeignKey(
                name: "FK_RewardClaims_Transactions_TransactionId",
                table: "RewardClaims",
                column: "TransactionId",
                principalTable: "Transactions",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RewardClaims_Transactions_TransactionId",
                table: "RewardClaims");

            migrationBuilder.AlterColumn<Guid>(
                name: "TransactionId",
                table: "RewardClaims",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_RewardClaims_Transactions_TransactionId",
                table: "RewardClaims",
                column: "TransactionId",
                principalTable: "Transactions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
