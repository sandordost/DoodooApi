using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DoodooApi.Migrations
{
    /// <inheritdoc />
    public partial class OptionalTransactionIdForRewardClaimv2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RewardClaims_Transactions_TransactionId",
                table: "RewardClaims");

            migrationBuilder.AddForeignKey(
                name: "FK_RewardClaims_Transactions_TransactionId",
                table: "RewardClaims",
                column: "TransactionId",
                principalTable: "Transactions",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RewardClaims_Transactions_TransactionId",
                table: "RewardClaims");

            migrationBuilder.AddForeignKey(
                name: "FK_RewardClaims_Transactions_TransactionId",
                table: "RewardClaims",
                column: "TransactionId",
                principalTable: "Transactions",
                principalColumn: "Id");
        }
    }
}
