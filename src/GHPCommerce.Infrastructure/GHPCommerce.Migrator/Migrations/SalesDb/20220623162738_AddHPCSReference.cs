using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace GHPCommerce.Migrator.Migrations.SalesDb
{
    public partial class AddHPCSReference : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DateDocumentHpcs",
                schema: "sales",
                table: "Orders",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RefDocumentHpcs",
                schema: "sales",
                table: "Orders",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DateDocumentHpcs",
                schema: "sales",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "RefDocumentHpcs",
                schema: "sales",
                table: "Orders");
        }
    }
}
