using Microsoft.EntityFrameworkCore.Migrations;

namespace GHPCommerce.Migrator.Migrations.ApplicationDb
{
    public partial class AddPickingZoneOrder : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Order",
                schema: "Catalog",
                table: "PickingZone",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Order",
                schema: "Catalog",
                table: "PickingZone");
        }
    }
}
