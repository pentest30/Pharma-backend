using Microsoft.EntityFrameworkCore.Migrations;

namespace GHPCommerce.Migrator.Migrations.InventoryDb
{
    public partial class addQuotaAndManufacturerToInventSum : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Manufacturer",
                schema: "inventory",
                table: "InventSum",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "Quota",
                schema: "inventory",
                table: "InventSum",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Manufacturer",
                schema: "inventory",
                table: "InventSum");

            migrationBuilder.DropColumn(
                name: "Quota",
                schema: "inventory",
                table: "InventSum");
        }
    }
}
