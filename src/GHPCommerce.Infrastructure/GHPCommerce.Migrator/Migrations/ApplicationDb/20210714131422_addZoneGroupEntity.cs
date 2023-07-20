using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace GHPCommerce.Migrator.Migrations.ApplicationDb
{
    public partial class addZoneGroupEntity : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ZoneGroup",
                schema: "Catalog",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false, defaultValueSql: "newsequentialid()"),
                    RowVersion = table.Column<byte[]>(rowVersion: true, nullable: true),
                    CreatedDateTime = table.Column<DateTimeOffset>(nullable: false),
                    CreatedByUserId = table.Column<Guid>(nullable: false),
                    UpdatedByUserId = table.Column<Guid>(nullable: true),
                    UpdatedDateTime = table.Column<DateTimeOffset>(nullable: true),
                    Name = table.Column<string>(nullable: true),
                    Description = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ZoneGroup", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PickingZone_ZoneGroupId",
                schema: "Catalog",
                table: "PickingZone",
                column: "ZoneGroupId");

            migrationBuilder.AddForeignKey(
                name: "FK_PickingZone_ZoneGroup_ZoneGroupId",
                schema: "Catalog",
                table: "PickingZone",
                column: "ZoneGroupId",
                principalSchema: "Catalog",
                principalTable: "ZoneGroup",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PickingZone_ZoneGroup_ZoneGroupId",
                schema: "Catalog",
                table: "PickingZone");

            migrationBuilder.DropTable(
                name: "ZoneGroup",
                schema: "Catalog");

            migrationBuilder.DropIndex(
                name: "IX_PickingZone_ZoneGroupId",
                schema: "Catalog",
                table: "PickingZone");
        }
    }
}
