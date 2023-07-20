using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace GHPCommerce.Migrator.Migrations.QuotaDb
{
    public partial class customer : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CustomerCode",
                schema: "sales",
                table: "QuotaRequest",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CustomerId",
                schema: "sales",
                table: "QuotaRequest",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CustomerName",
                schema: "sales",
                table: "QuotaRequest",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CustomerCode",
                schema: "sales",
                table: "QuotaRequest");

            migrationBuilder.DropColumn(
                name: "CustomerId",
                schema: "sales",
                table: "QuotaRequest");

            migrationBuilder.DropColumn(
                name: "CustomerName",
                schema: "sales",
                table: "QuotaRequest");
        }
    }
}
