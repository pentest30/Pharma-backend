using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace GHPCommerce.Migrator.Migrations.InventoryDb
{
    public partial class Init : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "inventory");

            migrationBuilder.CreateTable(
                name: "Batch",
                schema: "inventory",
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
                    OrganizationName = table.Column<string>(maxLength: 100, nullable: true),
                    ProductCode = table.Column<string>(maxLength: 100, nullable: true),
                    ProductFullName = table.Column<string>(maxLength: 100, nullable: true),
                    VendorBatchNumber = table.Column<string>(maxLength: 100, nullable: true),
                    InternalBatchNumber = table.Column<string>(maxLength: 100, nullable: true),
                    PurchaseUnitPrice = table.Column<double>(nullable: true),
                    PurchaseDiscountRatio = table.Column<float>(nullable: true, defaultValue: 0f),
                    SalesUnitPrice = table.Column<double>(nullable: true),
                    SalesDiscountRatio = table.Column<float>(nullable: true, defaultValue: 0f),
                    packing = table.Column<int>(nullable: false),
                    ExpiryDate = table.Column<DateTime>(nullable: true),
                    PFS = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PpaHT = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PpaTTC = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PpaPFS = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    SupplierId = table.Column<Guid>(nullable: false),
                    SupplierName = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Batch", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "InventSum",
                schema: "inventory",
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
                    OrganizationName = table.Column<string>(maxLength: 100, nullable: true),
                    ProductCode = table.Column<string>(maxLength: 100, nullable: true),
                    ProductFullName = table.Column<string>(maxLength: 100, nullable: true),
                    PackagingCode = table.Column<string>(maxLength: 100, nullable: true),
                    packing = table.Column<int>(nullable: false),
                    VendorBatchNumber = table.Column<string>(maxLength: 100, nullable: true),
                    InternalBatchNumber = table.Column<string>(maxLength: 100, nullable: true),
                    ProductionDate = table.Column<DateTime>(nullable: true),
                    ExpiryDate = table.Column<DateTime>(nullable: true),
                    BestBeforeDate = table.Column<DateTime>(nullable: true),
                    Color = table.Column<string>(maxLength: 100, nullable: true),
                    Size = table.Column<string>(maxLength: 100, nullable: true),
                    PurchaseUnitPrice = table.Column<double>(nullable: true),
                    PurchaseDiscountRatio = table.Column<float>(nullable: true, defaultValue: 0f),
                    SalesUnitPrice = table.Column<double>(nullable: true),
                    SalesDiscountRatio = table.Column<float>(nullable: true, defaultValue: 0f),
                    PhysicalOnhandQuantity = table.Column<double>(nullable: false),
                    PhysicalReservedQuantity = table.Column<double>(nullable: false, defaultValue: 0.0),
                    PhysicalDispenseQuantity = table.Column<double>(nullable: true),
                    IsPublic = table.Column<bool>(nullable: false),
                    SiteId = table.Column<Guid>(nullable: true),
                    SiteName = table.Column<string>(maxLength: 100, nullable: true),
                    WarehouseId = table.Column<Guid>(nullable: true),
                    WarehouseName = table.Column<string>(maxLength: 100, nullable: true),
                    MinThresholdAlert = table.Column<double>(nullable: true),
                    PFS = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PpaHT = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PpaTTC = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PpaPFS = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InventSum", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TransferLog",
                schema: "inventory",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false, defaultValueSql: "newsequentialid()"),
                    RowVersion = table.Column<byte[]>(rowVersion: true, nullable: true),
                    CreatedDateTime = table.Column<DateTimeOffset>(nullable: false),
                    CreatedByUserId = table.Column<Guid>(nullable: false),
                    UpdatedByUserId = table.Column<Guid>(nullable: true),
                    UpdatedDateTime = table.Column<DateTimeOffset>(nullable: true),
                    SequenceNumber = table.Column<int>(nullable: false),
                    OrganizationId = table.Column<Guid>(nullable: false),
                    ZoneSourceId = table.Column<Guid>(nullable: false),
                    ZoneSourceName = table.Column<string>(nullable: true),
                    ZoneDestId = table.Column<Guid>(nullable: false),
                    ZoneDestName = table.Column<string>(nullable: true),
                    StockStateId = table.Column<Guid>(nullable: false),
                    StockStateName = table.Column<string>(nullable: true),
                    Status = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TransferLog", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ZoneType",
                schema: "inventory",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false, defaultValueSql: "newsequentialid()"),
                    RowVersion = table.Column<byte[]>(rowVersion: true, nullable: true),
                    CreatedDateTime = table.Column<DateTimeOffset>(nullable: false),
                    CreatedByUserId = table.Column<Guid>(nullable: false),
                    UpdatedByUserId = table.Column<Guid>(nullable: true),
                    UpdatedDateTime = table.Column<DateTimeOffset>(nullable: true),
                    Name = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ZoneType", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Invent",
                schema: "inventory",
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
                    BatchId = table.Column<Guid>(nullable: false),
                    ZoneId = table.Column<Guid>(nullable: false),
                    ZoneName = table.Column<string>(nullable: true),
                    StockStateId = table.Column<Guid>(nullable: false),
                    StockStateName = table.Column<string>(nullable: true),
                    OrganizationName = table.Column<string>(maxLength: 100, nullable: true),
                    ProductCode = table.Column<string>(maxLength: 100, nullable: true),
                    ProductFullName = table.Column<string>(maxLength: 100, nullable: true),
                    PackagingCode = table.Column<string>(maxLength: 100, nullable: true),
                    packing = table.Column<int>(nullable: false),
                    VendorBatchNumber = table.Column<string>(maxLength: 100, nullable: true),
                    InternalBatchNumber = table.Column<string>(maxLength: 100, nullable: true),
                    ProductionDate = table.Column<DateTime>(nullable: true),
                    ExpiryDate = table.Column<DateTime>(nullable: true),
                    BestBeforeDate = table.Column<DateTime>(nullable: true),
                    Color = table.Column<string>(maxLength: 100, nullable: true),
                    Size = table.Column<string>(maxLength: 100, nullable: true),
                    PurchaseUnitPrice = table.Column<double>(nullable: true),
                    PurchaseDiscountRatio = table.Column<float>(nullable: true, defaultValue: 0f),
                    SalesUnitPrice = table.Column<double>(nullable: true),
                    SalesDiscountRatio = table.Column<float>(nullable: true, defaultValue: 0f),
                    PhysicalQuantity = table.Column<double>(nullable: false),
                    PhysicalReservedQuantity = table.Column<double>(nullable: false, defaultValue: 0.0),
                    MinThresholdAlert = table.Column<double>(nullable: true),
                    PFS = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PpaHT = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PpaTTC = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PpaPFS = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    SupplierName = table.Column<string>(nullable: true),
                    SupplierId = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Invent", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Invent_Batch_BatchId",
                        column: x => x.BatchId,
                        principalSchema: "inventory",
                        principalTable: "Batch",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StockState",
                schema: "inventory",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false, defaultValueSql: "newsequentialid()"),
                    RowVersion = table.Column<byte[]>(rowVersion: true, nullable: true),
                    CreatedDateTime = table.Column<DateTimeOffset>(nullable: false),
                    CreatedByUserId = table.Column<Guid>(nullable: false),
                    UpdatedByUserId = table.Column<Guid>(nullable: true),
                    UpdatedDateTime = table.Column<DateTimeOffset>(nullable: true),
                    ZoneTypeId = table.Column<Guid>(nullable: false),
                    Name = table.Column<string>(nullable: true),
                    StockStatus = table.Column<int>(nullable: false),
                    OrganizationId = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StockState", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StockState_ZoneType_ZoneTypeId",
                        column: x => x.ZoneTypeId,
                        principalSchema: "inventory",
                        principalTable: "ZoneType",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StockZone",
                schema: "inventory",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false, defaultValueSql: "newsequentialid()"),
                    RowVersion = table.Column<byte[]>(rowVersion: true, nullable: true),
                    CreatedDateTime = table.Column<DateTimeOffset>(nullable: false),
                    CreatedByUserId = table.Column<Guid>(nullable: false),
                    UpdatedByUserId = table.Column<Guid>(nullable: true),
                    UpdatedDateTime = table.Column<DateTimeOffset>(nullable: true),
                    ZoneTypeId = table.Column<Guid>(nullable: false),
                    ZoneState = table.Column<int>(nullable: false),
                    Name = table.Column<string>(nullable: true),
                    OrganizationId = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StockZone", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StockZone_ZoneType_ZoneTypeId",
                        column: x => x.ZoneTypeId,
                        principalSchema: "inventory",
                        principalTable: "ZoneType",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "InventItemTransaction",
                schema: "inventory",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false, defaultValueSql: "newsequentialid()"),
                    RowVersion = table.Column<byte[]>(rowVersion: true, nullable: true),
                    CreatedDateTime = table.Column<DateTimeOffset>(nullable: false),
                    CreatedByUserId = table.Column<Guid>(nullable: false),
                    UpdatedByUserId = table.Column<Guid>(nullable: true),
                    UpdatedDateTime = table.Column<DateTimeOffset>(nullable: true),
                    OrganizationId = table.Column<Guid>(nullable: false),
                    OrganizationName = table.Column<string>(nullable: true),
                    ProductCode = table.Column<string>(nullable: true),
                    ProductFullName = table.Column<string>(nullable: true),
                    CustomerId = table.Column<Guid>(nullable: false),
                    SupplierId = table.Column<Guid>(nullable: false),
                    InventId = table.Column<Guid>(nullable: false),
                    CustomerName = table.Column<string>(nullable: true),
                    SupplierName = table.Column<string>(nullable: true),
                    Quantity = table.Column<double>(nullable: false),
                    OriginQuantity = table.Column<double>(nullable: false),
                    NewQuantity = table.Column<double>(nullable: false),
                    OrderId = table.Column<Guid>(nullable: true),
                    OrderNumberSequence = table.Column<string>(nullable: true),
                    OrderDate = table.Column<DateTime>(nullable: false),
                    RefDoc = table.Column<string>(nullable: true),
                    BlId = table.Column<Guid>(nullable: true),
                    TransactionTime = table.Column<DateTime>(nullable: false),
                    TransactionTypeId = table.Column<Guid>(nullable: false),
                    TransactionCode = table.Column<int>(nullable: false),
                    VendorBatchNumber = table.Column<string>(nullable: true),
                    InternalBatchNumber = table.Column<string>(nullable: true),
                    TransactionType = table.Column<int>(nullable: false),
                    StockEntry = table.Column<bool>(nullable: false),
                    ProductId = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InventItemTransaction", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InventItemTransaction_Invent_InventId",
                        column: x => x.InventId,
                        principalSchema: "inventory",
                        principalTable: "Invent",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TransferLogItem",
                schema: "inventory",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false, defaultValueSql: "newsequentialid()"),
                    RowVersion = table.Column<byte[]>(rowVersion: true, nullable: true),
                    CreatedDateTime = table.Column<DateTimeOffset>(nullable: false),
                    CreatedByUserId = table.Column<Guid>(nullable: false),
                    UpdatedByUserId = table.Column<Guid>(nullable: true),
                    UpdatedDateTime = table.Column<DateTimeOffset>(nullable: true),
                    ProductId = table.Column<Guid>(nullable: false),
                    ProductName = table.Column<string>(nullable: true),
                    ProductCode = table.Column<string>(nullable: true),
                    InternalBatchNumber = table.Column<string>(nullable: true),
                    InventId = table.Column<Guid>(nullable: false),
                    Quantity = table.Column<double>(nullable: false),
                    ExpiryDate = table.Column<DateTime>(nullable: false),
                    TransferLogId = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TransferLogItem", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TransferLogItem_Invent_InventId",
                        column: x => x.InventId,
                        principalSchema: "inventory",
                        principalTable: "Invent",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TransferLogItem_TransferLog_TransferLogId",
                        column: x => x.TransferLogId,
                        principalSchema: "inventory",
                        principalTable: "TransferLog",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                schema: "inventory",
                table: "ZoneType",
                columns: new[] { "Id", "CreatedByUserId", "CreatedDateTime", "Name", "UpdatedByUserId", "UpdatedDateTime" },
                values: new object[] { new Guid("6bd42e21-e657-4f99-afef-1afe5ceacb16"), new Guid("00000000-0000-0000-0000-000000000000"), new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Vendable", null, null });

            migrationBuilder.InsertData(
                schema: "inventory",
                table: "ZoneType",
                columns: new[] { "Id", "CreatedByUserId", "CreatedDateTime", "Name", "UpdatedByUserId", "UpdatedDateTime" },
                values: new object[] { new Guid("6ad42e21-e657-4f99-afef-1afe5ceacb16"), new Guid("00000000-0000-0000-0000-000000000000"), new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Non vendable", null, null });

            migrationBuilder.InsertData(
                schema: "inventory",
                table: "StockState",
                columns: new[] { "Id", "CreatedByUserId", "CreatedDateTime", "Name", "OrganizationId", "StockStatus", "UpdatedByUserId", "UpdatedDateTime", "ZoneTypeId" },
                values: new object[,]
                {
                    { new Guid("7bd32e21-e657-4f99-afef-1afe5ceacb16"), new Guid("00000000-0000-0000-0000-000000000000"), new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Libéré", new Guid("00000000-0000-0000-0000-000000000000"), 0, null, null, new Guid("6bd42e21-e657-4f99-afef-1afe5ceacb16") },
                    { new Guid("7bd52e22-e657-4f99-afef-1afe5ceacb16"), new Guid("00000000-0000-0000-0000-000000000000"), new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Non libéré", new Guid("00000000-0000-0000-0000-000000000000"), 0, null, null, new Guid("6bd42e21-e657-4f99-afef-1afe5ceacb16") },
                    { new Guid("7bd62e23-e657-4f99-afef-1afe5ceacb16"), new Guid("00000000-0000-0000-0000-000000000000"), new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Abîmé", new Guid("00000000-0000-0000-0000-000000000000"), 0, null, null, new Guid("6ad42e21-e657-4f99-afef-1afe5ceacb16") },
                    { new Guid("7bd72e23-e657-4f99-afef-1afe5ceacb16"), new Guid("00000000-0000-0000-0000-000000000000"), new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Périmé", new Guid("00000000-0000-0000-0000-000000000000"), 0, null, null, new Guid("6ad42e21-e657-4f99-afef-1afe5ceacb16") },
                    { new Guid("7bd82e23-e657-4f99-afef-1afe5ceacb16"), new Guid("00000000-0000-0000-0000-000000000000"), new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Sans vignette", new Guid("00000000-0000-0000-0000-000000000000"), 0, null, null, new Guid("6ad42e21-e657-4f99-afef-1afe5ceacb16") },
                    { new Guid("7bd92e23-e657-4f99-afef-1afe5ceacb16"), new Guid("00000000-0000-0000-0000-000000000000"), new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Instance", new Guid("00000000-0000-0000-0000-000000000000"), 0, null, null, new Guid("6ad42e21-e657-4f99-afef-1afe5ceacb16") },
                    { new Guid("7bd13e23-e657-4f99-afef-1afe5ceacb16"), new Guid("00000000-0000-0000-0000-000000000000"), new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "RAL", new Guid("00000000-0000-0000-0000-000000000000"), 0, null, null, new Guid("6ad42e21-e657-4f99-afef-1afe5ceacb16") }
                });

            migrationBuilder.InsertData(
                schema: "inventory",
                table: "StockZone",
                columns: new[] { "Id", "CreatedByUserId", "CreatedDateTime", "Name", "OrganizationId", "UpdatedByUserId", "UpdatedDateTime", "ZoneState", "ZoneTypeId" },
                values: new object[,]
                {
                    { new Guid("7bd42e21-e657-4f99-afef-1afe5ceacb16"), new Guid("00000000-0000-0000-0000-000000000000"), new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Zone vendable", new Guid("00000000-0000-0000-0000-000000000000"), null, null, 0, new Guid("6bd42e21-e657-4f99-afef-1afe5ceacb16") },
                    { new Guid("7bd42e22-e657-4f99-afef-1afe5ceacb16"), new Guid("00000000-0000-0000-0000-000000000000"), new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Zone non vendable", new Guid("00000000-0000-0000-0000-000000000000"), null, null, 0, new Guid("6ad42e21-e657-4f99-afef-1afe5ceacb16") },
                    { new Guid("7bd42e23-e657-4f99-afef-1afe5ceacb16"), new Guid("00000000-0000-0000-0000-000000000000"), new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Zone Chez le fournisseur", new Guid("00000000-0000-0000-0000-000000000000"), null, null, 0, new Guid("6ad42e21-e657-4f99-afef-1afe5ceacb16") }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Invent_BatchId",
                schema: "inventory",
                table: "Invent",
                column: "BatchId");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryDimensions",
                schema: "inventory",
                table: "Invent",
                columns: new[] { "OrganizationId", "ProductId", "VendorBatchNumber", "InternalBatchNumber", "Color", "Size", "ZoneId", "StockStateId" },
                unique: true)
                .Annotation("SqlServer:Clustered", false);

            migrationBuilder.CreateIndex(
                name: "IX_InventItemTransaction_InventId",
                schema: "inventory",
                table: "InventItemTransaction",
                column: "InventId");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryDimensions",
                schema: "inventory",
                table: "InventSum",
                columns: new[] { "OrganizationId", "SiteId", "WarehouseId", "ProductId", "VendorBatchNumber", "InternalBatchNumber", "Color", "Size", "IsPublic" },
                unique: true)
                .Annotation("SqlServer:Clustered", false);

            migrationBuilder.CreateIndex(
                name: "IX_StockState_ZoneTypeId",
                schema: "inventory",
                table: "StockState",
                column: "ZoneTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_StockZone_ZoneTypeId",
                schema: "inventory",
                table: "StockZone",
                column: "ZoneTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_TransferLogItem_InventId",
                schema: "inventory",
                table: "TransferLogItem",
                column: "InventId");

            migrationBuilder.CreateIndex(
                name: "IX_TransferLogItem_TransferLogId",
                schema: "inventory",
                table: "TransferLogItem",
                column: "TransferLogId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "InventItemTransaction",
                schema: "inventory");

            migrationBuilder.DropTable(
                name: "InventSum",
                schema: "inventory");

            migrationBuilder.DropTable(
                name: "StockState",
                schema: "inventory");

            migrationBuilder.DropTable(
                name: "StockZone",
                schema: "inventory");

            migrationBuilder.DropTable(
                name: "TransferLogItem",
                schema: "inventory");

            migrationBuilder.DropTable(
                name: "ZoneType",
                schema: "inventory");

            migrationBuilder.DropTable(
                name: "Invent",
                schema: "inventory");

            migrationBuilder.DropTable(
                name: "TransferLog",
                schema: "inventory");

            migrationBuilder.DropTable(
                name: "Batch",
                schema: "inventory");
        }
    }
}
