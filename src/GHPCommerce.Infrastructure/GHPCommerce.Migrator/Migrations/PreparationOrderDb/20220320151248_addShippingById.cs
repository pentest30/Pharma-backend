using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace GHPCommerce.Migrator.Migrations.PreparationOrderDb
{
    public partial class addShippingById : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "ToBeRespected",
                schema: "logistics",
                table: "PreparationOrder",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "ReceivedInShippingId",
                schema: "logistics",
                table: "ConsolidationOrder",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ToBeRespected",
                schema: "logistics",
                table: "PreparationOrder");

            migrationBuilder.DropColumn(
                name: "ReceivedInShippingId",
                schema: "logistics",
                table: "ConsolidationOrder");
        }
    }
}
