using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace GHPCommerce.Migrator.Migrations.ApplicationDb
{
    public partial class addAddressesToUser : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "UserId",
                schema: "Shared",
                table: "Addresses",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Company",
                schema: "ids",
                table: "Users",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FirstName",
                schema: "ids",
                table: "Users",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LastName",
                schema: "ids",
                table: "Users",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Addresses_UserId",
                schema: "Shared",
                table: "Addresses",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Addresses_Users_UserId",
                schema: "Shared",
                table: "Addresses",
                column: "UserId",
                principalSchema: "ids",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Addresses_Users_UserId",
                schema: "Shared",
                table: "Addresses");

            migrationBuilder.DropIndex(
                name: "IX_Addresses_UserId",
                schema: "Shared",
                table: "Addresses");

            migrationBuilder.DropColumn(
                name: "UserId",
                schema: "Shared",
                table: "Addresses");

            migrationBuilder.DropColumn(
                name: "Company",
                schema: "ids",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "FirstName",
                schema: "ids",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "LastName",
                schema: "ids",
                table: "Users");
        }
    }
}
