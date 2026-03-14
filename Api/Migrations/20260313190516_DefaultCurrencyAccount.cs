using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DoodooApi.Migrations
{
    public partial class DefaultCurrencyAccount : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                INSERT INTO "CurrencyAccounts" ("Id", "OwnerId", "Gold", "Sapphires")
                SELECT gen_random_uuid(), u."Id", 0, 0
                FROM "AspNetUsers" u
                LEFT JOIN "CurrencyAccounts" ca ON ca."OwnerId" = u."Id"
                WHERE ca."Id" IS NULL;
            """);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DELETE FROM "CurrencyAccounts" ca
                WHERE ca."Gold" = 0
                  AND ca."Sapphires" = 0
                  AND EXISTS (
                      SELECT 1
                      FROM "AspNetUsers" u
                      WHERE u."Id" = ca."OwnerId"
                  );
            """);
        }
    }
}