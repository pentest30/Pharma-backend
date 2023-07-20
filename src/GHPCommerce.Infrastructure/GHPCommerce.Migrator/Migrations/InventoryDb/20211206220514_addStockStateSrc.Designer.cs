﻿// <auto-generated />
using System;
using GHPCommerce.Modules.Inventory.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace GHPCommerce.Migrator.Migrations.InventoryDb
{
    [DbContext(typeof(InventoryDbContext))]
    [Migration("20211206220514_addStockStateSrc")]
    partial class addStockStateSrc
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "3.1.8")
                .HasAnnotation("Relational:MaxIdentifierLength", 128)
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("GHPCommerce.Modules.Inventory.Entities.Batch", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier")
                        .HasDefaultValueSql("newsequentialid()");

                    b.Property<Guid>("CreatedByUserId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<DateTimeOffset>("CreatedDateTime")
                        .HasColumnType("datetimeoffset");

                    b.Property<DateTime?>("ExpiryDate")
                        .HasColumnType("datetime2");

                    b.Property<string>("InternalBatchNumber")
                        .HasColumnType("nvarchar(100)")
                        .HasMaxLength(100);

                    b.Property<Guid>("OrganizationId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("OrganizationName")
                        .HasColumnType("nvarchar(100)")
                        .HasMaxLength(100);

                    b.Property<decimal>("PFS")
                        .HasColumnType("decimal(18,2)");

                    b.Property<decimal>("PpaHT")
                        .HasColumnType("decimal(18,2)");

                    b.Property<decimal>("PpaPFS")
                        .HasColumnType("decimal(18,2)");

                    b.Property<decimal>("PpaTTC")
                        .HasColumnType("decimal(18,2)");

                    b.Property<string>("ProductCode")
                        .HasColumnType("nvarchar(100)")
                        .HasMaxLength(100);

                    b.Property<string>("ProductFullName")
                        .HasColumnType("nvarchar(100)")
                        .HasMaxLength(100);

                    b.Property<Guid>("ProductId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<float?>("PurchaseDiscountRatio")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("real")
                        .HasDefaultValue(0f);

                    b.Property<double?>("PurchaseUnitPrice")
                        .HasColumnType("float");

                    b.Property<byte[]>("RowVersion")
                        .IsConcurrencyToken()
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnType("rowversion");

                    b.Property<float?>("SalesDiscountRatio")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("real")
                        .HasDefaultValue(0f);

                    b.Property<double?>("SalesUnitPrice")
                        .HasColumnType("float");

                    b.Property<Guid>("SupplierId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("SupplierName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<Guid?>("UpdatedByUserId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<DateTimeOffset?>("UpdatedDateTime")
                        .HasColumnType("datetimeoffset");

                    b.Property<string>("VendorBatchNumber")
                        .HasColumnType("nvarchar(100)")
                        .HasMaxLength(100);

                    b.Property<int>("packing")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.ToTable("Batch","inventory");
                });

            modelBuilder.Entity("GHPCommerce.Modules.Inventory.Entities.Invent", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier")
                        .HasDefaultValueSql("newsequentialid()");

                    b.Property<Guid>("BatchId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<DateTime?>("BestBeforeDate")
                        .HasColumnType("datetime2");

                    b.Property<string>("Color")
                        .HasColumnType("nvarchar(100)")
                        .HasMaxLength(100);

                    b.Property<Guid>("CreatedByUserId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<DateTimeOffset>("CreatedDateTime")
                        .HasColumnType("datetimeoffset");

                    b.Property<DateTime?>("ExpiryDate")
                        .HasColumnType("datetime2");

                    b.Property<string>("InternalBatchNumber")
                        .HasColumnType("nvarchar(100)")
                        .HasMaxLength(100);

                    b.Property<double?>("MinThresholdAlert")
                        .HasColumnType("float");

                    b.Property<Guid>("OrganizationId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("OrganizationName")
                        .HasColumnType("nvarchar(100)")
                        .HasMaxLength(100);

                    b.Property<decimal>("PFS")
                        .HasColumnType("decimal(18,2)");

                    b.Property<string>("PackagingCode")
                        .HasColumnType("nvarchar(100)")
                        .HasMaxLength(100);

                    b.Property<double>("PhysicalQuantity")
                        .HasColumnType("float");

                    b.Property<double>("PhysicalReservedQuantity")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("float")
                        .HasDefaultValue(0.0);

                    b.Property<decimal>("PpaHT")
                        .HasColumnType("decimal(18,2)");

                    b.Property<decimal>("PpaPFS")
                        .HasColumnType("decimal(18,2)");

                    b.Property<decimal>("PpaTTC")
                        .HasColumnType("decimal(18,2)");

                    b.Property<string>("ProductCode")
                        .HasColumnType("nvarchar(100)")
                        .HasMaxLength(100);

                    b.Property<string>("ProductFullName")
                        .HasColumnType("nvarchar(100)")
                        .HasMaxLength(100);

                    b.Property<Guid>("ProductId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<DateTime?>("ProductionDate")
                        .HasColumnType("datetime2");

                    b.Property<float?>("PurchaseDiscountRatio")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("real")
                        .HasDefaultValue(0f);

                    b.Property<double?>("PurchaseUnitPrice")
                        .HasColumnType("float");

                    b.Property<byte[]>("RowVersion")
                        .IsConcurrencyToken()
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnType("rowversion");

                    b.Property<float?>("SalesDiscountRatio")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("real")
                        .HasDefaultValue(0f);

                    b.Property<double?>("SalesUnitPrice")
                        .HasColumnType("float");

                    b.Property<string>("Size")
                        .HasColumnType("nvarchar(100)")
                        .HasMaxLength(100);

                    b.Property<Guid>("StockStateId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("StockStateName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<Guid>("SupplierId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("SupplierName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<Guid?>("UpdatedByUserId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<DateTimeOffset?>("UpdatedDateTime")
                        .HasColumnType("datetimeoffset");

                    b.Property<string>("VendorBatchNumber")
                        .HasColumnType("nvarchar(100)")
                        .HasMaxLength(100);

                    b.Property<Guid>("ZoneId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("ZoneName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("packing")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("BatchId");

                    b.HasIndex("OrganizationId", "ProductId", "VendorBatchNumber", "InternalBatchNumber", "Color", "Size", "ZoneId", "StockStateId")
                        .IsUnique()
                        .HasName("IX_InventoryDimensions")
                        .HasAnnotation("SqlServer:Clustered", false);

                    b.ToTable("Invent","inventory");
                });

            modelBuilder.Entity("GHPCommerce.Modules.Inventory.Entities.InventItemTransaction", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier")
                        .HasDefaultValueSql("newsequentialid()");

                    b.Property<Guid?>("BlId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid>("CreatedByUserId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<DateTimeOffset>("CreatedDateTime")
                        .HasColumnType("datetimeoffset");

                    b.Property<Guid>("CustomerId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("CustomerName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("InternalBatchNumber")
                        .HasColumnType("nvarchar(max)");

                    b.Property<Guid>("InventId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<double>("NewQuantity")
                        .HasColumnType("float");

                    b.Property<DateTime>("OrderDate")
                        .HasColumnType("datetime2");

                    b.Property<Guid?>("OrderId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("OrderNumberSequence")
                        .HasColumnType("nvarchar(max)");

                    b.Property<Guid>("OrganizationId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("OrganizationName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<double>("OriginQuantity")
                        .HasColumnType("float");

                    b.Property<string>("ProductCode")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("ProductFullName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<Guid>("ProductId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<double>("Quantity")
                        .HasColumnType("float");

                    b.Property<string>("RefDoc")
                        .HasColumnType("nvarchar(max)");

                    b.Property<byte[]>("RowVersion")
                        .IsConcurrencyToken()
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnType("rowversion");

                    b.Property<bool>("StockEntry")
                        .HasColumnType("bit");

                    b.Property<Guid>("SupplierId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("SupplierName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("TransactionCode")
                        .HasColumnType("int");

                    b.Property<DateTime>("TransactionTime")
                        .HasColumnType("datetime2");

                    b.Property<int>("TransactionType")
                        .HasColumnType("int");

                    b.Property<Guid>("TransactionTypeId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid?>("UpdatedByUserId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<DateTimeOffset?>("UpdatedDateTime")
                        .HasColumnType("datetimeoffset");

                    b.Property<string>("VendorBatchNumber")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.HasIndex("InventId");

                    b.ToTable("InventItemTransaction","inventory");
                });

            modelBuilder.Entity("GHPCommerce.Modules.Inventory.Entities.InventSum", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier")
                        .HasDefaultValueSql("newsequentialid()");

                    b.Property<DateTime?>("BestBeforeDate")
                        .HasColumnType("datetime2");

                    b.Property<string>("Color")
                        .HasColumnType("nvarchar(100)")
                        .HasMaxLength(100);

                    b.Property<Guid>("CreatedByUserId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<DateTimeOffset>("CreatedDateTime")
                        .HasColumnType("datetimeoffset");

                    b.Property<DateTime?>("ExpiryDate")
                        .HasColumnType("datetime2");

                    b.Property<string>("InternalBatchNumber")
                        .HasColumnType("nvarchar(100)")
                        .HasMaxLength(100);

                    b.Property<bool>("IsPublic")
                        .HasColumnType("bit");

                    b.Property<double?>("MinThresholdAlert")
                        .HasColumnType("float");

                    b.Property<Guid>("OrganizationId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("OrganizationName")
                        .HasColumnType("nvarchar(100)")
                        .HasMaxLength(100);

                    b.Property<decimal>("PFS")
                        .HasColumnType("decimal(18,2)");

                    b.Property<string>("PackagingCode")
                        .HasColumnType("nvarchar(100)")
                        .HasMaxLength(100);

                    b.Property<double?>("PhysicalDispenseQuantity")
                        .HasColumnType("float");

                    b.Property<double>("PhysicalOnhandQuantity")
                        .HasColumnType("float");

                    b.Property<double>("PhysicalReservedQuantity")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("float")
                        .HasDefaultValue(0.0);

                    b.Property<decimal>("PpaHT")
                        .HasColumnType("decimal(18,2)");

                    b.Property<decimal>("PpaPFS")
                        .HasColumnType("decimal(18,2)");

                    b.Property<decimal>("PpaTTC")
                        .HasColumnType("decimal(18,2)");

                    b.Property<string>("ProductCode")
                        .HasColumnType("nvarchar(100)")
                        .HasMaxLength(100);

                    b.Property<string>("ProductFullName")
                        .HasColumnType("nvarchar(100)")
                        .HasMaxLength(100);

                    b.Property<Guid>("ProductId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<DateTime?>("ProductionDate")
                        .HasColumnType("datetime2");

                    b.Property<float?>("PurchaseDiscountRatio")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("real")
                        .HasDefaultValue(0f);

                    b.Property<double?>("PurchaseUnitPrice")
                        .HasColumnType("float");

                    b.Property<byte[]>("RowVersion")
                        .IsConcurrencyToken()
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnType("rowversion");

                    b.Property<float?>("SalesDiscountRatio")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("real")
                        .HasDefaultValue(0f);

                    b.Property<double?>("SalesUnitPrice")
                        .HasColumnType("float");

                    b.Property<Guid?>("SiteId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("SiteName")
                        .HasColumnType("nvarchar(100)")
                        .HasMaxLength(100);

                    b.Property<string>("Size")
                        .HasColumnType("nvarchar(100)")
                        .HasMaxLength(100);

                    b.Property<Guid?>("UpdatedByUserId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<DateTimeOffset?>("UpdatedDateTime")
                        .HasColumnType("datetimeoffset");

                    b.Property<string>("VendorBatchNumber")
                        .HasColumnType("nvarchar(100)")
                        .HasMaxLength(100);

                    b.Property<Guid?>("WarehouseId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("WarehouseName")
                        .HasColumnType("nvarchar(100)")
                        .HasMaxLength(100);

                    b.Property<int>("packing")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("OrganizationId", "SiteId", "WarehouseId", "ProductId", "VendorBatchNumber", "InternalBatchNumber", "Color", "Size", "IsPublic")
                        .IsUnique()
                        .HasName("IX_InventoryDimensions")
                        .HasAnnotation("SqlServer:Clustered", false);

                    b.ToTable("InventSum","inventory");
                });

            modelBuilder.Entity("GHPCommerce.Modules.Inventory.Entities.StockState", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier")
                        .HasDefaultValueSql("newsequentialid()");

                    b.Property<Guid>("CreatedByUserId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<DateTimeOffset>("CreatedDateTime")
                        .HasColumnType("datetimeoffset");

                    b.Property<string>("Name")
                        .HasColumnType("nvarchar(max)");

                    b.Property<Guid>("OrganizationId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<byte[]>("RowVersion")
                        .IsConcurrencyToken()
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnType("rowversion");

                    b.Property<int>("StockStatus")
                        .HasColumnType("int");

                    b.Property<Guid?>("UpdatedByUserId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<DateTimeOffset?>("UpdatedDateTime")
                        .HasColumnType("datetimeoffset");

                    b.Property<Guid>("ZoneTypeId")
                        .HasColumnType("uniqueidentifier");

                    b.HasKey("Id");

                    b.HasIndex("ZoneTypeId");

                    b.ToTable("StockState","inventory");

                    b.HasData(
                        new
                        {
                            Id = new Guid("7bd32e21-e657-4f99-afef-1afe5ceacb16"),
                            CreatedByUserId = new Guid("00000000-0000-0000-0000-000000000000"),
                            CreatedDateTime = new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)),
                            Name = "Libéré",
                            OrganizationId = new Guid("00000000-0000-0000-0000-000000000000"),
                            StockStatus = 0,
                            ZoneTypeId = new Guid("6bd42e21-e657-4f99-afef-1afe5ceacb16")
                        },
                        new
                        {
                            Id = new Guid("7bd52e22-e657-4f99-afef-1afe5ceacb16"),
                            CreatedByUserId = new Guid("00000000-0000-0000-0000-000000000000"),
                            CreatedDateTime = new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)),
                            Name = "Non libéré",
                            OrganizationId = new Guid("00000000-0000-0000-0000-000000000000"),
                            StockStatus = 0,
                            ZoneTypeId = new Guid("6bd42e21-e657-4f99-afef-1afe5ceacb16")
                        },
                        new
                        {
                            Id = new Guid("7bd62e23-e657-4f99-afef-1afe5ceacb16"),
                            CreatedByUserId = new Guid("00000000-0000-0000-0000-000000000000"),
                            CreatedDateTime = new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)),
                            Name = "Abîmé",
                            OrganizationId = new Guid("00000000-0000-0000-0000-000000000000"),
                            StockStatus = 0,
                            ZoneTypeId = new Guid("6ad42e21-e657-4f99-afef-1afe5ceacb16")
                        },
                        new
                        {
                            Id = new Guid("7bd72e23-e657-4f99-afef-1afe5ceacb16"),
                            CreatedByUserId = new Guid("00000000-0000-0000-0000-000000000000"),
                            CreatedDateTime = new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)),
                            Name = "Périmé",
                            OrganizationId = new Guid("00000000-0000-0000-0000-000000000000"),
                            StockStatus = 0,
                            ZoneTypeId = new Guid("6ad42e21-e657-4f99-afef-1afe5ceacb16")
                        },
                        new
                        {
                            Id = new Guid("7bd82e23-e657-4f99-afef-1afe5ceacb16"),
                            CreatedByUserId = new Guid("00000000-0000-0000-0000-000000000000"),
                            CreatedDateTime = new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)),
                            Name = "Sans vignette",
                            OrganizationId = new Guid("00000000-0000-0000-0000-000000000000"),
                            StockStatus = 0,
                            ZoneTypeId = new Guid("6ad42e21-e657-4f99-afef-1afe5ceacb16")
                        },
                        new
                        {
                            Id = new Guid("7bd92e23-e657-4f99-afef-1afe5ceacb16"),
                            CreatedByUserId = new Guid("00000000-0000-0000-0000-000000000000"),
                            CreatedDateTime = new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)),
                            Name = "Instance",
                            OrganizationId = new Guid("00000000-0000-0000-0000-000000000000"),
                            StockStatus = 0,
                            ZoneTypeId = new Guid("6ad42e21-e657-4f99-afef-1afe5ceacb16")
                        },
                        new
                        {
                            Id = new Guid("7bd13e23-e657-4f99-afef-1afe5ceacb16"),
                            CreatedByUserId = new Guid("00000000-0000-0000-0000-000000000000"),
                            CreatedDateTime = new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)),
                            Name = "RAL",
                            OrganizationId = new Guid("00000000-0000-0000-0000-000000000000"),
                            StockStatus = 0,
                            ZoneTypeId = new Guid("6ad42e21-e657-4f99-afef-1afe5ceacb16")
                        });
                });

            modelBuilder.Entity("GHPCommerce.Modules.Inventory.Entities.StockZone", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier")
                        .HasDefaultValueSql("newsequentialid()");

                    b.Property<Guid>("CreatedByUserId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<DateTimeOffset>("CreatedDateTime")
                        .HasColumnType("datetimeoffset");

                    b.Property<string>("Name")
                        .HasColumnType("nvarchar(max)");

                    b.Property<Guid>("OrganizationId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<byte[]>("RowVersion")
                        .IsConcurrencyToken()
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnType("rowversion");

                    b.Property<Guid?>("UpdatedByUserId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<DateTimeOffset?>("UpdatedDateTime")
                        .HasColumnType("datetimeoffset");

                    b.Property<int>("ZoneState")
                        .HasColumnType("int");

                    b.Property<Guid>("ZoneTypeId")
                        .HasColumnType("uniqueidentifier");

                    b.HasKey("Id");

                    b.HasIndex("ZoneTypeId");

                    b.ToTable("StockZone","inventory");

                    b.HasData(
                        new
                        {
                            Id = new Guid("7bd42e21-e657-4f99-afef-1afe5ceacb16"),
                            CreatedByUserId = new Guid("00000000-0000-0000-0000-000000000000"),
                            CreatedDateTime = new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)),
                            Name = "Zone vendable",
                            OrganizationId = new Guid("00000000-0000-0000-0000-000000000000"),
                            ZoneState = 0,
                            ZoneTypeId = new Guid("6bd42e21-e657-4f99-afef-1afe5ceacb16")
                        },
                        new
                        {
                            Id = new Guid("7bd42e22-e657-4f99-afef-1afe5ceacb16"),
                            CreatedByUserId = new Guid("00000000-0000-0000-0000-000000000000"),
                            CreatedDateTime = new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)),
                            Name = "Zone non vendable",
                            OrganizationId = new Guid("00000000-0000-0000-0000-000000000000"),
                            ZoneState = 0,
                            ZoneTypeId = new Guid("6ad42e21-e657-4f99-afef-1afe5ceacb16")
                        },
                        new
                        {
                            Id = new Guid("7bd42e23-e657-4f99-afef-1afe5ceacb16"),
                            CreatedByUserId = new Guid("00000000-0000-0000-0000-000000000000"),
                            CreatedDateTime = new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)),
                            Name = "Zone Chez le fournisseur",
                            OrganizationId = new Guid("00000000-0000-0000-0000-000000000000"),
                            ZoneState = 0,
                            ZoneTypeId = new Guid("6ad42e21-e657-4f99-afef-1afe5ceacb16")
                        });
                });

            modelBuilder.Entity("GHPCommerce.Modules.Inventory.Entities.TransferLog", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier")
                        .HasDefaultValueSql("newsequentialid()");

                    b.Property<Guid>("CreatedByUserId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<DateTimeOffset>("CreatedDateTime")
                        .HasColumnType("datetimeoffset");

                    b.Property<Guid>("OrganizationId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<byte[]>("RowVersion")
                        .IsConcurrencyToken()
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnType("rowversion");

                    b.Property<int>("SequenceNumber")
                        .HasColumnType("int");

                    b.Property<int>("Status")
                        .HasColumnType("int");

                    b.Property<Guid>("StockStateId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("StockStateName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<Guid>("StockStateSourceId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("StockStateSourceName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<Guid?>("UpdatedByUserId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<DateTimeOffset?>("UpdatedDateTime")
                        .HasColumnType("datetimeoffset");

                    b.Property<Guid>("ZoneDestId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("ZoneDestName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<Guid>("ZoneSourceId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("ZoneSourceName")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("TransferLog","inventory");
                });

            modelBuilder.Entity("GHPCommerce.Modules.Inventory.Entities.TransferLogItem", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier")
                        .HasDefaultValueSql("newsequentialid()");

                    b.Property<Guid>("CreatedByUserId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<DateTimeOffset>("CreatedDateTime")
                        .HasColumnType("datetimeoffset");

                    b.Property<DateTime>("ExpiryDate")
                        .HasColumnType("datetime2");

                    b.Property<string>("InternalBatchNumber")
                        .HasColumnType("nvarchar(max)");

                    b.Property<Guid>("InventId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("ProductCode")
                        .HasColumnType("nvarchar(max)");

                    b.Property<Guid>("ProductId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("ProductName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<double>("Quantity")
                        .HasColumnType("float");

                    b.Property<byte[]>("RowVersion")
                        .IsConcurrencyToken()
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnType("rowversion");

                    b.Property<Guid>("TransferLogId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid?>("UpdatedByUserId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<DateTimeOffset?>("UpdatedDateTime")
                        .HasColumnType("datetimeoffset");

                    b.HasKey("Id");

                    b.HasIndex("InventId");

                    b.HasIndex("TransferLogId");

                    b.ToTable("TransferLogItem","inventory");
                });

            modelBuilder.Entity("GHPCommerce.Modules.Inventory.Entities.ZoneType", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier")
                        .HasDefaultValueSql("newsequentialid()");

                    b.Property<Guid>("CreatedByUserId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<DateTimeOffset>("CreatedDateTime")
                        .HasColumnType("datetimeoffset");

                    b.Property<string>("Name")
                        .HasColumnType("nvarchar(max)");

                    b.Property<byte[]>("RowVersion")
                        .IsConcurrencyToken()
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnType("rowversion");

                    b.Property<Guid?>("UpdatedByUserId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<DateTimeOffset?>("UpdatedDateTime")
                        .HasColumnType("datetimeoffset");

                    b.HasKey("Id");

                    b.ToTable("ZoneType","inventory");

                    b.HasData(
                        new
                        {
                            Id = new Guid("6bd42e21-e657-4f99-afef-1afe5ceacb16"),
                            CreatedByUserId = new Guid("00000000-0000-0000-0000-000000000000"),
                            CreatedDateTime = new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)),
                            Name = "Vendable"
                        },
                        new
                        {
                            Id = new Guid("6ad42e21-e657-4f99-afef-1afe5ceacb16"),
                            CreatedByUserId = new Guid("00000000-0000-0000-0000-000000000000"),
                            CreatedDateTime = new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)),
                            Name = "Non vendable"
                        });
                });

            modelBuilder.Entity("GHPCommerce.Modules.Inventory.Entities.Invent", b =>
                {
                    b.HasOne("GHPCommerce.Modules.Inventory.Entities.Batch", "Batch")
                        .WithMany("Invents")
                        .HasForeignKey("BatchId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("GHPCommerce.Modules.Inventory.Entities.InventItemTransaction", b =>
                {
                    b.HasOne("GHPCommerce.Modules.Inventory.Entities.Invent", "Invent")
                        .WithMany("InventItemTransactions")
                        .HasForeignKey("InventId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("GHPCommerce.Modules.Inventory.Entities.StockState", b =>
                {
                    b.HasOne("GHPCommerce.Modules.Inventory.Entities.ZoneType", "ZoneType")
                        .WithMany()
                        .HasForeignKey("ZoneTypeId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("GHPCommerce.Modules.Inventory.Entities.StockZone", b =>
                {
                    b.HasOne("GHPCommerce.Modules.Inventory.Entities.ZoneType", "ZoneType")
                        .WithMany()
                        .HasForeignKey("ZoneTypeId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("GHPCommerce.Modules.Inventory.Entities.TransferLogItem", b =>
                {
                    b.HasOne("GHPCommerce.Modules.Inventory.Entities.Invent", "Invent")
                        .WithMany()
                        .HasForeignKey("InventId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("GHPCommerce.Modules.Inventory.Entities.TransferLog", "TransferLog")
                        .WithMany("Items")
                        .HasForeignKey("TransferLogId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });
#pragma warning restore 612, 618
        }
    }
}
