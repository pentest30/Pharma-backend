using Microsoft.EntityFrameworkCore.Migrations;

namespace GHPCommerce.Migrator.Migrations.ApplicationDb
{
    public partial class addSectorToSupplierCustomer : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_SupplierCustomers_DefaultDeliverySector",
                schema: "Tiers",
                table: "SupplierCustomers",
                column: "DefaultDeliverySector");

            migrationBuilder.AddForeignKey(
                name: "FK_SupplierCustomers_Sectors_DefaultDeliverySector",
                schema: "Tiers",
                table: "SupplierCustomers",
                column: "DefaultDeliverySector",
                principalSchema: "Tiers",
                principalTable: "Sectors",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SupplierCustomers_Sectors_DefaultDeliverySector",
                schema: "Tiers",
                table: "SupplierCustomers");

            migrationBuilder.DropIndex(
                name: "IX_SupplierCustomers_DefaultDeliverySector",
                schema: "Tiers",
                table: "SupplierCustomers");
        }
    }
}
