using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace GHPCommerce.Migrator.Migrations.ApplicationDb
{
    public partial class addConsolidatorRole : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                schema: "ids",
                table: "Roles",
                columns: new[] { "Id", "ConcurrencyStamp", "CreatedByUserId", "CreatedDateTime", "Name", "NormalizedName", "UpdatedByUserId", "UpdatedDateTime" },
                values: new object[] { new Guid("8c2f6a37-f308-438a-b39b-1400675ac734"), null, new Guid("00000000-0000-0000-0000-000000000000"), new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Consolidator", "CONSOLIDATOR", null, null });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                schema: "ids",
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("8c2f6a37-f308-438a-b39b-1400675ac734"));
        }
    }
}
