using Microsoft.EntityFrameworkCore.Migrations;

namespace GHPCommerce.Migrator.Migrations.ApplicationDb
{
    public partial class removeProductClass : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PaymentMethod",
                schema: "Tiers",
                table: "SupplierCustomers",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OrganizationActivity",
                schema: "Tiers",
                table: "Organizations",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PaymentMethod",
                schema: "Tiers",
                table: "SupplierCustomers");

            migrationBuilder.DropColumn(
                name: "OrganizationActivity",
                schema: "Tiers",
                table: "Organizations");
        }
    }
}
