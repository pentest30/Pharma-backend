using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace GHPCommerce.Migrator.Migrations.ApplicationDb
{
    public partial class superadmin : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                schema: "ids",
                table: "UserRoles",
                columns: new[] { "Id", "CreatedByUserId", "CreatedDateTime", "RoleId", "UpdatedByUserId", "UpdatedDateTime", "UserId" },
                values: new object[] { new Guid("12837d3d-793f-eb11-becc-5cea1d05f660"), new Guid("00000000-0000-0000-0000-000000000000"), new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new Guid("b512f030-544c-eb11-9ce0-a4c3f0d0209b"), null, null, new Guid("12837d3d-793f-ea11-becb-5cea1d05f660") });

            migrationBuilder.UpdateData(
                schema: "ids",
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("12837d3d-793f-ea11-becb-5cea1d05f660"),
                columns: new[] { "PasswordHash", "UserName" },
                values: new object[] { "AAJXI17tYiHj1+Yu+eoa4c9jq2FyRvD5WgPxem9c9TlhYDO8jdQjgOsOd2BicXSFxQA==", "superadmin" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                schema: "ids",
                table: "UserRoles",
                keyColumn: "Id",
                keyValue: new Guid("12837d3d-793f-eb11-becc-5cea1d05f660"));

            migrationBuilder.UpdateData(
                schema: "ids",
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("12837d3d-793f-ea11-becb-5cea1d05f660"),
                columns: new[] { "PasswordHash", "UserName" },
                values: new object[] { "AQAAAAEAACcQAAAAELBcKuXWkiRQEYAkD/qKs9neac5hxWs3bkegIHpGLtf+zFHuKnuI3lBqkWO9TMmFAQ==", "admin" });
        }
    }
}
