using Microsoft.EntityFrameworkCore.Migrations;

namespace GHPCommerce.Migrator.Migrations.ApplicationDb
{
    public partial class addCodeToCUstomer : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Code",
                schema: "Tiers",
                table: "SupplierCustomers",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CustomerGroup",
                schema: "Tiers",
                table: "SupplierCustomers",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "MonthlyObjective",
                schema: "Tiers",
                table: "SupplierCustomers",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "PaymentMode",
                schema: "Tiers",
                table: "SupplierCustomers",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "OrganizationGroupCode",
                schema: "Tiers",
                table: "Organizations",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Code",
                schema: "Tiers",
                table: "SupplierCustomers");

            migrationBuilder.DropColumn(
                name: "CustomerGroup",
                schema: "Tiers",
                table: "SupplierCustomers");

            migrationBuilder.DropColumn(
                name: "MonthlyObjective",
                schema: "Tiers",
                table: "SupplierCustomers");

            migrationBuilder.DropColumn(
                name: "PaymentMode",
                schema: "Tiers",
                table: "SupplierCustomers");

            migrationBuilder.DropColumn(
                name: "OrganizationGroupCode",
                schema: "Tiers",
                table: "Organizations");
        }
    }
}
