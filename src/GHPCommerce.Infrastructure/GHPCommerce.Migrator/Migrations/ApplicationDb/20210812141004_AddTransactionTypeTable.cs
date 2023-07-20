using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace GHPCommerce.Migrator.Migrations.ApplicationDb
{
    public partial class AddTransactionTypeTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                schema: "ids",
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("b512f030-544c-eb11-9ce0-a4c3f0d0217b"));

            migrationBuilder.CreateTable(
                name: "TransactionType",
                schema: "Catalog",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false, defaultValueSql: "newsequentialid()"),
                    RowVersion = table.Column<byte[]>(rowVersion: true, nullable: true),
                    CreatedDateTime = table.Column<DateTimeOffset>(nullable: false),
                    CreatedByUserId = table.Column<Guid>(nullable: false),
                    UpdatedByUserId = table.Column<Guid>(nullable: true),
                    UpdatedDateTime = table.Column<DateTimeOffset>(nullable: true),
                    TransactionTypeName = table.Column<string>(nullable: true),
                    Blocked = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TransactionType", x => x.Id);
                });

            migrationBuilder.InsertData(
                schema: "ids",
                table: "Roles",
                columns: new[] { "Id", "ConcurrencyStamp", "CreatedByUserId", "CreatedDateTime", "Name", "NormalizedName", "UpdatedByUserId", "UpdatedDateTime" },
                values: new object[] { new Guid("d63c47de-852f-4ce7-93e1-c3db21a06d48"), null, new Guid("00000000-0000-0000-0000-000000000000"), new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Executer", "EXECUTER", null, null });

            migrationBuilder.InsertData(
                schema: "ids",
                table: "Roles",
                columns: new[] { "Id", "ConcurrencyStamp", "CreatedByUserId", "CreatedDateTime", "Name", "NormalizedName", "UpdatedByUserId", "UpdatedDateTime" },
                values: new object[] { new Guid("ec6e3c62-a47d-4e35-bc41-cda44e16afc2"), null, new Guid("00000000-0000-0000-0000-000000000000"), new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Controller", "CONTROLLER", null, null });

            migrationBuilder.InsertData(
                schema: "ids",
                table: "Roles",
                columns: new[] { "Id", "ConcurrencyStamp", "CreatedByUserId", "CreatedDateTime", "Name", "NormalizedName", "UpdatedByUserId", "UpdatedDateTime" },
                values: new object[] { new Guid("8a8479d1-b0c7-43d6-8896-175cbcd27af8"), null, new Guid("00000000-0000-0000-0000-000000000000"), new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "PrintingAgent", "PRINTINGAGENT", null, null });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TransactionType",
                schema: "Catalog");

            migrationBuilder.DeleteData(
                schema: "ids",
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("8a8479d1-b0c7-43d6-8896-175cbcd27af8"));

            migrationBuilder.DeleteData(
                schema: "ids",
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("d63c47de-852f-4ce7-93e1-c3db21a06d48"));

            migrationBuilder.DeleteData(
                schema: "ids",
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("ec6e3c62-a47d-4e35-bc41-cda44e16afc2"));

            migrationBuilder.InsertData(
                schema: "ids",
                table: "Roles",
                columns: new[] { "Id", "ConcurrencyStamp", "CreatedByUserId", "CreatedDateTime", "Name", "NormalizedName", "UpdatedByUserId", "UpdatedDateTime" },
                values: new object[] { new Guid("b512f030-544c-eb11-9ce0-a4c3f0d0217b"), null, new Guid("00000000-0000-0000-0000-000000000000"), new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Supervisor", "SUPERVISOR", null, null });
        }
    }
}
