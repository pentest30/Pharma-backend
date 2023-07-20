using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace GHPCommerce.Migrator.Migrations.ProcurementDb
{
    public partial class Init : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "procurement");

            migrationBuilder.CreateTable(
                name: "SupplierOrder",
                schema: "procurement",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false, defaultValueSql: "newsequentialid()"),
                    RowVersion = table.Column<byte[]>(rowVersion: true, nullable: true),
                    CreatedDateTime = table.Column<DateTimeOffset>(nullable: false),
                    CreatedByUserId = table.Column<Guid>(nullable: false),
                    UpdatedByUserId = table.Column<Guid>(nullable: true),
                    UpdatedDateTime = table.Column<DateTimeOffset>(nullable: true),
                    OrderDate = table.Column<DateTime>(nullable: false),
                    CustomerId = table.Column<Guid>(nullable: false),
                    CustomerName = table.Column<string>(nullable: false),
                    SupplierId = table.Column<Guid>(nullable: false),
                    SupplierName = table.Column<string>(nullable: false),
                    OrderStatus = table.Column<int>(nullable: false),
                    ExpectedDeliveryDate = table.Column<DateTime>(nullable: true),
                    RefDocument = table.Column<string>(nullable: false),
                    Psychotropic = table.Column<bool>(nullable: false),
                    SequenceNumber = table.Column<int>(nullable: false),
                    OrganizationId = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SupplierOrder", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SupplierInvoice",
                schema: "procurement",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false, defaultValueSql: "newsequentialid()"),
                    RowVersion = table.Column<byte[]>(rowVersion: true, nullable: true),
                    CreatedDateTime = table.Column<DateTimeOffset>(nullable: false),
                    CreatedByUserId = table.Column<Guid>(nullable: false),
                    UpdatedByUserId = table.Column<Guid>(nullable: true),
                    UpdatedDateTime = table.Column<DateTimeOffset>(nullable: true),
                    InvoiceDate = table.Column<DateTime>(nullable: false),
                    CustomerId = table.Column<Guid>(nullable: false),
                    CustomerName = table.Column<string>(nullable: false),
                    SupplierId = table.Column<Guid>(nullable: false),
                    SupplierName = table.Column<string>(nullable: false),
                    InvoiceStatus = table.Column<int>(nullable: false),
                    ExpectedDeliveryDate = table.Column<DateTime>(nullable: true),
                    RefDocument = table.Column<string>(nullable: false),
                    InvoiceNumber = table.Column<string>(nullable: false),
                    OrderId = table.Column<Guid>(nullable: false),
                    TotalAmount = table.Column<decimal>(nullable: false),
                    TotalAmountExlTax = table.Column<decimal>(nullable: false),
                    ReceiptsAmount = table.Column<decimal>(nullable: false),
                    SequenceNumber = table.Column<int>(nullable: false),
                    OrganizationId = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SupplierInvoice", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SupplierInvoice_SupplierOrder_OrderId",
                        column: x => x.OrderId,
                        principalSchema: "procurement",
                        principalTable: "SupplierOrder",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SupplierOrderItem",
                schema: "procurement",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false, defaultValueSql: "newsequentialid()"),
                    RowVersion = table.Column<byte[]>(rowVersion: true, nullable: true),
                    CreatedDateTime = table.Column<DateTimeOffset>(nullable: false),
                    CreatedByUserId = table.Column<Guid>(nullable: false),
                    UpdatedByUserId = table.Column<Guid>(nullable: true),
                    UpdatedDateTime = table.Column<DateTimeOffset>(nullable: true),
                    OrderId = table.Column<Guid>(nullable: true),
                    ProductId = table.Column<Guid>(nullable: false),
                    ProductName = table.Column<string>(nullable: false),
                    ProductCode = table.Column<string>(nullable: false),
                    Quantity = table.Column<int>(nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Discount = table.Column<double>(nullable: false),
                    ExpiryDate = table.Column<DateTime>(nullable: true),
                    MinExpiryDate = table.Column<DateTime>(nullable: true),
                    DaysInStock = table.Column<int>(nullable: false),
                    InvoicedQuantity = table.Column<int>(nullable: false),
                    ReceivedQuantity = table.Column<int>(nullable: false),
                    RemainingQuantity = table.Column<int>(nullable: false),
                    WaitForDelivery = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SupplierOrderItem", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SupplierOrderItem_SupplierOrder_OrderId",
                        column: x => x.OrderId,
                        principalSchema: "procurement",
                        principalTable: "SupplierOrder",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DeliveryReceipt",
                schema: "procurement",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false, defaultValueSql: "newsequentialid()"),
                    RowVersion = table.Column<byte[]>(rowVersion: true, nullable: true),
                    CreatedDateTime = table.Column<DateTimeOffset>(nullable: false),
                    CreatedByUserId = table.Column<Guid>(nullable: false),
                    UpdatedByUserId = table.Column<Guid>(nullable: true),
                    UpdatedDateTime = table.Column<DateTimeOffset>(nullable: true),
                    DocRef = table.Column<string>(nullable: false),
                    DeliveryReceiptNumber = table.Column<string>(nullable: false),
                    InvoiceNumber = table.Column<string>(nullable: false),
                    InvoiceId = table.Column<Guid>(nullable: false),
                    InvoiceDate = table.Column<DateTime>(nullable: false),
                    DeliveryReceiptDate = table.Column<DateTime>(nullable: false),
                    TotalAmount = table.Column<decimal>(nullable: false),
                    TaxTotalAmount = table.Column<decimal>(nullable: false),
                    ReceiptsAmountExcTax = table.Column<decimal>(nullable: false),
                    DiscountTotalAmount = table.Column<decimal>(nullable: false),
                    Status = table.Column<int>(nullable: false),
                    OrganizationId = table.Column<Guid>(nullable: false),
                    SequenceNumber = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeliveryReceipt", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DeliveryReceipt_SupplierInvoice_InvoiceId",
                        column: x => x.InvoiceId,
                        principalSchema: "procurement",
                        principalTable: "SupplierInvoice",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SupplierInvoiceItem",
                schema: "procurement",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false, defaultValueSql: "newsequentialid()"),
                    RowVersion = table.Column<byte[]>(rowVersion: true, nullable: true),
                    CreatedDateTime = table.Column<DateTimeOffset>(nullable: false),
                    CreatedByUserId = table.Column<Guid>(nullable: false),
                    UpdatedByUserId = table.Column<Guid>(nullable: true),
                    UpdatedDateTime = table.Column<DateTimeOffset>(nullable: true),
                    InvoiceId = table.Column<Guid>(nullable: false),
                    ProductId = table.Column<Guid>(nullable: false),
                    ProductName = table.Column<string>(nullable: false),
                    ProductCode = table.Column<string>(nullable: false),
                    Quantity = table.Column<int>(nullable: false),
                    ExpiryDate = table.Column<DateTime>(nullable: true),
                    VendorBatchNumber = table.Column<string>(nullable: false),
                    InternalBatchNumber = table.Column<string>(nullable: false),
                    Color = table.Column<string>(nullable: false),
                    Size = table.Column<string>(nullable: false),
                    TotalInlTax = table.Column<decimal>(nullable: false),
                    TotalExlTax = table.Column<decimal>(nullable: false),
                    PackagingCode = table.Column<string>(nullable: false),
                    PickingZoneId = table.Column<Guid>(nullable: true),
                    PickingZoneName = table.Column<string>(nullable: false),
                    Packing = table.Column<int>(nullable: false),
                    PFS = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PpaHT = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PpaTTC = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PpaPFS = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    SalePrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    WholesaleMargin = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PharmacistMargin = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    InvoicedQuantity = table.Column<int>(nullable: false),
                    ReceivedQuantity = table.Column<int>(nullable: false),
                    RemainingQuantity = table.Column<int>(nullable: false),
                    PurchaseUnitPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PurchasePriceIncDiscount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Discount = table.Column<double>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SupplierInvoiceItem", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SupplierInvoiceItem_SupplierInvoice_InvoiceId",
                        column: x => x.InvoiceId,
                        principalSchema: "procurement",
                        principalTable: "SupplierInvoice",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DeliveryReceiptItem",
                schema: "procurement",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false, defaultValueSql: "newsequentialid()"),
                    RowVersion = table.Column<byte[]>(rowVersion: true, nullable: true),
                    CreatedDateTime = table.Column<DateTimeOffset>(nullable: false),
                    CreatedByUserId = table.Column<Guid>(nullable: false),
                    UpdatedByUserId = table.Column<Guid>(nullable: true),
                    UpdatedDateTime = table.Column<DateTimeOffset>(nullable: true),
                    DeliveryReceiptId = table.Column<Guid>(nullable: true),
                    ProductId = table.Column<Guid>(nullable: false),
                    ProductName = table.Column<string>(nullable: false),
                    ProductCode = table.Column<string>(nullable: false),
                    Quantity = table.Column<int>(nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    SalePrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ExpiryDate = table.Column<DateTime>(nullable: true),
                    Packing = table.Column<int>(nullable: false),
                    PackingNumber = table.Column<int>(nullable: false),
                    PFS = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ppa = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Bulk = table.Column<int>(nullable: false),
                    VendorBatchNumber = table.Column<string>(nullable: false),
                    InternalBatchNumber = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeliveryReceiptItem", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DeliveryReceiptItem_DeliveryReceipt_DeliveryReceiptId",
                        column: x => x.DeliveryReceiptId,
                        principalSchema: "procurement",
                        principalTable: "DeliveryReceipt",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DeliveryReceipt_InvoiceId",
                schema: "procurement",
                table: "DeliveryReceipt",
                column: "InvoiceId");

            migrationBuilder.CreateIndex(
                name: "IX_DeliveryReceiptItem_DeliveryReceiptId",
                schema: "procurement",
                table: "DeliveryReceiptItem",
                column: "DeliveryReceiptId");

            migrationBuilder.CreateIndex(
                name: "IX_SupplierInvoice_OrderId",
                schema: "procurement",
                table: "SupplierInvoice",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_SupplierInvoiceItem_InvoiceId",
                schema: "procurement",
                table: "SupplierInvoiceItem",
                column: "InvoiceId");

            migrationBuilder.CreateIndex(
                name: "IX_SupplierOrderItem_OrderId",
                schema: "procurement",
                table: "SupplierOrderItem",
                column: "OrderId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DeliveryReceiptItem",
                schema: "procurement");

            migrationBuilder.DropTable(
                name: "SupplierInvoiceItem",
                schema: "procurement");

            migrationBuilder.DropTable(
                name: "SupplierOrderItem",
                schema: "procurement");

            migrationBuilder.DropTable(
                name: "DeliveryReceipt",
                schema: "procurement");

            migrationBuilder.DropTable(
                name: "SupplierInvoice",
                schema: "procurement");

            migrationBuilder.DropTable(
                name: "SupplierOrder",
                schema: "procurement");
        }
    }
}
