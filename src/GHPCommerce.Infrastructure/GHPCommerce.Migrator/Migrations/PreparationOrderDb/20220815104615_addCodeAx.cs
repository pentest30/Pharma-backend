using Microsoft.EntityFrameworkCore.Migrations;

namespace GHPCommerce.Migrator.Migrations.PreparationOrderDb
{
    public partial class addCodeAx : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CodeAx",
                schema: "logistics",
                table: "PreparationOrder",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CodeAx",
                schema: "logistics",
                table: "PreparationOrder");
        }
    }
}
