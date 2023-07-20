using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace GHPCommerce.Migrator.Migrations.SalesDb
{
    public partial class addInvoice : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FinancialTransactions",
                schema: "sales",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false, defaultValueSql: "newsequentialid()"),
                    RowVersion = table.Column<byte[]>(rowVersion: true, nullable: true),
                    CreatedDateTime = table.Column<DateTimeOffset>(nullable: false),
                    CreatedByUserId = table.Column<Guid>(nullable: false),
                    UpdatedByUserId = table.Column<Guid>(nullable: true),
                    UpdatedDateTime = table.Column<DateTimeOffset>(nullable: true),
                    OrganizationId = table.Column<Guid>(nullable: false),
                    RefDocument = table.Column<string>(nullable: true),
                    DocumentDate = table.Column<DateTime>(nullable: false),
                    TransactionAmount = table.Column<decimal>(nullable: false),
                    FinancialTransactionType = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FinancialTransactions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Invoices",
                schema: "sales",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false, defaultValueSql: "newsequentialid()"),
                    RowVersion = table.Column<byte[]>(rowVersion: true, nullable: true),
                    CreatedDateTime = table.Column<DateTimeOffset>(nullable: false),
                    CreatedByUserId = table.Column<Guid>(nullable: false),
                    UpdatedByUserId = table.Column<Guid>(nullable: true),
                    UpdatedDateTime = table.Column<DateTimeOffset>(nullable: true),
                    OrganizationId = table.Column<Guid>(nullable: false),
                    OrderId = table.Column<Guid>(nullable: false),
                    DeliveryOrderId = table.Column<Guid>(nullable: false),
                    InvoiceDate = table.Column<DateTime>(nullable: false),
                    OrderDate = table.Column<DateTime>(nullable: false),
                    OrderNumber = table.Column<int>(nullable: false),
                    CustomerId = table.Column<Guid>(nullable: false),
                    CustomerName = table.Column<string>(nullable: true),
                    CustomerAddress = table.Column<string>(nullable: true),
                    CustomerCode = table.Column<string>(nullable: true),
                    SupplierId = table.Column<Guid>(nullable: false),
                    SequenceNumber = table.Column<int>(nullable: false),
                    TotalPackage = table.Column<int>(nullable: false),
                    TotalPackageThermolabile = table.Column<int>(nullable: false),
                    InvoiceType = table.Column<long>(nullable: false),
                    TotalTTC = table.Column<decimal>(nullable: false),
                    TotalHT = table.Column<decimal>(nullable: false),
                    TotalDiscount = table.Column<decimal>(nullable: false),
                    TotalDisplayedDiscount = table.Column<decimal>(nullable: false),
                    TotalTax = table.Column<decimal>(nullable: false),
                    CreatedBy = table.Column<string>(nullable: true),
                    UpdatedBy = table.Column<string>(nullable: true),
                    SectorCode = table.Column<string>(nullable: true),
                    Sector = table.Column<string>(nullable: true),
                    CodeRegion = table.Column<string>(nullable: true),
                    Region = table.Column<string>(nullable: true),
                    DueDate = table.Column<DateTime>(nullable: false),
                    NumberDueDays = table.Column<int>(nullable: false),
                    NumberOfPrints = table.Column<int>(nullable: false),
                    PrintedBy = table.Column<Guid>(nullable: false),
                    PrintedByName = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Invoices", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "InvoiceItems",
                schema: "sales",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false, defaultValueSql: "newsequentialid()"),
                    RowVersion = table.Column<byte[]>(rowVersion: true, nullable: true),
                    CreatedDateTime = table.Column<DateTimeOffset>(nullable: false),
                    CreatedByUserId = table.Column<Guid>(nullable: false),
                    UpdatedByUserId = table.Column<Guid>(nullable: true),
                    UpdatedDateTime = table.Column<DateTimeOffset>(nullable: true),
                    InvoiceId = table.Column<Guid>(nullable: true),
                    LineNum = table.Column<int>(nullable: false),
                    ProductId = table.Column<Guid>(nullable: false),
                    ProductCode = table.Column<string>(nullable: true),
                    ProductName = table.Column<string>(nullable: true),
                    VendorBatchNumber = table.Column<string>(nullable: true),
                    InternalBatchNumber = table.Column<string>(nullable: true),
                    ExpiryDate = table.Column<DateTime>(nullable: false),
                    Quantity = table.Column<int>(nullable: false),
                    PFS = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PpaHT = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PpaTTC = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PurchaseDiscountUnitPrice = table.Column<decimal>(nullable: false),
                    UnitPriceInclTax = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DiscountRate = table.Column<double>(nullable: false),
                    DisplayedDiscountRate = table.Column<double>(nullable: false),
                    Tax = table.Column<double>(nullable: false),
                    TotalTax = table.Column<decimal>(nullable: false),
                    TotalDiscount = table.Column<decimal>(nullable: false),
                    DisplayedTotalDiscount = table.Column<decimal>(nullable: false),
                    TotalExlTax = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalInlTax = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InvoiceItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InvoiceItems_Invoices_InvoiceId",
                        column: x => x.InvoiceId,
                        principalSchema: "sales",
                        principalTable: "Invoices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_InvoiceItems_InvoiceId",
                schema: "sales",
                table: "InvoiceItems",
                column: "InvoiceId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FinancialTransactions",
                schema: "sales");

            migrationBuilder.DropTable(
                name: "InvoiceItems",
                schema: "sales");

            migrationBuilder.DropTable(
                name: "Invoices",
                schema: "sales");
        }
    }
}
