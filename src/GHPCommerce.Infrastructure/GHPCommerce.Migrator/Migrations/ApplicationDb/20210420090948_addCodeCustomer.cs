using Microsoft.EntityFrameworkCore.Migrations;

namespace GHPCommerce.Migrator.Migrations.ApplicationDb
{
    public partial class addCodeCustomer : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SalesPersonName",
                schema: "Tiers",
                table: "SupplierCustomers",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Code",
                schema: "Tiers",
                table: "Customers",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SalesPersonName",
                schema: "Tiers",
                table: "SupplierCustomers");

            migrationBuilder.DropColumn(
                name: "Code",
                schema: "Tiers",
                table: "Customers");
        }
    }
}
