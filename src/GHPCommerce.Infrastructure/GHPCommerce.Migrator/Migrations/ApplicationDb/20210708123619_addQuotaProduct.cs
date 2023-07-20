using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace GHPCommerce.Migrator.Migrations.ApplicationDb
{
    public partial class addQuotaProduct : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Quota",
                schema: "Catalog",
                table: "Products",
                nullable: false,
                defaultValue: false);

            migrationBuilder.InsertData(
                schema: "ids",
                table: "Roles",
                columns: new[] { "Id", "ConcurrencyStamp", "CreatedByUserId", "CreatedDateTime", "Name", "NormalizedName", "UpdatedByUserId", "UpdatedDateTime" },
                values: new object[] { new Guid("b512f030-544c-eb11-9ce0-a4c3f0d0217b"), null, new Guid("00000000-0000-0000-0000-000000000000"), new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Supervisor", "SUPERVISOR", null, null });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                schema: "ids",
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("b512f030-544c-eb11-9ce0-a4c3f0d0217b"));

            migrationBuilder.DropColumn(
                name: "Quota",
                schema: "Catalog",
                table: "Products");
        }
    }
}
