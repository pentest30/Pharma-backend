using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace GHPCommerce.Migrator.Migrations.QuotaDb
{
    public partial class quotaTransaction : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CustomerCode",
                schema: "sales",
                table: "Quota");

            migrationBuilder.DropColumn(
                name: "CustomerId",
                schema: "sales",
                table: "Quota");

            migrationBuilder.DropColumn(
                name: "CustomerName",
                schema: "sales",
                table: "Quota");

            migrationBuilder.CreateTable(
                name: "QuotaTransaction",
                schema: "sales",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false, defaultValueSql: "newsequentialid()"),
                    RowVersion = table.Column<byte[]>(rowVersion: true, nullable: true),
                    CreatedDateTime = table.Column<DateTimeOffset>(nullable: false),
                    CreatedByUserId = table.Column<Guid>(nullable: false),
                    UpdatedByUserId = table.Column<Guid>(nullable: true),
                    UpdatedDateTime = table.Column<DateTimeOffset>(nullable: true),
                    QuotaId = table.Column<Guid>(nullable: false),
                    CustomerId = table.Column<Guid>(nullable: false),
                    Quantity = table.Column<int>(nullable: false),
                    CustomerName = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuotaTransaction", x => x.Id);
                    table.ForeignKey(
                        name: "FK_QuotaTransaction_Quota_QuotaId",
                        column: x => x.QuotaId,
                        principalSchema: "sales",
                        principalTable: "Quota",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_QuotaTransaction_QuotaId",
                schema: "sales",
                table: "QuotaTransaction",
                column: "QuotaId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "QuotaTransaction",
                schema: "sales");

            migrationBuilder.AddColumn<string>(
                name: "CustomerCode",
                schema: "sales",
                table: "Quota",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CustomerId",
                schema: "sales",
                table: "Quota",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<string>(
                name: "CustomerName",
                schema: "sales",
                table: "Quota",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
