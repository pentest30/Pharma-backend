using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace GHPCommerce.Migrator.Migrations.QuotaDb
{
    public partial class addSupervisorPropToQuotaRequest : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "DestSalesPersonId",
                schema: "sales",
                table: "QuotaRequest",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "ForSuperVisor",
                schema: "sales",
                table: "QuotaRequest",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DestSalesPersonId",
                schema: "sales",
                table: "QuotaRequest");

            migrationBuilder.DropColumn(
                name: "ForSuperVisor",
                schema: "sales",
                table: "QuotaRequest");
        }
    }
}
