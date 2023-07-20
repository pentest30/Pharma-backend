using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace GHPCommerce.Migrator.Migrations.SalesDb
{
    public partial class Init : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "sales");

            migrationBuilder.CreateSequence<int>(
                name: "OrderNumbers",
                schema: "sales");

            migrationBuilder.CreateTable(
                name: "Discount",
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
                    ProductId = table.Column<Guid>(nullable: false),
                    ThresholdQuantity = table.Column<int>(nullable: false),
                    DiscountRate = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    from = table.Column<DateTime>(nullable: false),
                    to = table.Column<DateTime>(nullable: false),
                    ProductFullName = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Discount", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Orders",
                schema: "sales",
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
                    CustomerName = table.Column<string>(nullable: true),
                    SupplierId = table.Column<Guid>(nullable: false),
                    SupplierName = table.Column<string>(nullable: true),
                    OrderStatus = table.Column<long>(nullable: false),
                    OrderDiscount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    OrderTotal = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    OrderNumberSequence = table.Column<int>(nullable: false, defaultValueSql: "NEXT VALUE FOR sales.OrderNumbers"),
                    PaymentStatus = table.Column<long>(nullable: false),
                    ExpectedShippingDate = table.Column<DateTime>(nullable: true),
                    RefDocument = table.Column<string>(nullable: true),
                    CancellationReason = table.Column<long>(nullable: false),
                    CanceledBy = table.Column<Guid>(nullable: true),
                    CanceledTime = table.Column<DateTime>(nullable: true),
                    RejectedReason = table.Column<string>(nullable: true),
                    RejectedBy = table.Column<Guid>(nullable: true),
                    RejectedTime = table.Column<DateTime>(nullable: true),
                    ApprovedBy = table.Column<Guid>(nullable: true),
                    ApprovedTime = table.Column<DateTime>(nullable: true),
                    ProcessedBy = table.Column<Guid>(nullable: true),
                    ProcessingTime = table.Column<DateTime>(nullable: true),
                    ShippedBy = table.Column<Guid>(nullable: true),
                    ShippingTime = table.Column<DateTime>(nullable: true),
                    CompletedBy = table.Column<Guid>(nullable: true),
                    CompletedTime = table.Column<DateTime>(nullable: true),
                    PaidBy = table.Column<Guid>(nullable: true),
                    PaidDateTime = table.Column<DateTime>(nullable: true),
                    DefaultSalesPersonId = table.Column<Guid>(nullable: true),
                    OrderType = table.Column<long>(nullable: false),
                    CreatedBy = table.Column<string>(nullable: true),
                    UpdatedBy = table.Column<string>(nullable: true),
                    GuestId = table.Column<Guid>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Orders", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ShoppingCartItems",
                schema: "sales",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false, defaultValueSql: "newsequentialid()"),
                    RowVersion = table.Column<byte[]>(rowVersion: true, nullable: true),
                    CreatedDateTime = table.Column<DateTimeOffset>(nullable: false),
                    CreatedByUserId = table.Column<Guid>(nullable: false),
                    UpdatedByUserId = table.Column<Guid>(nullable: true),
                    UpdatedDateTime = table.Column<DateTimeOffset>(nullable: true),
                    Quantity = table.Column<int>(nullable: false),
                    ProductId = table.Column<Guid>(nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Discount = table.Column<double>(nullable: false),
                    Tax = table.Column<double>(nullable: false),
                    CustomerId = table.Column<Guid>(nullable: true),
                    CustomerName = table.Column<string>(nullable: true),
                    SupplierId = table.Column<Guid>(nullable: true),
                    Total = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalDiscount = table.Column<double>(nullable: false),
                    TotalTax = table.Column<double>(nullable: false),
                    GuestId = table.Column<Guid>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShoppingCartItems", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "OrderItems",
                schema: "sales",
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
                    ProductName = table.Column<string>(nullable: true),
                    ProductCode = table.Column<string>(nullable: true),
                    Quantity = table.Column<int>(nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Discount = table.Column<double>(nullable: false),
                    ExtraDiscount = table.Column<double>(nullable: false),
                    Tax = table.Column<double>(nullable: false),
                    UnitPriceInclTax = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    VendorBatchNumber = table.Column<string>(nullable: true),
                    InternalBatchNumber = table.Column<string>(nullable: true),
                    Color = table.Column<string>(nullable: true),
                    Size = table.Column<string>(nullable: true),
                    ExpiryDate = table.Column<DateTime>(nullable: true),
                    TotalInlTax = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalExlTax = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PackagingCode = table.Column<string>(nullable: true),
                    PFS = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PpaHT = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PpaTTC = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PpaPFS = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrderItems_Orders_OrderId",
                        column: x => x.OrderId,
                        principalSchema: "sales",
                        principalTable: "Orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SalesDiscount",
                schema: "sales",
                table: "Discount",
                columns: new[] { "OrganizationId", "ProductId", "DiscountRate", "ThresholdQuantity", "from", "to" },
                unique: true)
                .Annotation("SqlServer:Clustered", false);

            migrationBuilder.CreateIndex(
                name: "IX_OrderItems_OrderId",
                schema: "sales",
                table: "OrderItems",
                column: "OrderId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Discount",
                schema: "sales");

            migrationBuilder.DropTable(
                name: "OrderItems",
                schema: "sales");

            migrationBuilder.DropTable(
                name: "ShoppingCartItems",
                schema: "sales");

            migrationBuilder.DropTable(
                name: "Orders",
                schema: "sales");

            migrationBuilder.DropSequence(
                name: "OrderNumbers",
                schema: "sales");
        }
    }
}
