using Microsoft.EntityFrameworkCore.Migrations;

namespace GHPCommerce.Migrator.Migrations.ApplicationDb
{
    public partial class AddZoneType : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "ZoneType",
                schema: "Catalog",
                table: "PickingZone",
                nullable: false,
                defaultValue: 0L);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ZoneType",
                schema: "Catalog",
                table: "PickingZone");
        }
    }
}
