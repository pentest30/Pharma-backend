using Microsoft.EntityFrameworkCore.Migrations;

namespace GHPCommerce.Migrator.Migrations.ApplicationDb
{
    public partial class addCustomerState : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<short>(
                name: "CustomerState",
                schema: "Tiers",
                table: "SupplierCustomers",
                nullable: false,
                defaultValue: (short)0);

            migrationBuilder.AddColumn<decimal>(
                name: "LimitCredit",
                schema: "Tiers",
                table: "SupplierCustomers",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CustomerState",
                schema: "Tiers",
                table: "SupplierCustomers");

            migrationBuilder.DropColumn(
                name: "LimitCredit",
                schema: "Tiers",
                table: "SupplierCustomers");
        }
    }
}
