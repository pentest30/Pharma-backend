﻿using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace GHPCommerce.Migrator.Migrations.ApplicationDb
{
    public partial class addSectors : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Sectors",
                schema: "Tiers",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false, defaultValueSql: "newsequentialid()"),
                    RowVersion = table.Column<byte[]>(rowVersion: true, nullable: true),
                    CreatedDateTime = table.Column<DateTimeOffset>(nullable: false),
                    CreatedByUserId = table.Column<Guid>(nullable: false),
                    UpdatedByUserId = table.Column<Guid>(nullable: true),
                    UpdatedDateTime = table.Column<DateTimeOffset>(nullable: true),
                    ExternalId = table.Column<int>(nullable: false),
                    Name = table.Column<string>(nullable: true),
                    Code = table.Column<string>(nullable: true),
                    OrganizationId = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sectors", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Sectors_Organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalSchema: "Tiers",
                        principalTable: "Organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Sectors_OrganizationId",
                schema: "Tiers",
                table: "Sectors",
                column: "OrganizationId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Sectors",
                schema: "Tiers");
        }
    }
}
