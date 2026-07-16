using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Doodoo.Modules.Inventory.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "inventory");

            migrationBuilder.CreateTable(
                name: "ItemDefinitions",
                schema: "inventory",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Key = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    Kind = table.Column<int>(type: "integer", nullable: false),
                    Slot = table.Column<int>(type: "integer", nullable: false),
                    Category = table.Column<string>(type: "text", nullable: true),
                    ContentType = table.Column<int>(type: "integer", nullable: false),
                    Content = table.Column<string>(type: "text", nullable: true),
                    Effect = table.Column<int>(type: "integer", nullable: false),
                    EffectAmount = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    Stackable = table.Column<bool>(type: "boolean", nullable: false),
                    UnlockAtLevel = table.Column<int>(type: "integer", nullable: true),
                    IsDefault = table.Column<bool>(type: "boolean", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ItemDefinitions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "InventoryEntries",
                schema: "inventory",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    OwnerId = table.Column<Guid>(type: "uuid", nullable: false),
                    DefinitionId = table.Column<int>(type: "integer", nullable: false),
                    Quantity = table.Column<int>(type: "integer", nullable: false),
                    IsEquipped = table.Column<bool>(type: "boolean", nullable: false),
                    AcquiredAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastUsedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ExpiresAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InventoryEntries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InventoryEntries_ItemDefinitions_DefinitionId",
                        column: x => x.DefinitionId,
                        principalSchema: "inventory",
                        principalTable: "ItemDefinitions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                schema: "inventory",
                table: "ItemDefinitions",
                columns: new[] { "Id", "Category", "Content", "ContentType", "Description", "Effect", "IsActive", "IsDefault", "Key", "Kind", "Name", "Slot", "Stackable", "UnlockAtLevel" },
                values: new object[,]
                {
                    { 1, "Animaties", "confetti", 2, "De standaard voltooiings-animatie.", 0, true, true, "default-confetti", 0, "Confetti", 5, false, null },
                    { 2, "Achtergrond", "default", 2, null, 0, true, true, "default-app-background", 0, "Standaard achtergrond", 1, false, null },
                    { 3, "Achtergrond", "{\"version\":1,\"slot\":\"AppBackground\",\"layers\":[{\"type\":\"gradient\",\"gradient\":{\"from\":\"#0f172a\",\"to\":\"#1e1b4b\",\"angle\":135},\"position\":\"cover\",\"zIndex\":0},{\"type\":\"image\",\"url\":\"https://assets.doo-doo.nl/backgrounds/stars.png\",\"position\":\"cover\",\"opacity\":0.6,\"zIndex\":1,\"animation\":{\"preset\":\"float\",\"durationMs\":8000}}]}", 1, "Een geanimeerde sterrenhemel-achtergrond.", 0, true, false, "starry-night", 0, "Sterrennacht", 1, false, null },
                    { 4, "Todo's", "default", 2, null, 0, true, true, "default-todo-card", 0, "Standaard todo-kaart", 2, false, null },
                    { 5, "Buttons", "default", 2, null, 0, true, true, "default-nav-buttons", 0, "Standaard knoppen", 3, false, null },
                    { 6, "Buttons", "{\"version\":1,\"slot\":\"NavButtons\",\"layers\":[],\"tokens\":{\"--nav-btn-bg\":\"#1e1b4b\",\"--nav-btn-fg\":\"#e0e7ff\",\"--nav-btn-radius\":\"16px\"}}", 1, "Herskin de navigatieknoppen (paars/neon).", 0, true, false, "neon-nav-buttons", 0, "Neon knoppen", 3, false, null },
                    { 7, "Profiel", "default", 2, null, 0, true, true, "default-profile-button", 0, "Standaard profielknop", 4, false, null }
                });

            migrationBuilder.InsertData(
                schema: "inventory",
                table: "ItemDefinitions",
                columns: new[] { "Id", "Category", "Content", "ContentType", "Description", "Effect", "EffectAmount", "IsActive", "IsDefault", "Key", "Kind", "Name", "Slot", "Stackable", "UnlockAtLevel" },
                values: new object[] { 8, null, null, 0, "Levert 20 goud op bij gebruik.", 1, 20, true, false, "bag-of-coins", 1, "Zakje goud", 0, true, null });

            migrationBuilder.InsertData(
                schema: "inventory",
                table: "ItemDefinitions",
                columns: new[] { "Id", "Category", "Content", "ContentType", "Description", "Effect", "IsActive", "IsDefault", "Key", "Kind", "Name", "Slot", "Stackable", "UnlockAtLevel" },
                values: new object[] { 9, null, null, 0, "Ontgrendelt pro-functies.", 0, true, false, "pro-membership", 2, "Pro membership", 0, false, null });

            migrationBuilder.CreateIndex(
                name: "IX_InventoryEntries_DefinitionId",
                schema: "inventory",
                table: "InventoryEntries",
                column: "DefinitionId");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryEntries_OwnerId",
                schema: "inventory",
                table: "InventoryEntries",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryEntries_OwnerId_DefinitionId",
                schema: "inventory",
                table: "InventoryEntries",
                columns: new[] { "OwnerId", "DefinitionId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ItemDefinitions_Key",
                schema: "inventory",
                table: "ItemDefinitions",
                column: "Key",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "InventoryEntries",
                schema: "inventory");

            migrationBuilder.DropTable(
                name: "ItemDefinitions",
                schema: "inventory");
        }
    }
}
