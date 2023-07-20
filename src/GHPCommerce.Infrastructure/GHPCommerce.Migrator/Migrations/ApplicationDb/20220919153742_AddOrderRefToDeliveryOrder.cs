using Microsoft.EntityFrameworkCore.Migrations;

namespace GHPCommerce.Migrator.Migrations.PreparationOrderDb
{
    public partial class AddOrderRefToDeliveryOrder : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CodeAx",
                schema: "logistics",
                table: "DeleiveryOrder",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OrderIdentifier",
                schema: "logistics",
                table: "DeleiveryOrder",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CodeAx",
                schema: "logistics",
                table: "DeleiveryOrder");

            migrationBuilder.DropColumn(
                name: "OrderIdentifier",
                schema: "logistics",
                table: "DeleiveryOrder");
        }
    }
}
