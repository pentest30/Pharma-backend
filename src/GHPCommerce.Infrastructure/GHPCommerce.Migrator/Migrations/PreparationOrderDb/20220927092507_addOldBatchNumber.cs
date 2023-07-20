using Microsoft.EntityFrameworkCore.Migrations;

namespace GHPCommerce.Migrator.Migrations.PreparationOrderDb
{
    public partial class addOldBatchNumber : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PreviousInternalBatchNumber",
                schema: "logistics",
                table: "PreparationOrderItem",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PreviousInternalBatchNumber",
                schema: "logistics",
                table: "PreparationOrderItem");
        }
    }
}
