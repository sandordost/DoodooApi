using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DoodooApi.Migrations
{
    /// <inheritdoc />
    public partial class WeeklyItemCategory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "LastWeeklyReset",
                table: "AspNetUsers",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastWeeklyReset",
                table: "AspNetUsers");
        }
    }
}
