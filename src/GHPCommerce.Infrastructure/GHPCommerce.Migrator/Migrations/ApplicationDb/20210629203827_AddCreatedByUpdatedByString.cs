using Microsoft.EntityFrameworkCore.Migrations;

namespace GHPCommerce.Migrator.Migrations.ApplicationDb
{
    public partial class AddCreatedByUpdatedByString : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                schema: "Tiers",
                table: "Organizations",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                schema: "Tiers",
                table: "Organizations",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                schema: "Tiers",
                table: "Customers",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                schema: "Tiers",
                table: "Customers",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedBy",
                schema: "Tiers",
                table: "Organizations");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                schema: "Tiers",
                table: "Organizations");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                schema: "Tiers",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                schema: "Tiers",
                table: "Customers");
        }
    }
}
