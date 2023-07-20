using Microsoft.EntityFrameworkCore.Migrations;

namespace GHPCommerce.Migrator.Migrations.ApplicationDb
{
    public partial class addAxProductAtts : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DciCode",
                schema: "Catalog",
                table: "Products",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProductGroup",
                schema: "Catalog",
                table: "Products",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Code",
                schema: "Catalog",
                table: "ProductClasses",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DciCode",
                schema: "Catalog",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "ProductGroup",
                schema: "Catalog",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "Code",
                schema: "Catalog",
                table: "ProductClasses");
        }
    }
}
