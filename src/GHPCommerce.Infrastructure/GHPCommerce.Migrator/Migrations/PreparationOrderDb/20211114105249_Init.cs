using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace GHPCommerce.Migrator.Migrations.PreparationOrderDb
{
    public partial class Init : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "logistics");

            migrationBuilder.CreateTable(
                name: "ConsolidationOrder",
                schema: "logistics",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false, defaultValueSql: "newsequentialid()"),
                    RowVersion = table.Column<byte[]>(rowVersion: true, nullable: true),
                    CreatedDateTime = table.Column<DateTimeOffset>(nullable: false),
                    CreatedByUserId = table.Column<Guid>(nullable: false),
                    UpdatedByUserId = table.Column<Guid>(nullable: true),
                    UpdatedDateTime = table.Column<DateTimeOffset>(nullable: true),
                    CustomerId = table.Column<Guid>(nullable: false),
                    OrganizationId = table.Column<Guid>(nullable: false),
                    OrganizationName = table.Column<string>(nullable: true),
                    CustomerName = table.Column<string>(nullable: true),
                    ReceptionExpedition = table.Column<bool>(nullable: false),
                    SentForExpedition = table.Column<bool>(nullable: false),
                    Printed = table.Column<bool>(nullable: false),
                    PrintCount = table.Column<int>(nullable: false),
                    PrintedBy = table.Column<Guid>(nullable: true),
                    PrintedByName = table.Column<string>(nullable: true),
                    PrintedTime = table.Column<DateTime>(nullable: true),
                    OrderId = table.Column<Guid>(nullable: false),
                    OrderDate = table.Column<DateTime>(nullable: true),
                    OrderIdentifier = table.Column<string>(nullable: true),
                    ConsolidatedById = table.Column<Guid>(nullable: true),
                    ConsolidatedTime = table.Column<DateTime>(nullable: true),
                    ConsolidatedByName = table.Column<string>(nullable: true),
                    EmployeeCode = table.Column<string>(nullable: true),
                    ReceivedInShippingBy = table.Column<string>(nullable: true),
                    ReceptionExpeditionTime = table.Column<DateTime>(nullable: true),
                    TotalPackage = table.Column<int>(nullable: false),
                    TotalPackageThermolabile = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConsolidationOrder", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DeleiveryOrder",
                schema: "logistics",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false, defaultValueSql: "newsequentialid()"),
                    RowVersion = table.Column<byte[]>(rowVersion: true, nullable: true),
                    CreatedDateTime = table.Column<DateTimeOffset>(nullable: false),
                    CreatedByUserId = table.Column<Guid>(nullable: false),
                    UpdatedByUserId = table.Column<Guid>(nullable: true),
                    UpdatedDateTime = table.Column<DateTimeOffset>(nullable: true),
                    OrderId = table.Column<Guid>(nullable: false),
                    DeleiveryOrderDate = table.Column<DateTime>(nullable: false),
                    OrderDate = table.Column<DateTime>(nullable: false),
                    CustomerId = table.Column<Guid>(nullable: false),
                    OrganizationId = table.Column<Guid>(nullable: false),
                    CustomerName = table.Column<string>(nullable: true),
                    SupplierId = table.Column<Guid>(nullable: false),
                    TotalPackage = table.Column<int>(nullable: false),
                    TotalPackageThermolabile = table.Column<int>(nullable: false),
                    Validated = table.Column<bool>(nullable: false),
                    CreatedBy = table.Column<string>(nullable: true),
                    UpdatedBy = table.Column<string>(nullable: true),
                    SequenceNumber = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeleiveryOrder", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PreparationOrder",
                schema: "logistics",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false, defaultValueSql: "newsequentialid()"),
                    RowVersion = table.Column<byte[]>(rowVersion: true, nullable: true),
                    CreatedDateTime = table.Column<DateTimeOffset>(nullable: false),
                    CreatedByUserId = table.Column<Guid>(nullable: false),
                    UpdatedByUserId = table.Column<Guid>(nullable: true),
                    UpdatedDateTime = table.Column<DateTimeOffset>(nullable: true),
                    CustomerId = table.Column<Guid>(nullable: false),
                    OrganizationId = table.Column<Guid>(nullable: false),
                    OrganizationName = table.Column<string>(nullable: true),
                    CustomerName = table.Column<string>(nullable: true),
                    IdentifierNumber = table.Column<string>(nullable: true),
                    PreparationOrderNumberSequence = table.Column<int>(nullable: false),
                    BarCode = table.Column<string>(nullable: true),
                    BarCodeImage = table.Column<byte[]>(nullable: true),
                    TotalPackage = table.Column<int>(nullable: false),
                    TotalPackageThermolabile = table.Column<int>(nullable: false),
                    TotalZoneCount = table.Column<int>(nullable: false),
                    ZoneDoneCount = table.Column<int>(nullable: false),
                    Printed = table.Column<bool>(nullable: false),
                    PrintCount = table.Column<int>(nullable: false),
                    PrintedBy = table.Column<Guid>(nullable: true),
                    PrintedByName = table.Column<string>(nullable: true),
                    PrintedTime = table.Column<DateTime>(nullable: true),
                    OrderId = table.Column<Guid>(nullable: false),
                    OrderDate = table.Column<DateTime>(nullable: true),
                    ZoneGroupOrder = table.Column<int>(nullable: false),
                    OrderIdentifier = table.Column<string>(nullable: true),
                    ZoneGroupId = table.Column<Guid>(nullable: false),
                    SectorName = table.Column<string>(nullable: true),
                    SectorCode = table.Column<string>(nullable: true),
                    ZoneGroupName = table.Column<string>(nullable: true),
                    ConsolidatedById = table.Column<Guid>(nullable: true),
                    ConsolidatedTime = table.Column<DateTime>(nullable: true),
                    ConsolidatedByName = table.Column<string>(nullable: true),
                    EmployeeCode = table.Column<string>(nullable: true),
                    PreparationOrderStatus = table.Column<long>(nullable: false),
                    SequenceNumber = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PreparationOrder", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DeleiveryOrderItem",
                schema: "logistics",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false, defaultValueSql: "newsequentialid()"),
                    RowVersion = table.Column<byte[]>(rowVersion: true, nullable: true),
                    CreatedDateTime = table.Column<DateTimeOffset>(nullable: false),
                    CreatedByUserId = table.Column<Guid>(nullable: false),
                    UpdatedByUserId = table.Column<Guid>(nullable: true),
                    UpdatedDateTime = table.Column<DateTimeOffset>(nullable: true),
                    DeleiveryOrderId = table.Column<Guid>(nullable: true),
                    ProductId = table.Column<Guid>(nullable: false),
                    ProductName = table.Column<string>(nullable: true),
                    ProductCode = table.Column<string>(nullable: true),
                    Quantity = table.Column<int>(nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PurchaseUnitPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Discount = table.Column<double>(nullable: false),
                    ExtraDiscount = table.Column<double>(nullable: false),
                    Tax = table.Column<double>(nullable: false),
                    VendorBatchNumber = table.Column<string>(nullable: true),
                    InternalBatchNumber = table.Column<string>(nullable: true),
                    ExpiryDate = table.Column<DateTime>(nullable: true),
                    PFS = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PpaHT = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeleiveryOrderItem", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DeleiveryOrderItem_DeleiveryOrder_DeleiveryOrderId",
                        column: x => x.DeleiveryOrderId,
                        principalSchema: "logistics",
                        principalTable: "DeleiveryOrder",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PreparationOrderExecuter",
                schema: "logistics",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false, defaultValueSql: "newsequentialid()"),
                    RowVersion = table.Column<byte[]>(rowVersion: true, nullable: true),
                    CreatedDateTime = table.Column<DateTimeOffset>(nullable: false),
                    CreatedByUserId = table.Column<Guid>(nullable: false),
                    UpdatedByUserId = table.Column<Guid>(nullable: true),
                    UpdatedDateTime = table.Column<DateTimeOffset>(nullable: true),
                    PreparationOrderId = table.Column<Guid>(nullable: false),
                    ExecutedById = table.Column<Guid>(nullable: false),
                    ExecutedByName = table.Column<string>(nullable: true),
                    ExecutedTime = table.Column<DateTime>(nullable: true),
                    PickingZoneId = table.Column<Guid>(nullable: false),
                    PickingZoneName = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PreparationOrderExecuter", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PreparationOrderExecuter_PreparationOrder_PreparationOrderId",
                        column: x => x.PreparationOrderId,
                        principalSchema: "logistics",
                        principalTable: "PreparationOrder",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PreparationOrderItem",
                schema: "logistics",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false, defaultValueSql: "newsequentialid()"),
                    RowVersion = table.Column<byte[]>(rowVersion: true, nullable: true),
                    CreatedDateTime = table.Column<DateTimeOffset>(nullable: false),
                    CreatedByUserId = table.Column<Guid>(nullable: false),
                    UpdatedByUserId = table.Column<Guid>(nullable: true),
                    UpdatedDateTime = table.Column<DateTimeOffset>(nullable: true),
                    PreparationOrderId = table.Column<Guid>(nullable: false),
                    OrderId = table.Column<Guid>(nullable: true),
                    ProductId = table.Column<Guid>(nullable: false),
                    ProductName = table.Column<string>(nullable: true),
                    ProductCode = table.Column<string>(nullable: true),
                    Quantity = table.Column<int>(nullable: false),
                    PackingQuantity = table.Column<int>(nullable: true),
                    Packing = table.Column<int>(nullable: false),
                    InternalBatchNumber = table.Column<string>(nullable: true),
                    Discount = table.Column<double>(nullable: false),
                    OldDiscount = table.Column<double>(nullable: false),
                    ExtraDiscount = table.Column<double>(nullable: false),
                    PickingZoneId = table.Column<Guid>(nullable: true),
                    PickingZoneName = table.Column<string>(nullable: true),
                    ZoneGroupId = table.Column<Guid>(nullable: true),
                    ZoneGroupName = table.Column<string>(nullable: true),
                    PickingZoneOrder = table.Column<int>(nullable: false),
                    DefaultLocation = table.Column<string>(nullable: true),
                    ExpiryDate = table.Column<DateTime>(nullable: true),
                    PpaHT = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Status = table.Column<long>(nullable: false),
                    IsControlled = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PreparationOrderItem", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PreparationOrderItem_PreparationOrder_PreparationOrderId",
                        column: x => x.PreparationOrderId,
                        principalSchema: "logistics",
                        principalTable: "PreparationOrder",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PreparationOrderVerifier",
                schema: "logistics",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false, defaultValueSql: "newsequentialid()"),
                    RowVersion = table.Column<byte[]>(rowVersion: true, nullable: true),
                    CreatedDateTime = table.Column<DateTimeOffset>(nullable: false),
                    CreatedByUserId = table.Column<Guid>(nullable: false),
                    UpdatedByUserId = table.Column<Guid>(nullable: true),
                    UpdatedDateTime = table.Column<DateTimeOffset>(nullable: true),
                    PreparationOrderId = table.Column<Guid>(nullable: false),
                    VerifiedById = table.Column<Guid>(nullable: false),
                    VerifiedByName = table.Column<string>(nullable: true),
                    VerifiedTime = table.Column<DateTime>(nullable: false),
                    PickingZoneId = table.Column<Guid>(nullable: false),
                    PickingZoneName = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PreparationOrderVerifier", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PreparationOrderVerifier_PreparationOrder_PreparationOrderId",
                        column: x => x.PreparationOrderId,
                        principalSchema: "logistics",
                        principalTable: "PreparationOrder",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DeleiveryOrderItem_DeleiveryOrderId",
                schema: "logistics",
                table: "DeleiveryOrderItem",
                column: "DeleiveryOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_PreparationOrderExecuter_PreparationOrderId",
                schema: "logistics",
                table: "PreparationOrderExecuter",
                column: "PreparationOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_PreparationOrderItem_PreparationOrderId",
                schema: "logistics",
                table: "PreparationOrderItem",
                column: "PreparationOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_PreparationOrderVerifier_PreparationOrderId",
                schema: "logistics",
                table: "PreparationOrderVerifier",
                column: "PreparationOrderId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ConsolidationOrder",
                schema: "logistics");

            migrationBuilder.DropTable(
                name: "DeleiveryOrderItem",
                schema: "logistics");

            migrationBuilder.DropTable(
                name: "PreparationOrderExecuter",
                schema: "logistics");

            migrationBuilder.DropTable(
                name: "PreparationOrderItem",
                schema: "logistics");

            migrationBuilder.DropTable(
                name: "PreparationOrderVerifier",
                schema: "logistics");

            migrationBuilder.DropTable(
                name: "DeleiveryOrder",
                schema: "logistics");

            migrationBuilder.DropTable(
                name: "PreparationOrder",
                schema: "logistics");
        }
    }
}
