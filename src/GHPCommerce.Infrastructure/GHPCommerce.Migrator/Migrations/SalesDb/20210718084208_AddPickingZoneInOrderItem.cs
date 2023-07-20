using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace GHPCommerce.Migrator.Migrations.SalesDb
{
    public partial class AddPickingZoneInOrderItem : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "PickingZoneId",
                schema: "sales",
                table: "OrderItems",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PickingZoneName",
                schema: "sales",
                table: "OrderItems",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ZoneGroupId",
                schema: "sales",
                table: "OrderItems",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ZoneGroupName",
                schema: "sales",
                table: "OrderItems",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PickingZoneId",
                schema: "sales",
                table: "OrderItems");

            migrationBuilder.DropColumn(
                name: "PickingZoneName",
                schema: "sales",
                table: "OrderItems");

            migrationBuilder.DropColumn(
                name: "ZoneGroupId",
                schema: "sales",
                table: "OrderItems");

            migrationBuilder.DropColumn(
                name: "ZoneGroupName",
                schema: "sales",
                table: "OrderItems");
        }
    }
}
