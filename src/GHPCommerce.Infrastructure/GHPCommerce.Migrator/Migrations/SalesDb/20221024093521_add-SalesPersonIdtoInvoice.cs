using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace GHPCommerce.Migrator.Migrations.SalesDb
{
    public partial class addSalesPersonIdtoInvoice : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {


            migrationBuilder.AddColumn<Guid>(
                name: "SalesPersonId",
                schema: "sales",
                table: "Invoices",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {


            migrationBuilder.DropColumn(
                name: "SalesPersonId",
                schema: "sales",
                table: "Invoices");
        }
    }
}
