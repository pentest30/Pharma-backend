using Microsoft.EntityFrameworkCore.Migrations;

namespace GHPCommerce.Migrator.Migrations.PreparationOrderDb
{
    public partial class addVendorBatchNumber : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "consolidated",
                schema: "logistics",
                table: "ConsolidationOrder",
                newName: "Consolidated");

            migrationBuilder.AddColumn<string>(
                name: "VendorBatchNumber",
                schema: "logistics",
                table: "PreparationOrderItem",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "VendorBatchNumber",
                schema: "logistics",
                table: "PreparationOrderItem");

            migrationBuilder.RenameColumn(
                name: "Consolidated",
                schema: "logistics",
                table: "ConsolidationOrder",
                newName: "consolidated");
        }
    }
}
