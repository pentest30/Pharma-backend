using Microsoft.EntityFrameworkCore.Migrations;

namespace GHPCommerce.Migrator.Migrations.ApplicationDb
{
    public partial class addDebt : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ConventionType",
                schema: "Tiers",
                table: "SupplierCustomers",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "Dept",
                schema: "Tiers",
                table: "SupplierCustomers",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "PaymentDeadline",
                schema: "Tiers",
                table: "SupplierCustomers",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ConventionType",
                schema: "Tiers",
                table: "SupplierCustomers");

            migrationBuilder.DropColumn(
                name: "Dept",
                schema: "Tiers",
                table: "SupplierCustomers");

            migrationBuilder.DropColumn(
                name: "PaymentDeadline",
                schema: "Tiers",
                table: "SupplierCustomers");
        }
    }
}
