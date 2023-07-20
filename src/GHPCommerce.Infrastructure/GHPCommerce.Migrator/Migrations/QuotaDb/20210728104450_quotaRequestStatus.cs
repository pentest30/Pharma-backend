using Microsoft.EntityFrameworkCore.Migrations;

namespace GHPCommerce.Migrator.Migrations.QuotaDb
{
    public partial class quotaRequestStatus : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Status",
                schema: "sales",
                table: "QuotaRequest",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Status",
                schema: "sales",
                table: "QuotaRequest");
        }
    }
}
