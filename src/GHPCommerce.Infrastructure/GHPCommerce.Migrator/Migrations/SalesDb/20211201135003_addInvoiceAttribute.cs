using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace GHPCommerce.Migrator.Migrations.SalesDb
{
    public partial class addInvoiceAttribute : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "CustomerId",
                schema: "sales",
                table: "FinancialTransactions",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<string>(
                name: "CustomerName",
                schema: "sales",
                table: "FinancialTransactions",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RefNumber",
                schema: "sales",
                table: "FinancialTransactions",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<Guid>(
                name: "SupplierId",
                schema: "sales",
                table: "FinancialTransactions",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<string>(
                name: "SupplierName",
                schema: "sales",
                table: "FinancialTransactions",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CustomerId",
                schema: "sales",
                table: "FinancialTransactions");

            migrationBuilder.DropColumn(
                name: "CustomerName",
                schema: "sales",
                table: "FinancialTransactions");

            migrationBuilder.DropColumn(
                name: "RefNumber",
                schema: "sales",
                table: "FinancialTransactions");

            migrationBuilder.DropColumn(
                name: "SupplierId",
                schema: "sales",
                table: "FinancialTransactions");

            migrationBuilder.DropColumn(
                name: "SupplierName",
                schema: "sales",
                table: "FinancialTransactions");
        }
    }
}
