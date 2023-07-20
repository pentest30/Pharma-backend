using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace GHPCommerce.Migrator.Migrations.SalesDb
{
    public partial class addcreditnotestatus : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "TotalPackageThermolabile",
                schema: "sales",
                table: "CreditNotes",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "TotalPackage",
                schema: "sales",
                table: "CreditNotes",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<long>(
                name: "State",
                schema: "sales",
                table: "CreditNotes",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<Guid>(
                name: "ValidatedByUserId",
                schema: "sales",
                table: "CreditNotes",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ValidatedOn",
                schema: "sales",
                table: "CreditNotes",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "State",
                schema: "sales",
                table: "CreditNotes");

            migrationBuilder.DropColumn(
                name: "ValidatedByUserId",
                schema: "sales",
                table: "CreditNotes");

            migrationBuilder.DropColumn(
                name: "ValidatedOn",
                schema: "sales",
                table: "CreditNotes");

            migrationBuilder.AlterColumn<int>(
                name: "TotalPackageThermolabile",
                schema: "sales",
                table: "CreditNotes",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "TotalPackage",
                schema: "sales",
                table: "CreditNotes",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldNullable: true);
        }
    }
}
