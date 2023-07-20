using Microsoft.EntityFrameworkCore.Migrations;

namespace GHPCommerce.Migrator.Migrations.SalesDb
{
    public partial class AddLocationAttributeInOrderItem : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DefaultLocation",
                schema: "sales",
                table: "OrderItems",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PickingZoneOrder",
                schema: "sales",
                table: "OrderItems",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DefaultLocation",
                schema: "sales",
                table: "OrderItems");

            migrationBuilder.DropColumn(
                name: "PickingZoneOrder",
                schema: "sales",
                table: "OrderItems");
        }
    }
}
