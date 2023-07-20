using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace GHPCommerce.Migrator.Migrations.InventoryDb
{
    public partial class addStockStateSrc : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "StockStateSourceId",
                schema: "inventory",
                table: "TransferLog",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<string>(
                name: "StockStateSourceName",
                schema: "inventory",
                table: "TransferLog",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "StockStateSourceId",
                schema: "inventory",
                table: "TransferLog");

            migrationBuilder.DropColumn(
                name: "StockStateSourceName",
                schema: "inventory",
                table: "TransferLog");
        }
    }
}
