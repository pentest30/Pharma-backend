using Microsoft.EntityFrameworkCore.Migrations;

namespace GHPCommerce.Migrator.Migrations.SalesDb
{
    public partial class cascadebehaviourdeletecreditnote : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CreditNoteItems_CreditNotes_CreditNoteId",
                schema: "sales",
                table: "CreditNoteItems");

            migrationBuilder.AddForeignKey(
                name: "FK_CreditNoteItems_CreditNotes_CreditNoteId",
                schema: "sales",
                table: "CreditNoteItems",
                column: "CreditNoteId",
                principalSchema: "sales",
                principalTable: "CreditNotes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CreditNoteItems_CreditNotes_CreditNoteId",
                schema: "sales",
                table: "CreditNoteItems");

            migrationBuilder.AddForeignKey(
                name: "FK_CreditNoteItems_CreditNotes_CreditNoteId",
                schema: "sales",
                table: "CreditNoteItems",
                column: "CreditNoteId",
                principalSchema: "sales",
                principalTable: "CreditNotes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
