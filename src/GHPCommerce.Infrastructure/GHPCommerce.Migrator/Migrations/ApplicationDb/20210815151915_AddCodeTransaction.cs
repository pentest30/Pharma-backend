using Microsoft.EntityFrameworkCore.Migrations;

namespace GHPCommerce.Migrator.Migrations.ApplicationDb
{
    public partial class AddCodeTransaction : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "CodeTransaction",
                schema: "Catalog",
                table: "TransactionType",
                nullable: false,
                defaultValue: 0L);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CodeTransaction",
                schema: "Catalog",
                table: "TransactionType");
        }
    }
}
