using Microsoft.EntityFrameworkCore.Migrations;

namespace GHPCommerce.Migrator.Migrations.SalesDb
{
    public partial class addAxCode : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CodeAx",
                schema: "sales",
                table: "Orders",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "AcceptedOnAx",
                schema: "sales",
                table: "OrderItems",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CodeAx",
                schema: "sales",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "AcceptedOnAx",
                schema: "sales",
                table: "OrderItems");
        }
    }
}
